namespace MhpdCommon.TokenValidation;

public interface IRequestValidator<TRequest>
{
    ValidationResult Validate(TRequest request);
}