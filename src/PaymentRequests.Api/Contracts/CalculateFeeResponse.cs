namespace PaymentRequests.Api.Contracts;

public sealed record CalculateFeeResponse(decimal Fee, decimal Total);