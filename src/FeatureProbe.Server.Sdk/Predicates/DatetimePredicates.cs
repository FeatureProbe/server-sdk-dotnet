namespace FeatureProbe.Server.Sdk.Predicates;

internal abstract class DatetimeMatcher : IMatcher
{
    public bool Match(MatchContext ctx)
    {
        var customValue = ctx.User[ctx.Subject];
        var cv = string.IsNullOrWhiteSpace(customValue)
            ? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            : long.Parse(customValue);
        return CheckObjs(ctx.Objects, cv);
    }

    protected abstract bool CheckObjs(List<string> objs, long target);
}

[MatcherFor("datetime", "after")]
internal class DatetimeAfter : DatetimeMatcher
{
    protected override bool CheckObjs(List<string> objs, long target) => objs.Select(long.Parse).Any(o => target >= o);
}

[MatcherFor("datetime", "before")]
internal class DatetimeBefore : DatetimeMatcher
{
    protected override bool CheckObjs(List<string> objs, long target) => objs.Select(long.Parse).Any(o => target < o);
}
