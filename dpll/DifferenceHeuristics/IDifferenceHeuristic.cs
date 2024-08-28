using dpll.DataStructures;
using dpll.SolvingState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.DifferenceHeuristics
{
    public interface IDifferenceHeuristic
    {
        public double Heuristic(Dictionary<WorkingClause, int> clauseLengthsBefore, Dictionary<WorkingClause, int> clauseLengthsAfter, VariableAssignment[] assignmentBefore, VariableAssignment[] assignmentAfter);
    }
}
