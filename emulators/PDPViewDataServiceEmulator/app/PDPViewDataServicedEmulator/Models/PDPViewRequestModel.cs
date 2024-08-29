using Microsoft.AspNetCore.Mvc;

namespace PDPViewDataServicedEmulator.Models
{
    public class PDPViewRequestModel
    {  
        [FromQuery(Name = "asset_guid")]
        public string? Asset_Guid { get; set; }
    }
}
