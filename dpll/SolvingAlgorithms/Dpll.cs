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
            int nextDecision = formula.PickNextDecision();

            formula.Decide(nextDecision);
            bool satisfiable = Solve(formula);
            if (satisfiable) return true;
            else formula.Backtrack();
            
            formula.Decide(-nextDecision);
            satisfiable = Solve(formula);
            if (satisfiable) return true;
            else formula.Backtrack();
            
            return false;
        }
        public void ApplySettings(AlgorithmSettings settings)
        {
            // NOOP
        }
    }
}
