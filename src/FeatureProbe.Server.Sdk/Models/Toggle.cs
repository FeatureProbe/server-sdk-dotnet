using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using FeatureProbe.Server.Sdk.Exceptions;
using FeatureProbe.Server.Sdk.Results;

namespace FeatureProbe.Server.Sdk.Models;

public class Toggle
{
    [JsonPropertyName("key")] public string Key { get; init; }

    [JsonPropertyName("enabled")] public bool Enabled { get; init; }

    [JsonPropertyName("trackAccessEvents")]
    public bool? TrackAccessEvents { get; init; }

    [JsonPropertyName("lastModified")] public long LastModified { get; init; }

    [JsonPropertyName("version")] public long Version { get; init; }

    [JsonPropertyName("disabledServe")] public Serve DisabledServe { get; init; }

    [JsonPropertyName("defaultServe")] public Serve DefaultServe { get; init; }

    [JsonPropertyName("rules")] public List<Rule>? Rules { get; init; }

    [JsonPropertyName("variations")] public List<object> Variations { get; init; }

    [JsonPropertyName("prerequisites")] public List<Prerequisite>? Prerequisites { get; init; }

    [JsonPropertyName("forClient")] public bool ForClient { get; init; }

    public EvaluationResult Eval(FPUser user, ImmutableDictionary<string, Toggle> toggles,
        ImmutableDictionary<string, Segment> segments,
        object? defaultValue, int deep)
    {
        string? reason;
        try
        {
            return DoEval(user, toggles, segments, defaultValue, deep);
        }
        catch (Exception e) when (e is PrerequisiteException or PrerequisitesDeepOverflowException)
        {
            reason = e.Message;
        }

        return HitValue(
            DefaultServe.EvalIndex(user, Key),
            defaultValue,
            null,
            reason
        );
    }

    private EvaluationResult DoEval(FPUser user, ImmutableDictionary<string, Toggle> toggles,
        ImmutableDictionary<string, Segment> segments, object? defaultValue, int deep)
    {
        if (!Enabled)
        {
            return HitValue(
                DisabledServe.EvalIndex(user, Key),
                defaultValue,
                null,
                "Toggle disabled"
            );
        }

        if (deep <= 0)
        {
            throw new PrerequisitesDeepOverflowException("Prerequisite deep overflow");
        }

        if (!MeetPrerequisite(user, toggles, segments, deep))
        {
            return HitValue(
                DefaultServe.EvalIndex(user, Key),
                defaultValue,
                null,
                "Default rule hit"
            );
        }

        string? warning = null;
        if (Rules is not null && Rules.Count > 0)
        {
            for (var i = 0; i < Rules.Count; i++)
            {
                var rule = Rules[i];
                var hitResult = rule.Hit(user, segments, Key);
                if (hitResult.Hit)
                {
                    return HitValue(hitResult, defaultValue, i);
                }

                warning = hitResult.Reason;
            }
        }

        return HitValue(
            DefaultServe.EvalIndex(user, Key),
            defaultValue,
            null,
            $"Default rule hit. {warning ?? string.Empty}"
        );
    }

    private bool MeetPrerequisite(FPUser user, ImmutableDictionary<string, Toggle> toggles,
        ImmutableDictionary<string, Segment> segments, int deep)
    {
        if (Prerequisites is null || Prerequisites.Count == 0)
        {
            return true;
        }

        foreach (var prerequisite in Prerequisites)
        {
            if (!toggles.TryGetValue(prerequisite.Key, out var toggle))
            {
                throw new PrerequisiteException($"Prerequisite not exist: {Key}");
            }

            var eval = toggle.DoEval(user, toggles, segments, null, deep - 1);
            if (eval.Value is null)
            {
                return false;
            }

            if (!JsonSerializer.Serialize(eval.Value).Equals(JsonSerializer.Serialize(prerequisite.Value)))
            {
                return false;
            }
        }

        return true;
    }

    private EvaluationResult HitValue(HitResult hitResult, object? defaultValue, int? ruleIndex,
        string? reasonOverride = null)
    {
        var value = defaultValue;
        var reason = hitResult.Reason;

        if (hitResult.Index is not null)
        {
            var variation = Variations[hitResult.Index.Value];
            value = defaultValue is double && variation is int ? Convert.ToDouble(variation) : variation;
            reason = ruleIndex is not null ? $"Rule {ruleIndex} hit" : hitResult.Reason;
        }

        return new EvaluationResult(
            value,
            RuleIndex: ruleIndex,
            VariationIndex: hitResult.Index,
            Version: Version,
            Reason: reasonOverride ?? reason
        );
    }
}
