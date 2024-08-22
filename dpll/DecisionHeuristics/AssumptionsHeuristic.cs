using dpll.DataStructures;
using dpll.SolvingState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.DecisionHeuristics
{
    public class AssumptionsHeuristic : IDecisionHeuristic
    {
        private readonly IDecisionHeuristic InnerHeuristic;
        private readonly List<int> Assumptions;

        public AssumptionsHeuristic(List<int> assumptions, IDecisionHeuristic innerHeuristic)
        {
            Assumptions = assumptions;
            InnerHeuristic = innerHeuristic;
        }

        public void AddInitialClause(WorkingClause c)
        {
            // NOOP
        }

        public void ReportVariablesInConflict(List<int> variablesInConflict)
        {
            // NOOP
        }

        public NextDecision GetNextDecision(VariableAssignment[] currentAssignment)
        {
            foreach (int i in Assumptions)
            {
                if (currentAssignment[Math.Abs(i)] == VariableAssignment.Undefined)
                {
                    return new NextDecision(i, true);
                }
            }

            return InnerHeuristic.GetNextDecision(currentAssignment);
        }
    }
}
