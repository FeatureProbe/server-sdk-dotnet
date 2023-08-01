using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using FeatureProbe.Server.Sdk.Models;

namespace FeatureProbe.Server.Sdk.UT;

[AttributeUsage(AttributeTargets.Method)]
internal class ConditionData : Attribute
{
    public ConditionData(string type, string predicate, string[] objects)
    {
        Type = type;
        Predicate = predicate;
        Objects = objects;
    }

    public string Type { get; }

    public string Predicate { get; }

    public string[] Objects { get; }
}

public class ConditionTest
{
    private readonly FPUser _user = new FPUser().StableRollout("test_user");

    private readonly Dictionary<string, Segment> _segments = new()
    {
        {
            "test_project$test_segment",
            new Segment
            {
                UniqueId = "test_project$test_segment",
                Version = 1,
                Rules = new List<SegmentRule>
                {
                    new()
                    {
                        Conditions = new List<Condition>
                        {
                            new("string", "is one of")
                            {
                                Subject = "testSubject", Objects = new List<string> { "1", "2" }
                            }
                        }
                    }
                }
            }
        }
    };

    private void DoTestCondition(string? value, bool expected)
    {
        MethodBase caller = new StackTrace().GetFrame(1)!.GetMethod()!;
        ConditionData data = caller.GetCustomAttribute<ConditionData>()!;
        var cond = new Condition(data.Type, data.Predicate)
        {
            Subject = "testSubject", Objects = new List<string>(data.Objects)
        };
        _user["testSubject"] = value;
        Assert.Equal(expected, cond.MatchObjects(_user, _segments.ToImmutableDictionary()));
    }


    [Theory]
    [ConditionData("foo", "bar", new[] { "12345", "123" })]
    [InlineData("12345", false)]
    [InlineData("true", false)]
    [InlineData("", false)]
    void TestDummyCondition_NotExists(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("string", "is one of", new[] { "12345", "987654", "665544", "13797347245" })]
    [InlineData("12345", true)]
    [InlineData("9999999", false)]
    [InlineData("  \t \n\r\b", false)]
    void TestStringCondition_IsOneOf(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("string", "ends with", new[] { "123", "888" })]
    [InlineData("123123", true)]
    [InlineData("88", false)]
    [InlineData("8888", true)]
    [InlineData(null, false)]
    void TestStringCondition_EndsWith(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("string", "starts with", new[] { "123" })]
    [InlineData("123321", true)]
    [InlineData("33333", false)]
    void TestStringCondition_StartsWith(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("string", "contains", new[] { "123", "456" })]
    [InlineData("456433", true)]
    [InlineData("999999", false)]
    void TestStringCondition_Contains(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("string", "matches regex", new[] { "0?(13|14|15|18)[0-9]{9}" })]
    [InlineData("13797347245", true)]
    [InlineData("122122", false)]
    void TestStringCondition_MatchesRegex(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("string", "matches regex", new[] { @"\\\" })]
    [InlineData("13797347245", false)]
    void TestStringCondition_TryMatchInvalidRegex(string? value, bool expected) =>
        DoTestCondition(value, expected);

    [Theory]
    [ConditionData("string", "is not any of", new[] { "12345", "987654", "665544" })]
    [InlineData("999999999", true)]
    [InlineData("12345", false)]
    void TestStringCondition_IsNotAnyOf(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("string", "does not end with", new[] { "123", "456" })]
    [InlineData("3333333", true)]
    [InlineData("456456", false)]
    void TestStringCondition_DoesNotEndWith(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("string", "does not start with", new[] { "123", "456" })]
    [InlineData("3333333", true)]
    [InlineData("123456", false)]
    void TestStringCondition_DoesNotStartWith(string? value, bool expected) =>
        DoTestCondition(value, expected);

    [Theory]
    [ConditionData("string", "does not contain", new[] { "12345", "987654", "665544" })]
    [InlineData("999999999", true)]
    [InlineData("12345", false)]
    void TestStringCondition_DoesNotContain(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("string", "does not match regex", new[] { "0?(13|14|15|18)[0-9]{9}" })]
    [InlineData("2122121", true)]
    [InlineData("13797347245", false)]
    void TestStringCondition_DoesNotMatchRegex(string? value, bool expected) =>
        DoTestCondition(value, expected);

    [Theory]
    [ConditionData("segment", "is in", new[] { "test_project$test_segment" })]
    [InlineData("1", true)]
    [InlineData("3", false)]
    void TestSegmentCondition_IsIn(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("segment", "is not in", new[] { "test_project$test_segment" })]
    [InlineData("1", false)]
    [InlineData("3", true)]
    void TestSegmentCondition_IsNotIn(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("datetime", "after", new[] { "1690869876", "1691869876" })]
    [InlineData("1690869876", true)]
    [InlineData("1790869876", true)]
    [InlineData("1690869875", false)]
    [InlineData("invalid datetime", false)]
    void TestDatetimeCondition_After(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("datetime", "before", new[] { "1690869876", "1691869876" })]
    [InlineData("1690869776", true)]
    [InlineData("1790869876", false)]
    [InlineData("1690869875", true)]
    [InlineData("invalid datetime", false)]
    void TestDatetimeCondition_Before(string? value, bool expected) => DoTestCondition(value, expected);


    [Theory]
    [ConditionData("number", "=", new[] { "12", "10.1" })]
    [InlineData("12.00000000 \n ", true)]
    [InlineData(" 10.10   ", true)]
    [InlineData("1.2e1", true)]
    [InlineData("foo.bar", false)]
    void TestNumberCondition_EqualTo(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("number", "!=", new[] { "12", "16" })]
    [InlineData("12.0000000000000001 \n ", false)]
    [InlineData(" 13.10 \t\n  ", true)]
    [InlineData("1.2e1", false)]
    [InlineData("foo.bar", false)]
    void TestNumberCondition_NotEqualTo(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("number", ">", new[] { "12" })]
    [InlineData("  13 \n", true)]
    [InlineData("\t11.998 ", false)]
    [InlineData("\t12.0 ", false)]
    void TestNumberCondition_GreaterThan(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("number", ">=", new[] { "12" })]
    [InlineData("  13 \n", true)]
    [InlineData("\t12.0 ", true)]
    [InlineData("\t11.919999998 ", false)]
    void TestNumberCondition_GreaterOrEqual(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("number", "<", new[] { "17" })]
    [InlineData("  13 \n", true)]
    [InlineData("\t18", false)]
    [InlineData("\t17.00000000000001 ", false)]
    void TestNumberCondition_LessThan(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("number", "<=", new[] { "17" })]
    [InlineData("  13 \n", true)]
    [InlineData("17", true)]
    [InlineData("\t18", false)]
    void TestNumberCondition_LessOrEqual(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("semver", "=", new[] { "1.1.3", "1.1.5" })]
    [InlineData("1.1.3", true)]
    [InlineData("1.1.5", true)]
    [InlineData("1.0.1", false)]
    [InlineData("", false)]
    void TestSemverCondition_EqualTo(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("semver", "!=", new[] { "1.1.0", "1.2.0" })]
    [InlineData("1.3.0", true)]
    [InlineData("1.1.0", false)]
    [InlineData("1.2.0", false)]
    void TestSemverCondition_NotEqualTo(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("semver", ">", new[] { "1.1.0", "1.2.0" })]
    [InlineData("1.1.1", true)]
    [InlineData("1.1.0", false)]
    [InlineData("1.0.0", false)]
    void TestSemverCondition_GreaterThan(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("semver", ">=", new[] { "1.1.0", "1.2.0" })]
    [InlineData("1.1.1", true)]
    [InlineData("1.1.0", true)]
    [InlineData("1.0.0", false)]
    void TestSemverCondition_GreaterOrEqual(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("semver", "<", new[] { "1.1.0", "1.2.0" })]
    [InlineData("1.0.1", true)]
    [InlineData("1.1.7", true)]
    [InlineData("1.2.0", false)]
    void TestSemverCondition_LessThan(string? value, bool expected) => DoTestCondition(value, expected);

    [Theory]
    [ConditionData("semver", "<=", new[] { "1.1.0", "1.2.0" })]
    [InlineData("1.0.1", true)]
    [InlineData("1.2.0", true)]
    [InlineData("1.2.1", false)]
    void TestSemverCondition_LessOrEqual(string? value, bool expected) => DoTestCondition(value, expected);
}
