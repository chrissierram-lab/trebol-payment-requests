namespace PaymentRequests.Api.Services;

public sealed class FeeCalculatorOptions
{
    public const string SectionName = "FeeCalculator";

    public string BaseUrl { get; init; } = "http://localhost:8081/";

    public int TimeoutSeconds { get; init; } = 3;
}