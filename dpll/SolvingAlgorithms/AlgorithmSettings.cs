using dpll.DifferenceHeuristics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.SolvingAlgorithms
{
    public class AlgorithmSettings
    {
        public readonly int LubyResetBase;
        public readonly float CacheRunCoefficient;
        public readonly float CacheVariableCoefficient;
        public readonly IDifferenceHeuristic DifferenceHeuristic;
        public readonly bool Debug;

        public AlgorithmSettings(int lubyResetBase, float cacheRunCoefficient, float cacheVariableCoefficient, IDifferenceHeuristic differenceHeuristic, bool debug)
        {
            LubyResetBase = lubyResetBase;
            CacheRunCoefficient = cacheRunCoefficient;
            CacheVariableCoefficient = cacheVariableCoefficient;
            DifferenceHeuristic = differenceHeuristic;
            Debug = debug;
        }
    }
}
