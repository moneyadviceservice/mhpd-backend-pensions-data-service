module "pension_data_service" {
  source                  = "github.com/moneyadviceservice/terraform-module-app-service.git?ref=add-deployment-slots"
  name                    = "pension-data-service"
  product                 = var.product
  resource_group_name     = data.azurerm_resource_group.this.name
  location                = var.location
  env                     = var.env
  os_type                 = "Linux"
  sku_name                = local.sku_name
  zone_redundant          = local.zone_redundant
  ftps_state              = var.ftps_state
  app_command_line        = "dotnet PensionsDataService.dll"
  dotnet_stack            = true
  enable_vnet_integration = local.enable_vnet_integration
  subnet_id               = local.subnet_id
  connection_strings = [
    local.cosmos_db_connection_string,
    local.service_bus_connection_string
  ]

  app_settings = local.pension_data_app_settings
  tags         = {}
}


module "pension_data_service_staging_slot" {
  count = var.env == "prod" ? 1 : 0

  source              = "github.com/moneyadviceservice/terraform-module-app-service.git/slots?ref=add-deployment-slots"
  env                 = var.env
  product             = var.product
  resource_group_name = data.azurerm_resource_group.this.name

  dotnet_stack = true

  slot_os_type = "Linux"
  name         = "pension-data"
  id           = module.pension_data_service.app_service_id

  public_network_access_enabled = local.public_network_access_enabled
  enable_vnet_integration       = local.enable_vnet_integration
  subnet_id                     = local.subnet_id

  connection_strings = [
    local.cosmos_db_connection_string,
    local.service_bus_connection_string
  ]

  app_settings           = local.pension_data_app_settings
  tags                   = {}
  enable_client_affinity = true
}