module "CDA_service_emulator" {
  source                 = "github.com/moneyadviceservice/terraform-module-app-service.git?ref=add-deployment-slots"
  name                   = "CDA-service-emulator"
  product                = var.product
  resource_group_name    = data.azurerm_resource_group.this.name
  location               = var.location
  env                    = var.env
  os_type                = "Linux"
  sku_name               = local.sku_name
  zone_redundant         = local.zone_redundant
  ftps_state             = var.ftps_state
  app_command_line       = "dotnet CDAServiceEmulator.dll"
  dotnet_stack           = true
  enable_client_affinity = true
  connection_strings = [
    local.emulator_cosmos_db_connection_string
  ]
  app_settings = local.cda_emulator_app_settings
  tags         = {}
}

resource "azurerm_key_vault_access_policy" "cda_emulator_application_access" {
  key_vault_id = data.azurerm_key_vault.mhpd.id

  object_id = module.CDA_service_emulator.system_assigned_identity_object_id
  tenant_id = data.azurerm_client_config.current.tenant_id

  key_permissions = [
    "List",
    "Get",
  ]

  secret_permissions = [
    "List",
    "Get",
  ]
  certificate_permissions = [
    "List",
    "Get",
  ]
}

# access for old emular - to be removed during decomissioning
resource "azurerm_key_vault_access_policy" "cda_old_emulator_application_access" {
  key_vault_id = data.azurerm_key_vault.mhpd.id

  object_id = "81894e6a-9e4f-4107-8ae4-1c67ee3c1f46"
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