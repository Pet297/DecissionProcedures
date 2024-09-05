using dpll.DataStructures;
using dpll.SolvingState;

namespace dpll.SolvingAlgorithms
{
    public interface ISolvingAlgorithm
    {
        bool Solve(WorkingFormula formula);
        void ApplySettings(AlgorithmSettings settings);
        bool LearnsClauses { get; }
    }
}
