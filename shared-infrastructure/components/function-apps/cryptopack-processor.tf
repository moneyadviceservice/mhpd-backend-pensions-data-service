module "cryptopack_processor_function" {
  source = "github.com/moneyadviceservice/terraform-module-function-app.git?ref=zone_redundancy"

  os_type = "Windows"
  product = var.product
  # create_service_plan = local.# create_service_plan
  # service_plan_id     = module.retrieved_pensions_details_function.asp_id
  resource_group_name = data.azurerm_resource_group.mhpd.name
  name                = "cryptopack-proc"
  location            = var.location
  env                 = var.env
  sku_name            = local.sku_name
  dotnet_stack        = true
  zone_redundant      = local.zone_redundant

  app_settings = local.cryptopack_app_settings
}

module "cryptopack_processor_staging_slot" {
  count = var.env == "prod" ? 1 : 0

  source = "github.com/moneyadviceservice/terraform-module-function-app.git/slots?ref=zone_redundancy"

  id           = module.cryptopack_processor_function.function_app_id
  slot_os_type = "Windows"
  product      = var.product

  resource_group_name = data.azurerm_resource_group.mhpd.name
  name                = "cryptopack-proc"
  location            = var.location
  env                 = var.env
  sku_name            = local.sku_name
  dotnet_stack        = true

  app_settings = local.cryptopack_app_settings
}