using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpll.InputParsing
{
    public class SmtlibToken
    {
        public readonly SmtlibTokenType Type;
        public readonly string? Data;

        public SmtlibToken(SmtlibTokenType type, string? data)
        {
            Type = type;
            Data = data;
        }
        public SmtlibToken(SmtlibTokenType type) : this(type, null)
        {

        }
    }
}
