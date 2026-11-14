# Hukidashi

[![Build and Release](https://github.com/denpadokei/Hukidashi/actions/workflows/build-and-release.yml/badge.svg)](https://github.com/denpadokei/Hukidashi/actions/workflows/build-and-release.yml)

ゲーム内に吹き出し (フキダシ) を表示し、外部ツールから送られたコメントを配信画面に出すための MOD です。ローカルに WebSocket サーバーを立ててテキストを受け取ります。

## 機能

- `ws://127.0.0.1:<ModPort>` (既定 4443) で WebSocket サーバーを起動
- 受信した JSON の `text` を吹き出し画像の上に表示し、`type` が `Show` のときだけ吹き出しを表示
- メニューとプレイ中の両方で動作し、シーンに応じて位置と大きさを個別に設定可能
- 吹き出しはカメラの子として配置され、指定したカメラ (空欄なら HMD) の方向を向く
- シーン遷移時に表示先カメラを自動で探し直す
- 設定変更は即座に反映 (BSIPA の設定ファイル変更を検知)

## 動作環境

| 項目 | 内容 |
| --- | --- |
| Beat Saber | 1.21.0 |
| 依存 MOD | BSIPA 4.2.0 / websocket-sharp 1.0.0 / BeatSaberMarkupLanguage 1.5.0 / SiraUtil 3.0.5 |

## インストール

1. Beat Saber を 1.21.0 に合わせる (BSManager などのバージョン管理ツールを推奨)
2. BSIPA と上記の依存 MOD をインストールする
3. [Releases](https://github.com/denpadokei/Hukidashi/releases) から `Hukidashi-2.0.0-bs1.21.0.zip` をダウンロードし、`Plugins` フォルダをゲームフォルダへ上書きコピーする

## 使い方

1. [BeatSaberConnector](https://github.com/denpadokei/BeatSaberConnector/releases/latest) をインストールして起動する
2. ゲームを起動すると `ws://127.0.0.1:4443` で待ち受けが始まる
3. BeatSaberConnector から次の形式の JSON を送ると吹き出しが表示される

```json
{ "type": "Show", "text": "表示するテキスト" }
```

`type` が `Show` 以外の場合はテキストだけ更新され、吹き出しは非表示になります。

## 設定

| 項目 | 型 | 既定値 | 説明 |
| --- | --- | --- | --- |
| MenuHukidashiScale | float | 0.03 | メニューでの吹き出しの大きさ |
| MenuHukidashiPosX | float | 5 | HMD 基準の左右位置。1 増えると 1m 動く |
| MenuHukidashiPosY | float | 0.3 | HMD 基準の上下位置。1 増えると 1m 動く |
| MenuHukidashiPosZ | float | 1 | HMD 基準の前後位置。1 増えると 1m 動く |
| MenuTargetCameraName | string | cameraplus.json | 吹き出しが向くカメラのオブジェクト名。空にすると HMD を向く |
| GameHukidashiScale | float | 0.03 | プレイ中の吹き出しの大きさ |
| GameHukidashiPosX | float | 5 | HMD 基準の左右位置。1 増えると 1m 動く |
| GameHukidashiPosY | float | 0.3 | HMD 基準の上下位置。1 増えると 1m 動く |
| GameHukidashiPosZ | float | 1 | HMD 基準の前後位置。1 増えると 1m 動く |
| GameTargetCameraName | string | cameraplus.json | 吹き出しが向くカメラのオブジェクト名。空にすると HMD を向く |
| ModPort | int | 4443 | WebSocket サーバーが受け付けるポート。基本的に変更する必要はない |

設定は `UserData/Hukidashi.json` に保存されます。ゲーム内に設定画面はないため、このファイルを直接編集してください。

## ビルド

```powershell
git clone https://github.com/denpadokei/Hukidashi.git
cd Hukidashi
dotnet build .\Hukidashi\Hukidashi.csproj -c Release
```

`BeatSaberDir` (または `GameDirectory`) に Beat Saber のインストール先を設定すると、ゲームの参照アセンブリを解決できます。既定では `..\Refs` フォルダが参照先になります。

GitHub Actions の `build-and-release.yml` が push ごとにビルドし、`X.X.X` 形式のタグを push するとリリースを作成します。

## ライセンス

MIT
