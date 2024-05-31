using dpll.Formula;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.TseitinEncoding
{
    internal class ImplicationEncoder : IDimacsEncoder
    {
        public EncodingResult VisitAnd(EncodingResult left, EncodingResult right)
        {
            int a = left.OuterOperatorVariableIndex;
            int b = right.OuterOperatorVariableIndex;
            int c = GenerateVariableNumber();

            VariableComments[c] = $"Gate variable: {a} AND {b}";

            Formula.CnfFormula formula = Formula.CnfFormula.Merge(left.InnerExpression, right.InnerExpression);
            formula.Clauses.Add(new CnfClause(new List<CnfLiteral>()
            {
                new CnfLiteral(false, c),  new CnfLiteral(true, a)
            }));
            formula.Clauses.Add(new CnfClause(new List<CnfLiteral>()
            {
                new CnfLiteral(false, c),  new CnfLiteral(true, b)
            }));

            return new EncodingResult(formula, c);
        }
        public EncodingResult VisitNot(EncodingResult input)
        {
            int a = input.OuterOperatorVariableIndex;
            int c = GenerateVariableNumber();

            VariableComments[c] = $"Gate variable: NOT {a}";

            Formula.CnfFormula formula = input.InnerExpression;
            formula.Clauses.Add(new CnfClause(new List<CnfLiteral>()
            {
                new CnfLiteral(false, c),  new CnfLiteral(false, a)
            }));

            return new EncodingResult(formula, c);
        }
        public EncodingResult VisitOr(EncodingResult left, EncodingResult right)
        {
            int a = left.OuterOperatorVariableIndex;
            int b = right.OuterOperatorVariableIndex;
            int c = GenerateVariableNumber();

            VariableComments[c] = $"Gate variable: {a} OR {b}";

            Formula.CnfFormula formula = Formula.CnfFormula.Merge(left.InnerExpression, right.InnerExpression);
            formula.Clauses.Add(new CnfClause(new List<CnfLiteral>()
            {
                new CnfLiteral(false, c),  new CnfLiteral(true, a), new CnfLiteral(true, b)
            }));

            return new EncodingResult(formula, c);
        }
        public EncodingResult VisitVariable(string name)
        {
            return new EncodingResult(
                new Formula.CnfFormula(new List<CnfClause>()),
                VariableNameToNumber(name)
            );
        }

        int nextVariableName = 1;
        private int GenerateVariableNumber()
        {
            nextVariableName++;
            return nextVariableName - 1;
        }
        private int VariableNameToNumber(string name)
        {
            if (!VariableIndices.ContainsKey(name))
            {
                int number = GenerateVariableNumber();
                VariableIndices[name] = number;
                VariableComments[number] = "Original variable " + name;
            }
            return VariableIndices[name];
        }

        public int VariableCount => nextVariableName - 1;
        public Dictionary<int, string> VariableComments { get; } = new Dictionary<int, string>();
        public Dictionary<string, int> VariableIndices { get; } = new Dictionary<string, int>();
    }
}
