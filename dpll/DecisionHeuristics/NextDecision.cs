namespace dpll.DecisionHeuristics
{
    public struct NextDecision
    {
        public readonly int Decision;
        public readonly bool IsAssumption;

        public NextDecision(int decision, bool isAssumption)
        {
            Decision = decision;
            IsAssumption = isAssumption;
        }
    }
}
