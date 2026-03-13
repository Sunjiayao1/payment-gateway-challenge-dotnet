using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Infrastructures;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Repositories;

namespace PaymentGateway.Api.Services;

public class PaymentService(
    IBankSimulatorClient bankSimulatorClient,
    IPaymentsRepository paymentsRepository
) : IPaymentService
{
    public async Task<PostPaymentResponse> ProcessPaymentAsync(PostPaymentRequest request)
    {
        var bankPaymentRequest = new BankPaymentRequest
        {
            CardNumber = request.CardNumber,
            ExpiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
            Currency = request.Currency,
            Amount = request.Amount,
            Cvv = request.Cvv
        };

        var paymentResult = await bankSimulatorClient.ProcessPaymentAsync(bankPaymentRequest);
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Status = paymentResult.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            CardNumberLastFour = request.CardNumber[^4..],
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount,
            AuthorizationCode = paymentResult.AuthorizationCode
        };

        paymentsRepository.Add(payment);

        return new PostPaymentResponse
        {
            Id = payment.Id,
            Status = paymentResult.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            CardNumberLastFour = request.CardNumber[^4..],
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount,
        };
    }

    public GetPaymentResponse? GetPayment(Guid id)
    {
        var payment = paymentsRepository.Get(id);
        return payment != null
            ? new GetPaymentResponse
            {
                Id = payment.Id,
                Status = payment.Status,
                CardNumberLastFour = payment.CardNumberLastFour,
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                Currency = payment.Currency,
                Amount = payment.Amount,
            }
            : null;
    }
}