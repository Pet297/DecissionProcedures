using dpll.Formula;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace dpll.InputParsing
{
    internal class DimacsToCnfParser : ICnfParser
    {
        public Formula.CnfFormula Parse(string input)
        {
            List<CnfClause> clauses = new List<CnfClause>();

            string[] lines = Regex.Split(input, "\r\n|\r|\n");
            foreach (string line in lines)
            {
                if (line.StartsWith("%"))
                {
                    break;
                }
                if (!line.StartsWith("p") && !line.StartsWith("c") && line != "")
                {
                    List<CnfLiteral> literals = new List<CnfLiteral>();
                    string[] parts = line.Split(' ');

                    foreach (string part in parts)
                    {
                        if (part == "0") break;
                        else if (part != "")
                        {
                            int index = int.Parse(part);
                            if (index < 0) literals.Add(new CnfLiteral(false, -index));
                            else literals.Add(new CnfLiteral(true, index));
                        }
                    }
                    clauses.Add(new CnfClause(literals));
                }
            }

            return new Formula.CnfFormula(clauses);
        }
    }
}
