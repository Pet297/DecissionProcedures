using System.Diagnostics;

namespace dpll.DataStructures
{
    public class WatchedFormula : IClauseStateDataStructure
    {
        private readonly Dictionary<WorkingClause, WatchedClause> clauseMap;
        private readonly WorkingFormula formula;

        public LinkedList<WorkingClause>[] PositiveVariableOccurences;
        public LinkedList<WorkingClause>[] NegativeVariableOccurences;

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
        public ClauseState AddClause(WorkingClause clause)
        {
            WatchedClause watchedClause = new(formula, clause, clause.Literals.ToArray());
            clauseMap.Add(clause, watchedClause);

            List<int> undefinedLiteralPositions = watchedClause.GetUndefinedLiteralPositions();
            List<int> otherLiteralPositions = watchedClause.GetDecidedLiteralPositionsByMostRecent();
            Debug.Assert(undefinedLiteralPositions.Count + otherLiteralPositions.Count == clause.Literals.Length);

            if (clause.Literals.Length >= 1)
            {
                int index;
                if (undefinedLiteralPositions.Count >= 1)
                {
                    index = undefinedLiteralPositions[0];
                }
                else
                {
                    index = otherLiteralPositions[0];
                }
                watchedClause.Head0 = index;
                int variableIndex = Math.Abs(clause.Literals[index]);
                bool truthValue = clause.Literals[index] > 0;
                (truthValue ? PositiveVariableOccurences : NegativeVariableOccurences)[variableIndex].AddLast(watchedClause.Head0node!);
            }
            if (clause.Literals.Length >= 2)
            {
                int index;
                if (undefinedLiteralPositions.Count >= 2)
                {
                    index = undefinedLiteralPositions[1];
                }
                else
                {
                    index = otherLiteralPositions[1 - undefinedLiteralPositions.Count];
                }
                watchedClause.Head1 = index;
                int variableIndex = Math.Abs(clause.Literals[index]);
                bool truthValue = clause.Literals[index] > 0;
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
    }
}
