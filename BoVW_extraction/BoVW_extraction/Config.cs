using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoVW_extraction {

    /// <summary>
    /// 本プログラムの設定値を管理します．
    /// コマンドライン引数により設定値の書き換えが可能です．
    /// </summary>
    class Config {
        /// <summary>
        /// 各種定数
        /// </summary>
        public static string INPUT_IMAGE_DIR = "";              // 入力ディレクトリ
        public static string INPUT_FILENAME_PATTERN = "*.jpg";  // 入力ファイル名パターン
        public static string OUTPUT_FILENAME = "histogram.txt"; // 出力ファイル名
        public static int SURF_HESSIAN_THRESHOLD = 400;         // SURF Hessian閾値
        public static int MAX_CLUSTER = 300;                    // クラスタ数 = Visual Words 次元数
        public static bool IS_PREVIEW_SURF = false;             // SURF抽出画像を確認するか？
        public static int MAX_INPUT_FILE_CLUSTERING = 500;      // 入力クラスタリングファイル数上限
        public static int MAX_INPUT_FILE_HISTOGRAM = 10000;     // 入力ヒストグラムファイル数上限

        /// <summary>
        /// コマンドライン引数理解
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        /// <returns>成功なら0，失敗なら1</returns>
        public static int ParseArgs(string[] args) {

            // オプション一覧
            const string INPUT_IMAGE_DIR = "-INPUT_IMAGE_DIR";
            const string INPUT_FILENAME_PATTERN = "-INPUT_FILE_NAME_PATTERN";
            const string OUTPUT_FILENAME = "-OUTPUT_FILE_NAME";
            const string SURF_HESSIAN_THRESHOLD = "-SURF_HESSIAN_THRESHOLD";
            const string MAX_CLUSTER = "-MAX_CLUSTER";
            const string IS_PREVIEW_SURF = "-IS_PREVIEW_SURF";
            const string MAX_INPUT_FILE_CLUSTERING = "-MAX_INPUT_FILE_CLUSTERING";
            const string MAX_INPUT_FILE_HISTOGRAM = "-MAX_INPUT_FILE_HISTOGRAM";

            // パース辞書
            var options = new HashSet<string> {
                INPUT_IMAGE_DIR,
                INPUT_FILENAME_PATTERN,
                OUTPUT_FILENAME,
                SURF_HESSIAN_THRESHOLD,
                MAX_CLUSTER,
                IS_PREVIEW_SURF,
                MAX_INPUT_FILE_CLUSTERING,
                MAX_INPUT_FILE_HISTOGRAM
            };

            // コマンドラインオプションを解析
            // http://neue.cc/2009/12/13_229.html
            string optionName = null;
            Dictionary<string, string> parsedDict = new Dictionary<string, string>();
            try {
                parsedDict = args
                    .GroupBy(s => options.Contains(s) ? optionName = s : optionName) //副作用
                    .ToDictionary(g => g.Key, g => g.Skip(1).FirstOrDefault()); //1番目はキーなのでskip
            } catch (Exception e) {
                Console.WriteLine("コマンドライン引数に問題があります．");
                Console.WriteLine("利用できる引数は以下の通りです．");
                foreach (string _option in options) {
                    Console.WriteLine("\t" + _option);
                }
                Console.WriteLine(e.Message);
                return 1;
            }

            // コマンドライン引数を反映
            foreach (KeyValuePair<string, string> kvp in parsedDict) {
                Console.WriteLine("コマンドライン引数により以下のパラメータが変更されました．");
                string option = kvp.Key;
                string value = kvp.Value;
                Console.WriteLine("\t" + kvp.Key + " = " + kvp.Value);

                //各オプションについて処理
                switch (option) {
                    case INPUT_IMAGE_DIR:
                        Config.INPUT_IMAGE_DIR = value;
                        break;
                    case INPUT_FILENAME_PATTERN:
                        Config.INPUT_FILENAME_PATTERN = value;
                        break;
                    case OUTPUT_FILENAME:
                        Config.OUTPUT_FILENAME = value;
                        break;
                    case SURF_HESSIAN_THRESHOLD:
                        try {
                            Config.SURF_HESSIAN_THRESHOLD = Convert.ToInt32(value);
                        } catch (Exception e) {
                            Console.WriteLine("コマンドライン引数（" + SURF_HESSIAN_THRESHOLD + "）に問題があります．");
                            Console.WriteLine(e.Message);
                            return 1;
                        }
                        break;
                    case MAX_CLUSTER:
                        try {
                            Config.MAX_CLUSTER = Convert.ToInt32(value);
                        } catch (Exception e) {
                            Console.WriteLine("コマンドライン引数（" + MAX_CLUSTER + "）に問題があります．");
                            Console.WriteLine(e.Message);
                            return 1;
                        }
                        break;
                    case IS_PREVIEW_SURF:
                        Config.IS_PREVIEW_SURF =
                            (value == "true") ? true :
                            (value == "false") ? false :
                            Config.IS_PREVIEW_SURF;
                        break;
                    case MAX_INPUT_FILE_CLUSTERING:
                        try {
                            Config.MAX_INPUT_FILE_CLUSTERING = Convert.ToInt32(value);
                        } catch (Exception e) {
                            Console.WriteLine("コマンドライン引数（" + MAX_INPUT_FILE_CLUSTERING + "）に問題があります．");
                            Console.WriteLine(e.Message);
                            return 1;
                        }
                        break;
                    case MAX_INPUT_FILE_HISTOGRAM:
                        try {
                            Config.MAX_INPUT_FILE_HISTOGRAM = Convert.ToInt32(value);
                        } catch (Exception e) {
                            Console.WriteLine("コマンドライン引数（" + MAX_INPUT_FILE_HISTOGRAM + "）に問題があります．");
                            Console.WriteLine(e.Message);
                            return 1;
                        }
                        break;
                    default:
                        Console.WriteLine("コマンドライン引数（" + option + "）は存在しません．");
                        Console.WriteLine("利用できる引数は以下の通りです．");
                        foreach (string _option in options) {
                            Console.WriteLine("\t" + _option);
                        }
                        return 1;
                }
            }

            // バリデーションチェック
            if (!System.IO.Directory.Exists(Config.INPUT_IMAGE_DIR)) {
                if (Config.INPUT_IMAGE_DIR == "") {
                    Console.WriteLine("入力ディレクトリ引数 -INPUT_IMAGE_DIR を設定してください．");
                } else {
                    Console.WriteLine("入力ディレクトリ -INPUT_IMAGE_DIR: \"" + Config.INPUT_IMAGE_DIR + "\" が存在しません．");
                }
                Console.WriteLine("コマンドライン引数に問題があります．");
                Console.WriteLine("利用できる引数は以下の通りです．");
                foreach (string _option in options) {
                    Console.WriteLine("\t" + _option);
                }
                return 1;
            }

            return 0;
        }                                       
    }
}
