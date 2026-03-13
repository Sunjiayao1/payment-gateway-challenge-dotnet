# Payment Gateway

A REST API that allows merchants to process card payments and get payment details.

## API Endpoints

### Process a Payment
`POST /api/payments`

```json
{
  "cardNumber": "2222405343248877",
  "expiryMonth": 4,
  "expiryYear": 2025,
  "currency": "GBP",
  "amount": 100,
  "cvv": "123"
}
```

Response:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Authorized",
  "cardNumberLastFour": "8877",
  "expiryMonth": 4,
  "expiryYear": 2025,
  "currency": "GBP",
  "amount": 100
}
```

### Get Payment info

`GET /api/payments/{id}`

Response:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Authorized",
  "cardNumberLastFour": "8877",
  "expiryMonth": 4,
  "expiryYear": 2025,
  "currency": "GBP",
  "amount": 100
}
```
## How to Run

### Requirements
* .NET 8
* Docker

### Start the Bank Simulator
```bash
docker-compose up
```

### Run the API
```bash
dotnet run --project src/PaymentGateway.Api/PaymentGateway.Api.csproj
```

### Run the Tests
```bash
dotnet test
```

## Assumptions

- Currency only support USD, GBP, EUR.
- Only last 4 digits stored for card numbers, cvv never stored, and authorization_code is stored for future audit and reconcile.
- Rejected payments are not stored in the repository.
- Expiry date combination of month + year must be in the future.

## Design Decisions
1. Use data annotations for request validation, if failed, directly return 400 badRequest without calling the bank.
2. Using four layer architecture, Controller layer for routing, Service layer for business logic, Infra layer for third party access, and Repository layer for data management.
3. Separate Domain Model and API Response, `Payment` (domain) and `PostPaymentResponse` (API) are separate. Domain model store sensitive data like `AuthorizationCode`, this one should never be exposed in the API response.
4. `PaymentsRepository` is registered as Singleton because it is in-memory storage. Will change to `AddScoped` if real DB is used.
5. Change `Cvv` and `CardNumber` type from int to string to support numbers that start with 0.
6. Used custom `BankSimulatorException` instead of generic Exception so each layer can handle bank failures explicitly.
7. Using integration test to verify the full pipeline, only mock third party `BankSimulatorClient` to ensure logic are tested.
8. Add interface to test easily using mock, and it is easier to change implementations without changing the business logic.

## What I Would Add With More Time
1. Persistent storage using relational database (eg. PostgreSQL) with encryption at rest
2. Add rate limiting for merchants
3. Use idempotency key to prevent duplicate charges
4. Validate merchant with API key
5. Retry policy with exponential backoff for bank failures
