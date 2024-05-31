using dpll.Formula;
using dpll.SolvingState;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.DataStructures
{
    public class WorkingFormula
    {
        // data structure
        private readonly IClauseStateDataStructure DataStructure;
        private readonly CnfFormula Formula;
        private readonly Random Rng = new(88209); // 88209 is a fixed seed for determinism

        // data
        private readonly Dictionary<WorkingClause, LinkedListNode<WorkingClause>> ClauseNodes;
        private readonly Dictionary<ClauseState, LinkedList<WorkingClause>> ClausesPerState;
        private readonly Stack<Decision> DecisionStack;
        private readonly VariableAssignment[] Assignment;
        private readonly WorkingClause?[] Antecedent;
        private readonly int[] LiteralDecisionLevel;
        private readonly long[] LiteralDecisionOrder;
        private int DecisionLevel;
        private readonly List<WorkingClause> LearnedClauses;

        // statistics
        private long decisionCount = 0;
        private long propagatedLiteralsCount = 0;
        private long checkedClauses = 0;
        private long clausesRemovedBySubsumption = 0;
        private long totalLearnedClauses = 0;

        public long DecisionCount => decisionCount;
        public long CheckedClauses => checkedClauses;
        public long PropagatedLiteralsCount => propagatedLiteralsCount;
        public long ClausesRemovedBySubsumption => clausesRemovedBySubsumption;
        public long LearnedClausesCount => LearnedClauses.Count;
        public long TotalLearnedClausesCount =>totalLearnedClauses;

        public WorkingFormula(CnfFormula formula, Func<WorkingFormula, IClauseStateDataStructure> dataStructureGenerator)
        {
            Formula = formula;

            ClauseNodes = new();
            ClausesPerState = new()
            {
                { ClauseState.Unresolved, new() },
                { ClauseState.Unit, new() },
                { ClauseState.Conflict, new() },
                { ClauseState.Satisfied, new() }
            };
            DecisionStack = new();
            LearnedClauses = new();
            Assignment = new VariableAssignment[formula.VariableCount + 1];
            Antecedent = new WorkingClause?[formula.VariableCount + 1];
            LiteralDecisionLevel = new int[formula.VariableCount + 1];
            LiteralDecisionOrder = new long[formula.VariableCount + 1];
            DecisionLevel = 0;

            DataStructure = dataStructureGenerator(this);

            foreach (CnfClause c in formula.Clauses)
            {
                List<int> literals = c.Literals.Select(l => l.Positive ? l.Index : -l.Index).ToList();
                WorkingClause clause = new(literals.ToArray());

                ClauseState state = DataStructure.AddClause(clause);
                var node = ClausesPerState[state].AddLast(clause);
                ClauseNodes.Add(clause, node);
            }

            DataStructure.ClauseStateReport += DataStructure_ClauseStateReport;
        }
        private void DataStructure_ClauseStateReport(object? sender, ClauseStateReportEventArgs e)
        {
            UpdateClauseState(e.Clause, e.CurrentState);
        }

        // solver methods
        public int PickNextDecision()
        {
            List<int> unassigned = new();
            for (int i = 1; i < Assignment.Length; i++)
            {
                if (Assignment[i] == VariableAssignment.Undefined)
                {
                    unassigned.Add(i);
                }
            }

            Debug.Assert(unassigned.Count > 0);

            int pickedIndex = unassigned[Rng.Next(unassigned.Count)];
            return pickedIndex;
        }

        public void Decide(int literal)
        {
            DecisionLevel++;
            Decision decision = new(literal, DecisionLevel);
            DecisionStack.Push(decision);

            Assignment[Math.Abs(literal)] = literal > 0 ? VariableAssignment.Satisfied : VariableAssignment.Falsified;
            LiteralDecisionLevel[Math.Abs(literal)] = DecisionLevel;
            LiteralDecisionOrder[Math.Abs(literal)] = decisionCount + 1 + propagatedLiteralsCount;

            DataStructure.Decide(literal);
            decisionCount++;
        }
        public void UnitPropagation()
        {
            while (ClausesPerState[ClauseState.Unit].Count > 0)
            {
                WorkingClause clause = ClausesPerState[ClauseState.Unit].First!.Value;
                int literal = DataStructure.GetUndefinedLiteral(clause);
                PropagateLiteral(literal, clause);

                if (ClausesPerState[ClauseState.Conflict].Count > 0) break;
            }
        }
        public void Backtrack()
        {
            while (DecisionStack.Count > 0 && DecisionStack.Peek().DecisionLevel == DecisionLevel)
            {
                Decision decision = DecisionStack.Pop();
                Assignment[Math.Abs(decision.Value)] = VariableAssignment.Undefined;
                Antecedent[Math.Abs(decision.Value)] = null;
                LiteralDecisionLevel[Math.Abs(decision.Value)] = 0;
                DataStructure.UndoDecision(decision.Value);
            }
            DecisionLevel--;
        }
        public void BackJump(int level)
        {
            while (DecisionLevel > level)
            {
                Backtrack();
            }
        }
        public bool PropagateLiteral(int literal, WorkingClause antecedent)
        {
            Debug.Assert(Assignment[Math.Abs(literal)] == VariableAssignment.Undefined);
            propagatedLiteralsCount++;
            Assignment[Math.Abs(literal)] = literal > 0 ? VariableAssignment.Satisfied : VariableAssignment.Falsified;
            Antecedent[Math.Abs(literal)] = antecedent;
            DecisionStack.Push(new Decision(literal, DecisionLevel));
            LiteralDecisionLevel[Math.Abs(literal)] = DecisionLevel;
            LiteralDecisionOrder[Math.Abs(literal)] = decisionCount + propagatedLiteralsCount;
            DataStructure.Decide(literal);
            return true;
        }
        public void AddClause(int[] clause)
        {
            WorkingClause workingClause = new(clause);

            // Subsumption Removal
            HashSet<int> literals = clause.ToHashSet();
            bool learnClause = true;
            for (int i = 0; i < LearnedClauses.Count; i++)
            {
                HashSet<int> literals2 = LearnedClauses[i].Literals.ToHashSet();
                if (literals.IsSubsetOf(literals2))
                {
                    RemoveClause(LearnedClauses[i]);
                    i--;
                }
                else if (literals2.IsSubsetOf(literals))
                {
                    learnClause = false;
                    break;
                }
            }

            // Adding to structure
            if (learnClause)
            {
                ClauseState state = DataStructure.AddClause(workingClause);
                Debug.Assert(state != ClauseState.Conflict);
                var node = ClausesPerState[state].AddLast(workingClause);
                ClauseNodes.Add(workingClause, node);
                LearnedClauses.Add(workingClause);
            }

            totalLearnedClauses++;
        }
        public void RemoveClause(WorkingClause clause)
        {
            // TODO: Check if it breaks antecedents (it shouldn't)
            ClauseState state = DataStructure.RemoveClause(clause);
            ClausesPerState[state].Remove(clause);
            ClauseNodes.Remove(clause);
            LearnedClauses.Remove(clause);
            clausesRemovedBySubsumption++;
        }

        public void ClearLearnedClauses(int numberOfClausesToKeep)
        {
            List<WorkingClause> learnedClauses = new(LearnedClauses);
            learnedClauses.Sort((a, b) => a.Literals.Length - b.Literals.Length);
            IEnumerable<WorkingClause> clausesToRemove = learnedClauses.Skip(numberOfClausesToKeep);
            foreach(WorkingClause clause in clausesToRemove)
            {
                RemoveClause(clause);
            }
        }

        public Tuple<int[],int> FindAssertiveClauseAndDecisionLevel()
        {
            Debug.Assert(Antecedent[0] != null);

            List<int> clause = Antecedent[0]!.Literals.ToList();

            while (true)
            {
                int goodLiteral = 0;
                foreach(int literal in clause)
                {
                    if (LiteralDecisionLevel[Math.Abs(literal)] == DecisionLevel && Antecedent[Math.Abs(literal)] != null)
                    {
                        if (goodLiteral == 0 || LiteralDecisionOrder[Math.Abs(literal)] > LiteralDecisionOrder[Math.Abs(literal)])
                        {
                            goodLiteral = literal;
                        }
                    }
                }
                if (goodLiteral == 0)
                {
                    break;
                }
                foreach (int literal in Antecedent[Math.Abs(goodLiteral)]!.Literals)
                {
                    Debug.Assert(literal != goodLiteral);
                    if (clause.Contains(-literal))
                    {
                        clause.Remove(-literal);
                    }
                    else if (!clause.Contains(literal))
                    {
                        clause.Add(literal);
                    }
                }
            }

            // Get decision level
            if (clause.Count == 0) return new Tuple<int[], int>(clause.ToArray(), -1);
            if (clause.Count == 1) return new Tuple<int[], int>(clause.ToArray(), 0);

            List<int> decisionLevels = clause.Select((literal) => LiteralDecisionLevel[Math.Abs(literal)]).ToList();
            decisionLevels.Sort();

            return new Tuple<int[], int>(clause.ToArray(), decisionLevels[^2]);
        }

        public bool IsSatisfied =>
            ClausesPerState[ClauseState.Unresolved].Count == 0 &&
            ClausesPerState[ClauseState.Unit].Count == 0 &&
            ClausesPerState[ClauseState.Conflict].Count == 0;
        public bool IsConflict =>
            Antecedent[0] != null;

        // clause state report
        public void UpdateClauseState(WorkingClause clause, ClauseState newState)
        {
            checkedClauses++;
            LinkedListNode<WorkingClause> node = ClauseNodes[clause];
            node.List!.Remove(node);
            ClausesPerState[newState].AddLast(node);

            if (newState == ClauseState.Conflict && Antecedent[0] == null)
            {
                Antecedent[0] = clause;
            }
            else if (newState != ClauseState.Conflict && Antecedent[0] == clause)
            {
                Antecedent[0] = null;
            }
        }

        // general methods
        public bool IsLiteralSatisfied(int literal)
        {
            int variableIndex = Math.Abs(literal);
            bool truthValue = literal > 0;

            return
                Assignment[variableIndex] == VariableAssignment.Satisfied && truthValue == true ||
                Assignment[variableIndex] == VariableAssignment.Falsified && truthValue == false;
        }
        public bool IsLiteralUnsatisfied(int literal)
        {
            int variableIndex = Math.Abs(literal);
            bool truthValue = literal > 0;

            return
                Assignment[variableIndex] == VariableAssignment.Satisfied && truthValue == false ||
                Assignment[variableIndex] == VariableAssignment.Falsified && truthValue == true;
        }
        public bool IsLiteralUndefined(int literal)
        {
            return Assignment[Math.Abs(literal)] == VariableAssignment.Undefined;
        }
        public long GetVariableDecisionOrder(int variable)
        {
            return LiteralDecisionOrder[variable];
        }
        public int VariableCount => Assignment.Length - 1;
        public void PrintAsignment()
        {
            bool first = true;
            Console.Write("Assignment: { ");
            for (int i = 0; i < Assignment.Length; i++)
            {
                if (Assignment[i] != VariableAssignment.Undefined)
                {
                    if (!first) Console.Write(", ");
                    if (Assignment[i] == VariableAssignment.Falsified) Console.Write("-");
                    if (Formula.VariableNames.ContainsKey(i)) Console.Write(Formula.VariableNames[i]);
                    else Console.Write(i);
                    first = false;
                }
            }
            Console.WriteLine(" }");
        }
        public void PrintStats(double elapsedSeconds)
        {
            Console.WriteLine($"Total CPU time: {elapsedSeconds:0.000000} s");
            Console.WriteLine($"Number of decisions: {DecisionCount}");
            Console.WriteLine($"Steps of unit propagation: {PropagatedLiteralsCount}");
            Console.WriteLine($"Number of checked clauses: {CheckedClauses}");
            Console.WriteLine($"Number of clauses removed by subsumptions: {ClausesRemovedBySubsumption}");
        }
    }
}
