using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.FormulaTree
{
    public class AndNode : IFormulaNode
    {
        private readonly IFormulaNode leftChild;
        private readonly IFormulaNode rightChild;

        public AndNode(IFormulaNode leftChild, IFormulaNode rightChild)
        {
            this.leftChild = leftChild;
            this.rightChild = rightChild;
        }

        T IFormulaNode.Accept<T>(IFormulaVisitor<T> visitor)
        {
            T resultLeft = leftChild.Accept<T>(visitor);
            T resultRight= rightChild.Accept<T>(visitor);

            return visitor.VisitAnd(resultLeft, resultRight);
        }
    }
}
