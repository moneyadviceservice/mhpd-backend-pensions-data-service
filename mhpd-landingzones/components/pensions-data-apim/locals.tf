locals {
  location_short = {
    uksouth = "uks"
    ukwest  = "ukw"
  }
  loc = local.location_short[var.location]

  is_pe_env           = var.env == "nft" || var.env == "stg" || var.env == "prod"
  apim_name           = local.is_pe_env ? "apim-internal-mhpd-${var.env}-${local.loc}" : "apim-mhpd-${var.env}-${local.loc}"
  apim_resource_group = "rg-mhpd-${var.env}-apim-${var.location}"

  api_management_logger_id = "/subscriptions/${var.subscription_id}/resourceGroups/${local.apim_resource_group}/providers/Microsoft.ApiManagement/service/${local.apim_name}/loggers/apim-logger-mhpd-${var.env}"

  pension_data_backend_url_uks = "https://pension-data-service-uks-${var.env}.azurewebsites.net"
  pension_data_backend_url_ukw = "https://pension-data-service-ukw-${var.env}.azurewebsites.net"
}
