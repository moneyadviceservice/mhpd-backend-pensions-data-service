module "pei_integration_service" {
  source                  = "github.com/moneyadviceservice/terraform-module-app-service.git?ref=add-deployment-slots"
  name                    = "pei-integration-service"
  product                 = var.product
  resource_group_name     = data.azurerm_resource_group.this.name
  location                = var.location
  env                     = var.env
  os_type                 = "Linux"
  sku_name                = local.sku_name
  zone_redundant          = local.zone_redundant
  ftps_state              = var.ftps_state
  app_command_line        = "dotnet PeiIntegrationService.dll"
  dotnet_stack            = true
  enable_vnet_integration = local.enable_vnet_integration
  subnet_id               = local.subnet_id

  app_settings = local.pei_integration_app_settings
  tags         = {}
}

module "pei_integration_staging_slot" {
  count = var.env == "prod" ? 1 : 0

  source              = "github.com/moneyadviceservice/terraform-module-app-service.git/slots?ref=add-deployment-slots"
  env                 = var.env
  product             = var.product
  resource_group_name = data.azurerm_resource_group.this.name

  dotnet_stack = true

  slot_os_type = "Linux"
  name         = "pei-integration"
  id           = module.pei_integration_service.app_service_id

  public_network_access_enabled = local.public_network_access_enabled
  enable_vnet_integration       = local.enable_vnet_integration
  subnet_id                     = local.subnet_id

  app_settings           = local.pei_integration_app_settings
  tags                   = {}
  enable_client_affinity = true
}