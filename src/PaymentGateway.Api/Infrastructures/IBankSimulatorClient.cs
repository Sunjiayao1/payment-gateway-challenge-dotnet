using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Infrastructures;

public interface IBankSimulatorClient
{
    Task<BankPaymentResponse> ProcessPaymentAsync(BankPaymentRequest request);
}