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

using System.Text.RegularExpressions;

namespace FeatureProbe.Server.Sdk.Predicates;

internal abstract class StringMatcher : IMatcher
{
    public bool Match(MatchContext ctx)
    {
        return ctx.User.Attributes.TryGetValue(ctx.Subject, out var target)
               && CheckObjs(ctx.Objects, target);
    }

    protected abstract bool CheckObjs(List<string> objs, string target);
}

[MatcherFor("string", "is one of")]
internal class StringIsOneOf : StringMatcher
{
    protected override bool CheckObjs(List<string> objs, string target)
    {
        return objs.Contains(target);
    }
}

[MatcherFor("string", "ends with")]
internal class StringEndsWith : StringMatcher
{
    protected override bool CheckObjs(List<string> objs, string target)
    {
        return objs.Any(target.EndsWith);
    }
}

[MatcherFor("string", "starts with")]
internal class StringStartsWith : StringMatcher
{
    protected override bool CheckObjs(List<string> objs, string target)
    {
        return objs.Any(target.StartsWith);
    }
}

[MatcherFor("string", "contains")]
internal class StringContains : StringMatcher
{
    protected override bool CheckObjs(List<string> objs, string target)
    {
        return objs.Any(target.Contains);
    }
}

[MatcherFor("string", "matches regex")]
internal class StringMatchesRegex : StringMatcher
{
    protected override bool CheckObjs(List<string> objs, string target)
    {
        return objs.Any(s => Regex.IsMatch(target, s));
    }
}

[MatcherFor("string", "is not any of")]
internal class StringIsNotAnyOf : StringMatcher
{
    protected override bool CheckObjs(List<string> objs, string target)
    {
        return !objs.Contains(target);
    }
}

[MatcherFor("string", "does not end with")]
internal class StringDoesNotEndWith : StringMatcher
{
    protected override bool CheckObjs(List<string> objs, string target)
    {
        return objs.All(s => !target.EndsWith(s));
    }
}

[MatcherFor("string", "does not start with")]
internal class StringDoesNotStartWith : StringMatcher
{
    protected override bool CheckObjs(List<string> objs, string target)
    {
        return objs.All(s => !target.StartsWith(s));
    }
}

[MatcherFor("string", "does not contain")]
internal class StringDoesNotContain : StringMatcher
{
    protected override bool CheckObjs(List<string> objs, string target)
    {
        return objs.All(s => !target.Contains(s));
    }
}

[MatcherFor("string", "does not match regex")]
internal class StringDoesNotMatchRegex : StringMatcher
{
    protected override bool CheckObjs(List<string> objs, string target)
    {
        return objs.All(s => !Regex.IsMatch(target, s));
    }
}
