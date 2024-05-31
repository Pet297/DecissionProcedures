using System.Diagnostics.CodeAnalysis;

namespace dpll.SolvingState
{
    public readonly struct Decision
    {
        public readonly int Value;
        public readonly int DecisionLevel;

        public Decision(int value, int decisionLevel)
        {
            Value = value;
            DecisionLevel = decisionLevel;
        }

        public readonly Decision Negate()
        {
            return new Decision(-Value, DecisionLevel);
        }

        public static bool operator ==(Decision left, Decision right)
        {
            return left.Value == right.Value;
        }
        public static bool operator !=(Decision left, Decision right)
        {
            return left.Value != right.Value;
        }
        public override readonly bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is Decision decision && this == decision;
        }
        public override readonly int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public override string ToString()
        {
            return $"{Value} @ {DecisionLevel}";
        }
    }
}
