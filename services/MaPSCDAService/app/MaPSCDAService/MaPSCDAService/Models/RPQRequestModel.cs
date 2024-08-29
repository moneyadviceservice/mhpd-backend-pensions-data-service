namespace MaPSCDAService.Models
{
    public class RPQRequestModel
    {
        public string? Iss { get; set; }
        
        public string? UserSessionId { get; set; }

        public bool Validate(RPQRequestModel model)
        {
            return (string.IsNullOrEmpty(model.Iss) || string.IsNullOrEmpty(model.UserSessionId)) ? false : true;
        }
    }
}