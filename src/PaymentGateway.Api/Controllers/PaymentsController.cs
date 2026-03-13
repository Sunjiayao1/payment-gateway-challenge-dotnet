using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public ActionResult<GetPaymentResponse> GetPayment(Guid id)
    {
        var payment = paymentService.GetPayment(id);
        if (payment == null)
        {
            return NotFound();
        }

        return new OkObjectResult(payment);
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPayment(PostPaymentRequest request)
    {
        try
        {
            var result = await paymentService.ProcessPaymentAsync(request);
            return new OkObjectResult(result);
        }
        catch (BankSimulatorException)
        {
            return StatusCode(502, "Bank simulator error");
        }
    }
}