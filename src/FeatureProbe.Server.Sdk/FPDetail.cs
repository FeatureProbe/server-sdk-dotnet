namespace FeatureProbe.Server.Sdk;

public record FPDetail<T>(
    T? Value,
    int? RuleIndex,
    long? Version,
    string? Reason
);
