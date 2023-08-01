namespace FeatureProbe.Server.Sdk.Predicates;

internal abstract class SemverMatcher : IMatcher
{
    public bool Match(MatchContext ctx)
    {
        var customValue = ctx.User[ctx.Subject];
        return !string.IsNullOrWhiteSpace(customValue) && CheckObjs(ctx.Objects, new Version(customValue));
    }

    protected abstract bool CheckObjs(List<string> objs, Version target);
}

[MatcherFor("semver", "=")]
internal class SemverEqualTo : SemverMatcher
{
    protected override bool CheckObjs(List<string> objs, Version target) =>
        objs.Select(Version.Parse).Any(o => target == o);
}

[MatcherFor("semver", "!=")]
internal class SemverNotEqualTo : SemverMatcher
{
    protected override bool CheckObjs(List<string> objs, Version target) =>
        objs.Select(Version.Parse).All(o => target != o);
}

[MatcherFor("semver", ">")]
internal class SemverGreaterThan : SemverMatcher
{
    protected override bool CheckObjs(List<string> objs, Version target) =>
        objs.Select(Version.Parse).Any(o => target > o);
}

[MatcherFor("semver", ">=")]
internal class SemverGreaterOrEqual : SemverMatcher
{
    protected override bool CheckObjs(List<string> objs, Version target) =>
        objs.Select(Version.Parse).Any(o => target >= o);
}

[MatcherFor("semver", "<")]
internal class SemverLessThan : SemverMatcher
{
    protected override bool CheckObjs(List<string> objs, Version target) =>
        objs.Select(Version.Parse).Any(o => target < o);
}

[MatcherFor("semver", "<=")]
internal class SemverLessOrEqual : SemverMatcher
{
    protected override bool CheckObjs(List<string> objs, Version target) =>
        objs.Select(Version.Parse).Any(o => target <= o);
}
