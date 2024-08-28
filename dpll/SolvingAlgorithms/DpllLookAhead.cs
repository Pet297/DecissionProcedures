using dpll.DataStructures;
using dpll.DecisionHeuristics;
using dpll.DifferenceHeuristics;
using dpll.SolvingState;
using Microsoft.VisualBasic;
using System.Linq.Expressions;

namespace dpll.SolvingAlgorithms
{
    public class DpllLookAhead : ISolvingAlgorithm
    {
        private IDifferenceHeuristic? Heuristic;

        public bool Solve(WorkingFormula formula)
        {
            formula.UnitPropagation();
            if (formula.IsSatisfied) return true;
            if (formula.IsConflict) return false;

            // May change the formula by autarky reasoning.
            int? decisionVariable = LookAhead(formula);

            if (formula.IsConflict) return false;
            if (decisionVariable == null) return Solve(formula);

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
        }

        private int? LookAhead(WorkingFormula formula)
        {
            throw new NotImplementedException();

            Dictionary<WorkingClause, int> clauseLengthsBefore = formula.GetClauseLengths();
            double bestHeuristicValue = double.NegativeInfinity;
            int bestVariable = 0;

            List<int> literals = new List<int>();
            foreach (int i in literals)
            {
                VariableAssignment[] assignmentBefore = formula.GetCurrentAssignment();

                formula.Decide(i);
                Dictionary<WorkingClause, int> clauseLengthsAfterPositive = formula.GetClauseLengths();
                VariableAssignment[] positiveAssignment = formula.GetCurrentAssignment();
                formula.Backtrack();

                formula.Decide(-i);
                Dictionary<WorkingClause, int> clauseLengthsAfterNegative = formula.GetClauseLengths();
                VariableAssignment[] negativeAssignment = formula.GetCurrentAssignment();
                formula.Backtrack();

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

        private List<int> Preselect()
        {
            throw new NotImplementedException();
        }
    }
}
