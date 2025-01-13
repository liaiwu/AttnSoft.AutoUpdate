using System.Text;

namespace GeneralUpdate.Common.Compress;

public interface ICompressionStrategy
{
    void Compress(string sourcePath, string destinationPath, bool includeRootDirectory);
    void Decompress(string archivePath, string destinationPath);
}