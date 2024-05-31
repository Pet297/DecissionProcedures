using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.DataStructures
{
    public class WorkingClause
    {
        public readonly int[] Literals;

        public WorkingClause(int[] literals)
        {
            this.Literals = literals;
        }
    }
}
