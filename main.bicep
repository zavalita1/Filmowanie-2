// input by pipeline
param env string = 'Undefined'


var appServicePlanName = toLower('AppServicePlan-${webAppName}')
var appInsightsName = toLower('appins-${webAppName}')
var serviceBusName = toLower('sb-${webAppName}')
var dbAccountName = toLower('dba-${webAppName}')
var dbName = toLower('db-${webAppName}')
var keyVaultName = toLower('kv-${webAppName}')

var environments = {
  Development: {
    webAppName: 'filmowanie2'
  }
}

var webAppName = environments[env].webAppName

param tenantId string = subscription().tenantId
param location string = resourceGroup().location

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

resource appInsightsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  location: location
  name: 'appsworkspace${webAppName}'
  properties: {
    sku: {
      name: 'PerGB2018'
    }
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
    RetentionInDays: 30
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    WorkspaceResourceId: appInsightsWorkspace.id
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

resource serviceBus 'Microsoft.ServiceBus/namespaces@2023-01-01-preview' = {
  location: location
  name: serviceBusName
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    minimumTlsVersion: '1.2'
  }
}

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: dbAccountName
  kind: 'GlobalDocumentDB'
  location: location
  properties: {
    enableFreeTier: true
    consistencyPolicy: {
      defaultConsistencyLevel: 'ConsistentPrefix'
    }
    locations: [{
      failoverPriority: 0
      isZoneRedundant: false
      locationName: location
    }]
    databaseAccountOfferType: 'Standard'
    enableAutomaticFailover: false
  }
}

resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: cosmosDbAccount
  name: dbName
  properties: {
    resource: {
      id: dbName
    }
    options: {
      throughput: 800
    }
  }
}

resource cosmosDbMoviesContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: cosmosDb
  name: 'Entities'
  location: location
  properties: {
    resource: {
      id: 'Entities'
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
      }
      indexingPolicy: {
        automatic: true // maybe change later?
      }
      partitionKey: {
        kind: 'Hash'
        paths: ['/id']
      }
    }
  }
}

resource cosmosDbMoviesContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: cosmosDb
  name: 'Events'
  location: location
  properties: {
    resource: {
      id: 'Events'
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
      }
      indexingPolicy: {
        automatic: true // maybe change later?
      }
      partitionKey: {
        kind: 'Hash'
        paths: ['/id']
      }
    }
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2024-04-01-preview' = {
  location: location
  name: keyVaultName
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenantId
    enabledForTemplateDeployment: true
    accessPolicies: []
  }
}

resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2024-04-01-preview' = {
  parent: keyVault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        tenantId: tenantId
        objectId: filmowanie.identity.principalId
        permissions: {
          keys: ['get']
          secrets: ['get', 'list']
        }
      }
    ]
  }
}

// resource application 'Microsoft.Graph/applications@1.0' = {
//   displayName: 'Microsoft Graph App for filmowanie'
//   uniqueName: 'sp-filmowanie'
// }

// resource resourceSp 'Microsoft.Graph/servicePrincipals@v1.0' = {
//   appId: application.appId
//   displayName: 'Service Principal for filmowanie'
// }

// resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   scope: filmowanie
//   name: guid(filmowanie.id, 'sprole')
//   properties: {
//     roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c') // contributor role
//     principalId: resourceSp.id
//     principalType: 'ServicePrincipal'
//   }
// }

// output roleAssigmentName string = roleAssignment.name
// output roleAssignmentId string = roleAssignment.id
