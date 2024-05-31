using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.InputParsing
{
    public static class SmtlibLexer
    {
        public static List<SmtlibToken> Tokenize(string s)
        {
            List<SmtlibToken> tokens = new List<SmtlibToken>();
            string wordBuffer = string.Empty;

            void FinishWord()
            {
                if (wordBuffer == "and") tokens.Add(new SmtlibToken(SmtlibTokenType.AND));
                else if (wordBuffer == "or") tokens.Add(new SmtlibToken(SmtlibTokenType.OR));
                else if (wordBuffer == "not") tokens.Add(new SmtlibToken(SmtlibTokenType.NOT));
                else if (wordBuffer.Length > 0)
                {
                    tokens.Add(new SmtlibToken(SmtlibTokenType.VARIABLE, wordBuffer));
                }
                wordBuffer = string.Empty;
            }

            foreach (char c in s)
            {
                if (c == '(')
                {
                    FinishWord();
                    tokens.Add(new SmtlibToken(SmtlibTokenType.LEFT_BRACKET));
                }
                else if (c == ')')
                {
                    FinishWord();
                    tokens.Add(new SmtlibToken(SmtlibTokenType.RIGHT_BRACKET));
                }
                else if (IsLatinLetter(c))
                {
                    wordBuffer += c;
                }
                else if (char.IsDigit(c))
                {
                    if (wordBuffer.Length == 0) throw new Exception("Syntax error: Variable name in SAT formula can't start with a number.");
                    wordBuffer += c;
                }
                else if (char.IsWhiteSpace(c))
                {
                    FinishWord();
                }
                else
                {
                    throw new Exception($"Syntax error: Unexpected character '{c}' in SAT formula.");
                }
            }
            FinishWord();

            return tokens;
        }

        private static bool IsLatinLetter(char c)
        {
            return c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';
        }
    }
}
