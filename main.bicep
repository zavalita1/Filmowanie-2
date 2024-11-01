param webAppName string = 'filmowanie2'
param location string = resourceGroup().location

var appServicePlanName = toLower('AppServicePlan-${webAppName}')
var appInsightsName = toLower('appins-${webAppName}')

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: 'filmowanie2'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

resource appInsightsComponents 'Microsoft.Insights/components@2020-02-02' = {
  kind: 'web'
  name: appInsightsName
  location: location
  tags: {}
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Redfield'
    Request_Source: 'IbizaWebAppExtensionCreate'
    SamplingPercentage: 100
    RetentionInDays: 8
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource ASPFilmowaniegroup 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  kind: 'app'
  location: location
  tags: {}
  properties: {
    workerTierName: null
    hostingEnvironmentProfile: null
    perSiteScaling: false
    elasticScaleEnabled: false
    maximumElasticWorkerCount: 1
    isSpot: false
    spotExpirationTime: null
    freeOfferExpirationTime: null
    reserved: false
    isXenon: false
    hyperV: false
    targetWorkerCount: 0
    targetWorkerSizeId: 0
    kubeEnvironmentProfile: null
    zoneRedundant: false
  }
  sku: {
    name: 'D1'
    tier: 'Shared'
    size: 'D1'
    family: 'D'
    capacity: 0
  }
}

resource filmowanie 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  kind: 'app'
  location: location
  tags: {}
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    enabled: true
    serverFarmId: ASPFilmowaniegroup.id
    hostNameSslStates: [
      {
        name: 'filmowanie2.azurewebsites.net'
        sslState: 'Disabled'
        virtualIP: null
        thumbprint: null
        toUpdate: null
        hostType: 'Standard'
      }
    ]
    reserved: false
    isXenon: false
    hyperV: false
    dnsConfiguration: {}
    vnetRouteAllEnabled: false
    vnetImagePullEnabled: false
    vnetContentShareEnabled: false
    siteConfig: {
      numberOfWorkers: 1
      linuxFxVersion: ''
      minimumElasticInstanceCount: 0
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsComponents.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsComponents.properties.ConnectionString
        }
      ]
    }
    clientCertMode: 'Required'
    containerSize: 0
    dailyMemoryTimeQuota: 0
    redundancyMode: 'None'
    publicNetworkAccess: 'Enabled'
    storageAccountRequired: false
    keyVaultReferenceIdentity: 'SystemAssigned'
  }
}
