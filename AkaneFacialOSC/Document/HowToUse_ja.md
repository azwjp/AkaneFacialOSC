# AkaneFacialOSC の使い方 （VRChat 向け）

AkaneFacialOSC はフェイストラッキングのデータを OSC を使って送信するアプリです．

VRChat で使用する場合，アニメーターやブレンドシェイプの知識がある程度必要です．

```
[トラッカ] -> [AkaneFacialOSC] -(OSC)-> [VRChat]
```

## 機能
- トラッカのデータを OSC を経由して送信します
- アバタのパラメータを節約するため，集計してまとめた値も送信することができます
- Vive Facial Tracker, Vive Pro Eye, Pimax Droolon P1 に対応しています

## アプリの使い方
### 事前準備
- Vive Facial Tracker と Vive Pro Eye を使う場合は，**SRanipal Runtime** をインストールして起動する必要があります
    - HTC Vive の開発者向け公式ウェブページからダウンロードすることができます
    - https://developer-express.vive.com/resources/vive-sense/eye-and-facial-tracking-sdk/download/latest/
- Pimax Droolon Pi1 を使う場合は **aSee VR Runtime** のインストールが必要です
    - アイトラッカが接続されていれば Pitool からインストールができます
- **.NET デスクトップ ランタイム 6.0** のインストールが必要です
    - Microsoft の公式ページからダウンロードしてください
    - https://dotnet.microsoft.com/ja-jp/download/dotnet/6.0

### 起動方法
1. SteamVR を起動します
1. SRAnipal Runtime (Vive 製品を使う場合) や aSee VR Runtime (Pimax を使用する場合) を起動します
1. このアプリを実行します
    - 初回起動時に，「Windows セキュリティの重大な警告」が表示される場合があります
        - この場合，**プライベート ネットワークでのアクセスを許可してください**
        - これは，OSC は「ネットワークの機能を用いてデータを送信する」という仕組みのためです
        - このアプリは，ネットワークを使って PC の外にデータを送りません．このアプリが走っている PC の中でのみデータのやりとりをします（ただし，アップデートの確認のためにウェブページへのアクセスが起動時に一度実行されます）

SteamVR と SRAnipal Runtime を先に起動した方が安定して開始できると思います．
VRChat とこのアプリはどちらを先に起動しても問題ありません．

## アバタのセットアップ方法
### 技術者向け情報
- このアプリは `/avatar/parameters/{key}` のアドレスに対して float でデータを送ります．
- `{key}` はアプリ内のチェックボックス横のテキストがそれです．
- 基本的に値は `[0, 1]` の間で送信します
    - 中央の値を変更できる項目は，`[-1, 1]` に変更できます

### 設定方法
1. 動かしたい顔の部位のブレンドシェイプが入っているアバタを用意します
    - 目のボーン（視線トラッキング）のみをする場合は必要ありません
1. `Expression Parameters` に動かしたい部位に対応する `{key}` を追加します
    - すべての値は float で追加する必要があります．それぞれ 8 のメモリを消費します．
1. `Animator Parameter` に `{key}` と同じ値を入力します
    - VRChat の制約として
        - ブレンドシェイプを動かす場合は `FX` レイヤに入れる必要があります
        - ボーンを動かす場合は `FX` 以外のレイヤに入れます
1. ブレンドシェイプを動かすためのアニメーションを作成します
1. アニメータにアニメーションを追加します
    - 一般的には Motion Time やブレンドツリーを使用します

### 送信データ
このアプリでは，トラッカで取得した値だけでなく，アプリ内で計算してまとめたデータも送信しています．
これは，VRChat のパラメータは制限が厳しいため，その数の節約のための機能です．

元々のフェイストラッカ・アイトラッカのデータでは，顔の動きのデータが左右で分かれています．
また，`Mouth_Upper_UpRight` (上唇を左に動かす) と `Mouth_Upper_UpLeft` (上唇を右に動かす) のように同時に使われることのない値もあります．
この無駄を省くため，2 つのデータを組み合わせて 0.5 を基準としてそれ以上・それ以下で切り替えたり，左右のデータの平均を取ったデータも送るようにしています．

アバタで行いたい表情に合わせて値を使ってみてください．

## key の一覧

### 目の周りのデータ （SDK の計算方法と同様に計算した値）
| キー                   | デフォルトの値 | 最小値 | 最大値 |  説明 | 
| ---------------------- | --- | --- | --- | --- |
| Eye_Left_Blink         | 0   | 自然な状態 | 左目を閉じる | 左目のまばたき |
| Eye_Left_Wide          | 0   | 自然な状態 | 眉が上がって左目を大きく開ける | 目を大きく見開いた状態．eye_wide の値． |
| Eye_Left_Right         | 0   | 自然な状態 | 右を見ようとして左目の右側に力を入れる | 瞳孔の位置が中央より右にあるときに送信される |
| Eye_Left_Left          | 0   | 自然な状態 | 左を見ようとして左目の左側に力を入れる | 瞳孔の位置が中央より左にあるときに送信される |
| Eye_Left_Up            | 0   | 自然な状態 | 上を見ようとしてうわまぶたが上がる | Eye_Left_Wide と全く同一の値が送信される |
| Eye_Left_Down          | 0   | 自然な状態 | 左眉やうわまぶた，下まぶたが下がり，やや目の開き方が小さくなる |  目が中央より下に向いている時に送信される．vrc.lookingdown のイメージ |
| Eye_Right_Blink        | 0   | 自然な状態 | 右目を閉じる | 右目のまばたき |
| Eye_Right_Wide         | 0   | 自然な状態 | 眉が上がって右目を大きく開ける | 目を大きく見開いた状態．eye_wide の値． |
| Eye_Right_Right        | 0   | 自然な状態 | 右を見ようとして右目の右側に力を入れる | 瞳孔の位置が中央より右にあるときに送信される |
| Eye_Right_Left         | 0   | 自然な状態 | 左を見ようとして右目の左側に力を入れる | 瞳孔の位置が中央より左にあるときに送信される |
| Eye_Right_Up           | 0   | 自然な状態 | 上を見ようとして右目のうわまぶたが上がる | Eye_Left_Wide と全く同一の値が送信される |
| Eye_Right_Down         | 0   | 自然な状態 | 右眉やうわまぶた，下まぶたが下がり，やや目の開き方が小さくなる |  目が中央より下に向いている時に送信される．vrc.lookingdown のイメージ |
| Eye_Left_Frown         | 0   | 自然な状態 | 左目をしかめて顔の中央に力を入れる | eye_frown の値
| Eye_Right_Frown        | 0   | 自然な状態 | 右目をしかめて顔の中央に力を入れる | eye_frown の値
| Eye_Left_Squeeze       | 0   | 自然な状態 | 左眉をしかめて下に押し下げる | eye_squeeze の値
| Eye_Right_Squeeze      | 0   | 自然な状態 | 右眉をしかめて下に押し下げる | eye_squeeze の値

### 視線（アプリ内で計算された値）
| キー                   | デフォルトの値 | 最小値 | 最大値 |  説明 | 
| ---------------------- | --- | --- | --- | --- |
| Gaze_Left_Vertical    | 0.5 (0) | 左目が下を向く | 左目が上を向く | 範囲を `[0, 1]` と `[-1, 1]` の間で変更可能 |
| Gaze_Left_Horizontal  | 0.5 (0) | 左目が左を向く | 左目が右を向く | 範囲を `[0, 1]` と `[-1, 1]` の間で変更可能 |
| Gaze_Right_Vertical   | 0.5 (0) | 右目が下を向く | 右目が上を向く | 範囲を `[0, 1]` と `[-1, 1]` の間で変更可能 |
| Gaze_Right_Horizontal | 0.5 (0) | 右目が左を向く | 右目が右を向く | 範囲を `[0, 1]` と `[-1, 1]` の間で変更可能 |
| Gaze_Vertical         | 0.5 (0) | 両目が下を向く | 両目が上を向く | 範囲を `[0, 1]` と `[-1, 1]` の間で変更可能．左右の目の平均 |
| Gaze_Horizontal       | 0.5 (0) | 両目が左を向く | 両目が右を向く | 範囲を `[0, 1]` と `[-1, 1]` の間で変更可能．左右の目の平均 |

### 目（計算処理済みの値）
| キー                   | デフォルトの値 | 最小値 | 最大値 |  説明 | 
| ---------------------- | --- | --- | --- | --- |
| Eye_Blink   | 0 | 自然な状態 | 両目を閉じる | Eye_Left_Blink と Eye_Right_Blink の平均 |
| Eye_Wide    | 0 | 自然な状態 | 眉が上がって両目を大きく開ける | Eye_Left_Wide と Eye_Right_Wide の平均 |
| Eye_Right   | 0 | 自然な状態 | 右を見ようとして両目の右側に力を入れる | Eye_Left_Right と Eye_Right_Right の平均 |
| Eye_Left    | 0 | 自然な状態 | 左を見ようとして両目の左側に力を入れる | Eye_Left_Left と Eye_Right_Left の平均 |
| Eye_Up      | 0 | 自然な状態 | 上を見ようとしてうわまぶたが上がる | Eye_Left_Up と Eye_Right_Up の平均 |
| Eye_Down    | 0 | 自然な状態 | 眉やうわまぶた，下まぶたが下がり，やや目の開き方が小さくなる | Eye_Left_Down と Eye_Right_Down の平均 |
| Eye_Frown   | 0 | 自然な状態 | 両目をしかめて顔の中央に力が入る | Eye_Left_Frown と Eye_Right_Frown の平均． |
| Eye_Squeeze | 0 | 自然な状態 | 両眉をしかめて下に押し下げる | Eye_Left_Squeeze と Eye_Right_Squeeze の平均． |

### 顔（トラッカで取得した生の値）
| キー                   | デフォルトの値 | 最小値 | 最大値 |  説明 | 
| ---------------------- | --- | --- | --- | --- |
| Jaw_Right              | 0 | 自然な状態 | 顎を右に動かす | Jaw_Left と同時に 1 に設定した場合は元の状態に戻る |
| Jaw_Left               | 0 | 自然な状態 | 顎を左に動かす | Jaw_Right と同時に 1 に設定した場合は元の状態に戻る |
| Jaw_Forward            | 0 | 自然な状態 | 顎を前に突き出す |  |
| Jaw_Open               | 0 | 自然な状態 | 顎を開く |  |
| Mouth_Ape_Shape        | 0 | 自然な状態 | 口を閉じたまま顎を開く |  |
| Mouth_Upper_Right      | 0 | 自然な状態 | 口を閉じたまま上唇を右に動かす | Mouth_Upper_Left と同時に 1 に設定した場合は元の状態に戻る |
| Mouth_Upper_Left       | 0 | 自然な状態 | 口を閉じたまま上唇を左に動かす | Mouth_Upper_Right と同時に 1 に設定した場合は元の状態に戻る |
| Mouth_Lower_Right      | 0 | 自然な状態 | 口を閉じたまま下唇を右に動かす | Mouth_Lower_Left と同時に 1 に設定した場合は元の状態に戻る |
| Mouth_Lower_Left       | 0 | 自然な状態 | 口を閉じたまま下唇を左に動かす | Mouth_Lower_Right と同時に 1 に設定した場合は元の状態に戻る |
| Mouth_Upper_Overturn   | 0 | 自然な状態 | 口を閉じたまま上唇を突き出す |  |
| Mouth_Lower_Overturn   | 0 | 自然な状態 | 口を閉じたまま下唇を突き出す |  |
| Mouth_Pout             | 0 | 自然な状態 | 口を閉じたまま口をすぼめる | vrc.v_ou 用のシェイプでも代用できなくはないが，口の開き方が意図しない形になる可能性あり |
| Mouth_Smile_Right      | 0 | 自然な状態 | 口を閉じたまま右側の口角を上げる |  |
| Mouth_Smile_Left       | 0 | 自然な状態 | 口を閉じたまま左側の口角を上げる |  |
| Mouth_Sad_Right        | 0 | 自然な状態 | 口を閉じたまま右側の口角を下げる |  |
| Mouth_Sad_Left         | 0 | 自然な状態 | 口を閉じたまま左側の口角を下げる |  |
| Cheek_Puff_Right       | 0 | 自然な状態 | 右側の頬を膨らます |  |
| Cheek_Puff_Left        | 0 | 自然な状態 | 左側の頬を膨らます |  |
| Cheek_Suck             | 0 | 自然な状態 | 両側の頬をすぼます |  |
| Mouth_Upper_UpRight    | 0 | 自然な状態 | 口を閉じたまま右側の上唇を上げて歯が見える |  |
| Mouth_Upper_UpLeft     | 0 | 自然な状態 | 口を閉じたまま左側の上唇を上げて歯が見える |  |
| Mouth_Lower_DownRight  | 0 | 自然な状態 | 口を閉じたまま右側の下唇を下げて歯が見える |  |
| Mouth_Lower_DownLeft   | 0 | 自然な状態 | 口を閉じたまま左側の下唇を下げて歯が見える |  |
| Mouth_Upper_Inside     | 0 | 自然な状態 | 上唇を前歯に挟み込むように丸める |  |
| Mouth_Lower_Inside     | 0 | 自然な状態 | 下唇を前歯に挟み込むように丸める |  |
| Mouth_Lower_Overlay    | 0 | 自然な状態 | 下唇が上唇の前に被さる | 上唇が前にくる信号はない |
| Tongue_LongStep1       | 0 | 自然な状態 | 舌を前に出す | th の発音程度．口を閉じた状態では舌が外に出ない |
| Tongue_LongStep2       | 0 | 自然な状態 | 舌を大きく前に出す | 舌を前に大きく突き出した状態 |
| Tongue_Down            | 0 | 自然な状態 | 舌の先端を下に動かす | Tongue_LongStep2 と組み合わせたとき，あかんべーをしている状態 |
| Tongue_Up              | 0 | 自然な状態 | 舌の先端を上に動かす | R の発音の舌 |
| Tongue_Right           | 0 | 自然な状態 | 舌を右に動かす |  |
| Tongue_Left            | 0 | 自然な状態 | 舌を左に動かす |  |
| Tongue_Roll            | 0 | 自然な状態 | 舌をストローのように筒状に丸める |  |
| Tongue_UpLeft_Morph    | 0 | 自然な状態 | 舌の先端を左上に曲げる |  |
| Tongue_UpRight_Morph   | 0 | 自然な状態 | 舌の先端を右上に曲げる |  |
| Tongue_DownLeft_Morph  | 0 | 自然な状態 | 舌の先端を左下に曲げる |  |
| Tongue_DownRight_Morph | 0 | 自然な状態 | 舌の先端を右下に曲げる |  |

### 顔（アプリ内で計算・統合したデータ）
| キー                   | デフォルトの値 | 最小値 | 最大値 |  説明 | 
| ---------------------- | --- | --- | --- | --- |
| Jaw_Left_Right              | 0.5 (0) | 顎を左に動かす | 顎を右に動かす | Jaw_Left と Jaw_Right から計算 |
| Mouth_Sad_Smile_Right       | 0.5 (0) | 口を閉じたまま右側の口角を上げる | 口を閉じたまま右側の口角を上げる | Mouth_Sad_Right と Mouth_Smile_Right から計算 |
| Mouth_Sad_Smile_Left        | 0.5 (0) | 口を閉じたまま左側の口角を上げる | 口を閉じたまま左側の口角を上げる | Mouth_Sad_Left と Mouth_Smile_Left から計算 |
| Mouth_Smile                 | 0 | 自然な状態 | 口を閉じたまま両方の口角を上げる | Mouth_Smile_Left と Mouth_Smile_Right の平均 |
| Mouth_Sad                   | 0 | 自然な状態 | 口を閉じたまま両方の口角を下げる | Mouth_Sad_Left と Mouth_Sad_Right の平均 |
| Mouth_Sad_Smile             | 0.5 (0) | 両方の口角を下げる | 両方の口角を上げる | Mouth_Sad と Mouth_Smile から計算 |
| Mouth_Upper_Left_Right      | 0.5 (0) | 口を閉じたまま上唇を左に動かす | 口を閉じたまま上唇を右に動かす | Mouth_Upper_Left と Mouth_Upper_Right から計算 |
| Mouth_Lower_Left_Right      | 0.5 (0) | 口を閉じたまま下唇を左に動かす | 口を閉じたまま下唇を右に動かす | Mouth_Lower_Left と Mouth_Lower_Right から計算 |
| Mouth_Left_Right            | 0.5 (0) | 口を閉じたまま上下の唇を左に動かす | 口を閉じたまま上下の唇を右に動かす | Mouth_Upper_Left_Right と Mouth_Lower_Left_Right の平均 |
| Mouth_Upper_Inside_Overturn | 0.5 (0) | 上唇を前歯に挟み込むように丸める | 口を閉じたまま上唇を突き出す | Mouth_Upper_Inside と Mouth_Upper_Overturn から計算 |
| Mouth_Lower_Inside_Overturn | 0.5 (0) | 下唇を前歯に挟み込むように丸める | 口を閉じたまま下唇を突き出す | Mouth_Lower_Inside と Mouth_Lower_Overturn から計算 |
| Cheek_Puff                  | 0 | 自然な状態 | 両方の頬を膨らます | Cheek_Puff_Left と Cheek_Puff_Right の平均 |
| Cheek_Suck_Puff             | 0.5 (0) | 両側の頬をすぼます | 両方の頬を膨らます | Cheek_Suck と Cheek_Puff から計算 |
| Mouth_Upper_Up              | 0 | 自然な状態 | 上唇を上げて歯が見える | Mouth_Upper_UpLeft と Mouth_Upper_UpRight の平均 |
| Mouth_Lower_Down            | 0 | 自然な状態 | 下唇を下げて歯が見える | Mouth_Lower_DownLeft と Mouth_Lower_DownRight の平均 |
| Tongue_Left_Right           | 0.5 (0) | 舌を左に動かす | 舌を右に動かす | Tongue_Left　と Tongue_Right から計算 |
| Tongue_Down_Up              | 0.5 (0) | 舌の先端を下に動かす | 舌の先端を上に動かす | Tongue_Down　と Tongue_Up から計算 |

