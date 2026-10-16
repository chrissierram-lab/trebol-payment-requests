using PaymentRequests.Api.Contracts;

namespace PaymentRequests.Api.Services;

public interface IFeeCalculatorClient
{
    Task<CalculateFeeResponse> CalculateAsync(
        CalculateFeeRequest request,
        CancellationToken cancellationToken);
}