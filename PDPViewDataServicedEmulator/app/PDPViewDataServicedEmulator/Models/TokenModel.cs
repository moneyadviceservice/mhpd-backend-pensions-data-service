namespace PDPViewDataServicedEmulator.Models
{
    public class TokenModel
    {
        public string? Issuer { get; set; }

        public string? Subject { get; set; }

        public string? Audience { get; set; }

        public string? IssuedAt { get; set; }

        public string? Expiry { get; set; }

        public string? Jti { get; set; }

        public string? ViewData { get; set; }
    }
}
