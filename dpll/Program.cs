using dpll.DataStructures;
using dpll.InputParsing;
using dpll.Formula;
using dpll.SolvingAlgorithms;
using dpll.SolvingState;
using System.Diagnostics;

namespace dpll
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Argument default values
            string? inputFile = null;
            string? inputType = null;
            bool useImplications = false;
            Func<WorkingFormula, IClauseStateDataStructure> dataStructureGenerator = (formula) => (new WatchedFormula(formula));
            ISolvingAlgorithm solvingAlgorithm = new Cdcl();
            int lubyResetBase = 100;
            float cacheRunCoefficient = 0.1f;
            float cacheVariableCoefficient = 0.1f;

            // Parse arguments
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--implication") useImplications = true;
                    else if (args[i] == "--equivalence") useImplications = false;
                    else if (args[i] == "--dimacs") inputType = "dimacs";
                    else if (args[i] == "--smt-lib") inputType = "smtlib";
                    else if (args[i] == "--adjacency-list") dataStructureGenerator = (formula) => (new AdjacencyListFormula(formula));
                    else if (args[i] == "--head-tail") throw new NotSupportedException("Head tail structure isn't supported yet.");
                    else if (args[i] == "--watched") dataStructureGenerator = (formula) => (new WatchedFormula(formula));
                    else if (args[i] == "--dpll") solvingAlgorithm = new Dpll();
                    else if (args[i] == "--dpll-plus") throw new NotSupportedException("Dpll+ algorithm isn't supported yet.");
                    else if (args[i] == "--cdcl") solvingAlgorithm = new Cdcl();
                    else if (args[i] == "--luby-reset-base")
                    {
                        lubyResetBase = int.Parse(args[i + 1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                        i++;
                    }
                    else if (args[i] == "--cache-run-coef")
                    {
                        cacheRunCoefficient = float.Parse(args[i + 1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                        i++;
                    }
                    else if (args[i] == "--cache-variable-coef")
                    {
                        cacheVariableCoefficient = float.Parse(args[i + 1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                        i++;
                    }
                    else if (inputFile == null) inputFile = args[i];
                    else throw new Exception("Incorect argument format.");
                }
            }
            catch
            {
                Console.WriteLine("Argument format error.");
                return;
            }

            // Determine input parser
            ICnfParser? parser = null;
            if (inputType == null)
            {
                if (inputFile == null)
                {
                    throw new Exception("Input type couldn't be inferred. Please specify it when using stdin. (--dimacs or --smt-lib)");
                }
                else
                {
                    if (inputFile.EndsWith(".cnf")) inputType = "dimacs";
                    else if (inputFile.EndsWith(".sat")) inputType = "smtlib";
                    else
                    {
                        throw new Exception("Input type couldn't be inferred from extension. Please specify it. (--dimacs or --smt-lib)");
                    }
                }
            }
            if (inputType == "dimacs") parser = new DimacsToCnfParser();
            if (inputType == "smtlib") parser = new SmtlibToCnfParser(useImplications);
            if (parser == null) throw new Exception("This shouldn't happen");

            // Determine input stream and read it
            TextReader reader = (inputFile == null) ? Console.In : new StreamReader(new FileStream(inputFile, FileMode.Open));
            string input = reader.ReadToEnd();
            reader.Close();

            // Parse input into a CNF formula
            Formula.CnfFormula formula = parser.Parse(input);

            // Send parameters to solving algorithm
            AlgorithmSettings settings = new(lubyResetBase, cacheRunCoefficient, cacheVariableCoefficient);
            solvingAlgorithm.ApplySettings(settings);

            // Prepare solving data structure
            WorkingFormula workingFormula = new(formula, dataStructureGenerator);
            
            // Start solving
            Stopwatch sw = new();
            sw.Start();
            solvingAlgorithm.Solve(workingFormula);
            sw.Stop();
            if (workingFormula.IsSatisfied)
            {
                Console.WriteLine("SATISFIABLE");
                workingFormula.PrintAsignment();
            }
            else Console.WriteLine("UNSATISFIABLE");
            workingFormula.PrintStats(sw.Elapsed.TotalSeconds);
        }
    }
}