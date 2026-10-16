namespace PaymentRequests.Api.Contracts;

public sealed record PaymentRequestResponse(
    Guid Id,
    string ExternalReference,
    decimal Amount,
    string Channel,
    decimal Fee,
    decimal Total,
    string Status);