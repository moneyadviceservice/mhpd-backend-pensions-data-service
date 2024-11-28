using Microsoft.AspNetCore.Mvc;

namespace PDPViewDataServiceEmulator.Models
{
    public class PdpViewRequestModel
    {  
        [FromQuery(Name = "asset_guid")]
        public string? assetGuid { get; set; }
    }
}
