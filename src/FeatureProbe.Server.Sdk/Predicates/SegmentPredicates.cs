namespace FeatureProbe.Server.Sdk.Predicates;

[MatcherFor("segment", "is in")]
internal class SegmentIsIn : IMatcher
{
    public bool Match(MatchContext ctx)
    {
        return ctx.Objects.Any(s => ctx.Segments.TryGetValue(s, out var seg) && seg.Contains(ctx.User, ctx.Segments));
    }
}

[MatcherFor("segment", "is not in")]
internal class SegmentIsNotIn : IMatcher
{
    public bool Match(MatchContext ctx)
    {
        return !ctx.Objects.All(s => ctx.Segments.TryGetValue(s, out var seg) && seg.Contains(ctx.User, ctx.Segments));
    }
}
