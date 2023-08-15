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
    protected override bool CheckObjs(List<string> objs, double target)
    {
        return objs.Select(double.Parse).Any(o => Math.Abs(target - o) < Tolerance);
    }
}

[MatcherFor("number", "!=")]
internal class NumberNotEqualTo : NumberMatcher
{
    protected override bool CheckObjs(List<string> objs, double target)
    {
        return objs.Select(double.Parse).All(o => Math.Abs(target - o) >= Tolerance);
    }
}

[MatcherFor("number", ">")]
internal class NumberGreaterThan : NumberMatcher
{
    protected override bool CheckObjs(List<string> objs, double target)
    {
        return objs.Select(double.Parse).Any(o => target > o);
    }
}

[MatcherFor("number", ">=")]
internal class NumberGreaterOrEqual : NumberMatcher
{
    protected override bool CheckObjs(List<string> objs, double target)
    {
        return objs.Select(double.Parse).Any(o => target >= o);
    }
}

[MatcherFor("number", "<")]
internal class NumberLessThan : NumberMatcher
{
    protected override bool CheckObjs(List<string> objs, double target)
    {
        return objs.Select(double.Parse).Any(o => target < o);
    }
}

[MatcherFor("number", "<=")]
internal class NumberLessOrEqual : NumberMatcher
{
    protected override bool CheckObjs(List<string> objs, double target)
    {
        return objs.Select(double.Parse).Any(o => target <= o);
    }
}
