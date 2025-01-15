
using GeneralUpdate.Common.HashAlgorithms;
using System;
using System.Linq;

namespace AttnSoft.AutoUpdate.Common
{
    internal static class HashAlgorithmService
    {
        static HashAlgorithmBase hashAlgorithm = new Sha256HashAlgorithm();

        public static string ComputeHash(string fileName)
        {
            return hashAlgorithm.ComputeHash(fileName);
        }
        public static byte[] ComputeHashBytes(string fileName)
        {
            return hashAlgorithm.ComputeHashBytes(fileName);
        }
        public static bool HashEquals(string leftPath, string rightPath)
        {
            var hashLeft = hashAlgorithm.ComputeHash(leftPath);
            var hashRight = hashAlgorithm.ComputeHash(rightPath);
            return hashLeft.SequenceEqual(hashRight);
        }
    }
}
