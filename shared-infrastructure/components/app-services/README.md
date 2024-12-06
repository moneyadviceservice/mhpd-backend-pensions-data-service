<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) | 4.4.0 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) | 4.4.0 |

## Modules

| Name | Source | Version |
|------|--------|---------|
| <a name="module_CDA_service"></a> [CDA\_service](#module\_CDA\_service) | github.com/moneyadviceservice/terraform-module-app-service.git | n/a |
| <a name="module_CDA_service_emulator"></a> [CDA\_service\_emulator](#module\_CDA\_service\_emulator) | github.com/moneyadviceservice/terraform-module-app-service.git | add_connection_string |
| <a name="module_pdp_view_data_service_emulator"></a> [pdp\_view\_data\_service\_emulator](#module\_pdp\_view\_data\_service\_emulator) | github.com/moneyadviceservice/terraform-module-app-service.git | n/a |
| <a name="module_pei_integration_service"></a> [pei\_integration\_service](#module\_pei\_integration\_service) | github.com/moneyadviceservice/terraform-module-app-service.git | n/a |
| <a name="module_pension_data_service"></a> [pension\_data\_service](#module\_pension\_data\_service) | github.com/moneyadviceservice/terraform-module-app-service.git | n/a |
| <a name="module_token_integration_service"></a> [token\_integration\_service](#module\_token\_integration\_service) | github.com/moneyadviceservice/terraform-module-app-service.git | n/a |

## Resources

| Name | Type |
|------|------|
| [azurerm_key_vault_access_policy.cda_application_access](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault_access_policy) | resource |
| [azurerm_client_config.current](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/data-sources/client_config) | data source |
| [azurerm_cosmosdb_account.this](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/data-sources/cosmosdb_account) | data source |
| [azurerm_key_vault.mhpd](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/data-sources/key_vault) | data source |
| [azurerm_key_vault_secret.CDA_service_emulator_private_key](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/data-sources/key_vault_secret) | data source |
| [azurerm_key_vault_secret.cda_service_private_key](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/data-sources/key_vault_secret) | data source |
| [azurerm_resource_group.this](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/data-sources/resource_group) | data source |
| [azurerm_servicebus_namespace.pension_data](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/data-sources/servicebus_namespace) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_env"></a> [env](#input\_env) | n/a | `any` | n/a | yes |
| <a name="input_ftps_state"></a> [ftps\_state](#input\_ftps\_state) | n/a | `string` | `"FtpsOnly"` | no |
| <a name="input_location"></a> [location](#input\_location) | n/a | `string` | `"UK South"` | no |
| <a name="input_product"></a> [product](#input\_product) | n/a | `string` | `"mhpd"` | no |

## Outputs

No outputs.
<!-- END_TF_DOCS -->