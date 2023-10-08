# Сервер проверки подлинности и авторизации для приложения CityTalk.
Ссылка на CityTalk: https://github.com/v4dikos1/CityTalk

## Деплой приложения

### Docker
Для загрузки docker-образа, выполните следующую команду(пока не реализовано):
```
docker pull v4dikos/citytalkidentity:latest
```

### Docker-compose
Для запуска приложения, необходимо выполнить следующую команду(пока не реализовано):
```
docker-compose up -d
```
Для конфигурации сертфикатов для целей тестирования понадобится выполнить следующую команду:
```
dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p {{YOUR_PASSWORD}}
dotnet dev-certs https --trust
```

### Git
Для запуска приложения после загрузки из Git, понадобится настроить файл конфигурации.

### Структура appsettings.json (будет дополняться):
```json
{
  {
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "CityTalkIdentityContext": ""
  },
  "NeedSeedData": true,
  "Clients": {
    "IdentityServerClient": {
      "BaseUrl": "",
      "ClientSecret": "",
      "AccessTokenLifetime": ""
    },
    "CityTalkServerClient": {
      "BaseUrl": "",
      "ClientSecret": "",
      "AccessTokenLifetime": ""
    },
    "CityTalkSwaggerClient": {
      "BaseUrl": "",
      "ClientSecret": "",
      "AccessTokenLifetime": ""
    },
    "CityTalkWebJsClient": {
      "BaseUrl": "",
      "ClientSecret": "",
      "AccessTokenLifetime": ""
    },
    "CityTalkMobileClient": {
      "BaseUrl": "",
      "ClientSecret": "",
      "AccessTokenLifetime": ""
    }
  }
}
```
