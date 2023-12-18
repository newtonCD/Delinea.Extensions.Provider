# Delinea.Extensions.Provider
Delinea Secret Vault configuration provider implementation for Microsoft.Extensions.Configuration. This library enables you to read your application's settings directly from Delinea API.

# How to use
Check the example project **Delinea.Example.Api**

## To use this library, first you need to have the ClientId and ClientSecret of your application created in the Delinea Secret Vault.

### Configuration

- Edit the Delinea settings (DelineaSecretVaultSettings) in the appsettings.json file.
- The basic and mandatory configuration items are: **BaseAddress, ClientId, ClientSecret e SecretsBasePath**.
- The **SecretsBasePath** configuration item should contain the path of your application within Delinea, separating the path with colons ( : ). For example: **apps:company:app_name:sit**

### Secrets Storage

- In the Delinea environment, you should store your application's secrets in JSON format, identical to what is used in the .NET User Secrets. For example, if you have something like this in your application's appsettings.json:

```
{
 "AzureAdB2E": {
    "Domain": "company.onmicrosoft.com",
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
  }
}
```

In the User Secrets format, it will look like this:

```
{
  "AzureAdB2E:Domain": "company.onmicrosoft.com",
  "AzureAdB2E:Instance": "https://login.microsoftonline.com/",
  "AzureAdB2E:TenantId": "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
}
```

Or you can also use it in the AKS secrets format like this:

```
{
  "AzureAdB2E__Domain": "company.onmicrosoft.com",
  "AzureAdB2E__Instance": "https://login.microsoftonline.com/",
  "AzureAdB2E__TenantId": "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
}
```

### Using the Extension in Your Application

To use the Delinea extension into your application, just use the configuration provider **"AddDelineaSecretVault"** passing the current service collection (builder.Services) as a parameter. See usage example in the Program.cs file of the example API **Delinea.Example.Api**. Example:
**builder.Configuration.AddDelineaSecretVault(builder.Services);**

### Automatic Configuration Update

Internally, the component will instantiate a thread that will periodically (default is 300 seconds) consult the Delinea vault and update the configuration item values, without the need to restart the application. The interval of this polling can be adjusted through the configuration item **DelineaSecretVaultSettings:PollingIntervalSeconds**.

### Exceptions

A **DelineaServiceException** will be thrown when the configuration items for accessing the Delinea API are not provided, are provided incorrectly, or when there are communication problems with the Delinea API.