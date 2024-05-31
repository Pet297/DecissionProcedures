using dpll.FormulaTree;
using dpll.Formula;
using dpll.TseitinEncoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.InputParsing
{
    internal class SmtlibToCnfParser : ICnfParser
    {
        private readonly bool useImplications = false;

        public SmtlibToCnfParser(bool useImplications)
        {
            this.useImplications = useImplications;
        }

        public Formula.CnfFormula Parse(string input)
        {
            // Determine Encoder
            IDimacsEncoder encoder = useImplications ? new ImplicationEncoder() : new EquivalenceEncoder();

            // Parse input
            List<SmtlibToken> tokens = SmtlibLexer.Tokenize(input);
            IFormulaNode nnfFormula = SmtlibParser.Parse(tokens);

            // Encode formula
            EncodingResult encodingResult = nnfFormula.Accept(encoder);

            // Appends the condition that the outermost operator must be true (single clause with single variable):
            Formula.CnfFormula cnfFormula = encodingResult.InnerExpression;
            cnfFormula.Clauses.Add(new CnfClause(new List<CnfLiteral>() { new CnfLiteral(true, encodingResult.OuterOperatorVariableIndex) }));

            foreach(KeyValuePair<string, int> map in encoder.VariableIndices)
            {
                cnfFormula.VariableComments[map.Value] = map.Key;
            }
            foreach (KeyValuePair<int, string> comment in encoder.VariableComments)
            {
                cnfFormula.VariableComments[comment.Key] = comment.Value;
            }

            return cnfFormula;
        }
    }
}
