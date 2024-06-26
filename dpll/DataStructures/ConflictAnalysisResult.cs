using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.DataStructures
{
    public class ConflictAnalysisResult
    {
        public readonly int[] Clause;
        public readonly int AssertionLevel;
        public readonly int TopLevelLiteralIndex;
        public readonly int AssertionLevelLiteralIndex;

        public ConflictAnalysisResult(int[] clause, int assertionLevel, int topLevelLiteralIndex, int assertionLevelLiteralIndex)
        {
            Clause = clause;
            AssertionLevel = assertionLevel;
            TopLevelLiteralIndex = topLevelLiteralIndex;
            AssertionLevelLiteralIndex = assertionLevelLiteralIndex;
        }
    }
}
