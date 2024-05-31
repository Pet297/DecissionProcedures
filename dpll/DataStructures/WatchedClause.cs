using dpll.SolvingState;

namespace dpll.DataStructures
{
    public class WatchedClause
    {
        public WorkingFormula Formula;
        private readonly int[] Literals;

        public int Head0 = -1;
        public int Head1 = -1;
        public readonly LinkedListNode<WorkingClause>? Head0node;
        public readonly LinkedListNode<WorkingClause>? Head1node;

        public WatchedClause(WorkingFormula formula, WorkingClause clauseRefference, int[] literals)
        {
            this.Formula = formula;
            Literals = literals;

            if (literals.Length >= 1)
            {
                Head0 = 0;
                Head0node = new LinkedListNode<WorkingClause>(clauseRefference);
            }
            else
            {
                Head0 = -1;
                Head0node = null;
            }

            if (literals.Length >= 2)
            {
                Head1 = 1;
                Head1node = new LinkedListNode<WorkingClause>(clauseRefference);
            }
            else
            {
                Head1 = -1;
                Head1node = null;
            }
        }

        // TODO: Rewrite definition of clause states
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
        public bool IsSatisfied =>
            IsHead0Satisfied ||
            IsHead1Satisfied;
        public bool IsUnit =>
            IsHead1Unsatisfied && IsHead0Undefined ||
            IsHead0Unsatisfied && IsHead1Undefined ||
            Head0 == -1 && IsHead1Undefined ||
            Head1 == -1 && IsHead0Undefined;
        public bool IsConflict => 
            IsHead1Unsatisfied && IsHead0Unsatisfied ||
            Head0 == -1 && IsHead1Unsatisfied ||
            Head1 == -1 && IsHead0Unsatisfied ||
            Head0 == -1 && Head1 == -1;
        public bool IsUnresolved =>
            IsHead0Undefined && IsHead1Undefined;
        public int UnitLiteral => IsHead1Undefined ? Literals[Head1] : Literals[Head0];

        public bool IsHead0Satisfied => Head0 != -1 && Formula.IsLiteralSatisfied(Literals[Head0]);
        public bool IsHead1Satisfied => Head1 != -1 && Formula.IsLiteralSatisfied(Literals[Head1]);
        public bool IsHead0Unsatisfied => Head0 != -1 && Formula.IsLiteralUnsatisfied(Literals[Head0]);
        public bool IsHead1Unsatisfied => Head1 != -1 && Formula.IsLiteralUnsatisfied(Literals[Head1]);
        public bool IsHead0Undefined => Head0 != -1 && Formula.IsLiteralUndefined(Literals[Head0]);
        public bool IsHead1Undefined => Head1 != -1 && Formula.IsLiteralUndefined(Literals[Head1]);

        public bool IsHead0PositiveLiteral => Head0 != -1 && Literals[Head0] > 0;
        public bool IsHead1PositiveLiteral => Head1 != -1 && Literals[Head1] > 0;
        public bool IsHead0NegativeLiteral => Head0 != -1 && Literals[Head0] < 0;
        public bool IsHead1NegativeLiteral => Head1 != -1 && Literals[Head1] < 0;

        public List<int> GetUndefinedLiteralPositions()
        {
            List<int> undefinedLiteralPositions = new List<int>();
            for (int i = 0; i < Literals.Length; i++)
            {
                int literal = Literals[i];
                if (Formula.IsLiteralUndefined(literal))
                {
                    undefinedLiteralPositions.Add(i);
                }
            }
            return undefinedLiteralPositions;
        }

        public List<int> GetDecidedLiteralPositionsByMostRecent()
        {
            List<Tuple<long,int>> decidedLiteralOredersPositions = new();
            for (int i = 0; i < Literals.Length; i++)
            {
                int literal = Literals[i];
                if (!Formula.IsLiteralUndefined(literal))
                {
                    decidedLiteralOredersPositions.Add(new Tuple<long, int>(Formula.GetVariableDecisionOrder(Math.Abs(literal)), i));
                }
            }
            // Sorts (decision order, literal's index in clause) pairs by the decision order from most recent.
            decidedLiteralOredersPositions.Sort((a, b) => -a.Item1.CompareTo(b.Item1));
            // Tranforms to list of just the indices in clause
            List<int> indices = new();
            foreach (Tuple<long, int> t in decidedLiteralOredersPositions)
            {
                indices.Add(t.Item2);
            }
            return indices;
        }

        public int Decide(int decision)
        {
            if (IsSatisfied) return 0;

            int literal = Math.Abs(decision);

            if (Math.Abs(Literals[Head1]) == literal)
            {
                if (Literals[Head1] != decision)
                {
                    // Literal unsatisfied, we are moving the watch
                    for (int offset = 1; offset < Literals.Length; offset++)
                    {
                        int newPos = (Head1 + offset) % Literals.Length;

                        if (newPos == Head0) continue;

                        if (Formula.IsLiteralUndefined(Literals[newPos]) || Formula.IsLiteralSatisfied(Literals[newPos]))
                        {
                            Head1 = newPos;
                            return Literals[newPos];
                        }
                    }
                    // Watch couldn't be moved, head 0 remains at the unsatisfied literal and is the witness.
                    return 0;
                }
                // else formula was satisfied and head 0 is the witness.
                return 0;
            }

            if (Math.Abs(Literals[Head0]) == literal)
            {
                if (Literals[Head0] != decision)
                {
                    // Literal unsatisfied, we are moving the watch
                    for (int offset = 1; offset < Literals.Length; offset++)
                    {
                        int newPos = (Head0 + offset) % Literals.Length;

                        if (newPos == Head1) continue;

                        if (Formula.IsLiteralUndefined(Literals[newPos]) || Formula.IsLiteralSatisfied(Literals[newPos]))
                        {
                            Head0 = newPos;
                            return Literals[newPos];
                        }
                    }
                    // Watch couldn't be moved, head 0 remains at the unsatisfied literal and is the witness.
                    return 0;
                }
                // else formula was satisfied and head 0 is the witness.
                return 0;
            }
            
            // shouldn't happen if called correctly.
            return 0;
        }
    }
}
