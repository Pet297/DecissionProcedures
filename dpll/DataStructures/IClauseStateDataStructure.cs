using dpll.SolvingState;

namespace dpll.DataStructures
{
    public interface IClauseStateDataStructure
    {
        event EventHandler<ClauseStateReportEventArgs>? ClauseStateReport; 

        ClauseState AddClause(WorkingClause clauseToAdd);
        ClauseState RemoveClause(WorkingClause clauseToRemove);
        void Decide(int decision);
        void UndoDecision(int decision);
        int GetUndefinedLiteral(WorkingClause clause);
    }
}
