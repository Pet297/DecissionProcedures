using dpll.Formula;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.TseitinEncoding
{
    public class EncodingResult
    {
        public readonly Formula.CnfFormula InnerExpression;
        public readonly int OuterOperatorVariableIndex;

        public EncodingResult(Formula.CnfFormula innerExpression, int outerOperatorVariableIndex)
        {
            InnerExpression = innerExpression;
            OuterOperatorVariableIndex = outerOperatorVariableIndex;
        }
    }
}
