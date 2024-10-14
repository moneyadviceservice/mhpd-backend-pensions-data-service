using MhpdCommon.Models.MessageBodyModels;

namespace MhpdCommon.TokenValidation;

public class PensionsDataRequestValidatorPipeline : IRequestValidator<PensionsDataRequestModel>
{
    private readonly IEnumerable<ITokenRequestValidator<PensionsDataRequestModel>> _validators;

    public PensionsDataRequestValidatorPipeline(IEnumerable<ITokenRequestValidator<PensionsDataRequestModel>> validators)
    {
        _validators = validators.OrderBy(v => v.Order);
    }

    public ValidationResult Validate(PensionsDataRequestModel request)
    {
        if (!_validators.Any())
        {
            throw new InvalidOperationException("No validators found.");
        }
        
        foreach (var validator in _validators)
        {
            var result = validator.Validate(request);
            if (!result.IsValid)
            {
                return result;
            }
        }

        return ValidationResult.Success();
    }
}
