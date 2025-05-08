module "pdp_view_data_service_emulator" {
  source              = "github.com/moneyadviceservice/terraform-module-app-service.git?ref=add-deployment-slots"
  name                = "pdp-view-data-service-emulator"
  product             = var.product
  resource_group_name = data.azurerm_resource_group.this.name
  location            = var.location
  env                 = var.env
  os_type             = "Linux"
  sku_name            = local.sku_name
  zone_redundant      = local.zone_redundant
  ftps_state          = var.ftps_state
  app_command_line    = "dotnet PDPViewDataServiceEmulator.dll"
  dotnet_stack        = true

  connection_strings = [
    local.emulator_cosmos_db_connection_string
  ]
  app_settings = local.pdp_view_data_service_emulator_app_settings
  tags         = {}
}

resource "azurerm_key_vault_access_policy" "pdp_view_data_service_application_access" {
  key_vault_id = data.azurerm_key_vault.mhpd.id

  object_id = module.pdp_view_data_service_emulator.system_assigned_identity_object_id
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