using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.Formula
{
    public class CnfFormula
    {
        public readonly List<CnfClause> Clauses;
        public Dictionary<int, string> VariableNames { get; private set; } = new Dictionary<int, string>();
        public Dictionary<int, string> VariableComments { get; private set; } = new Dictionary<int, string>();

        public CnfFormula(List<CnfClause> clauses)
        {
            this.Clauses = clauses;
        }
        public static CnfFormula Merge(CnfFormula c1, CnfFormula c2)
        {
            return new CnfFormula(c1.Clauses.Concat(c2.Clauses).ToList());
        }

        public int VariableCount
        {
            get
            {
                return Clauses.SelectMany(c => c.Literals).Select(l => l.Index).Max();
            }
        }
    }
}
