using dpll.SolvingState;

namespace dpll.DataStructures
{
    public class EagerAdjacencyListFormula : IClauseStateDataStructure
    {
        private readonly Dictionary<WorkingClause, AdjacencyListClause> clauseMap;
        private readonly WorkingFormula formula;

        public List<int>[] PositiveVariableImplications;
        public List<int>[] NegativeVariableImplications;
        public List<WorkingClause>[] PositiveVariableOccurences;
        public List<WorkingClause>[] NegativeVariableOccurences;

        public bool CanWorkWithLearnedClauses => false;

        public EagerAdjacencyListFormula(WorkingFormula formula)
        {
            clauseMap = new Dictionary<WorkingClause, AdjacencyListClause>();

            PositiveVariableOccurences = new List<WorkingClause>[formula.VariableCount + 1];
            NegativeVariableOccurences = new List<WorkingClause>[formula.VariableCount + 1];
            PositiveVariableImplications = new List<int>[formula.VariableCount + 1];
            NegativeVariableImplications = new List<int>[formula.VariableCount + 1];

            for (int i = 0; i < PositiveVariableOccurences.Length; i++)
            {
                PositiveVariableOccurences[i] = new List<WorkingClause>();
                NegativeVariableOccurences[i] = new List<WorkingClause>();
                PositiveVariableImplications[i] = new List<int>();
                NegativeVariableImplications[i] = new List<int>();
            }

            this.formula = formula;
        }

        public event EventHandler<ClauseStateReportEventArgs>? ClauseStateReport;
        public ClauseState AddInitialClause(WorkingClause clause)
        {
            return AddClause(clause);
        }
        public ClauseState AddLearnedClause(WorkingClause clause, int topLevelLiteralIndex, int assertionLevelLiteralIndex)
        {
            return AddClause(clause);
        }
        private ClauseState AddClause(WorkingClause clause)
        {
            if (clause.Literals.Length == 2)
            {
                return AddBinaryClause(clause);
            }
            else
            {
                return AddNonBinaryClause(clause);
            }
        }
        private ClauseState AddBinaryClause(WorkingClause clause)
        {
            int literal1 = clause.Literals[0];
            int literal2 = clause.Literals[1];

            bool sign1 = literal1 > 0;
            bool sign2 = literal2 > 0;
            int index1 = Math.Abs(literal1);
            int index2 = Math.Abs(literal2);

            // (not literal1) => literal2, (not literal2) => literal1
            (sign1 ? NegativeVariableImplications : PositiveVariableImplications)[index1].Add(literal2);
            (sign2 ? NegativeVariableImplications : PositiveVariableImplications)[index2].Add(literal1);

            return ClauseState.ManagedByImplications;
        }
        private ClauseState AddNonBinaryClause(WorkingClause clause)
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

            return listClause.GetClauseState();
        }
        public ClauseState RemoveClause(WorkingClause clause)
        {
            if (clause.Literals.Length == 2)
            {
                return RemoveBinaryClause(clause);
            }
            else
            {
                return RemoveNonBinaryClause(clause);
            }
        }
        public ClauseState RemoveBinaryClause(WorkingClause clause)
        {
            int literal1 = clause.Literals[0];
            int literal2 = clause.Literals[1];

            bool sign1 = literal1 > 0;
            bool sign2 = literal2 > 0;
            int index1 = Math.Abs(literal1);
            int index2 = Math.Abs(literal2);

            // REMOVE (not literal1) => literal2, (not literal2) => literal1
            (sign1 ? NegativeVariableImplications : PositiveVariableImplications)[index1].Remove(literal2);
            (sign2 ? NegativeVariableImplications : PositiveVariableImplications)[index2].Remove(literal1);

            return ClauseState.ManagedByImplications;
        }
        public ClauseState RemoveNonBinaryClause(WorkingClause clause)
        {
            ClauseState state = clauseMap[clause].GetClauseState();
            foreach (int literal in clause.Literals)
            {
                int variable = Math.Abs(literal);
                bool truthValue = literal > 0;
                (truthValue ? PositiveVariableOccurences : NegativeVariableOccurences)[variable].Remove(clause);
            }
            clauseMap.Remove(clause);
            return state;
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
        }
        public int GetUndefinedLiteral(WorkingClause clause)
        {
            return clauseMap[clause].GetUnitLiteral();
        }

        public List<int> GetImplications(int literal)
        {
            bool sign = literal > 0;
            int index = Math.Abs(literal);
            return (sign ? PositiveVariableImplications : NegativeVariableImplications)[index];
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
            if (clause.Literals.Length == 2)
            {
                return clause.GetCurrentLength(assignment);
            }
            else
            {
                return clauseMap[clause].GetCurrentLength();
            }
        }
    }
}
