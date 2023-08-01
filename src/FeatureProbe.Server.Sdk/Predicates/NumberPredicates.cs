namespace FeatureProbe.Server.Sdk.Predicates;

internal abstract class NumberMatcher : IMatcher
{
    protected const double Tolerance = 1e-6;

    public bool Match(MatchContext ctx)
    {
        var customValue = ctx.User[ctx.Subject];
        return !string.IsNullOrWhiteSpace(customValue) && CheckObjs(ctx.Objects, double.Parse(customValue));
    }

    protected abstract bool CheckObjs(List<string> objs, double target);
}

[MatcherFor("number", "=")]
internal class NumberEqualTo : NumberMatcher
{
    protected override bool CheckObjs(List<string> objs, double target) =>
        objs.Select(double.Parse).Any(o => Math.Abs(target - o) < Tolerance);
}

[MatcherFor("number", "!=")]
internal class NumberNotEqualTo : NumberMatcher
{
    protected override bool CheckObjs(List<string> objs, double target) =>
        objs.Select(double.Parse).All(o => Math.Abs(target - o) >= Tolerance);
}

[MatcherFor("number", ">")]
internal class NumberGreaterThan : NumberMatcher
{
    protected override bool CheckObjs(List<string> objs, double target) =>
        objs.Select(double.Parse).Any(o => target > o);
}

[MatcherFor("number", ">=")]
internal class NumberGreaterOrEqual : NumberMatcher
{
    protected override bool CheckObjs(List<string> objs, double target) =>
        objs.Select(double.Parse).Any(o => target >= o);
}

[MatcherFor("number", "<")]
internal class NumberLessThan : NumberMatcher
{
    protected override bool CheckObjs(List<string> objs, double target) =>
        objs.Select(double.Parse).Any(o => target < o);
}

[MatcherFor("number", "<=")]
internal class NumberLessOrEqual : NumberMatcher
{
    protected override bool CheckObjs(List<string> objs, double target) =>
        objs.Select(double.Parse).Any(o => target <= o);
}
