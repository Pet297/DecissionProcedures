using dpll.DataStructures;
using dpll.SolvingState;

namespace dpll.SolvingAlgorithms
{
    public interface ISolvingAlgorithm
    {
        public bool Solve(WorkingFormula formula);
        public void ApplySettings(AlgorithmSettings settings);
    }
}
