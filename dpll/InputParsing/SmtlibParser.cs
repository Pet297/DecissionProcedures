using dpll.FormulaTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.InputParsing
{
    public static class SmtlibParser
    {
        // STATES: 0
        //         1 (
        //         2 ( and
        //         3 ( and <formula>
        //         4 ( and <formula> <formula>
        //         5 ( or
        //         6 ( or <formula>
        //         7 ( or <formula> <formula>
        //         8 ( not
        //         9 ( not <variable>

        public static IFormulaNode Parse(List<SmtlibToken> tokens)
        {
            int state = 0;
            IFormulaNode? node1 = new VariableNode("ShouldntHappen");
            IFormulaNode? node2 = new VariableNode("ShouldntHappen");

            while (tokens.Count > 0)
            {
                SmtlibToken token = tokens[0];

                switch (state)
                {
                    case 0:
                        tokens.RemoveAt(0);
                        if (token.Type == SmtlibTokenType.LEFT_BRACKET) state = 1;
                        else if (token.Type == SmtlibTokenType.VARIABLE) return new VariableNode(token.Data ?? "ShouldntHappen");
                        else throw new Exception($"Unexpected token {token.Type} while parsing with rule 0.");
                        break;
                    case 1:
                        tokens.RemoveAt(0);
                        if (token.Type == SmtlibTokenType.AND) state = 2;
                        else if (token.Type == SmtlibTokenType.OR) state = 5;
                        else if (token.Type == SmtlibTokenType.NOT) state = 8;
                        else throw new Exception($"Unexpected token {token.Type} while parsing with rule 1.");
                        break;
                    case 2:
                        node1 = Parse(tokens);
                        state = 3;
                        break;
                    case 3:
                        node2 = Parse(tokens);
                        state = 4;
                        break;
                    case 4:
                        tokens.RemoveAt(0);
                        if (token.Type == SmtlibTokenType.RIGHT_BRACKET) return new AndNode(node1, node2);
                        else throw new Exception($"Unexpected token {token.Type} while parsing with rule 4.");
                    case 5:
                        node1 = Parse(tokens);
                        state = 6;
                        break;
                    case 6:
                        node2 = Parse(tokens);
                        state = 7;
                        break;
                    case 7:
                        tokens.RemoveAt(0);
                        if (token.Type == SmtlibTokenType.RIGHT_BRACKET) return new OrNode(node1, node2);
                        else throw new Exception($"Unexpected token {token.Type} while parsing with rule 7.");
                    case 8:
                        tokens.RemoveAt(0);
                        if (token.Type == SmtlibTokenType.VARIABLE)
                        {
                            node1 = new VariableNode(token.Data ?? "ShouldntHappen");
                            state = 9;
                        }
                        else throw new Exception($"Unexpected token {token.Type} while parsing with rule 8.");
                        break;
                    case 9:
                        tokens.RemoveAt(0);
                        if (token.Type == SmtlibTokenType.RIGHT_BRACKET) return new NotNode(node1);
                        else throw new Exception($"Unexpected token {token.Type} while parsing with rule 9.");
                    default:
                        throw new Exception($"This shouldn't happen.");
                }
            }
            throw new Exception($"Unexpected end of file while parsing formula.");
        }
    }
}
