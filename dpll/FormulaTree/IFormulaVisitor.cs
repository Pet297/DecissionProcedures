using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.FormulaTree
{
    public interface IFormulaVisitor<T>
    {
        T VisitAnd(T left, T right);
        T VisitNot(T input);
        T VisitOr(T left, T right);
        T VisitVariable(string name);
    }
}
