# AkaneFacialOSC の使い方 （VRChat 向け）
（一部翻訳中）
フェイストラッキングのデータを OSC を使って送信するアプリです．

VRChat で使用する場合，アニメーターやブレンドシェイプの知識がある程度必要です．

```
[トラッカ] -> [AkaneFacialOSC] -(OSC)-> [VRChat]
```

## アプリの使い方
1. 使う前に，SRanipal SDK をインストールして起動してください．
    - HTC Vive の開発者向け公式ウェブページからダウンロードすることができます
1. このアプリを実行します
    - 初回起動時に，「Windows セキュリティの重大な警告」が表示される場合があります
        - この場合，**プライベート ネットワークでのアクセスを許可してください**
        - これは，OSC は「ネットワークの機能を用いてデータを送信する」という仕組みのためです
        - このアプリは，ネットワークを使って PC の外にデータを送りません．このアプリが走っている PC の中でのみデータのやりとりをします

## アバタのセットアップ方法
### 概要 (技術者向け)
- このアプリは `/avatar/parameters/{key}` のアドレスに対して float でデータを送ります．
- `{key}` はアプリ内のチェックボックス横のテキストがそれです．

## 送信データ
このアプリでは，トラッカで取得した値だけでなく，アプリ内で計算してまとめたデータも送信しています．
これは，VRChat のパラメータは制限が厳しいため，その数の節約のための機能です．

元々のフェイストラッカ・アイトラッカのデータでは，顔の動きのデータが左右で分かれています．
また，`Mouth_Upper_UpRight` と `Mouth_Upper_UpLeft` のように同時に使われることのない値もあります．
この無駄を省くため，2 つのデータを組み合わせて 0.5 を基準としてそれ以上・それ以下で切り替えたり，左右のデータの平均を取ったデータも送るようにしています．

アバタで行いたい表情に合わせて値を使ってみてください．

### key の一覧
アプリで送信するパラメータは以下の通りです
- 目のデータ（SDK の計算方法と同様に計算した値）
    - Eye_Left_Blink
    - Eye_Left_Wide
    - Eye_Left_Right
    - Eye_Left_Left
    - Eye_Left_Up
    - Eye_Left_Down
    - Eye_Right_Blink
    - Eye_Right_Wide
    - Eye_Right_Right
    - Eye_Right_Left
    - Eye_Right_Up
    - Eye_Right_Down
    - Eye_Left_Frown
    - Eye_Right_Frown
    - Eye_Left_Squeeze
    - Eye_Right_Squeeze
- 視線データ（アプリ内で計算された値）
    - Gaze_Left_Vertical
    - Gaze_Left_Horizontal
    - Gaze_Right_Vertical
    - Gaze_Right_Horizontal
    - Gaze_Vertical
    - Gaze_Horizontal
    - Eye_Blink
    - Eye_Wide
    - Eye_Right
    - Eye_Left
    - Eye_Up
    - Eye_Down
    - Eye_Frown
    - Eye_Squeeze
- 顔（トラッカで取得した生の値）
    - Jaw_Right
    - Jaw_Left
    - Jaw_Forward
    - Jaw_Open
    - Mouth_Ape_Shape
    - Mouth_Upper_Right
    - Mouth_Upper_Left
    - Mouth_Lower_Right
    - Mouth_Lower_Left
    - Mouth_Upper_Overturn
    - Mouth_Lower_Overturn
    - Mouth_Pout
    - Mouth_Smile_Right
    - Mouth_Smile_Left
    - Mouth_Sad_Right
    - Mouth_Sad_Left
    - Cheek_Puff_Right
    - Cheek_Puff_Left
    - Cheek_Suck
    - Mouth_Upper_UpRight
    - Mouth_Upper_UpLeft
    - Mouth_Lower_DownRight
    - Mouth_Lower_DownLeft
    - Mouth_Upper_Inside
    - Mouth_Lower_Inside
    - Mouth_Lower_Overlay
    - Tongue_LongStep1
    - Tongue_LongStep2
    - Tongue_Down
    - Tongue_Up
    - Tongue_Right
    - Tongue_Left
    - Tongue_Roll
    - Tongue_UpLeft_Morph
    - Tongue_UpRight_Morph
    - Tongue_DownLeft_Morph
    - Tongue_DownRight_Morph
- 顔（アプリ内で計算・統合したデータ）
    - Mouth_Sad_Smile_Right
    - Mouth_Sad_Smile_Left
    - Mouth_Smile
    - Mouth_Sad
    - Mouth_Sad_Smile
    - Mouth_Upper_Left_Right
    - Mouth_Lower_Left_Right
    - Mouth_Left_Right
    - Cheek_Puff
    - Cheek_Suck_Puff

### 計算した値の説明
| キー                   | 中央の値 | 説明 | 
| ---------------------- | --- | --- |
| Mouth_Sad_Smile_Right  | 0.5 | 口角の上下移動．0.5 以下が sad，0.5 以上が smile．右半分の口の動きのみ |
| Mouth_Sad_Smile_Left   | 0.5 | 口角の上下移動．0.5 以下が sad，0.5 以上が smile．左半分の口の動きのみ  |
| Mouth_Smile            | 0   | smile の値の左右平均を取ったもの |
| Mouth_Sad              | 0   | sad の値の左右平均を取ったもの |
| Mouth_Sad_Smile        | 0.5 | 口角の上下移動．0.5 以下が sad，0.5 以上が smile．左右平均を取ったデータ． |
| Mouth_Upper_Left_Right | 0.5 | 上唇の左右方向の動き．0.5 以下で上唇を左に，0.5 以上で上唇を右に動かしたもの |
| Mouth_Lower_Left_Right | 0.5 | 下唇の左右方向の動き．0.5 以下で下唇を左に，0.5 以上で下唇を右に動かしたもの |
| Mouth_Left_Right       | 0.5 | 唇の左右方向の動き．0.5 以下で唇を左に，0.5 以上で唇を右に動かしたもの．上唇と下唇のデータの平均 |
| Cheek_Puff             | 0.5 | 頬を膨らませる動き．左右の平均 |
| Cheek_Suck_Puff        | 0.5 | 頬を吸い込んでへこませる動き．左右の平均 |