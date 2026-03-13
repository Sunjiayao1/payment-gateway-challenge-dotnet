using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Infrastructures;

public class BankSimulatorClient(HttpClient httpClient, ILogger<BankSimulatorClient> logger) : IBankSimulatorClient
{
    public async Task<BankPaymentResponse> ProcessPaymentAsync(BankPaymentRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("/payments", request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("Bank simulator client returned error: {Error}", error);
            throw new BankSimulatorException("Bank simulator error");
        }

        var bankResponse = await response.Content.ReadFromJsonAsync<BankPaymentResponse>();

        if (bankResponse == null)
        {
            logger.LogError("Bank simulator client returned null.");
            throw new BankSimulatorException("Bank simulator returned null");
        }

        return bankResponse;
    }
}