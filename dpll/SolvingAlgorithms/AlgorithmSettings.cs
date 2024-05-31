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

        public AlgorithmSettings(int lubyResetBase, float cacheRunCoefficient, float cacheVariableCoefficient)
        {
            LubyResetBase = lubyResetBase;
            CacheRunCoefficient = cacheRunCoefficient;
            CacheVariableCoefficient = cacheVariableCoefficient;
        }
    }
}
