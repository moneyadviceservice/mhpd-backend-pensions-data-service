
namespace MhpdCommon.TokenValidation;

public interface ITokenRequestValidator<TRequest>
{
    ValidationResult Validate(TRequest request);
    
    int Order { get; }
    
    string GrantType { get; }
}