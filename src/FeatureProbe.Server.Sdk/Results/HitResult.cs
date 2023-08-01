namespace FeatureProbe.Server.Sdk.Results;

public record HitResult
(
    bool Hit,
    int? Index = null,
    string? Reason = null
);
