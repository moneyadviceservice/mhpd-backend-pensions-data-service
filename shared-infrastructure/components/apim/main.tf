resource "azurerm_api_management_product" "mhpd" {
  product_id            = var.product
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  display_name          = var.product
  subscription_required = true
  subscriptions_limit   = var.subscriptions_limit
  approval_required     = false
  published             = true
}

resource "azurerm_key_vault_access_policy" "apim_access" {
  key_vault_id = data.azurerm_key_vault.mhpd.id

  object_id = data.azurerm_api_management.this.identity[0].principal_id
  tenant_id = data.azurerm_client_config.current.tenant_id

  certificate_permissions = [
    "List",
    "Get",
  ]

  secret_permissions = [
    "List",
    "Get",
  ]
}

resource "azurerm_api_management_certificate" "pdp-mtls" {
  name                = "PdpMtls"
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  key_vault_secret_id = data.azurerm_key_vault_certificate.pdp-mtls.secret_id
}