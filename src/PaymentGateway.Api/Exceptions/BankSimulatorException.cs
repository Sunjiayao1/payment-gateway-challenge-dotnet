namespace PaymentGateway.Api.Exceptions;

public class BankSimulatorException(string message) : Exception(message);