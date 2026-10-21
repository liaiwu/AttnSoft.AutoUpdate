using System;

namespace GeneralUpdate.Common.FileBasic;

public class FileNode
{
    #region Public Properties

    public long Id { get; set; }

    public string Name { get; set; }

    public string FullName { get; set; }

    public string Path { get; set; }

    public string Hash { get; set; }

    public string RelativePath { get; set; }

    #endregion Public Properties

    #region Constructors

    public FileNode()
    {
    }

    public FileNode(int id)
    {
        Id = id;
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Compare tree nodes equally by Hash and file names.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        var tempNode = obj as FileNode;
        if (tempNode == null) throw new ArgumentException(nameof(tempNode));
        return string.Equals(Hash, tempNode.Hash, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(Name, tempNode.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() => base.GetHashCode();

    #endregion Public Methods
}
