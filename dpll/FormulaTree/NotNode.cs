using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.FormulaTree
{
    public class NotNode : IFormulaNode
    {
        private readonly IFormulaNode child;

        public NotNode(IFormulaNode child)
        {
            this.child = child;
        }

        T IFormulaNode.Accept<T>(IFormulaVisitor<T> visitor)
        {
            T result = child.Accept<T>(visitor);

            return visitor.VisitNot(result);
        }
    }
}
