# GPGCloudStorage

Overview

UnityでGoogle Play Game Servicesのクラウドセーブ・ロード機能を使うサンプル

## Requirement

Google Play Games plugin for Unity  Ver.0.9.34 or higher Required

[https://github.com/playgameservices/play-games-plugin-for-unity]

## インストール

GPGCloudStorage.csをAssets以下に配置します

## Usage

```
var gpg = new GPGCloudStorage();
gpg.GPG_CloudSave("fileName", "data_string", (bool success) => { ... });
gpg.GPG_CloudLoad("fileName", (string data) => { ... });
```

## 動作

実機でのみ動作します
