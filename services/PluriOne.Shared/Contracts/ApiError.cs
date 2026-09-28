namespace PluriOne.Shared.Contracts;

/// <summary>Formato uniforme de error para las APIs internas.</summary>
public sealed record ApiError(string Code, string Message, string? CorrelationId = null)
{
    public static ApiError From(string code, string message) => new(code, message);
}
