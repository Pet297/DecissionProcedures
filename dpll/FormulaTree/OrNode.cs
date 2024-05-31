using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.FormulaTree
{
    public class OrNode : IFormulaNode
    {
        private readonly IFormulaNode leftChild;
        private readonly IFormulaNode rightChild;

        public OrNode(IFormulaNode leftChild, IFormulaNode rightChild)
        {
            this.leftChild = leftChild;
            this.rightChild = rightChild;
        }

        T IFormulaNode.Accept<T>(IFormulaVisitor<T> visitor)
        {
            T resultLeft = leftChild.Accept<T>(visitor);
            T resultRight = rightChild.Accept<T>(visitor);

            return visitor.VisitOr(resultLeft, resultRight);
        }
    }
}
