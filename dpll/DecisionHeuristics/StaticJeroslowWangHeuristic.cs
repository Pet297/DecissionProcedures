using dpll.DataStructures;
using dpll.SolvingState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.DecisionHeuristics
{
    public class StaticJeroslowWangHeuristic : IDecisionHeuristic
    {
        private readonly double[] VariableScore;
        private readonly List<int> VariableOrder;
        private bool sorted = false;

        public StaticJeroslowWangHeuristic(WorkingFormula formula)
        {
            VariableScore = new double[formula.VariableCount + 1];
            VariableOrder = new List<int>();

            for (int i = 1; i <= formula.VariableCount; i++)
            {
                VariableOrder.Add(i);
            }
        }

        public void AddInitialClause(WorkingClause c)
        {
            foreach (int i in c.Literals)
            {
                VariableScore[Math.Abs(i)] += Math.Pow(2, -c.Literals.Length);
            }
        }

        public void ReportVariablesInConflict(List<int> variablesInConflict)
        {
            // NOOP
        }

        public NextDecision GetNextDecision(VariableAssignment[] currentAssignment)
        {
            if (!sorted)
            {
                // Desceding order based on score.
                VariableOrder.Sort((a, b) => VariableScore[b].CompareTo(VariableScore[a]));
                sorted = true;
            }

            foreach (int i in VariableOrder)
            {
                if (currentAssignment[Math.Abs(i)] == VariableAssignment.Undefined)
                {
                    return new NextDecision(i, false);
                }
            }

            throw new Exception("This shouldn't happen");
        }
    }
}
