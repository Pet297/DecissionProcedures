using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.Formula
{
    public class CnfClause 
    {
        public readonly List<CnfLiteral> Literals;

        public CnfClause(List<CnfLiteral> literals)
        {
            this.Literals = literals;
        }

        public static CnfClause Merge(CnfClause c1, CnfClause c2)
        {
            return new CnfClause(c1.Literals.Concat(c2.Literals).ToList());
        }
        public static CnfClause Empty
        {
            get
            {
                List<CnfLiteral> emptyList = new();
                return new CnfClause(emptyList);
            }
        }
    }
}
