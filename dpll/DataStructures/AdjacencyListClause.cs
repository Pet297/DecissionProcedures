using System.Diagnostics;

namespace dpll.DataStructures
{
    public class AdjacencyListClause
    {
        private readonly WorkingFormula Formula;
        private readonly int[] Literals;

        // Tracks the clause state.
        private int FalsifiedLiteralCount = 0;
        private int SatisfactionWitness = 0;

        public AdjacencyListClause(WorkingFormula formula, int[] literals)
        {
            Literals = literals;
            Formula = formula;
        }

        public void FalsifyLiteral()
        {
            FalsifiedLiteralCount++;
            Debug.Assert(FalsifiedLiteralCount <= Literals.Length);
        }
        public void SatisfyLiteral(int literal)
        {
            if (SatisfactionWitness == 0)
            {
                SatisfactionWitness = literal;
            }
        }
        public void UndoFalsifyLiteral()
        {
            FalsifiedLiteralCount--;
            Debug.Assert(FalsifiedLiteralCount >= 0);
        }
        public void UndoSatisfyLiteral(int literal)
        {
            if (SatisfactionWitness == literal)
            {
                SatisfactionWitness = 0;
            }
        }

        public int GetUnitLiteral()
        {
            foreach(int literal in Literals)
            {
                if (Formula.IsLiteralUndefined(literal))
                {
                    return literal;
                }
            }
            throw new Exception("Unexpected state - no not-falsified literal in clause marked as unit.");
        }

        public ClauseState GetClauseState()
        {
            return this switch
            {
                { IsUnit: true } => ClauseState.Unit,
                { IsConflict: true } => ClauseState.Conflict,
                { IsUnresolved: true } => ClauseState.Unresolved,
                { IsSatisfied: true } => ClauseState.Satisfied,
                _ => ClauseState.Unresolved,
            };
        }
        public bool IsSatisfied => SatisfactionWitness != 0;
        public bool IsUnit => !IsSatisfied && (FalsifiedLiteralCount == Literals.Length - 1);
        public bool IsConflict => !IsSatisfied && (FalsifiedLiteralCount == Literals.Length);
        public bool IsUnresolved => !IsSatisfied && (FalsifiedLiteralCount <= Literals.Length - 2);
    }
}
