# Template for Playwright .Net in a Linux Consumption Plan (Isolated) Azure Functions

A template repo to show you how to set up playwright .net in an azure function.

This requires a few things to work:

* Linux consumption plan
* .net 6.0 isolated azure function
* Deploy via RBAC, rather than the standard built-in zip-deploy
  * This is so that the browser's executable permission bit is kept during deployment
* Github Actions to build the project, and include the browser in the deployment

## Installation

* Clone this repo
* Create your own `AZURE_CREDENTIALS` secret ([instructions here](https://github.com/marketplace/actions/azure-app-service-settings#configure-github-secrets-with-azure-credentials-app-settings-and-connection-strings))
* Update the github actions to reference your own app name

## Steps to recreate manually

1. In Visual Studio (for windows), add new Isolated Function
2. Push to new github repo
3. Publish to Azure (Linux consumption), choose github actions CI/CD
4. Add playwright code & nuget reference
5. Add AZURE_CREDENTIALS to github secrets:
* Install Azure CLI
  * In windows git bash, add MSYS_NO_PATHCONV=1 in computer environment variables ([see issue](https://github.com/Azure/azure-cli/issues/16317))
* Generate a service principle (contributor) in your subscription & add to github secrets ([instructions](https://github.com/marketplace/actions/azure-app-service-settings#configure-github-secrets-with-azure-credentials-app-settings-and-connection-strings))
6. Update the auto-generated github actions .yml file with:

```yaml
env:
    # If targeting .net 6, Playwright needs .net 5:
    DOTNET_CORE_VERSION_PLAYWRIGHT: 5.0.x
    # download playwright browser to our app's folder, not ~/.cache/ms-playwright
    PLAYWRIGHT_BROWSERS_PATH: NameOfMyApp/publish/.playwright/ms-playwright

jobs:
    # If targeting .net 6, Playwright needs .net 5:
    - name: Setup .NET Core for Playwright
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: ${{ env.DOTNET_CORE_VERSION_PLAYWRIGHT }}
    
    # add after publish step:
    - name: Playwright install
      shell: bash
      run: |
        dotnet tool install --global Microsoft.Playwright.CLI
        playwright install chromium
    - name: Login via Azure CLI
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
    
    # In the "Deploy to Azure Function App" step:
    # remove "publish-profile"
    
    # add after deploy step:
    - uses: azure/appservice-settings@v1
      with:
        app-name: ${{ env.AZURE_FUNCTIONAPP_NAME }}
        mask-inputs: false
        app-settings-json: '[{ "name": "WEBSITE_MOUNT_ENABLED", "value": 1, "slotSetting": false }, { "name": "FUNCTIONS_WORKER_RUNTIME", "value": "dotnet-isolated", "slotSetting": false }, { "name": "PLAYWRIGHT_BROWSERS_PATH", "value": "/home/site/wwwroot/.playwright/ms-playwright", "slotSetting": false }]'
```