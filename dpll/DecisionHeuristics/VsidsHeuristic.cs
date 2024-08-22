using dpll.DataStructures;
using dpll.SolvingState;

namespace dpll.DecisionHeuristics
{
    public class VsidsHeuristic : IDecisionHeuristic
    {
        private const double BumpDivisor = 0.95;
        private const double RescaleFactor = 1e10;

        private readonly List<int> VariableOrder;
        private readonly double[] Activity;
        private double bump;

        public VsidsHeuristic(WorkingFormula formula)
        {
            Activity = new double[formula.VariableCount + 1];
            VariableOrder = new List<int>();
            bump = 1;

            for (int i = 1; i <= formula.VariableCount; i++)
            {
                VariableOrder.Add(i);
            }
        }

        public void AddInitialClause(WorkingClause c)
        {
            // NOOP
        }

        public void ReportVariablesInConflict(List<int> variablesInConflict)
        {
            foreach(int variable in variablesInConflict)
            {
                Activity[variable] += bump;
            }

            bump /= BumpDivisor;
            if (bump > RescaleFactor) RescaleActivity();

            // Desceding order based on activity.
            VariableOrder.Sort((a, b) => Activity[b].CompareTo(Activity[a]));
        }

        private void RescaleActivity()
        {
            bump /= RescaleFactor;
            for (int i = 0; i < Activity.Length; i++)
            {
                Activity[i] /= RescaleFactor;
            }
        }

        public NextDecision GetNextDecision(VariableAssignment[] currentAssignment)
        {
            foreach (int i in VariableOrder)
            {
                if (currentAssignment[Math.Abs(i)] == VariableAssignment.Undefined)
                {
                    return new NextDecision(-i, false);
                }
            }

            throw new Exception("This shouldn't happen");
        }
    }
}
