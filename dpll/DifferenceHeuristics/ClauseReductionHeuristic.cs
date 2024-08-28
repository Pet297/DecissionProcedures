using dpll.DataStructures;
using dpll.SolvingState;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.DifferenceHeuristics
{
    public class ClauseReductionHeuristic : IDifferenceHeuristic
    {
        public double Heuristic(Dictionary<WorkingClause, int> clauseLengthsBefore, Dictionary<WorkingClause, int> clauseLengthsAfter, VariableAssignment[] assignmentBefore, VariableAssignment[] assignmentAfter)
        {
            double sum = 0;

            foreach (WorkingClause clause in clauseLengthsAfter.Keys)
            {
                int k = clauseLengthsAfter[clause];
                if (k >= 2)
                {
                    if (clauseLengthsBefore.ContainsKey(clause) && clauseLengthsBefore[clause] == k)
                    {
                        // This clause is unaffected by assignment -> ignored in the sum.
                        continue;
                    }
                    else
                    {
                        sum += Gamma(k);
                    }
                }
            }

            return sum;
        }
        private static double Gamma(int k)
        {
            return k switch
            {
                2 => 1,
                3 => 0.2,
                4 => 0.05,
                5 => 0.01,
                6 => 0.003,
                _ => 20.4514 * Math.Pow(0.218673, k),
            };
        }
    }
}
