using dpll.SolvingState;

namespace dpll.DataStructures
{
    public class WatchedFormula : IClauseStateDataStructure
    {
        private readonly Dictionary<WorkingClause, WatchedClause> clauseMap;
        private readonly WorkingFormula formula;

        public LinkedList<WorkingClause>[] PositiveVariableOccurences;
        public LinkedList<WorkingClause>[] NegativeVariableOccurences;

        public bool CanWorkWithLearnedClauses => true;

        public WatchedFormula(WorkingFormula formula)
        {
            clauseMap = new Dictionary<WorkingClause, WatchedClause>();

            PositiveVariableOccurences = new LinkedList<WorkingClause>[formula.VariableCount + 1];
            NegativeVariableOccurences = new LinkedList<WorkingClause>[formula.VariableCount + 1];

            for (int i = 0; i < PositiveVariableOccurences.Length; i++)
            {
                PositiveVariableOccurences[i] = new LinkedList<WorkingClause>();
                NegativeVariableOccurences[i] = new LinkedList<WorkingClause>();
            }

            this.formula = formula;
        }

        public event EventHandler<ClauseStateReportEventArgs>? ClauseStateReport;
        public ClauseState AddInitialClause(WorkingClause clause)
        {
            if (clause.Literals.Length == 0)
            {
                return AddClause(clause, -1, -1);
            }
            else if (clause.Literals.Length == 1)
            {
                return AddClause(clause, 0, -1);
            }
            else
            {
                return AddClause(clause, 0, 1);
            }
        }
        public ClauseState AddLearnedClause(WorkingClause clause, int topLevelLiteralIndex, int assertionLevelLiteralIndex)
        {
            return AddClause(clause, topLevelLiteralIndex, assertionLevelLiteralIndex);
        }
        public ClauseState AddClause(WorkingClause clause, int head0Location, int head1Location)
        {
            WatchedClause watchedClause = new(formula, clause, clause.Literals.ToArray());
            clauseMap.Add(clause, watchedClause);

            if (head0Location != -1)
            {
                watchedClause.Head0 = head0Location;
                int variableIndex = Math.Abs(clause.Literals[head0Location]);
                bool truthValue = clause.Literals[head0Location] > 0;
                (truthValue ? PositiveVariableOccurences : NegativeVariableOccurences)[variableIndex].AddLast(watchedClause.Head0node!);
            }
            if (head1Location != -1)
            {
                watchedClause.Head1 = head1Location;
                int variableIndex = Math.Abs(clause.Literals[head1Location]);
                bool truthValue = clause.Literals[head1Location] > 0;
                (truthValue ? PositiveVariableOccurences : NegativeVariableOccurences)[variableIndex].AddLast(watchedClause.Head1node!);
            }

            return watchedClause.GetClauseState();
        }
        public ClauseState RemoveClause(WorkingClause clause)
        {
            ClauseState state = clauseMap[clause].GetClauseState();
            clauseMap[clause].Head0node?.List?.Remove(clauseMap[clause].Head0node!);
            clauseMap[clause].Head1node?.List?.Remove(clauseMap[clause].Head1node!);
            clauseMap.Remove(clause);
            return state;
        }
        public void Decide(int decision)
        {
            int variableIndex = Math.Abs(decision);

            LinkedListNode<WorkingClause>? node = PositiveVariableOccurences[variableIndex].First;
            while (node != null)
            {
                LinkedListNode<WorkingClause>? next = node.Next;
                WorkingClause clause = node.Value;
                int newLiteral = clauseMap[clause].Decide(decision);
                if (newLiteral != 0)
                {
                    node.List!.Remove(node);
                    int variableIndex2 = Math.Abs(newLiteral);
                    bool truthValue2 = newLiteral > 0;
                    (truthValue2 ? PositiveVariableOccurences : NegativeVariableOccurences)[variableIndex2].AddLast(node);
                }
                ReportClauseState(clause, clauseMap[clause].GetClauseState());
                node = next;
            }
            node = NegativeVariableOccurences[variableIndex].First;
            while (node != null)
            {
                LinkedListNode<WorkingClause>? next = node.Next;
                WorkingClause clause = node.Value;
                int newLiteral = clauseMap[clause].Decide(decision);
                if (newLiteral != 0)
                {
                    node.List!.Remove(node);
                    int variableIndex2 = Math.Abs(newLiteral);
                    bool truthValue2 = newLiteral > 0;
                    (truthValue2 ? PositiveVariableOccurences : NegativeVariableOccurences)[variableIndex2].AddLast(node);
                }
                ReportClauseState(clause, clauseMap[clause].GetClauseState());
                node = next;
            }
        }
        public void UndoDecision(int decision)
        {
            foreach (WorkingClause clause in PositiveVariableOccurences[Math.Abs(decision)])
            {
                ReportClauseState(clause, clauseMap[clause].GetClauseState());
            }
            foreach (WorkingClause clause in NegativeVariableOccurences[Math.Abs(decision)])
            {
                ReportClauseState(clause, clauseMap[clause].GetClauseState());
            }
        }
        public int GetUndefinedLiteral(WorkingClause clause)
        {
            return clauseMap[clause].UnitLiteral;
        }     

        private void ReportClauseState(WorkingClause clause, ClauseState currentState)
        {
            OnClauseStateReport(new ClauseStateReportEventArgs(clause, currentState));
        }
        protected virtual void OnClauseStateReport(ClauseStateReportEventArgs e)
        {
            ClauseStateReport?.Invoke(this, e);
        }

        public List<int> GetImplications(int literal)
        {
            return new List<int>();
        }
        public int GetCurrentLength(WorkingClause clause, VariableAssignment[] assignment)
        {
            // Use inefficient default implementation
            return clause.GetCurrentLength(assignment);
        }
    }
}
