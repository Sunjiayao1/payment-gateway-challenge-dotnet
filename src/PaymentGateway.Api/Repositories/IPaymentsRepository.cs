using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Repositories;

public interface IPaymentsRepository
{
    public Payment? Get(Guid id);
    public void Add(Payment request);
}