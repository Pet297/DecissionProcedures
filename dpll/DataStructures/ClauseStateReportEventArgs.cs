using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.DataStructures
{
    public class ClauseStateReportEventArgs : EventArgs
    {
        public readonly WorkingClause Clause;
        public readonly ClauseState CurrentState;

        public ClauseStateReportEventArgs(WorkingClause clause, ClauseState currentState)
        {
            Clause = clause;
            CurrentState = currentState;
        }
    }
}
