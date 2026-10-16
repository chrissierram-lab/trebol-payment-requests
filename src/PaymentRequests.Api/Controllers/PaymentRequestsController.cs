using Microsoft.AspNetCore.Mvc;
using PaymentRequests.Api.Contracts;
using PaymentRequests.Api.Services;

namespace PaymentRequests.Api.Controllers;

[ApiController]
[Route("api/v1/payment-requests")]
public sealed class PaymentRequestsController(PaymentRequestService service) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<PaymentRequestResponse>> GetAll() => Ok(service.GetAll());

    [HttpPost]
    public async Task<ActionResult<PaymentRequestResponse>> Create(
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await service.CreateAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, created);
        }
        catch (RequestValidationException exception)
        {
            return BadRequest(new ApiErrorResponse("INVALID_REQUEST", exception.Message));
        }
        catch (DuplicatePaymentReferenceException exception)
        {
            return Conflict(new ApiErrorResponse("DUPLICATE_REFERENCE", exception.Message));
        }
        catch (FeeCalculatorUnavailableException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ApiErrorResponse(
                    "CALCULATION_SERVICE_UNAVAILABLE",
                    "No fue posible calcular la comisión. Intente nuevamente."));
        }
    }
}