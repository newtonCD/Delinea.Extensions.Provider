[Read this in English](README.md)

# Delinea.Extensions.Provider
Implementa��o do provedor de configura��o Delinea Secret Vault para o Microsoft.Extensions.Configuration. Esta biblioteca permite que voc� leia as configura��es da sua aplica��o diretamente de um cofre de segredos da API do Delinea.

# Como Utilizar
Verifique o projeto de exemplo **Delinea.Example.Api**

## Para utilizar esta biblioteca, primeiramente � necess�rio que voc� tenha o ClientId e ClientSecret da sua aplica��o criada no ambiente do Delinea.

### Configura��o

- Edite as configura��es do Delinea ( DelineaSecretVaultSettings ) no arquivo appsettings.json
- Os itens de configura��o b�sicos e obrigat�rios s�o: **BaseAddress, ClientId, ClientSecret e SecretsBasePath**.
- O item de configura��o **SecretsBasePath** deve conter o caminho da sua aplica��o dentro do Delinea separando o caminho com dois pontos ":" . Por exemplo: "apps:company:app_name:sit"

### Armazenamento dos segredos

- No ambiente do Delinea, voc� dever� armazenar os segredos da sua aplica��o em formato JSON, id�ntico ao que � usado no User Secrets do .NET. Por exemplo, se voc� tem no appsettings.json da sua aplica��o algo como:

```
{
 "AzureAdB2E": {
    "Domain": "company.onmicrosoft.com",
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
  }
}
```

No formato do User Secrets ficar� assim:

```
{
  "AzureAdB2E:Domain": "company.onmicrosoft.com",
  "AzureAdB2E:Instance": "https://login.microsoftonline.com/",
  "AzureAdB2E:TenantId": "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
}
```

Ou voc� pode utilizar tamb�m no formato do secrets da AKS que � assim:

```
{
  "AzureAdB2E__Domain": "company.onmicrosoft.com",
  "AzureAdB2E__Instance": "https://login.microsoftonline.com/",
  "AzureAdB2E__TenantId": "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
}
```

### Usando o componente na sua aplica��o

Para plugar o componente do Delinea na sua aplica��o, basta usar o provedor de configura��o **"AddDelineaSecretVault"** passando como par�metro a cole��o de servi�os atual (builder.Services). Veja exemplo de uso no arquivo Program.cs da api de exemplo **Delinea.Example.Api**. Exemplo:
**builder.Configuration.AddDelineaSecretVault(builder.Services);**

### Atualiza��o autom�tica das Secrets

Internamente, o componente instanciar� uma thread que ficar� de tempos em tempos (o padr�o � 300 segundos) consultando o cofre do Delinea e atualizando os valores dos itens de configura��o, sem a necessidade de reiniciar a aplica��o. O valor do intervalo desse pooling pode se ajustado atrav�s do item de configura��o **DelineaSecretVaultSettings:PollingIntervalSeconds**.

### Observa��es

Uma exce��o do tipo **DelineaServiceException** ser� lan�ada quando os itens de configura��o de acesso � API do Delinea n�o forem informados, forem informados incorretamente ou quando houver problemas de comunica��o com a API do Delinea.