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
    /// クラスタリングを行うクラスです．
    /// </summary>
    class Clustering {

        /// <summary>
        /// KMeansクラスタリンクを行います．
        /// </summary>
        /// <param name="samples">標本</param>
        /// <param name="centroid">クラスタ中心値</param>
        /// <returns></returns>
        public static int KMeansClustering(ref CvMat samples, ref CvMat centroid) {
            CvMat labels = new CvMat(samples.Rows, /*cols*/1, MatrixType.S32C1);
            double compactness = 0.0;
            Cv.KMeans2(
                samples,
                Config.MAX_CLUSTER,
                labels,
                Cv.TermCriteria(CriteriaType.Epsilon, /*max itr*/10, /*epsilon*/1.0),
                /*attempt*/1,
                /*CvRNG*/null,
                /*KMeansFlg*/0,
                centroid,
                out compactness);
            labels.Dispose();
            return 0;
        }
    }
}
