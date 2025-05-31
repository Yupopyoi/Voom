# Voom

MediaPipeによるポーズ推定を、３Dアバターへリアルタイムに適用する  
ベンチマークであった VHub の後継版  

## 進捗

![progress_image](https://github.com/Yupopyoi/Voom/blob/main/Sample/0519_progress.gif)

## 環境

Unity 6000.0.31  

## 事始め

### ダウンロード

```bash
git clone https://github.com/Yupopyoi/Voom.git
```

### VRMモデル

VRMモデルは ```Voom\Assets\StreamingAssets\VRMModel``` に保存してください。
VRMモデルは、V1.0以上のものを使用してください。

### メインメニュー

メインメニューは、Gameビューで、**「マウスの左右のボタンを同時に押しながら、下にドラッグする」** ことで表示出来ます。
メインメニューでは、VRMのモデル選択や、カメラデバイスの指定が出来ます。

### UI フォント

TextMeshPro はこのレポジトリに含まれていないので各自でインポートしてください。  
```Voom\Assets\Prefabs\UI\CircleMenuItem``` における、TMP_FontSetterコンポーネントにより、フォントの一括変更が可能です。

## Contribution

This project uses [MediaPipeUnityPlugin](https://github.com/homuler/MediaPipeUnityPlugin) created by homuler.  
I would like to express my sincere gratitude to him.
