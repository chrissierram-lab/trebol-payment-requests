namespace PaymentRequests.Api.Contracts;

public sealed class CreatePaymentRequest
{
    public string? ExternalReference { get; init; }

    public decimal Amount { get; init; }

    public string? Channel { get; init; }
}