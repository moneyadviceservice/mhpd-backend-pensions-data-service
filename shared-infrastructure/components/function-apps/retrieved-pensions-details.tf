module "retrieved_pensions_details_function" {
  source = "github.com/moneyadviceservice/terraform-module-function-app.git?ref=zone_redundancy"

  os_type = "Windows"
  product = var.product
  # create_service_plan     = true
  # asp_name                = "${var.product}-functions-asp-${var.env}"
  resource_group_name     = data.azurerm_resource_group.mhpd.name
  name                    = "retrieved-pensions"
  location                = var.location
  env                     = var.env
  sku_name                = local.sku_name
  dotnet_stack            = true
  enable_vnet_integration = local.enable_vnet_integration
  subnet_id               = local.subnet_id
  connection_strings = [
    local.cosmos_db_connection_string,
    local.service_bus_connection_string
  ]

  app_settings   = local.retrieved_pensions_app_settings
  zone_redundant = local.zone_redundant
}

module "retrieved_pensions_details_staging_slot" {
  count = var.env == "prod" ? 1 : 0

  source = "github.com/moneyadviceservice/terraform-module-function-app.git/slots?ref=zone_redundancy"

  id           = module.retrieved_pensions_details_function.function_app_id
  slot_os_type = "Windows"
  product      = var.product

  resource_group_name     = data.azurerm_resource_group.mhpd.name
  name                    = "retrieved-pensions"
  location                = var.location
  env                     = var.env
  sku_name                = local.sku_name
  dotnet_stack            = true
  enable_vnet_integration = local.enable_vnet_integration
  subnet_id               = local.subnet_id
  connection_strings = [
    local.cosmos_db_connection_string,
    local.service_bus_connection_string
  ]

  app_settings = local.retrieved_pensions_app_settings
}