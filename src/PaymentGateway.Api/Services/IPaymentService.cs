using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentService
{
    public Task<PostPaymentResponse> ProcessPaymentAsync(PostPaymentRequest request);

    public GetPaymentResponse? GetPayment(Guid id);
}