namespace PaymentRequests.Api.Models;

public sealed record PaymentRequest(
    Guid Id,
    string ExternalReference,
    decimal Amount,
    string Channel,
    decimal Fee,
    decimal Total,
    string Status);