using dpll.Formula;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.InputParsing
{
    internal interface ICnfParser
    {
        Formula.CnfFormula Parse(string input);
    }
}
