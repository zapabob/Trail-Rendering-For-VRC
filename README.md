# ArmTrailRenderer

ArmTrailRendererは、VRChatのモジュラーアバター用に開発されたUnityスクリプトで、アバターの腕の動きに追従するトレイルエフェクトを生成します。このスクリプトは、カスタマイズ可能なトレイルエフェクトとオーディオリアクティブ機能を提供します。

## 特徴

* アバターの腕の動きに追従するトレイルエフェクト
* トレイルの長さ、幅、色をカスタマイズ可能
* FFTスペクトラムデータを利用したオーディオリアクティブ機能
* VRChatのモジュラーアバター向けに最適化
* エラーハンドリングと例外処理による堅牢性

## 必要条件

* Unity 2019.4以降
* VRChat SDK3 for Avatars
* Animatorコンポーネントとエクスプレッションパラメータが正しく設定されたモジュラーアバター

## インストール方法

1. 最新のArmTrailRendererパッケージを[リリースページ](https://github.com/your-username/ArmTrailRenderer/releases)からダウンロードします。
2. Unityプロジェクトにパッケージをインポートします。
3. `ArmTrailRenderer`プレハブをアバターのヒエラルキーにドラッグ＆ドロップします。
4. アバターのルートオブジェクトを`ArmTrailRenderer`コンポーネントの`Avatar Setup`セクションにドラッグ＆ドロップします。
5. アバターをVRChatにアップロードします。

## 使用方法

1. VRChatでアバターを選択します。
2. アバターの腕を動かすと、トレイルエフェクトが表示されます。
3. トレイルエフェクトはオーディオ入力に反応します。

## カスタマイズ

* `ArmTrailRenderer`コンポーネントのインスペクターでトレイルの設定を調整することで、トレイルエフェクトの外観や動作をカスタマイズできます。
* `ArmTrailRenderer`スクリプトを編集して、機能を追加したり、エフェクトをさらにカスタマイズしたりすることができます。

## トラブルシューティング

問題が発生した場合は、以下の点を確認してください：

* `ArmTrailRenderer`コンポーネントのインスペクターで、すべての必要なコンポーネントと参照が正しく設定されていることを確認します。
* Unityコンソールでエラーメッセージや例外がないか確認します。スクリプトにはエラーハンドリングが含まれており、発生した問題はログに記録されます。
* アバターのAnimatorとエクスプレッションパラメータが正しく設定されていることを確認します。
* 問題が解決しない場合は、[イシューを作成](https://github.com/your-username/ArmTrailRenderer/issues)して、問題の詳細と再現手順を提供してください。

## 貢献

ArmTrailRendererプロジェクトへの貢献を歓迎します！バグ報告、機能リクエスト、改善のためのプルリクエストを[イシュー](https://github.com/your-username/ArmTrailRenderer/issues)または[プルリクエスト](https://github.com/your-username/ArmTrailRenderer/pulls)として提出してください。

## ライセンス

このプロジェクトは[MITライセンス](LICENSE)の下で公開されています。

---

English version:

# ArmTrailRenderer

ArmTrailRenderer is a Unity script developed for modular avatars in VRChat that generates a trail effect following the avatar's arm movements. This script provides a customizable trail effect and audio-reactive features.

## Features

* Trail effect that follows the avatar's arm movements
* Customizable trail length, width, and color
* Audio-reactive features using FFT spectrum data
* Optimized for modular avatars in VRChat
* Robustness with error handling and exception handling

## Requirements

* Unity 2019.4 or later
* VRChat SDK3 for Avatars
* Modular avatar with properly configured Animator component and expression parameters

## Installation

1. Download the latest ArmTrailRenderer package from the [releases page](https://github.com/your-username/ArmTrailRenderer/releases).
2. Import the package into your Unity project.
3. Drag and drop the `ArmTrailRenderer` prefab into your avatar's hierarchy.
4. Drag and drop the avatar's root object into the `Avatar Setup` section of the `ArmTrailRenderer` component.
5. Upload your avatar to VRChat.

## Usage

1. Select your avatar in VRChat.
2. Move the avatar's arms, and the trail effect will be displayed.
3. The trail effect reacts to audio input.

## Customization

* Customize the appearance and behavior of the trail effect by adjusting the trail settings in the inspector of the `ArmTrailRenderer` component.
* Modify the `ArmTrailRenderer` script to add functionality or further customize the effect.

## Troubleshooting

If you encounter any issues, please check the following:

* Ensure that all required components and references are properly set in the inspector of the `ArmTrailRenderer` component.
* Check the Unity console for any error messages or exceptions. The script includes error handling and will log any issues encountered.
* Verify that your avatar's Animator and expression parameters are set up correctly.
* If the issue persists, please [open an issue](https://github.com/your-username/ArmTrailRenderer/issues) with a detailed description of the problem and steps to reproduce it.

## Contributing

Contributions to the ArmTrailRenderer project are welcome! Please submit bug reports, feature requests, or pull requests for improvements as [issues](https://github.com/your-username/ArmTrailRenderer/issues) or [pull requests](https://github.com/your-username/ArmTrailRenderer/pulls).

## License

This project is released under the [MIT License](LICENSE).

