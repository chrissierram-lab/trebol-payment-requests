namespace PaymentRequests.Api.Contracts;

public sealed record CalculateFeeRequest(decimal Amount, string Channel);