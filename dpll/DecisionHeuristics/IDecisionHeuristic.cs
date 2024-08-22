using dpll.DataStructures;
using dpll.SolvingState;

namespace dpll.DecisionHeuristics
{
    public interface IDecisionHeuristic
    {
        NextDecision GetNextDecision(VariableAssignment[] currentAssignment);
        void AddInitialClause(WorkingClause c);
        void ReportVariablesInConflict(List<int> variablesInConflict);
    }
}
