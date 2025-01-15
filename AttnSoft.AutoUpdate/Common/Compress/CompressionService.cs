using GeneralUpdate.Common.Compress;
using System;
using System.Collections.Generic;
using System.Text;

namespace AttnSoft.AutoUpdate.Common
{
    internal static class CompressionService
    {
        static ICompressionStrategy ziphelper = new ZipCompressionStrategy();
        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        /// <param name="includeRootDirectory"></param>
        public static void Compress(string sourcePath, string destinationPath, bool includeRootDirectory)
        {
            ziphelper.Compress(sourcePath, destinationPath, includeRootDirectory);
        }
        /// <summary>
        /// 解压
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="destinationPath"></param>
        public static void Decompress(string archivePath, string destinationPath)
        {
            ziphelper.Decompress(archivePath, destinationPath);
        }
    }
}
