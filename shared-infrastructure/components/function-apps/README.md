<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) | 4.5.0 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) | 4.5.0 |

## Modules

| Name | Source | Version |
|------|--------|---------|
| <a name="module_pension_request_function"></a> [pension\_request\_function](#module\_pension\_request\_function) | github.com/moneyadviceservice/terraform-module-function-app.git | main |
| <a name="module_pensions_retrieval_function"></a> [pensions\_retrieval\_function](#module\_pensions\_retrieval\_function) | github.com/moneyadviceservice/terraform-module-function-app.git | main |
| <a name="module_retrieved_pensions_details_function"></a> [retrieved\_pensions\_details\_function](#module\_retrieved\_pensions\_details\_function) | github.com/moneyadviceservice/terraform-module-function-app.git | main |

## Resources

| Name | Type |
|------|------|
| [azurerm_cosmosdb_account.this](https://registry.terraform.io/providers/hashicorp/azurerm/4.5.0/docs/data-sources/cosmosdb_account) | data source |
| [azurerm_resource_group.mhpd](https://registry.terraform.io/providers/hashicorp/azurerm/4.5.0/docs/data-sources/resource_group) | data source |
| [azurerm_servicebus_namespace.this](https://registry.terraform.io/providers/hashicorp/azurerm/4.5.0/docs/data-sources/servicebus_namespace) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_env"></a> [env](#input\_env) | n/a | `any` | n/a | yes |
| <a name="input_location"></a> [location](#input\_location) | n/a | `string` | `"UK South"` | no |
| <a name="input_product"></a> [product](#input\_product) | n/a | `string` | `"mhpd"` | no |

## Outputs

No outputs.
<!-- END_TF_DOCS -->