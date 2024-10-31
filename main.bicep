param webAppName string = 'filmowanie-2'
param location string = resourceGroup().location
param sku string = 'D1'
param linuxFxVersion string = 'DOTNET|8.0' 
param repositoryUrl string = 'https://github.com/zavalita1/Filmowanie-2'
param branch string = 'main'

var appServicePlanName = toLower('AppServicePlan-${webAppName}')
var webSiteName = toLower('wapp-${webAppName}')

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: 'filmowanie-2'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  properties: {
    reserved: true
  }
  sku: {
    name: sku
  }
  kind: 'linux'
}

resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: webSiteName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: linuxFxVersion
    }
  }
}

resource srcControls 'Microsoft.Web/sites/sourcecontrols@2023-12-01' = {
  parent: appService
  name: 'web'
  properties: {
    repoUrl: repositoryUrl
    branch: branch
    isManualIntegration: true
  }
}
