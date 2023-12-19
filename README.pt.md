[Read this in English](README.md)

# Delinea.Extensions.Provider
Implementação do provedor de configuração Delinea Secret Vault para o Microsoft.Extensions.Configuration. Esta biblioteca permite que você leia as configurações da sua aplicação diretamente de um cofre de segredos da API do Delinea.

# Como Utilizar
Verifique o projeto de exemplo **Delinea.Example.Api**

## Para utilizar esta biblioteca, primeiramente é necessário que você tenha o ClientId e ClientSecret da sua aplicação criada no ambiente do Delinea.

### Configuração

- Edite as configurações do Delinea ( DelineaSecretVaultSettings ) no arquivo appsettings.json
- Os itens de configuração básicos e obrigatórios são: **BaseAddress, ClientId, ClientSecret e SecretsBasePath**.
- O item de configuração **SecretsBasePath** deve conter o caminho da sua aplicação dentro do Delinea separando o caminho com dois pontos ":" . Por exemplo: "apps:company:app_name:sit"

### Armazenamento dos segredos

- No ambiente do Delinea, você deverá armazenar os segredos da sua aplicação em formato JSON, idêntico ao que é usado no User Secrets do .NET. Por exemplo, se você tem no appsettings.json da sua aplicação algo como:

```
{
 "AzureAdB2E": {
    "Domain": "company.onmicrosoft.com",
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
  }
}
```

No formato do User Secrets ficará assim:

```
{
  "AzureAdB2E:Domain": "company.onmicrosoft.com",
  "AzureAdB2E:Instance": "https://login.microsoftonline.com/",
  "AzureAdB2E:TenantId": "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
}
```

Ou você pode utilizar também no formato do secrets da AKS que é assim:

```
{
  "AzureAdB2E__Domain": "company.onmicrosoft.com",
  "AzureAdB2E__Instance": "https://login.microsoftonline.com/",
  "AzureAdB2E__TenantId": "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
}
```

### Usando o componente na sua aplicação

Para plugar o componente do Delinea na sua aplicação, basta usar o provedor de configuração **"AddDelineaSecretVault"** passando como parâmetro a coleção de serviços atual (builder.Services). Veja exemplo de uso no arquivo Program.cs da api de exemplo **Delinea.Example.Api**. Exemplo:
**builder.Configuration.AddDelineaSecretVault(builder.Services);**

### Atualização automática das Secrets

Internamente, o componente instanciará uma thread que ficará de tempos em tempos (o padrão é 300 segundos) consultando o cofre do Delinea e atualizando os valores dos itens de configuração, sem a necessidade de reiniciar a aplicação. O valor do intervalo desse pooling pode se ajustado através do item de configuração **DelineaSecretVaultSettings:PollingIntervalSeconds**.

### Observações

Uma exceção do tipo **DelineaServiceException** será lançada quando os itens de configuração de acesso à API do Delinea não forem informados, forem informados incorretamente ou quando houver problemas de comunicação com a API do Delinea.