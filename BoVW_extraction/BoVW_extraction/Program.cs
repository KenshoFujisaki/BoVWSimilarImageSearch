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

    class Program {

        /// <summary>
        /// メイン関数
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args) {

            // 引数理解
            if (Config.ParseArgs(args) != /*成功*/0) {

                Console.WriteLine("error in Parse Args.");
                Environment.Exit(1);
            }

            // SURFを使えるようにライセンス（重要）
            Cv2.InitModule_NonFree();

            // IMAGE_DIR の各画像から局所特徴量を抽出
            Console.WriteLine("Load Descriptors ...");
            CvMat samples = new CvMat();                                            // 全特徴点についてのdesscriptor(CvMat)
            float[] all_descriptors = new float[] { };                              // 全特徴点についてのdesscriptor(float[])
            if (ExtractFeature.General.LoadDescriptors(
                ref samples, ref all_descriptors, Config.SURF_HESSIAN_THRESHOLD, Config.IS_PREVIEW_SURF, 
                Config.INPUT_IMAGE_DIR, Config.INPUT_FILENAME_PATTERN, Config.MAX_INPUT_FILE_CLUSTERING) != /*成功*/0) {
                
                Console.WriteLine("error in Load Descriptors.");
                Environment.Exit(1);
            }

            // 局所特徴量をクラスタリングして各クラスタのセントロイドを計算
            Console.WriteLine("Clustering ...");
            const int SURFFeatureDimension = 128;
            CvMat visualWords = new CvMat(Config.MAX_CLUSTER, SURFFeatureDimension, MatrixType.F32C1);
            if (Clustering.KMeansClustering(ref samples, ref visualWords) != /*成功*/0) {

                Console.WriteLine("error in Clustering.");
                Environment.Exit(1);
            }

            // 各画像をVisual Wordsのヒストグラムに変換
            // 各クラスタの中心ベクトル，セントロイドがそれぞれVisual Wordsになる
            Console.WriteLine("Calc Histograms ...");          
            if (MakeHistogram.CalcHistograms(
                visualWords, Config.INPUT_IMAGE_DIR, Config.INPUT_FILENAME_PATTERN, Config.OUTPUT_FILENAME,
                Config.MAX_INPUT_FILE_HISTOGRAM, Config.SURF_HESSIAN_THRESHOLD) != /*正常*/0) {

                Console.WriteLine("error in Calc Histograms.");
                Environment.Exit(1);
            }
            
            // 後処理
            visualWords.Dispose();
            samples.Dispose();

            Console.WriteLine("\nComplete!!");   
        }
    }
}
