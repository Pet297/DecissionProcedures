using dpll.DataStructures;
using dpll.InputParsing;
using dpll.Formula;
using dpll.SolvingAlgorithms;
using System.Diagnostics;
using dpll.DecisionHeuristics;
using dpll.DifferenceHeuristics;

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
            Func<WorkingFormula, IClauseStateDataStructure> dataStructureGenerator = (formula) => new AdjacencyListFormula(formula);
            ISolvingAlgorithm solvingAlgorithm = new DpllLookAhead();
            Func<WorkingFormula, IDecisionHeuristic> decisionHeuristicGenerator = (formula) => new RandomDecisionHeuristic();
            IDifferenceHeuristic differenceHeuristic = new BackboneSearchHeuristic();
            List<int> assumptions = new();
            int lubyResetBase = 100;
            float cacheRunCoefficient = 0.03f;
            float cacheVariableCoefficient = 0.17f;
            bool debug = false;

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
                    else if (args[i] == "--watched") dataStructureGenerator = (formula) => (new WatchedFormula(formula));
                    else if (args[i] == "--eager") dataStructureGenerator = (formula) => (new EagerAdjacencyListFormula(formula));
                    else if (args[i] == "--dpll") solvingAlgorithm = new Dpll();
                    else if (args[i] == "--dpll-look-ahead") solvingAlgorithm = new DpllLookAhead();
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
                    else if (args[i] == "--random-decisions") decisionHeuristicGenerator = (formula) => new RandomDecisionHeuristic();
                    else if (args[i] == "--static-jw") decisionHeuristicGenerator = (formula) => new StaticJeroslowWangHeuristic(formula);
                    else if (args[i] == "--vsids") decisionHeuristicGenerator = (formula) => new VsidsHeuristic(formula);
                    else if (args[i] == "--assumptions")
                    {
                        int indexTo = -1;
                        string assumptionsString = "";

                        if (!args[i + 1].StartsWith("["))
                        {
                            throw new Exception("Incorect argument format.");
                        }
                        for (int j = i + 1; j < args.Length; j++)
                        {
                            assumptionsString += args[j];
                            if (args[j].EndsWith("]"))
                            {
                                indexTo = j;
                                break;
                            }
                        }
                        if (indexTo == -1)
                        {
                            throw new Exception("Incorect argument format.");
                        }

                        assumptionsString = assumptionsString[1..^1];
                        assumptionsString = assumptionsString.Replace(" ", "");
                        string[] parts = assumptionsString.Split(",");
                        foreach (string part in parts)
                        {
                            assumptions.Add(int.Parse(part));
                        }

                        i = indexTo;
                    }
                    else if (args[i] == "--crh") differenceHeuristic = new ClauseReductionHeuristic();
                    else if (args[i] == "--wbh") differenceHeuristic = new WeightedBinariesHeuristic();
                    else if (args[i] == "--bsh") differenceHeuristic = new BackboneSearchHeuristic();
                    else if (args[i] == "--debug") debug = true;
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
            CnfFormula formula = parser.Parse(input);

            // If assumptions are present, put them at the begining of the algorithm
            if (assumptions.Count > 0) solvingAlgorithm = new AssumptionAlgorithm(solvingAlgorithm, assumptions);

            // Send parameters to solving algorithm
            AlgorithmSettings settings = new(lubyResetBase, cacheRunCoefficient, cacheVariableCoefficient, differenceHeuristic, debug);
            solvingAlgorithm.ApplySettings(settings);

            // Prepare solving data structure
            WorkingFormula? workingFormula = new(formula, dataStructureGenerator, decisionHeuristicGenerator);
            if (solvingAlgorithm.LearnsClauses && !workingFormula.CanWorkWithLearnedClauses)
            {
                Console.WriteLine("Solving data structure doesn't support learning clauses, while solving algorithm requires it.");
                return;
            }
            
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