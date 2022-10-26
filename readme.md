# SaveClipboardImage

## 概要

Windows のクリップボードにある画像を保存する

## コマンドライン引数

### 任意:`-o <outputDirPath | outputFilePath>`

出力ファイルまたはディレクトリを指定する。

#### ファイル名を指定した場合
* `SaveClipboardImage.exe -o ./outputpath.png`

GUIなしで起動し、ファイルを保存し終了する。

#### ディレクトリを指定した場合
* `SaveClipboardImage.exe -o ./aaa/`
  
GUIで起動し、保存ボタンを押すと終了する。

#### 引数自体を省略した場合

GUIで起動し、保存ボタンを押すとダイアログでフォルダとファイル名を決める。
保存しても終了しない。
