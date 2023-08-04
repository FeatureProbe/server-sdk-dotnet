/*
 * Copyright 2023 FeatureProbe
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
    protected override bool CheckObjs(List<string> objs, Version target)
    {
        return objs.Select(Version.Parse).Any(o => target == o);
    }
}

[MatcherFor("semver", "!=")]
internal class SemverNotEqualTo : SemverMatcher
{
    protected override bool CheckObjs(List<string> objs, Version target)
    {
        return objs.Select(Version.Parse).All(o => target != o);
    }
}

[MatcherFor("semver", ">")]
internal class SemverGreaterThan : SemverMatcher
{
    protected override bool CheckObjs(List<string> objs, Version target)
    {
        return objs.Select(Version.Parse).Any(o => target > o);
    }
}

[MatcherFor("semver", ">=")]
internal class SemverGreaterOrEqual : SemverMatcher
{
    protected override bool CheckObjs(List<string> objs, Version target)
    {
        return objs.Select(Version.Parse).Any(o => target >= o);
    }
}

[MatcherFor("semver", "<")]
internal class SemverLessThan : SemverMatcher
{
    protected override bool CheckObjs(List<string> objs, Version target)
    {
        return objs.Select(Version.Parse).Any(o => target < o);
    }
}

[MatcherFor("semver", "<=")]
internal class SemverLessOrEqual : SemverMatcher
{
    protected override bool CheckObjs(List<string> objs, Version target)
    {
        return objs.Select(Version.Parse).Any(o => target <= o);
    }
}
