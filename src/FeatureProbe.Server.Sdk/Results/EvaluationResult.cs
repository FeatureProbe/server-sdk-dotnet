namespace FeatureProbe.Server.Sdk.Results;

public record EvaluationResult
(
    object? Value,
    long Version,
    int? RuleIndex,
    int? VariationIndex,
    string? Reason
);
