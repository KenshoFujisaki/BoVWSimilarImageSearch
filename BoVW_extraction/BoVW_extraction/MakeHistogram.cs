using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

using OpenCvSharp;
using System.Runtime.InteropServices;
using OpenCvSharp.CPlusPlus;

namespace BoVW_extraction {

    /// <summary>
    /// ヒストグラムを作成します．
    /// </summary>
    class MakeHistogram {

        /// <summary>
        /// IMAGE_DIRの全画像をヒストグラムに変換して出力
        /// 各画像の各局所特徴量を一番近いVisual Wordsに投票してヒストグラムを作成
        /// </summary>
        /// <param name="visualWords">VisualWords</param>
        /// <param name="inputImageDir">入力画像ディレクトリ</param>
        /// <param name="inputFilenamePattern">入力ファイルパターン文字列</param>
        /// <param name="maxInputFiles">最大入力ファイル数</param>
        /// <param name="SURFHessianThreshold">SURF検出閾値ヘッシアン</param>
        /// <param name="outputFileName">出力ファイル名</param>
        /// <returns>成功なら0，失敗なら1</returns>
        static public int CalcHistograms(
            CvMat visualWords, string inputImageDir, string inputFilenamePattern, string outputFileName, 
            int maxInputFiles, double SURFHessianThreshold) {

            const int SURFFeatureDimension = 128;

            // 一番近いVisual Wordsを高速検索できるようにVisual WordsをKD-Treeでインデキシング
            CvFeatureTree feature_tree = Cv.CreateKDTree(visualWords);

            // IMAGE_DIRを開く
            if (!Directory.Exists(inputImageDir)) {
                Console.WriteLine("cannot open directory: " + inputImageDir);
                return 1;
            }

            // 各画像のヒストグラムを出力するファイルを開く
            string OUTPUT_HISTGRAM_PATH = inputImageDir + outputFileName;
            int filenumber = 1;
            using (StreamWriter sw = new StreamWriter(OUTPUT_HISTGRAM_PATH, /*isAppend*/false)) {

                // IMAGE_DIRの画像ファイル名を走査
                Parallel.ForEach(Directory.GetFiles(inputImageDir, inputFilenamePattern, SearchOption.TopDirectoryOnly), (filepath, loop) => {

                    // ファイル名を表示
                    Console.WriteLine(filenumber++ + ":" + filepath);

                    // ヒストグラム初期化
                    int[] histogram = new int[visualWords.Rows];
                    histogram.Select(i => 0);

                    // SURFを抽出
                    CvSURFPoint[] keypoints = null;
                    float[][] descriptors = null;
                    if (ExtractFeature.SURF.ExtractSURF(
                        filepath, ref keypoints, ref descriptors,
                        SURFHessianThreshold, /*preview SURF*/false) != /*成功*/0) {

                        Console.WriteLine(
                            filenumber + ":" + filepath + "\terror in Extract SURF.");
                        return;
                    }

                    // 境界上の特徴点を除去
                    CvSURFPoint[] newKeypoints = null;
                    float[][] newDescriptors = null;
                    using (IplImage img = new IplImage(filepath)) {
                        ExtractFeature.General.RemoveDescriptorsSURF(
                            keypoints, descriptors, ref newKeypoints, ref newDescriptors, img.Width, img.Height);
                    }
                    keypoints = newKeypoints;
                    descriptors = newDescriptors;

                    // KD-treeで高速検索できるように特徴ベクトルをCvMatに展開
                    // CvMat構造化descriptorの準備
                    CvMat descMat = Cv.CreateMat(descriptors.Length, SURFFeatureDimension, MatrixType.F32C1);

                    // descriptorの各要素(=各局所特徴点)について走査
                    for (int i = 0; i < descriptors.Length; i++) {

                        // 各特徴量についてdataに追加
                        for (int j = 0; j < SURFFeatureDimension; j++) {
                            descMat[i, j] = descriptors[i][j];
                        }
                    }

                    // 局所特徴点について最も類似したVisual Wordsを見つけて投票
                    const int K = 1;    // 1-NN
                    CvMat indices = Cv.CreateMat(keypoints.Length, K, MatrixType.S32C1); // 最近傍のVisual Wordsのインデックス
                    CvMat dists = Cv.CreateMat(keypoints.Length, K, MatrixType.F64C1);   // その距離
                    lock (feature_tree) {
                        Cv.FindFeatures(feature_tree, descMat, indices, dists, K, /*emax*/250);
                    }
                    for (int i = 0; i < indices.Rows; i++) {
                        int index = Cv.CV_MAT_ELEM<int>(indices, i, /*col*/0);
                        histogram[index] += 1;
                    }

                    // ヒストグラムをファイルに出力
                    lock (sw) {
                        sw.Write(filepath + "\t");
                        for (int i = 0; i < visualWords.Rows; i++) {
                            if (descriptors.Length != 0) {
                                sw.Write((float)histogram[i] / (float)descriptors.Count() + "\t");
                            } else {
                                sw.Write("INF\t");
                            }
                        }
                        sw.WriteLine();
                    }

                    // メモリ解放
                    keypoints = null;
                    descriptors = null;
                    descMat.Dispose();
                    indices.Dispose();
                    dists.Dispose();

                    if (filenumber > maxInputFiles) {
                        Console.WriteLine("上限ファイル数:" + maxInputFiles + "を超えたため打ち切り");
                        //break;
                        loop.Stop();
                    }
                });
            }
            visualWords.Dispose();
            return 0;
        }
    }
}
