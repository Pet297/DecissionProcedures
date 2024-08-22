using dpll.DecisionHeuristics;
using dpll.Formula;
using dpll.SolvingState;
using System.Diagnostics;

namespace dpll.DataStructures
{
    public class WorkingFormula
    {
        // data structure
        private readonly IClauseStateDataStructure DataStructure;
        private readonly CnfFormula Formula;
        private readonly IDecisionHeuristic DecisionHeuristic;

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
        private readonly Dictionary<WorkingClause, HashSet<int>> LearnedClauseLiteralSets;

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

        public WorkingFormula(CnfFormula formula, Func<WorkingFormula, IClauseStateDataStructure> dataStructureGenerator, Func<WorkingFormula, IDecisionHeuristic> decisionHeuristicGenerator)
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
            LearnedClauseLiteralSets = new();
            DecisionLevel = 0;

            DataStructure = dataStructureGenerator(this);
            DecisionHeuristic = decisionHeuristicGenerator(this);

            foreach (CnfClause c in formula.Clauses)
            {
                List<int> literals = c.Literals.Select(l => l.Positive ? l.Index : -l.Index).ToList();
                WorkingClause clause = new(literals.ToArray());

                ClauseState state = DataStructure.AddInitialClause(clause);
                var node = ClausesPerState[state].AddLast(clause);
                ClauseNodes.Add(clause, node);

                DecisionHeuristic.AddInitialClause(clause);
            }

            DataStructure.ClauseStateReport += DataStructure_ClauseStateReport;
        }
        private void DataStructure_ClauseStateReport(object? sender, ClauseStateReportEventArgs e)
        {
            UpdateClauseState(e.Clause, e.CurrentState);
        }

        // solver methods
        public NextDecision PickNextDecision()
        {
            return DecisionHeuristic.GetNextDecision(Assignment);
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
        public bool PropagateLiteral(int literal, WorkingClause? antecedent)
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
        public void AddLearnedClause(int[] clause, int topLevelLiteralIndex, int assertionLevelLiteralIndex)
        {
            WorkingClause workingClause = new(clause);

            // Subsumption Removal
            bool learnClause = true;
            HashSet<int> literals = clause.ToHashSet();
            for (int i = 0; i < LearnedClauses.Count; i++)
            {
                HashSet<int> literals2 = LearnedClauseLiteralSets[LearnedClauses[i]];
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
                ClauseState state = DataStructure.AddLearnedClause(workingClause, topLevelLiteralIndex, assertionLevelLiteralIndex);
                Debug.Assert(state != ClauseState.Conflict);
                var node = ClausesPerState[state].AddLast(workingClause);
                ClauseNodes.Add(workingClause, node);
                LearnedClauses.Add(workingClause);
                LearnedClauseLiteralSets.Add(workingClause, literals);
            }

            totalLearnedClauses++;
        }
        public void RemoveClause(WorkingClause clause)
        {
            ClauseState state = DataStructure.RemoveClause(clause);
            ClausesPerState[state].Remove(clause);
            ClauseNodes.Remove(clause);
            LearnedClauses.Remove(clause);
            LearnedClauseLiteralSets.Remove(clause);
            clausesRemovedBySubsumption++;
        }

        public void ClearLearnedClauses(int numberOfClausesToKeep)
        {
            HashSet<WorkingClause> learnedClauses = new(LearnedClauses);

            for (int i = 0; i < Antecedent.Length; i++)
            {
                if (Antecedent[i] != null)
                {
                    learnedClauses.Remove(Antecedent[i]!);
                }
            }

            int numberOfAntecedents = LearnedClauses.Count - learnedClauses.Count;

            List<WorkingClause> learnedClauses2 = new(learnedClauses);

            learnedClauses2.Sort((a, b) => a.Literals.Length - b.Literals.Length);
            IEnumerable<WorkingClause> clausesToRemove = learnedClauses2.Skip(Math.Min(0, numberOfClausesToKeep - numberOfAntecedents));
            foreach(WorkingClause clause in clausesToRemove)
            {
                RemoveClause(clause);
            }
        }

        public ConflictAnalysisResult DoConflictAnalysis()
        {
            Debug.Assert(Antecedent[0] != null);

            HashSet<int> clause = Antecedent[0]!.Literals.ToHashSet();
            HashSet<int> involvedVariables = new HashSet<int>();

            while (true)
            {
                int goodLiteral = 0;
                foreach (int literal in clause)
                {
                    involvedVariables.Add(Math.Abs(literal));
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

            // Report to decision heuristic
            DecisionHeuristic.ReportVariablesInConflict(involvedVariables.ToList());

            // Get decision level
            if (clause.Count == 0) return new ConflictAnalysisResult(clause.ToArray(), -1, -1, -1);
            if (clause.Count == 1) return new ConflictAnalysisResult(clause.ToArray(), 0, 0, -1);

            // Sorts literals by decision level from highest to lowest
            List<int> learnedClauseLiterals = clause.ToList();
            learnedClauseLiterals.Sort((a, b) => LiteralDecisionLevel[Math.Abs(b)] - LiteralDecisionLevel[Math.Abs(a)]);

            int[] learnedClause = learnedClauseLiterals.ToArray();

            // Literal at index 0 should have highest decision level, literal at index 1 should be on the assertion level.
            return new ConflictAnalysisResult(learnedClause, LiteralDecisionLevel[Math.Abs(learnedClause[1])], 0, 1);
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
