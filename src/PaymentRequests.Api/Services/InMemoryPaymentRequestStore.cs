using System.Collections.Concurrent;
using PaymentRequests.Api.Models;

namespace PaymentRequests.Api.Services;

public sealed class InMemoryPaymentRequestStore : IPaymentRequestStore
{
    private readonly ConcurrentDictionary<string, PaymentRequest> _requests = new(StringComparer.Ordinal);

    public bool Contains(string externalReference) => _requests.ContainsKey(externalReference);

    public bool TryAdd(PaymentRequest paymentRequest) =>
        _requests.TryAdd(paymentRequest.ExternalReference, paymentRequest);

    public IReadOnlyCollection<PaymentRequest> GetAll() => _requests.Values.ToArray();
}