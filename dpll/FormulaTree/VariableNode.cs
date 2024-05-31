using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.FormulaTree
{
    public class VariableNode : IFormulaNode
    {
        private readonly string name;

        public VariableNode(string name)
        {
            this.name = name;
        }

        T IFormulaNode.Accept<T>(IFormulaVisitor<T> visitor)
        {
            return visitor.VisitVariable(name);
        }
    }
}
