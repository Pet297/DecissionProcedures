using dpll.SolvingState;

namespace dpll.DataStructures
{
    public interface IClauseStateDataStructure
    {
        event EventHandler<ClauseStateReportEventArgs>? ClauseStateReport; 

        ClauseState AddInitialClause(WorkingClause clauseToAdd);
        ClauseState AddLearnedClause(WorkingClause clauseToAdd, int topLevelLiteralIndex, int assertionLevelLiteralIndex);
        ClauseState RemoveClause(WorkingClause clauseToRemove);
        void Decide(int decision);
        void UndoDecision(int decision);
        int GetUndefinedLiteral(WorkingClause clause);
    }
}
