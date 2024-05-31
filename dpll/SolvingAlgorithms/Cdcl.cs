using dpll.DataStructures;
using dpll.SolvingState;
using Microsoft.VisualBasic;
using System.Reflection.Metadata.Ecma335;

namespace dpll.SolvingAlgorithms
{
    public class Cdcl : ISolvingAlgorithm
    {
        private int lubyResetBase;
        private float cacheRunCoefficient;
        private float cacheVariableCoefficient;

        public bool Solve(WorkingFormula formula)
        {
            long runIndex = 1;
            while (true)
            {
                long maxConflicts = lubyResetBase * Luby(runIndex);
                long conflictCount = 0;
                int maxClauses = (int)((runIndex * cacheRunCoefficient + 1) * formula.VariableCount * cacheVariableCoefficient);

                while (true)
                {
                    if (conflictCount >= maxConflicts)
                    {
                        formula.ClearLearnedClauses(maxClauses / 2);
                        break;
                    }

                    formula.UnitPropagation();

                    if (formula.IsSatisfied) return true;

                    else if (formula.IsConflict)
                    {
                        conflictCount++;
                        var (assertiveClause, decisionLevel) = formula.FindAssertiveClauseAndDecisionLevel();
                        if (decisionLevel == -1) return false;
                        formula.BackJump(decisionLevel);
                        formula.AddClause(assertiveClause);

                        if (formula.LearnedClausesCount > maxClauses)
                        {
                            formula.ClearLearnedClauses(maxClauses / 2);
                        }
                    }
                    else
                    {
                        int nextDecision = formula.PickNextDecision();
                        formula.Decide(nextDecision);
                    }
                }

                runIndex++;
            }
        }
        public void ApplySettings(AlgorithmSettings settings)
        {
            lubyResetBase = settings.LubyResetBase;
            cacheRunCoefficient = settings.CacheRunCoefficient;
            cacheVariableCoefficient = settings.CacheVariableCoefficient;
        }

        private long Luby(long runIndex)
        {
            if (runIndex == 0) return 1;

            long r0 = runIndex;
            long log0 = -1;
            while (r0 > 0)
            {
                r0 >>= 1;
                log0++;
            }

            long r1 = runIndex + 1;
            long log1 = -1;
            while (r1 > 0)
            {
                r1 >>= 1;
                log1++;
            }

            if (log1 > log0) return 1L << (int)log0;
            else return Luby(runIndex - (1L << (int)log0) + 1);
        }
    }
}
