using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.Requests;

public class PostPaymentRequest : IValidatableObject
{
    [Required(ErrorMessage = "Card Number is required")]
    [RegularExpression(@"^\d{14,19}$", ErrorMessage = "Must contain 14-19 numeric characters only.")]
    public string CardNumber { get; set; }

    [Required(ErrorMessage = "Expiry month is required")]
    [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12.")]
    public int ExpiryMonth { get; set; }

    [Required(ErrorMessage = "Expiry year is required")]
    [Range(0, 9999, ErrorMessage = "Expiry year is invalid.")]
    public int ExpiryYear { get; set; }

    [Required(ErrorMessage = "Currency is required")]
    [AllowedValues("EUR", "USD", "GBP", ErrorMessage = "Supported currencies are EUR, USD, GBP")]
    public string Currency { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    public int Amount { get; set; }

    [Required(ErrorMessage = "Cvv is required")]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Must be 3-4 numeric characters only.")]
    public string Cvv { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var expiryDate = new DateTime(ExpiryYear, ExpiryMonth, 1).AddMonths(1);
        if (expiryDate < DateTime.Today)
        {
            yield return new ValidationResult(
                "The expiry date must be in the future", ["ExpiryYear", "ExpiryMonth"]);
        }
    }
}