using PaymentRequests.Api.Contracts;
using PaymentRequests.Api.Services;

namespace PaymentRequests.Api.Tests;

public sealed class PaymentRequestServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenCalculationServiceFails_DoesNotSaveRequest()
    {
        var store = new InMemoryPaymentRequestStore();
        var service = new PaymentRequestService(store, new FailingFeeCalculatorClient());

        await Assert.ThrowsAsync<FeeCalculatorUnavailableException>(() => service.CreateAsync(
            new CreatePaymentRequest
            {
                ExternalReference = "PAY-2026-0001",
                Amount = 1000.00m,
                Channel = "WEB"
            },
            CancellationToken.None));

        Assert.Empty(store.GetAll());
    }

    [Fact]
    public async Task CreateAsync_WhenAmountHasMoreThanTwoDecimals_RejectsBeforeCalculation()
    {
        var store = new InMemoryPaymentRequestStore();
        var service = new PaymentRequestService(store, new FailingFeeCalculatorClient());

        await Assert.ThrowsAsync<RequestValidationException>(() => service.CreateAsync(
            new CreatePaymentRequest
            {
                ExternalReference = "PAY-2026-0002",
                Amount = 10.001m,
                Channel = "WEB"
            },
            CancellationToken.None));

        Assert.Empty(store.GetAll());
    }

    [Fact]
    public async Task CreateAsync_NormalizesReferenceAndComparesDuplicatesCaseSensitively()
    {
        var store = new InMemoryPaymentRequestStore();
        var service = new PaymentRequestService(store, new SuccessfulFeeCalculatorClient());
        var request = new CreatePaymentRequest
        {
            ExternalReference = " PAY-2026-0001 ",
            Amount = 1000.00m,
            Channel = "WEB"
        };

        var created = await service.CreateAsync(request, CancellationToken.None);

        Assert.Equal("PAY-2026-0001", created.ExternalReference);
        Assert.Equal("PENDING", created.Status);
        await Assert.ThrowsAsync<DuplicatePaymentReferenceException>(() => service.CreateAsync(
            new CreatePaymentRequest
            {
                ExternalReference = "PAY-2026-0001",
                Amount = request.Amount,
                Channel = request.Channel
            },
            CancellationToken.None));

        var differentCase = await service.CreateAsync(
            new CreatePaymentRequest
            {
                ExternalReference = "pay-2026-0001",
                Amount = request.Amount,
                Channel = request.Channel
            },
            CancellationToken.None);

        Assert.Equal("pay-2026-0001", differentCase.ExternalReference);
        Assert.Equal(2, store.GetAll().Count);
    }

    private sealed class FailingFeeCalculatorClient : IFeeCalculatorClient
    {
        public Task<CalculateFeeResponse> CalculateAsync(
            CalculateFeeRequest request,
            CancellationToken cancellationToken) =>
            throw new FeeCalculatorUnavailableException("Servicio no disponible.");
    }

    private sealed class SuccessfulFeeCalculatorClient : IFeeCalculatorClient
    {
        public Task<CalculateFeeResponse> CalculateAsync(
            CalculateFeeRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new CalculateFeeResponse(10.00m, 1010.00m));
    }
}