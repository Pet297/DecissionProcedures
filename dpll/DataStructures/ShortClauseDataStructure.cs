using dpll.SolvingState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.DataStructures
{
    internal class ShortClauseDataStructure : IClauseStateDataStructure
    {
        private readonly Dictionary<WorkingClause, AdjacencyListClause> clauseMap;
        private readonly IClauseStateDataStructure InnerDataStructure;
        public event EventHandler<ClauseStateReportEventArgs>? ClauseStateReport;
        private readonly WorkingFormula formula;

        public List<WorkingClause>[] PositiveVariableOccurences;
        public List<WorkingClause>[] NegativeVariableOccurences;
        public List<int>[] PositiveVariableImplications;
        public List<int>[] NegativeVariableImplications;

        public ShortClauseDataStructure(WorkingFormula formula, IClauseStateDataStructure innerDataStructure)
        {
            InnerDataStructure = innerDataStructure;
            clauseMap = new();

            PositiveVariableOccurences = new List<WorkingClause>[formula.VariableCount + 1];
            NegativeVariableOccurences = new List<WorkingClause>[formula.VariableCount + 1];
            PositiveVariableImplications = new List<int>[formula.VariableCount + 1];
            NegativeVariableImplications = new List<int>[formula.VariableCount + 1];

            for (int i = 0; i < PositiveVariableOccurences.Length; i++)
            {
                PositiveVariableOccurences[i] = new List<WorkingClause>();
                NegativeVariableOccurences[i] = new List<WorkingClause>();
            }

            this.formula = formula;
            InnerDataStructure.ClauseStateReport += ClauseStateReport;
        }

        public ClauseState AddInitialClause(WorkingClause clause)
        {
            if (clause.Literals.Length > 2)
            {
                return InnerDataStructure.AddInitialClause(clause);
            }
            else
            {
                return AddSmallClause(clause);
            }
        }
        public ClauseState AddLearnedClause(WorkingClause clause, int topLevelLiteralIndex, int assertionLevelLiteralIndex)
        {
            if (clause.Literals.Length > 2)
            {
                return InnerDataStructure.AddLearnedClause(clause, topLevelLiteralIndex, assertionLevelLiteralIndex);
            }
            else
            {
                return AddSmallClause(clause);
            }
        }
        private ClauseState AddSmallClause(WorkingClause clause)
        {
            AdjacencyListClause listClause = new(formula, clause.Literals.ToArray());
            clauseMap.Add(clause, listClause);

            long lowestDecisionOrder = long.MaxValue;
            int firstSatisfactionWitness = 0;

            foreach (int literal in clause.Literals)
            {
                int variableIndex = Math.Abs(literal);
                bool truthValue = literal > 0;

                (truthValue ? PositiveVariableOccurences : NegativeVariableOccurences)[variableIndex].Add(clause);

                if (formula.IsLiteralSatisfied(literal))
                {
                    if (formula.GetVariableDecisionOrder(Math.Abs(literal)) < lowestDecisionOrder)
                    {
                        lowestDecisionOrder = formula.GetVariableDecisionOrder(Math.Abs(literal));
                        firstSatisfactionWitness = literal;
                    }
                }
                if (formula.IsLiteralUnsatisfied(literal))
                {
                    listClause.FalsifyLiteral();
                }
            }

            if (firstSatisfactionWitness != 0)
            {
                listClause.SatisfyLiteral(firstSatisfactionWitness);
            }

            (clause.Literals[0] > 0 ? NegativeVariableImplications : PositiveVariableImplications)[Math.Abs(clause.Literals[0])].Add(clause.Literals[1]);
            (clause.Literals[1] > 0 ? NegativeVariableImplications : PositiveVariableImplications)[Math.Abs(clause.Literals[1])].Add(clause.Literals[0]);

            return listClause.GetClauseState();
        }
        
        public void Decide(int decision)
        {
            int variableIndex = Math.Abs(decision);
            bool truthValue = decision > 0;

            foreach (WorkingClause clause in (truthValue ? PositiveVariableOccurences : NegativeVariableOccurences)[variableIndex])
            {
                clauseMap[clause].SatisfyLiteral(decision);
                ReportClauseState(clause, clauseMap[clause].GetClauseState());
            }
            foreach (WorkingClause clause in (truthValue ? NegativeVariableOccurences : PositiveVariableOccurences)[variableIndex])
            {
                clauseMap[clause].FalsifyLiteral();
                ReportClauseState(clause, clauseMap[clause].GetClauseState());
            }

            InnerDataStructure.Decide(decision);
        }
        public List<int> GetImplications(int literal)
        {
            return (literal > 0 ? PositiveVariableImplications : NegativeVariableImplications)[Math.Abs(literal)];
        }
        public int GetUndefinedLiteral(WorkingClause clause)
        {
            if (clause.Literals.Length > 2)
            {
                return InnerDataStructure.GetUndefinedLiteral(clause);
            }
            else
            {
                return clauseMap[clause].GetUnitLiteral();
            }
        }
        public ClauseState RemoveClause(WorkingClause clause)
        {
            if (clause.Literals.Length > 2)
            {
                return InnerDataStructure.RemoveClause(clause);
            }
            else
            {
                ClauseState state = clauseMap[clause].GetClauseState();
                foreach (int literal in clause.Literals)
                {
                    int variable = Math.Abs(literal);
                    bool truthValue = literal > 0;
                    (truthValue ? PositiveVariableOccurences : NegativeVariableOccurences)[variable].Remove(clause);
                }
            (clause.Literals[0] > 0 ? NegativeVariableImplications : PositiveVariableImplications)[Math.Abs(clause.Literals[0])].Remove(clause.Literals[1]);
                (clause.Literals[1] > 0 ? NegativeVariableImplications : PositiveVariableImplications)[Math.Abs(clause.Literals[1])].Remove(clause.Literals[0]);
                clauseMap.Remove(clause);
                return state;
            }
        }
        public void UndoDecision(int decision)
        {
            int variableIndex = Math.Abs(decision);
            bool truthValue = decision > 0;

            foreach (WorkingClause clause in (truthValue ? PositiveVariableOccurences : NegativeVariableOccurences)[variableIndex])
            {
                clauseMap[clause].UndoSatisfyLiteral(decision);
                ReportClauseState(clause, clauseMap[clause].GetClauseState());
            }
            foreach (WorkingClause clause in (truthValue ? NegativeVariableOccurences : PositiveVariableOccurences)[variableIndex])
            {
                clauseMap[clause].UndoFalsifyLiteral();
                ReportClauseState(clause, clauseMap[clause].GetClauseState());
            }

            InnerDataStructure.UndoDecision(decision);
        }

        private void ReportClauseState(WorkingClause clause, ClauseState currentState)
        {
            OnClauseStateReport(new ClauseStateReportEventArgs(clause, currentState));
        }
        protected virtual void OnClauseStateReport(ClauseStateReportEventArgs e)
        {
            ClauseStateReport?.Invoke(this, e);
        }

        public int GetCurrentLength(WorkingClause clause, VariableAssignment[] assignment)
        {
            if (clause.Literals.Length > 2) return InnerDataStructure.GetCurrentLength(clause, assignment);
            else return clauseMap[clause].GetCurrentLength();
        }
    }
}
