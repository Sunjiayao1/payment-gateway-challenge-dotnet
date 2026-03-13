using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Infrastructures;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Repositories;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    private readonly Mock<IBankSimulatorClient> _bankSimulatorClientMock = new();
    private readonly Mock<IPaymentsRepository> _paymentsRepositoryMock = new();

    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var mockGuid = Guid.NewGuid();

        var payment = new GetPaymentResponse
        {
            Id = mockGuid,
            ExpiryYear = DateTime.Now.Month + 1,
            ExpiryMonth = DateTime.Now.Year + 1,
            Amount = 1000,
            CardNumberLastFour = "1234",
            Currency = "GBP"
        };

        var paymentFromRepostory = new Payment
        {
            Id = mockGuid,
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = "1234",
            ExpiryYear = DateTime.Now.Month + 1,
            ExpiryMonth = DateTime.Now.Year + 1,
            Currency = "GBP",
            Amount = 1000,
            AuthorizationCode = "authorization_code"
        };

        _paymentsRepositoryMock.Setup(p => p.Get(It.IsAny<Guid>())).Returns(paymentFromRepostory);
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                    services.AddSingleton(_paymentsRepositoryMock.Object)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{mockGuid}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(paymentResponse.Id, payment.Id);
        Assert.Equal(paymentResponse.ExpiryYear, payment.ExpiryYear);
        Assert.Equal(paymentResponse.ExpiryMonth, payment.ExpiryMonth);
        Assert.Equal(paymentResponse.Amount, payment.Amount);
        Assert.Equal(paymentResponse.CardNumberLastFour, payment.CardNumberLastFour);
        Assert.Equal(paymentResponse.Currency, payment.Currency);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ReturnsBadRequestIfRequestIsInvalid()
    {
        var request = new PostPaymentRequest
        {
            CardNumber = "1234567891234",
            ExpiryMonth = 0,
            ExpiryYear = 1000,
            Currency = "ABC",
            Amount = 100,
            Cvv = "123"
        };

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/Payments", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Returns200WhenGetAuthorizationCodeFromBankSimulator()
    {
        var request = new PostPaymentRequest
        {
            CardNumber = "123789759430727384",
            ExpiryMonth = 1,
            ExpiryYear = 2028,
            Currency = "EUR",
            Amount = 1000,
            Cvv = "123"
        };

        var paymentResponse = new BankPaymentResponse { Authorized = true, AuthorizationCode = "authorization_code" };

        var postPaymentResponse = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = "7384",
            ExpiryMonth = 1,
            ExpiryYear = 2028,
            Currency = "EUR",
            Amount = 1000
        };

        _bankSimulatorClientMock
            .Setup(b => b.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>()))
            .ReturnsAsync(paymentResponse);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(service => service.AddSingleton(_bankSimulatorClientMock.Object));
        }).CreateClient();

        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var responseBody = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(responseBody.Amount, postPaymentResponse.Amount);
        Assert.Equal(responseBody.Status, postPaymentResponse.Status);
        Assert.Equal(responseBody.CardNumberLastFour, postPaymentResponse.CardNumberLastFour);
        Assert.Equal(responseBody.ExpiryMonth, postPaymentResponse.ExpiryMonth);
        Assert.Equal(responseBody.ExpiryYear, postPaymentResponse.ExpiryYear);
        Assert.Equal(responseBody.Currency, postPaymentResponse.Currency);
    }

    [Fact]
    public async Task Returns200WithStatusDeclinedWhenNoAuthorizedIsFalseAndNoAuthorizationCodeReturned()
    {
        var request = new PostPaymentRequest
        {
            CardNumber = "12673847832912876",
            ExpiryMonth = 2,
            ExpiryYear = DateTime.Now.Year + 1,
            Currency = "EUR",
            Amount = 2000,
            Cvv = "123"
        };
        var bankPaymentResponse = new BankPaymentResponse { Authorized = false };
        _bankSimulatorClientMock
            .Setup(b => b.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>()))
            .ReturnsAsync(bankPaymentResponse);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(service =>
            {
                service.AddSingleton(_bankSimulatorClientMock.Object);
            })).CreateClient();

        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var responseBody = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(PaymentStatus.Declined, responseBody.Status);
    }
}