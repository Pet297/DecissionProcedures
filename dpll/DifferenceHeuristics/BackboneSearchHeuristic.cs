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
    public class BackboneSearchHeuristic : IDifferenceHeuristic
    {
        public double Heuristic(Dictionary<WorkingClause, int> clauseLengthsBefore, Dictionary<WorkingClause, int> clauseLengthsAfter, VariableAssignment[] assignmentBefore, VariableAssignment[] assignmentAfter)
        {
            Tuple<double[], double[]> LiteralValues = ValuePerLiteral(assignmentBefore.Length - 1, clauseLengthsAfter);
            double[] PositiveLiteralValues = LiteralValues.Item1;
            double[] NegativeLiteralValues = LiteralValues.Item2;

            double sum = 0;

            foreach (WorkingClause clause in clauseLengthsAfter.Keys)
            {
                if (clauseLengthsAfter[clause] == 2)
                {
                    if (clauseLengthsBefore.ContainsKey(clause) && clauseLengthsBefore[clause] == 2)
                    {
                        // This clause is unaffected by assignment -> ignored in the sum.
                        continue;
                    }
                    else
                    {
                        List<int> unassignedLiterals = clause.GetUndefinedLiterals(assignmentAfter);
                        Debug.Assert(unassignedLiterals.Count == 2);

                        int l1 = unassignedLiterals[0];
                        int l2 = unassignedLiterals[1];

                        // Less than 0, because the literal is inverted in the formula
                        double variableContribution1 = (l1 < 0 ? PositiveLiteralValues : NegativeLiteralValues)[Math.Abs(l1)];
                        double variableContribution2 = (l2 < 0 ? PositiveLiteralValues : NegativeLiteralValues)[Math.Abs(l2)];
                        sum += variableContribution1 * variableContribution2;
                    }
                }
            }

            return sum;
        }
        private static Tuple<double[], double[]> ValuePerLiteral(int variableCount, Dictionary<WorkingClause, int> clauseLengthsAfterPositive)
        {
            double[] PositiveLiteralValues = new double[variableCount + 1];
            double[] NegativeLiteralValues = new double[variableCount + 1];

            foreach (WorkingClause clause in clauseLengthsAfterPositive.Keys)
            {
                // k holds the actual size of the clause given the partial assignment.
                int k = clauseLengthsAfterPositive[clause];

                // More than k values might be iterated here, because falsified literals are included.
                // Their heuristic value is irrelevant in the main calculation.
                foreach (int literal in clause.Literals)
                {
                    bool sign = literal > 0;
                    int value = Math.Abs(literal);

                    (sign ? PositiveLiteralValues : NegativeLiteralValues)[value] += Math.Pow(2, 3 - k);
                }
            }

            return new Tuple<double[], double[]>(PositiveLiteralValues, NegativeLiteralValues);
        }
    }
}
