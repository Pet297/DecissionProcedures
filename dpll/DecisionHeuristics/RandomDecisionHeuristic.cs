using dpll.DataStructures;
using dpll.SolvingState;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.DecisionHeuristics
{
    public class RandomDecisionHeuristic : IDecisionHeuristic
    {
        // 88209 is a fixed seed for determinism
        private readonly Random Rng = new(88209);

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
            List<int> unassigned = new();
            for (int i = 1; i < currentAssignment.Length; i++)
            {
                if (currentAssignment[i] == VariableAssignment.Undefined)
                {
                    unassigned.Add(i);
                }
            }

            Debug.Assert(unassigned.Count > 0);

            int pickedIndex = unassigned[Rng.Next(unassigned.Count)];
            return new NextDecision(pickedIndex, false);
        }
    }
}
