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
    /// 特徴抽出を行うクラスです．
    /// </summary>
    class ExtractFeature {

        /// <summary>
        /// SURF特徴の抽出を行うクラスです．
        /// </summary>
        public class SURF {
            /// <summary>
            /// 画像ファイルからSURF特徴量を抽出する
            /// </summary>
            /// <param name="filepath">画像ファイル名</param>
            /// <param name="keypoints">キーポイント</param>
            /// <param name="descriptors">各キーポイントのSURF特徴量</param>
            /// <param name="SURFHessianThreshold">SURF検出閾値ヘッシアン</param>
            /// <param name="isPreviewSURF">SURF抽出を画像プレビューするか否か</param>
            /// <returns>成功なら0，失敗なら1</returns>
            static public int ExtractSURF(
                string filepath, ref CvSURFPoint[] keypoints, ref float[][] descriptors, 
                double SURFHessianThreshold, bool isPreviewSURF) {

                //グレースケールで画像をロード
                using (IplImage img = Cv.LoadImage(filepath, LoadMode.GrayScale)) {
                    try {
                        Cv.ExtractSURF(
                            img,
                            /*mask*/null,
                            out keypoints,
                            out descriptors,
                            new CvSURFParams(SURFHessianThreshold, /*extended 128*/true));

                        //画像出力
                        if (isPreviewSURF) {
                            previewExtractSURF(filepath, ref keypoints);
                        }
                    } catch (Exception e) {
                        Console.WriteLine(e.Message);
                        return 1;
                    }
                }
                return 0;
            }

            /// <summary>
            /// 画像からSURF特徴量を抽出するのを画面確認する
            /// </summary>
            /// <param name="img">画像</param>
            /// <param name="keypoints">キーポイント</param>
            /// 
            /// <returns></returns>
            static private int previewExtractSURF(string filepath, ref CvSURFPoint[] keypoints) {
                using (IplImage img = Cv.LoadImage(filepath, LoadMode.GrayScale))
                using (IplImage preview = Cv.CreateImage(new CvSize(img.Width, img.Height), BitDepth.U8, /*channel*/3)) {
                    Cv.CvtColor(img, preview, ColorConversion.GrayToBgr);

                    // 特徴点の場所に円を描く
                    for (int i = 0; i < keypoints.Count(); i++) {
                        CvSURFPoint r = keypoints[i];
                        CvPoint center = new CvPoint(Cv.Round(r.Pt.X), Cv.Round(r.Pt.Y));
                        int radius = Cv.Round(r.Size * (1.2 / 9.0) * 2);
                        Cv.Circle(preview, center, radius, CvColor.Red, 1, LineType.AntiAlias, 0);
                    }

                    // ウィンドウに表示
                    Cv.NamedWindow("SURF Extraction", WindowMode.AutoSize);
                    Cv.ShowImage("SURF Extraction", preview);
                    Cv.WaitKey(0);
                    Cv.DestroyWindow("SURF Extraction");
                }
                return 0;
            }
        }

        /// <summary>
        /// 特徴抽出に関する汎用的なクラスです．
        /// SURFやSIFTといった特徴による特徴抽出を活用します．
        /// </summary>
        public class General {

            /// <summary>
            /// 不要なSURF特徴点を除去します．
            /// </summary>
            /// <param name="keypoints">キーポイント</param>
            /// <param name="descriptors">SURF特徴量の配列</param>
            /// <returns>成功なら0，失敗なら1</returns>
            static public int RemoveDescriptorsSURF(
                CvSURFPoint[] keypoints, float[][] descriptors,
                ref CvSURFPoint[] newKeypoints, ref float[][] newDescriptors,
                int imageWidth, int imageHeight) {

                // 定数
                const int BoundaryGap = 10;
                const int HorizontalBlocks = 8;
                const int VerticalBlocks = 8;

                // バリデーションチェック
                if(imageHeight / VerticalBlocks < BoundaryGap ||
                    imageWidth / HorizontalBlocks < BoundaryGap) {

                    Console.WriteLine("定数に問題があります．");
                    return 0;
                }

                // マスク作成
                bool[,] removeMask = new bool[imageHeight, imageWidth];
                for (int y = 0; y < imageHeight; y++) {
                    for (int x = 0; x < imageWidth; x++) {
                        removeMask[y, x] = false;
                    }
                }
                for (int vBlocks = 1; vBlocks < VerticalBlocks; vBlocks++) {
                    int vOffset = imageHeight / VerticalBlocks * vBlocks;
                    for (int y = vOffset - BoundaryGap; y < vOffset + BoundaryGap; y++) {
                        for (int x = 0; x < imageWidth; x++) {
                            removeMask[y, x] = true;
                        }
                    }
                }
                for (int hBlocks = 1; hBlocks < HorizontalBlocks; hBlocks++) {
                    int hOffset = imageWidth / HorizontalBlocks * hBlocks;
                    for (int x = hOffset - BoundaryGap; x < hOffset + BoundaryGap; x++) {
                        for (int y = 0; y < imageHeight; y++) {
                            removeMask[y, x] = true;
                        }
                    }
                }

                // 各特徴点について走査
                List<CvSURFPoint> _newKeypoints = new List<CvSURFPoint>();
                List<float[]> _newDescriptors = new List<float[]>();
                for (int i = 0; i < keypoints.Length; i++) {
                    int x = (int)keypoints[i].Pt.X;
                    int y = (int)keypoints[i].Pt.Y;

                    if (removeMask[y, x] == false) {
                        _newKeypoints.Add(keypoints[i]);
                        _newDescriptors.Add(descriptors[i]);
                    }
                }
                newKeypoints = _newKeypoints.ToArray();
                newDescriptors = _newDescriptors.ToArray();

                return 0;
            }

            /// <summary>
            /// IMAGE_DIRにある全画像から局所特徴量を抽出し行列へ格納する
            /// </summary>
            /// <param name="samples">局所特徴量の行列</param>
            /// <param name="data">samplesのデータ領域</param>
            /// <param name="all_descriptors">全検出点格納配列</param>
            /// <param name="inputFilenamePattern">入力ファイル名パターン文字列</param>
            /// <param name="inputImageDir">入力画像ディレクトリ</param>
            /// <param name="maxInputFile">最大入力ファイル数</param>
            /// <param name="SURFHessianThreshold">SURF検出閾値ヘッシアン</param>
            /// <param name="isPreviewFeatureExtraction">特徴抽出プレビューするか否か</param>
            /// <returns>成功なら0，失敗なら1</returns>
            static public int LoadDescriptors(ref CvMat samples, ref float[] all_descriptors,
                double SURFHessianThreshold, bool isPreviewFeatureExtraction,
                string inputImageDir, string inputFilenamePattern, int maxInputFile) {

                const int SURFFeatureDimension = 128;
                
                // IMAGE_DIRを開く
                if (!Directory.Exists(inputImageDir)) {
                    Console.WriteLine("cannot open directory: " + inputImageDir);
                    return 1;
                }

                // IMAGE_DIRの画像ファイル名を走査
                string[] filepaths = Directory.GetFiles(inputImageDir, inputFilenamePattern, SearchOption.TopDirectoryOnly).
                    OrderBy(i => Guid.NewGuid()).ToArray();
                for(int filenumber=0; filenumber < filepaths.Length ; filenumber++) {
                    if (filenumber > maxInputFile) {
                        Console.WriteLine("上限ファイル数:" + maxInputFile + "を超えたため打ち切り");
                        break;
                    }
                    string filepath = filepaths[filenumber];

                    // SURFを抽出
                    CvSURFPoint[] keypoints = null;
                    float[][] descriptors = null;
                    if (ExtractFeature.SURF.ExtractSURF(
                        filepath, ref keypoints, ref descriptors, SURFHessianThreshold, isPreviewFeatureExtraction) != /*成功*/0) {

                        Console.WriteLine(
                            filenumber + ":" + filepath + "\terror in Extract SURF.");
                        continue;
                    }

                    // 境界上の特徴点を除去
                    CvSURFPoint[] newKeypoints = null;
                    float[][] newDescriptors = null;
                    using(IplImage img = new IplImage(filepath)) {
                        ExtractFeature.General.RemoveDescriptorsSURF(
                            keypoints, descriptors, ref newKeypoints, ref newDescriptors, img.Width, img.Height);
                    }

                    // SURF特徴次元数チェック（128次元）
                    if (SURFFeatureDimension != descriptors.First().Length) {
                        Console.WriteLine("error in extractSURF: SURFFeatureDimension is mismatched.");
                        Environment.Exit(1);
                    }

                    // ファイル名と局所特徴点の数を標準出力
                    Console.WriteLine(
                        filenumber + ":" + filepath + "\t" +
                        "#descriptors:" + newDescriptors.Length + "\t" +
                        "(removed:" + (descriptors.Length - newDescriptors.Length) + ")\t" +
                        string.Format("memory:{0:#,#}byte", all_descriptors.Length * sizeof(float)));
                    
                    // 境界上の特徴点を除去
                    keypoints = newKeypoints;
                    descriptors = newDescriptors;

                    // 各特徴量について構造化せずにall_descripotorsに追加
                    // descriptorの各要素(=各局所特徴点)について走査
                    int pre_all_descriptors_len = all_descriptors.Length;
                    Array.Resize<float>(
                        ref all_descriptors,
                        pre_all_descriptors_len + descriptors.Length * SURFFeatureDimension);
                    for (int i = 0; i < descriptors.Length; i++) {
                        Array.Copy(
                            /*source array*/descriptors[i],
                            /*source index*/0,
                            /*destination array*/all_descriptors,
                            /*destination index*/pre_all_descriptors_len + (i * SURFFeatureDimension),
                            /*length*/SURFFeatureDimension);
                    }

                    // メモリ解放
                    keypoints = null;
                }

                // dataをCvMat形式に変換
                // CvMatはall_descriptorsを参照するためall_descriptorsは解放されない
                int feature_points = all_descriptors.Length / SURFFeatureDimension; // CvMatの行数（=全ファイルの累積SURF局所特徴点数）
                Cv.InitMatHeader<float>(
                    /*mat*/samples,
                    /*rows*/feature_points,
                    /*cols*/SURFFeatureDimension,
                    /*type*/MatrixType.F32C1,
                    /*data*/all_descriptors);

                // 累計特徴点数
                Console.WriteLine("total feature points: " + feature_points);

                return 0;
            }
        }
    }
}
