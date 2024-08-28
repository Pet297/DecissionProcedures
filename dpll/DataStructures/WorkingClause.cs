using dpll.SolvingState;
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
        public int GetCurrentLength(VariableAssignment[] assignment)
        {
            int undefinedLiterals = 0;
            foreach (int literal in Literals)
            {
                int index = Math.Abs(literal);
                if (assignment[index] == VariableAssignment.Undefined) undefinedLiterals++;
            }
            return undefinedLiterals;
        }
        public List<int> GetUndefinedLiterals(VariableAssignment[] assignment)
        {
            List<int> undefinedLiterals = new();
            foreach (int literal in Literals)
            {
                int index = Math.Abs(literal);
                if (assignment[index] == VariableAssignment.Undefined) undefinedLiterals.Add(literal);
            }
            return undefinedLiterals;
        }
    }
}
