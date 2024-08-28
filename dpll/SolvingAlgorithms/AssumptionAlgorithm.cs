using dpll.DataStructures;
using dpll.DecisionHeuristics;
using dpll.SolvingState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.SolvingAlgorithms
{
    public class AssumptionAlgorithm : ISolvingAlgorithm
    {
        private ISolvingAlgorithm InnerAlgrithm;
        private readonly List<int> Assumptions;
        private int NextAssumptionIndex = 0;

        public AssumptionAlgorithm(ISolvingAlgorithm innerAlgorithm, List<int> assumptions)
        {
            InnerAlgrithm = innerAlgorithm;
            Assumptions = assumptions;
        }

        public bool Solve(WorkingFormula formula)
        {
            formula.UnitPropagation();
            if (formula.IsSatisfied) return true;
            if (formula.IsConflict) return false;

            int decision = 0;
            if (NextAssumptionIndex < Assumptions.Count)
            {
                decision = Assumptions[NextAssumptionIndex];
                NextAssumptionIndex++;
            }

            bool satisfiable;
            if (decision == 0)
            {
                satisfiable = InnerAlgrithm.Solve(formula);
            }
            else
            {
                if (formula.IsLiteralUnsatisfied(decision))
                {
                    // Previous assumptions resulted led to a conflict with this assumption.
                    satisfiable = false;
                }
                else 
                {
                    if (formula.IsLiteralUndefined(decision))
                    {
                        formula.Assume(decision);
                    }
                    satisfiable = Solve(formula);
                }
            }

            if (satisfiable) return true;
            else return false;
        }
        public void ApplySettings(AlgorithmSettings settings)
        {
            InnerAlgrithm.ApplySettings(settings);
        }
    }
}
