using System.Net.Http.Json;
using System.Text.Json;
using PaymentRequests.Api.Contracts;

namespace PaymentRequests.Api.Services;

public sealed class JavaFeeCalculatorClient(HttpClient httpClient) : IFeeCalculatorClient
{
    public async Task<CalculateFeeResponse> CalculateAsync(
        CalculateFeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "api/v1/fees/calculate",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new FeeCalculatorUnavailableException("El servicio de cálculo respondió con error.");
            }

            return await response.Content.ReadFromJsonAsync<CalculateFeeResponse>(cancellationToken)
                ?? throw new FeeCalculatorUnavailableException("El servicio de cálculo devolvió una respuesta vacía.");
        }
        catch (FeeCalculatorUnavailableException)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            throw new FeeCalculatorUnavailableException("No fue posible conectar con el servicio de cálculo.", exception);
        }
        catch (JsonException exception)
        {
            throw new FeeCalculatorUnavailableException("El servicio de cálculo devolvió una respuesta inválida.", exception);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new FeeCalculatorUnavailableException("El servicio de cálculo excedió el tiempo de espera.", exception);
        }
    }
}