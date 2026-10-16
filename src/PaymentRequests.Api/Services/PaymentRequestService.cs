using PaymentRequests.Api.Contracts;
using PaymentRequests.Api.Models;

namespace PaymentRequests.Api.Services;

public sealed class PaymentRequestService(
    IPaymentRequestStore store,
    IFeeCalculatorClient feeCalculatorClient)
{
    private static readonly HashSet<string> SupportedChannels = new(StringComparer.Ordinal)
    {
        "WEB",
        "MOBILE",
        "BRANCH"
    };

    public IReadOnlyCollection<PaymentRequestResponse> GetAll() =>
        store.GetAll().Select(ToResponse).ToArray();

    public async Task<PaymentRequestResponse> CreateAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var externalReference = request.ExternalReference?.Trim();
        if (string.IsNullOrEmpty(externalReference) || externalReference.Length > 50)
        {
            throw new RequestValidationException("La referencia es obligatoria y debe tener como máximo 50 caracteres.");
        }

        if (request.Amount <= 0 || GetDecimalScale(request.Amount) > 2)
        {
            throw new RequestValidationException("El monto debe ser mayor que cero y tener como máximo dos decimales.");
        }

        if (request.Channel is null || !SupportedChannels.Contains(request.Channel))
        {
            throw new RequestValidationException("El canal debe ser WEB, MOBILE o BRANCH.");
        }

        if (store.Contains(externalReference))
        {
            throw new DuplicatePaymentReferenceException();
        }

        var calculation = await feeCalculatorClient.CalculateAsync(
            new CalculateFeeRequest(request.Amount, request.Channel),
            cancellationToken);

        if (calculation.Fee < 0 || calculation.Total < 0 || calculation.Total != request.Amount + calculation.Fee)
        {
            throw new FeeCalculatorUnavailableException("El servicio de cálculo devolvió un resultado inválido.");
        }

        var paymentRequest = new PaymentRequest(
            Guid.NewGuid(),
            externalReference,
            request.Amount,
            request.Channel,
            calculation.Fee,
            calculation.Total,
            "PENDING");

        if (!store.TryAdd(paymentRequest))
        {
            throw new DuplicatePaymentReferenceException();
        }

        return ToResponse(paymentRequest);
    }

    private static int GetDecimalScale(decimal value) =>
        (decimal.GetBits(value)[3] >> 16) & 0x7F;

    private static PaymentRequestResponse ToResponse(PaymentRequest request) => new(
        request.Id,
        request.ExternalReference,
        request.Amount,
        request.Channel,
        request.Fee,
        request.Total,
        request.Status);
}