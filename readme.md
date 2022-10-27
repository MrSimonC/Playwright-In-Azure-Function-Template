# Template for Playwright .Net in a Linux Consumption Plan (Isolated) Azure Functions

A template repo to show you how to set up playwright .net in an azure function.

This requires a few things to work:

* Linux consumption plan
* .net 6.0 isolated azure function
* Deploy via a Service Principal (rather than zip-deploy publish profile) - this is so that the browser's executable permission bit is kept during deployment
* Github Actions to build the project, and include the browser in the deployment

**Note:** Last known working Playwright release: v1.18.1. It seems if you upgrade the version, doesn't publish to the published folder from v1.19.0. I've rasied [a bug](https://github.com/microsoft/playwright-dotnet/issues/2354) with the team ...

## Installation

* Create template from this repo
* Optionally rename project, solution
* Publish initially to Azure: Visual studio, Publish, Azure, Azure Function App (Linux), create new, next, CI/CD using GitHub Actions workflows
* Set up [Using Azure Service Principal for RBAC as Deployment Credential](https://github.com/Azure/functions-action#using-azure-service-principal-for-rbac-as-deployment-credential) (naming the secret AZURE_RBAC_CREDENTIALS)
* Edit the .github/.workflows/<yourProject>.yml:

Add Env variable:

```yaml
  # added for playwright: download playwright browser to our app's folder, not ~/.cache/ms-playwright
  PLAYWRIGHT_BROWSERS_PATH: <NameOfMyApp>/publish/.playwright/ms-playwright
```

before deployment, add playwright install:

```yaml
      # added for playwright
    - name: Playwright install
      shell: bash
      run: |
        dotnet tool install --global Microsoft.Playwright.CLI
        playwright install chromium
    - name: 'Login via Azure CLI'
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_RBAC_CREDENTIALS }}
```

To make the `Azure/functions-action` deploy using a service principal, find `publish-profile: ${{ secrets....` and comment it out/remove it.

Then, at the end, add:

```yaml
# added for playwright
    - name: App Settings Update
      uses: azure/appservice-settings@v1
      with:
        app-name: ${{ env.AZURE_FUNCTIONAPP_NAME }}
        mask-inputs: false
        app-settings-json: '[{ "name": "WEBSITE_MOUNT_ENABLED", "value": 1, "slotSetting": false }, { "name": "FUNCTIONS_WORKER_RUNTIME", "value": "dotnet-isolated", "slotSetting": false }, { "name": "PLAYWRIGHT_BROWSERS_PATH", "value": "/home/site/wwwroot/.playwright/ms-playwright", "slotSetting": false }]'
```