using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.Formula
{
    public class CnfLiteral
    {
        public readonly bool Positive;
        public readonly int Index;

        public CnfLiteral(bool positive, int index)
        {
            Positive = positive;
            Index = index;
        }
    }
}
