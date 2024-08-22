namespace dpll.DataStructures
{
    public class AdjacencyListFormula : IClauseStateDataStructure
    {
        private readonly Dictionary<WorkingClause, AdjacencyListClause> clauseMap;
        private readonly WorkingFormula formula;

        public List<WorkingClause>[] PositiveVariableOccurences;
        public List<WorkingClause>[] NegativeVariableOccurences;

        public AdjacencyListFormula(WorkingFormula formula)
        {
            clauseMap = new Dictionary<WorkingClause, AdjacencyListClause>();

            PositiveVariableOccurences = new List<WorkingClause>[formula.VariableCount + 1];
            NegativeVariableOccurences = new List<WorkingClause>[formula.VariableCount + 1];

            for (int i = 0; i < PositiveVariableOccurences.Length; i++)
            {
                PositiveVariableOccurences[i] = new List<WorkingClause>();
                NegativeVariableOccurences[i] = new List<WorkingClause>();
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

        private void ReportClauseState(WorkingClause clause, ClauseState currentState)
        {
            OnClauseStateReport(new ClauseStateReportEventArgs(clause, currentState));
        }
        protected virtual void OnClauseStateReport(ClauseStateReportEventArgs e)
        {
            ClauseStateReport?.Invoke(this, e);
        }

    }
}
