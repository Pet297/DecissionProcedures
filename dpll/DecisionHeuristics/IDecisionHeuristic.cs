using dpll.DataStructures;
using dpll.SolvingState;

namespace dpll.DecisionHeuristics
{
    public interface IDecisionHeuristic
    {
        int GetNextDecision(VariableAssignment[] currentAssignment);
        void AddInitialClause(WorkingClause c);
        void ReportVariablesInConflict(List<int> variablesInConflict);
    }
}
