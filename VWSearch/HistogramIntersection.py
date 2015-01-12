#coding:utf-8
import codecs
import os
import sys
from PIL import Image, ImageDraw

# 詳細はこちらのページを確認すること
# Visual Wordsを用いた類似画像検索
# http://aidiary.hatenablog.com/entry/20100227/1267277731http://aidiary.hatenablog.com/entry/20100227/1267277731

# ヒストグラムをロードする
def load_hist(filepath):
    hist = {}
    fp = open(filepath, "r")
    for line in fp:
        line = line.rstrip()
        data = line.split("\t")
        file = data[0]
        h = [float(x) for x in data[1:]]
        hist[file] = h
    fp.close()
    return hist

# 正規化Histogram Intersectionを計算する
def calc_hist_intersection(hist1, hist2):
    total = 0
    for i in range(len(hist1)):
        total += min(hist1[i], hist2[i])
    return float(total) / sum(hist1)

def main():
    if (len(sys.argv) != 2):
        print "Usage: python %s [filepath]" % (sys.argv[0])
        quit()
	
    hist = load_hist(sys.argv[1])

    while True:
        # クエリとなるヒストグラムファイル名を入力
        query_file = raw_input("query? > ")
        
        # 終了
        if query_file == "quit":
            break
                
        # 存在しないヒストグラムファイル名のときは戻る
        if not hist.has_key(query_file):
            print "no histogram"
            continue
        
        # クエリと他の全画像の間で類似度を計算
        result = []
        query_hist = hist[query_file]
        for target_file in hist.keys():
            target_hist = hist[target_file]
            d = calc_hist_intersection(query_hist, target_hist)
            result.append((d, target_file))
        
        # 類似度の大きい順にソート
        result.sort(reverse=True)
        
        # 上位10位を表示（1位はクエリ画像）
        # PILを使って300x300を1単位として2行5列で10個の画像を並べて描画
        p = 0
        canvas = Image.new("RGB", (4500, 1800), (255,255,255))  # 白いキャンバス
        for score, filename in result[0:10]:
            print "%f\t%s" % (score, filename)
            img = Image.open(filename)
            pos = (900*(p%5), 900*(p/5))
            canvas.paste(img, pos)
            p += 1
        canvas.resize((4500/2, 1500/2))
        canvas.save("result.jpg", "JPEG")

if __name__ == "__main__":
    main()
