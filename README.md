---
layout:
  title:
    visible: true
  description:
    visible: false
  tableOfContents:
    visible: true
  outline:
    visible: true
  pagination:
    visible: true
---

# 🛠️ Settings

## Clients

### Telegram Bot

| Id      | Source                                  | Description       |
| ------- | --------------------------------------- | ----------------- |
| ApiId   | [My Telegram](https://my.telegram.org/) |                   |
| ApiHash | [My Telegram](https://my.telegram.org/) |                   |
| Token   | [BotFather](https://t.me/BotFather)     | Token of your bot |

```json
{
    "Bot": {
        "ApiId": 1234567890,
        "ApiHash": "ApiHash",
        "Token": "Token"
    }
}
```

***

## Services

### General

#### Multithreading

| Id     | Source | Description                   |
| ------ | ------ | ----------------------------- |
| Proxy  |        | Max threads for check proxies |
| Parser |        | Max threads for parsing       |

```json
{
    "Multithreading": {
        "Proxy": 100,
        "Parser": 100
    }
}
```

#### Files

| Id   | Source | Description                     |
| ---- | ------ | ------------------------------- |
| Root |        | Root directory of project files |

```json
{
    "Files": {
        "Root": "/home/username/.logs-handler"
    }
}
```

### Payments

#### CryptoPay

| Id     | Source                                                                   | Description |
| ------ | ------------------------------------------------------------------------ | ----------- |
| Token  | [Mainnet](https://t.me/send) or [Testnet](https://t.me/CryptoTestnetBot) |             |
| ApiUrl | [Mainnet](https://t.me/send) or [Testnet](https://t.me/CryptoTestnetBot) |             |

```json
{
    "CryptoPay": {
        "Token": "Token",
        "ApiUrl": "ApiUrl"
    }
}
```

### Services

#### Proxies

| Id           | Source | Description                                                   |
| ------------ | ------ | ------------------------------------------------------------- |
| OnInCheck    |        | Check proxies on adding to database                           |
| OnOutCheck   |        | Check proxies on getting from database                        |
| CheckUrl     |        | <p>Url for check proxies working.<br>* Require 200 on GET</p> |
| CheckTimeout |        | CheckUrl request timeout                                      |

```json
{
    "Proxy": {
        "OnInCheck": false,
        "OnOutCheck": true,
        "CheckUrl": "https://api.ipify.org",
        "CheckTimeout": 2000
    }
}
```

## Tests

### Proxies

| Id      | Source                                           | Description                                                                                    |
| ------- | ------------------------------------------------ | ---------------------------------------------------------------------------------------------- |
| Valid   | Your valid proxies, written in different formats | <p>List of valid proxies for <br>Test: string to Proxy convertion<br>Test: proxies checker</p> |
| Invalid | Yourself created invalid proxies                 | List of invalid proxies                                                                        |

{% hint style="info" %}
Supported proxy formats:

* host:port:username:password
* username:password:host:port
* host:port@username:password
* username:password@host:port
{% endhint %}

```json
{
    "Proxies": {
        "Valid": [
            "1.1.1.1:80:username:password",
            "username:password:1.1.1.1:80",
            "1.1.1.1:80@username:password",
            "username:password@1.1.1.1:80"
        ],
        "Invalid":[
            "qwe:12345:qwe:qwe",
            "qwe:qwe:12345:qwe",
            "qwe:12345@qwe:qwe",
            "qwe:qwe@qwe:12345"
        ]
    }
}
```

### Discord

| Id      | Source | Description                                            |
| ------- | ------ | ------------------------------------------------------ |
| Logs    |        | Directory with logs which contains 'Discord' directory |
| Log     |        | Directory with 'Discord' directory                     |
| Proxy   |        | Proxy for checking tokens                              |
| Valid   |        | List of valid tokens                                   |
| Invalid |        | List of invalid tokens                                 |

{% hint style="info" %}
Directory searching case insensitive
{% endhint %}

```json
{
    "Discord": {
        "Logs": "/home/username/.logs-handler/Extracted/logs",
        "Log": "/home/username/.logs-handler/Extracted/logs/some-log",
        "Proxy": "1.1.1.1:80:username:password",
        "Valid": [
            "Token"
        ],
        "Invalid": [
            "Token"
        ]
    }
}
```

### Twitch

| Id   | Source | Description                                            |
| ---- | ------ | ------------------------------------------------------ |
| Logs |        | Directory with logs which contains 'Cookies' directory |
| Log  |        | Directory with 'Cookies' directory                     |

{% hint style="info" %}
Directory searching case insensitive
{% endhint %}

```json
{
    "Discord": {
        "Logs": "/home/username/.logs-handler/Extracted/logs",
        "Log": "/home/username/.logs-handler/Extracted/logs/some-log"
    }
}
```
