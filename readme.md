# Template for Playwright .Net in a Linux Consumption Plan (Isolated) Azure Functions

A template repo to show you how to set up playwright .net in an azure function.

This requires a few things to work:

* Linux consumption plan
* .net 6.0 isolated azure function (.net 7 update coming soon)
* Deploy via a Service Principal (rather than zip-deploy publish profile) - this is so that the browser's executable permission bit is kept during deployment
* Github Actions to build the project, and include the browser in the deployment

## Installation

* Create template from this repo
* Optionally rename project, solution
* Publish initially to Azure: Visual studio, Publish, Azure, Azure Function App (Linux), create new, next, CI/CD using GitHub Actions workflows
* Set up [Using Azure Service Principal for RBAC as Deployment Credential](https://github.com/Azure/functions-action#using-azure-service-principal-for-rbac-as-deployment-credential) (naming the secret AZURE_RBAC_CREDENTIALS)
* Edit the .github/.workflows/<yourProject>.yml:

Add Env variable:

```yaml
  # changed '/published' to '/publish' on this line
  AZURE_FUNCTIONAPP_PACKAGE_PATH: <NameOfMyApp>/publish
  # download playwright browser to the folder to be deployed, not ~/.cache/ms-playwright
  PLAYWRIGHT_BROWSERS_PATH: <NameOfMyApp>/publish/.playwright/ms-playwright
```

after `dotnet publish`, add playwright install:

```yaml
      # added for playwright. tar the directory as this keeps permission bits when publishing artifacts
    - name: Playwright install
      shell: bash
      run: |
        dotnet tool install --global Microsoft.Playwright.CLI
        playwright install chromium
        tar -cvf publish_directory.tar "${{ env.AZURE_FUNCTIONAPP_PACKAGE_PATH }}"
```

update `Publish Artifacts` to:

```yaml
    - name: Publish Artifacts
      uses: actions/upload-artifact@v1.0.0
      with:
        name: functionapp
        path: publish_directory.tar
```

In the `deploy` section, before `Deploy to Azure Function App`, add:

```yaml
    - name: Download artifact from build job
      uses: actions/download-artifact@v2
      with:
        name: functionapp
    - name: Restore artifact with permissions
      shell: bash
      run: |
        mkdir --parents ${{ env.AZURE_FUNCTIONAPP_PACKAGE_PATH }}
        tar xvf publish_directory.tar
    - name: 'Login via Azure CLI'
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_RBAC_CREDENTIALS }}
```

To make the `Azure/functions-action` deploy using a service principal, find `publish-profile: ${{ secrets....` and remove it.

Then, at the end, add:

```yaml
    # added for playwright
    - name: App Settings Update
      uses: azure/appservice-settings@v1
      with:
        app-name: ${{ env.AZURE_FUNCTIONAPP_NAME }}
        mask-inputs: false
        app-settings-json: '[{ "name": "WEBSITE_MOUNT_ENABLED", "value": 1, "slotSetting": false }, { "name": "FUNCTIONS_WORKER_RUNTIME", "value": "dotnet-isolated", "slotSetting": false }, { "name": "PLAYWRIGHT_BROWSERS_PATH", "value": "/home/site/wwwroot/.playwright/ms-playwright", "slotSetting": false }]'
    - uses: geekyeggo/delete-artifact@v2
      with:
          name: functionapp
```