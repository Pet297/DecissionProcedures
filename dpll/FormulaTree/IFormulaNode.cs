using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.FormulaTree
{
    public interface IFormulaNode
    {
        public T Accept<T>(IFormulaVisitor<T> visitor);
    }
}
