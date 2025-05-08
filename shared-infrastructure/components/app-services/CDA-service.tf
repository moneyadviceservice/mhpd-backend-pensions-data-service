module "CDA_service" {
  source                 = "github.com/moneyadviceservice/terraform-module-app-service.git?ref=add-deployment-slots"
  name                   = "CDA-service"
  product                = var.product
  resource_group_name    = data.azurerm_resource_group.this.name
  location               = var.location
  env                    = var.env
  os_type                = "Linux"
  sku_name               = local.sku_name
  zone_redundant         = local.zone_redundant
  ftps_state             = var.ftps_state
  app_command_line       = "dotnet MaPSCDAService.dll"
  dotnet_stack           = true
  enable_client_affinity = true
  connection_strings = [
    local.cosmos_db_connection_string
  ]
  enable_vnet_integration = local.enable_vnet_integration
  subnet_id               = local.subnet_id

  app_settings = local.cda_service_app_settings
  tags         = {}
}

resource "azurerm_key_vault_access_policy" "cda_application_access" {
  key_vault_id = data.azurerm_key_vault.mhpd.id

  object_id = module.CDA_service.system_assigned_identity_object_id
  tenant_id = data.azurerm_client_config.current.tenant_id

  key_permissions = [
    "List",
    "Get",
  ]

  secret_permissions = [
    "List",
    "Get",
  ]
}

module "cda_staging_slot" {
  count = var.env == "prod" ? 1 : 0

  source              = "github.com/moneyadviceservice/terraform-module-app-service.git/slots?ref=add-deployment-slots"
  env                 = var.env
  product             = var.product
  resource_group_name = data.azurerm_resource_group.this.name

  dotnet_stack = true

  slot_os_type = "Linux"
  name         = "CDA-service"
  id           = module.CDA_service.app_service_id

  public_network_access_enabled = local.public_network_access_enabled
  enable_vnet_integration       = local.enable_vnet_integration
  subnet_id                     = local.subnet_id

  connection_strings = [
    local.cosmos_db_connection_string
  ]
  app_settings           = local.cda_service_app_settings
  tags                   = {}
  enable_client_affinity = true
}