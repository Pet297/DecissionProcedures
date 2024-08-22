using dpll.DataStructures;
using dpll.DecisionHeuristics;

namespace dpll.SolvingAlgorithms
{
    public class Dpll : ISolvingAlgorithm
    {
        public bool Solve(WorkingFormula formula)
        {
            formula.UnitPropagation();
            if (formula.IsSatisfied) return true;
            if (formula.IsConflict) return false;
            NextDecision nextDecision = formula.PickNextDecision();

            formula.Decide(nextDecision.Decision);
            bool satisfiable = Solve(formula);
            if (satisfiable) return true;
            else formula.Backtrack();

            if (!nextDecision.IsAssumption)
            {
                formula.Decide(-nextDecision.Decision);
                satisfiable = Solve(formula);
                if (satisfiable) return true;
                else formula.Backtrack();
            }
            
            return false;
        }
        public void ApplySettings(AlgorithmSettings settings)
        {
            // NOOP
        }
    }
}
