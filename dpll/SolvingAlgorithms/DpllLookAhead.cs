using dpll.DataStructures;
using dpll.DifferenceHeuristics;
using dpll.SolvingState;

namespace dpll.SolvingAlgorithms
{
    public class DpllLookAhead : ISolvingAlgorithm
    {
        private IDifferenceHeuristic? Heuristic;
        private bool Debug = false;

        public bool LearnsClauses => false;

        public bool Solve(WorkingFormula formula)
        {
            formula.UnitPropagation();
            if (formula.IsSatisfied) return true;
            if (formula.IsConflict) return false;

            // May change the formula by autarky reasoning. If so, returns null.
            int? decisionVariable = LookAhead(formula);

            if (formula.IsConflict) return false;
            if (decisionVariable == null)
            {
                // Handles case with autarky reasoning
                bool sat = Solve(formula);
                if (sat) return true;
                else formula.Backtrack();
                return false;
            }

            int decision = decisionVariable.Value;

            formula.Decide(decision);
            bool satisfiable = Solve(formula);
            if (satisfiable) return true;
            else formula.Backtrack();

            formula.Decide(-decision);
            satisfiable = Solve(formula);
            if (satisfiable) return true;
            else formula.Backtrack();

            return false;
        }
        public void ApplySettings(AlgorithmSettings settings)
        {
            Heuristic = settings.DifferenceHeuristic;
            Debug = settings.Debug;
        }

        private int? LookAhead(WorkingFormula formula)
        {
            Dictionary<WorkingClause, int> clauseLengthsBefore = formula.GetClauseLengths();
            double bestHeuristicValue = double.NegativeInfinity;
            int bestVariable = 0;

            List<int> literals = Preselect(formula);
            foreach (int i in literals)
            {
                VariableAssignment[] assignmentBefore = formula.GetCurrentAssignment();

                // Positive asignment
                formula.Decide(i);
                formula.UnitPropagation();
                Dictionary<WorkingClause, int> clauseLengthsAfterPositive = formula.GetClauseLengths();
                bool autarkyDetected = AutarkyDetected(clauseLengthsBefore, clauseLengthsAfterPositive);
                if (autarkyDetected || formula.IsSatisfied)
                {
                    // Autarky detected, don't backtrack and end look ahead
                    if (Debug && autarkyDetected) Console.WriteLine($"Autarky detected by {i}.");
                    if (Debug && formula.IsSatisfied) Console.WriteLine($"Formula satisfied by {i}.");
                    return null;
                }
                bool conflictOnPositive = formula.IsConflict;
                VariableAssignment[] positiveAssignment = formula.GetCurrentAssignment();
                formula.Backtrack();

                formula.Decide(-i);
                formula.UnitPropagation();
                Dictionary<WorkingClause, int> clauseLengthsAfterNegative = formula.GetClauseLengths();
                autarkyDetected = AutarkyDetected(clauseLengthsBefore, clauseLengthsAfterNegative);
                if (autarkyDetected || formula.IsSatisfied)
                {
                    if (Debug && autarkyDetected) Console.WriteLine($"Autarky detected by {-i}.");
                    if (Debug && formula.IsSatisfied) Console.WriteLine($"Formula satisfied by {-i}.");
                    return null;
                }
                bool conflictOnNegative = formula.IsConflict;
                VariableAssignment[] negativeAssignment = formula.GetCurrentAssignment();
                formula.Backtrack();

                if (conflictOnPositive)
                {
                    if (Debug && conflictOnNegative) Console.WriteLine($"Conflict on {i} and {-i}.");
                    else if (Debug) Console.WriteLine($"Conflict on {i}.");
                    // Also handles (conflictOnPositive && conflictOnNegative), because on recursion in Solve(WF), conflict is checked.
                    formula.Decide(-i);
                    return null;
                }
                if (conflictOnNegative)
                {
                    if (Debug) Console.WriteLine($"Conflict on {-i}.");
                    formula.Decide(i);
                    return null;
                }

                double positiveHeuristicValue = Heuristic!.Heuristic(clauseLengthsBefore, clauseLengthsAfterPositive, assignmentBefore, positiveAssignment);
                double negativeHeuristicValue = Heuristic!.Heuristic(clauseLengthsBefore, clauseLengthsAfterNegative, assignmentBefore, negativeAssignment);
                double mixedHeuristicValue = 1024 * positiveHeuristicValue * negativeHeuristicValue + positiveHeuristicValue + negativeHeuristicValue;

                if (mixedHeuristicValue > bestHeuristicValue)
                {
                    bestHeuristicValue = mixedHeuristicValue;
                    bestVariable = (positiveHeuristicValue > negativeHeuristicValue ? i : -i);
                }
            }

            if (bestVariable == 0) return null;
            else return bestVariable;
        }

        private List<int> Preselect(WorkingFormula formula)
        {
            int[] cra = ClauseReductionApproximation(formula);

            List<int> unassignedVariables = new List<int>();
            for(int i = 1; i <= formula.VariableCount; i++)
            {
                if (formula.IsLiteralUndefined(i))
                {
                    unassignedVariables.Add(i);
                }
            }

            // Sort by CRA, decreasing
            unassignedVariables.Sort((a, b) => cra[b] - cra[a]);

            // Select at least 10 (or all) and at most 10% of variables.
            int subsetSize = Math.Min(Math.Max(unassignedVariables.Count / 10, 10), unassignedVariables.Count);
            return unassignedVariables.Take(subsetSize).ToList();
        }

        private int[] ClauseReductionApproximation(WorkingFormula formula)
        {
            Tuple<int[], int[]> literalOccurences = LiteralOccurencesInTernaryClauses(formula);
            int[] positiveLiteralOccurences = literalOccurences.Item1;
            int[] negativeLiteralOccurences = literalOccurences.Item2;

            int[] sumForPositiveLiterals = new int[formula.VariableCount + 1];
            int[] sumForNegativeLiterals = new int[formula.VariableCount + 1];
            VariableAssignment[] variableAssignment = formula.GetCurrentAssignment();

            foreach (WorkingClause clause in formula.NonUnitClauses())
            {
                int k = clause.GetCurrentLength(variableAssignment);
                if (k == 2)
                {
                    // The clause is literal1 or literal2
                    int literal1 = clause.Literals[0];
                    int literal2 = clause.Literals[1];
                    bool sign1 = literal1 > 0;
                    int value1 = Math.Abs(literal1);
                    bool sign2 = literal1 > 0;
                    int value2 = Math.Abs(literal2);
                    // CRA sum increase for literal1 by negation of literal2
                    (sign1 ? sumForPositiveLiterals : sumForNegativeLiterals)[value1] +=
                        (sign2 ? negativeLiteralOccurences : positiveLiteralOccurences)[value2];
                    // CRA sum increase for literal2 by negation of literal1
                    (sign2 ? sumForPositiveLiterals : sumForNegativeLiterals)[value2] +=
                        (sign1 ? negativeLiteralOccurences : positiveLiteralOccurences)[value1];
                }
            }

            int[] productPerLiteral = new int[formula.VariableCount + 1];

            for (int i = 0; i < productPerLiteral.Length; i++)
            {
                productPerLiteral[i] = sumForNegativeLiterals[i] * sumForNegativeLiterals[i];
            }

            return productPerLiteral;
        }
        private Tuple<int[], int[]> LiteralOccurencesInTernaryClauses(WorkingFormula formula)
        {
            int[] PositiveLiteralValues = new int[formula.VariableCount + 1];
            int[] NegativeLiteralValues = new int[formula.VariableCount + 1];
            VariableAssignment[] variableAssignment = formula.GetCurrentAssignment();

            foreach (WorkingClause clause in formula.NonUnitClauses())
            {
                int k = clause.GetCurrentLength(variableAssignment);
                if (k > 2)
                {
                    foreach (int literal in clause.Literals)
                    {
                        bool sign = literal > 0;
                        int value = Math.Abs(literal);

                        (sign ? PositiveLiteralValues : NegativeLiteralValues)[value]++;
                    }
                }
            }

            return new Tuple<int[], int[]>(PositiveLiteralValues, NegativeLiteralValues);
        }
        

        private bool AutarkyDetected(Dictionary<WorkingClause, int> clauseLengthsBefore, Dictionary<WorkingClause, int> clauseLengthsAfter)
        {
            bool autarkyDetected = true;

            foreach (WorkingClause clause in clauseLengthsAfter.Keys)
            {
                if (clauseLengthsBefore.ContainsKey(clause))
                {
                    if (clauseLengthsBefore[clause] != clauseLengthsAfter[clause])
                    {
                        autarkyDetected = false;
                        break;
                    }
                }
                else
                {
                    throw new Exception("This shouldn't happen");
                }
            }

            return autarkyDetected;
        }
    }
}
