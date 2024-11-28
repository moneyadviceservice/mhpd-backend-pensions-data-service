using System.Text.Json.Serialization;

namespace PDPViewDataServiceEmulator.Models
{
    public class ViewDataResponseModel
    {
        [JsonPropertyName("view_data_token")]
        public string? ViewDataToken { get; set;}     
    }
}
