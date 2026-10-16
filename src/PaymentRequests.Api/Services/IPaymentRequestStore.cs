using PaymentRequests.Api.Models;

namespace PaymentRequests.Api.Services;

public interface IPaymentRequestStore
{
    bool Contains(string externalReference);

    bool TryAdd(PaymentRequest paymentRequest);

    IReadOnlyCollection<PaymentRequest> GetAll();
}