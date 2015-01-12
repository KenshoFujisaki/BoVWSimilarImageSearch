# BoVWSimilarImageSearch
BoVWの手法を用いてSURFをヒストグラム化し，ヒストグラム交差により類似画像を検索します．  
参考サイト:[Visual Wordsを用いた類似画像検索](http://aidiary.hatenablog.com/entry/20100227/1267277731 "Visual Wordsを用いた類似画像検索")  
環境は，Windows＋Cygwin(Bash)を前提とします．

## 1. BoVWによるSURFヒストグラム作成
例:
```sh
$ ./BoVW_extraction/BoVW_extraction/bin/x64/Release/BoVW_extraction.exe -INPUT_IMAGE_DIR "E:\\image\\thumbs\\" -MAX_INPUT_FILE_CLUSTERING 300 -MAX_INPUT_FILE_HISTOGRAM 300 -OUTPUT_FILE_NAME "histograms.txt"
```
使えるパラメータは以下の通り
```sh
$ ./BoVW_extraction/BoVW_extraction/bin/x64/Release/BoVW_extraction.exe
入力ディレクトリ引数 -INPUT_IMAGE_DIR を設定してください．
コマンドライン引数に問題があります．
利用できる引数は以下の通りです．
        -INPUT_IMAGE_DIR
        -INPUT_FILE_NAME_PATTERN
        -OUTPUT_FILE_NAME
        -SURF_HESSIAN_THRESHOLD
        -MAX_CLUSTER
        -IS_PREVIEW_SURF
        -MAX_INPUT_FILE_CLUSTERING
        -MAX_INPUT_FILE_HISTOGRAM
error in Parse Args.
```

## 2. ヒストグラム交差による類似画像検索
例:
```sh
$ python ./VWSearch/HistogramIntersection.py E:\\image\\thumbs\\histograms.txt
query? > E:\image\thumbs\[DVDenc] 妄想代理人 第05話 「聖戦士」_thumb.jpg
1.000000        E:\image\thumbs\[DVDenc] 妄想代理人 第05話 「聖戦士」_thumb.jpg
0.825243        E:\image\thumbs\STAR DRIVER 輝きのタクト #14_thumb.jpg
0.823405        E:\image\thumbs\[DVDenc] 妄想代理人 第01話 「少年バット参上」_thumb.jpg
0.823214        E:\image\thumbs\[DVDenc] 妄想代理人 第02話 「金の靴」_thumb.jpg
0.820706        E:\image\thumbs\RD潜脳調査室 第26話 「リアルドライブ」_thumb.jpg
0.820120        E:\image\thumbs\TIGER & BUNNY 第04話 「Fear is often greater than the danger. 案ずるより、生むが易し」 (1280x720 x264 AAC)_thumb.jpg
0.820112        E:\image\thumbs\[DVDenc] 妄想代理人 第03話 「ダブルリップ」_thumb.jpg
0.820026        E:\image\thumbs\[M2TSenc] アマガミSS 第11話「中多紗江編 第三章 ヘンカク」_thumb.jpg
0.819338        E:\image\thumbs\神のみぞ知るセカイ FLAG 4.0「今そこにある聖戦」_thumb.jpg
0.817407        E:\image\thumbs\神のみぞ知るセカイ FLAG 2.0「あくまでも妹です」FLAG 2.5「ベイビー・ユー・アー・ア・リッチ・ガール」(TX 1280x720 x264 AAC)_thumb.jpg
query? > quit
Aborted
```
結果画像が ./result.jpg に書きだされる（類似度が高い順に，上段左から右に向かって1,2,3,4,5，下段左から右に向かって6,7,8,9,10）．  
※HistogramIntersection.pyにおける「query? >」では，入力ファイル（ここではE:\\image\\thumbs\\histograms.txt）の第一カラム（フルパス）の値を入力すること．  
※例では，動画のサムネイルを対象（[MakeVideoThumbnail](https://github.com/KenshoFujisaki/MakeVideoThumbnail "KenshoFujisaki/MakeVideoThumbnail")）に実施した．結果画像は以下．
![result.jpg](https://github.com/KenshoFujisaki/BoVWSimilarImageSearch/blob/master/result.jpg)
