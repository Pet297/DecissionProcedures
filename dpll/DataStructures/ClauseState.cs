using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.DataStructures
{
    public enum ClauseState
    {
        Unresolved,
        Unit,
        Conflict,
        Satisfied,
    }
}
