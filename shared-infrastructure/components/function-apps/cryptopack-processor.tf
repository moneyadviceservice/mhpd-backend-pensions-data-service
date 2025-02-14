module "cryptopack_processor_function" {
  source = "github.com/moneyadviceservice/terraform-module-function-app.git?ref=main"

  os_type             = "Windows"
  product             = var.product
  create_service_plan = true
  resource_group_name = data.azurerm_resource_group.mhpd.name
  name                = "cryptopack-proc"
  location            = var.location
  env                 = var.env
  sku_name            = "EP1"
  dotnet_stack        = true

  app_settings = {
    "StorageConnectionString"                  = "DefaultEndpointsProtocol=https;AccountName=cryptopackproc${var.env};AccountKey=${module.cryptopack_processor_function.sa_primary_access_key}"
    "KeyVaultSettings__KeyVaultUrl"            = "https://mhpd-${var.env}.vault.azure.net/"
    "KeyVaultSettings__TenantId"               = "bbe41032-8fce-4d42-bab5-44e21510886d"
    "KeyVaultSettings__ClientId"               = "83a2a90c-3d22-4cb6-b6e2-c3a5938f9db7"
    "KeyVaultSettings__ClientSecret"           = data.azurerm_key_vault_secret.user_client_secret.value
    "CryptopackSettings__MtlsCertificateName"  = "PdpMtls"
    "CryptopackSettings__PrivateKeySecretName" = "maps-cda-service-private-key"
    "CryptopackSettings__KidSecretName"        = "maps-cda-service-kid"
    "CryptopackSettings__PfxPassword"          = data.azurerm_key_vault_secret.pfx_password.value
    "WebAppSettings__AppName"                  = "CDA-service-${var.env}"
    "WebAppSettings__SubscriptionId"           = data.azurerm_client_config.current.subscription_id
    "WebAppSettings__ResourceGroupName"        = data.azurerm_resource_group.mhpd.name
    "WebAppSettings__JwtKeyVariable"           = "JwtSettings__PrivateKey"
    "WebAppSettings__JwtKidVariable"           = "JwtSettings__Kid"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"          = "true"
    "WEBSITE_RUN_FROM_PACKAGE"                 = "1"
    "WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED"   = "1"
    "APPLICATIONINSIGHTS_CONNECTION_STRING"    = "InstrumentationKey=${module.cryptopack_processor_function.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.cryptopack_processor_function.app_insights_app_id}"
    "Manifest__KeyId"                          = "kid"
    "Manifest__MtlsCertificate"                = "certificate"
    "Manifest__MtlsChain"                      = "certificateChain"
    "Manifest__CertificatePair__PrivateKey"    = "certPrivateKey"
    "Manifest__CertificatePair__PublicKey"     = "certPublicKey"
    "Manifest__CertificatePair__AlgorithmType" = "1"
    "Manifest__JwtPair__PrivateKey"            = "jwtPrivateKey"
    "Manifest__JwtPair__PublicKey"             = "jwtPublicKey"
    "Manifest__JwtPair__AlgorithmType"         = "0"
  }
}