using dpll.FormulaTree;
using dpll.Formula;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.TseitinEncoding
{
    public interface IDimacsEncoder : IFormulaVisitor<EncodingResult>
    {
        public int VariableCount { get; }
        Dictionary<int, string> VariableComments { get; }
        Dictionary<string, int> VariableIndices { get; }
    }
}
