using AttnSoft.AutoUpdate.Common;
using GeneralUpdate.Common.FileBasic;
using GeneralUpdate.Common.JsonContext;
using GeneralUpdate.Differential.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GeneralUpdate.Differential
{
    public sealed class DifferentialCore
    {
        private static readonly object _lockObj = new ();
        private static DifferentialCore? _instance;
        private const string PATCH_FORMAT = ".patch";
        private const string DELETE_FILES_NAME = "generalupdate_delete_files.json";

        public static DifferentialCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObj)
                    {
                        _instance ??= new DifferentialCore();
                    }
                }
                return _instance;
            }
        }

        public async Task Clean(string sourcePath, string targetPath, string patchPath)
        {
            try
            {
                var fileManager = new StorageManager();
                var comparisonResult = fileManager.Compare(sourcePath, targetPath);
                foreach (var file in comparisonResult.DifferentNodes)
                {
                    var tempDir = GetTempDirectory(file, targetPath, patchPath);
                    var oldFile = comparisonResult.LeftNodes.FirstOrDefault(i => i.Name.Equals(file.Name));
                    var newFile = file;

                    if (oldFile is not null
                        && File.Exists(oldFile.FullName) 
                        && File.Exists(newFile.FullName) 
                        && string.Equals(oldFile.RelativePath, newFile.RelativePath))
                    {
                        if (!HashAlgorithmService.HashEquals(oldFile.FullName, newFile.FullName))
                        {
                            var tempPatchPath = Path.Combine(tempDir, $"{file.Name}{PATCH_FORMAT}");
                            await new BinaryHandler().Clean(oldFile.FullName, newFile.FullName, tempPatchPath);
                        }
                    }
                    else
                    {
                        File.Copy(newFile.FullName, Path.Combine(tempDir, Path.GetFileName(newFile.FullName)), true);
                    }
                }

                var exceptFiles = fileManager.Except(sourcePath, targetPath);
                if (exceptFiles is not null
                    && exceptFiles.Any())
                {
                    var path = Path.Combine(patchPath, DELETE_FILES_NAME);
                    StorageManager.CreateJson(path, exceptFiles);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Generate error : {ex.Message} !", ex.InnerException);
            }
        }
        
        public async Task Dirty(string appPath, string patchPath, string backupDirectory)
        {
            if (!Directory.Exists(appPath) || !Directory.Exists(patchPath))
            {
                return;
            }
            List<string> installfiles = new List<string>();
            try
            {
                List<string> skipDirectory = BlackListManager.Instance.SkipDirectorys.ToList();
                List<FileInfo> patchFiles = StorageManager.GetAllFiles(patchPath, skipDirectory);
                List<FileInfo> oldFiles2 = StorageManager.GetAllFiles(appPath, skipDirectory);
                HandleDeleteList(patchFiles, oldFiles2);
                oldFiles2 = StorageManager.GetAllFiles(appPath, skipDirectory);
                foreach (FileInfo oldFile in oldFiles2)
                {
                    var findFile = patchFiles.FirstOrDefault(f =>
                    Path.GetFileNameWithoutExtension(f.Name).Replace(PATCH_FORMAT, "").Equals(oldFile.Name));

                    if (findFile != null && string.Equals(Path.GetExtension(findFile.FullName), PATCH_FORMAT))
                    {
                        await DirtyPatch(oldFile.FullName, findFile.FullName);
                        installfiles.Add(findFile.FullName);
                    }
                }
                await DirtyUnknow(appPath, patchPath, backupDirectory);
            }
            catch (Exception ex)
            {
                Rollback(installfiles, appPath, patchPath, backupDirectory);
                throw new Exception("Dirty error : " + ex.Message + " !", ex.InnerException);
            }
        }

        #region Private Methods

        private static string GetTempDirectory(FileNode file, string targetPath, string patchPath)
        {
            var tempPath = file.FullName.Replace(targetPath, "").Replace(Path.GetFileName(file.FullName), "").Trim(Path.DirectorySeparatorChar);
            var tempDir = string.IsNullOrEmpty(tempPath) ? patchPath : Path.Combine(patchPath, tempPath);
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }
        
        private void HandleDeleteList(IEnumerable<FileInfo> patchFiles, IEnumerable<FileInfo> oldFiles)
        {
            var json = patchFiles.FirstOrDefault(i => i.Name.Equals(DELETE_FILES_NAME));
            if (json == null)
                return;
            
            var deleteFiles = StorageManager.GetJson<List<FileNode>>(json.FullName, FileNodesJsonContext.Default.ListFileNode);
            if (deleteFiles == null)
                return;
            
            //Match the collection of files to be deleted based on the file hash values stored in the JSON file.
            //var hashAlgorithm = new Sha256HashAlgorithm();
            var tempDeleteFiles = oldFiles.Where(old => deleteFiles.Any(del => del.Hash.SequenceEqual(HashAlgorithmService.ComputeHash(old.FullName)))).ToList();
            foreach (var file in tempDeleteFiles)
            {
                if (!File.Exists(file.FullName))
                    continue;
                
                File.SetAttributes(file.FullName, FileAttributes.Normal);
                File.Delete(file.FullName);
            }
        }

        private async Task DirtyPatch(string appPath, string patchPath)
        {
            try
            {
                if (!File.Exists(appPath) || !File.Exists(patchPath))
                    return;

                var newPath = Path.Combine(Path.GetDirectoryName(appPath)!, $"{Path.GetRandomFileName()}_{Path.GetFileName(appPath)}");
                await new BinaryHandler().Dirty(appPath, newPath, patchPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"RevertFile error : {ex.Message} !", ex.InnerException);
            }
        }
        private async Task DirtyUnknow(string appPath, string patchPath, string backupDirectory)
        {
            string appPath2 = appPath;
            string patchPath2 = patchPath;
            string backupDirectory2 = backupDirectory;
            await Task.Run(delegate
            {
                List<string> list = new List<string>();
                try
                {
                    StorageManager storageManager = new StorageManager();
                    ComparisonResult comparisonResult = storageManager.Compare(appPath2, patchPath2);
                    foreach (FileNode current in comparisonResult.DifferentNodes)
                    {
                        string extension = Path.GetExtension(current.FullName);
                        if (!BlackListManager.Instance.IsBlacklisted(extension))
                        {
                            string path = current.FullName.Replace(patchPath2, "").TrimStart(Path.DirectorySeparatorChar);
                            string text = Path.Combine(appPath2, path);
                            DirectoryInfo parent = Directory.GetParent(text);
                            if (parent != null && !parent.Exists)
                            {
                                parent.Create();
                            }
                            File.Copy(current.FullName, text, overwrite: true);
                            list.Add(current.FullName);
                        }
                    }
                    if (Directory.Exists(patchPath2))
                    {
                        StorageManager.DeleteDirectory(patchPath2);
                    }
                }
                catch (Exception ex)
                {
                    Rollback(list, appPath2, patchPath2, backupDirectory2);
                    throw new Exception("DirtyNew error : " + ex.Message + " !", ex.InnerException);
                }
            });
        }
        private void Rollback(List<string> installfiles, string appPath, string patchPath, string backupDirectory)
        {
            try
            {
                foreach (string file in installfiles)
                {
                    string targetFileName = file.Replace(patchPath, "").TrimStart(Path.DirectorySeparatorChar);
                    string targetPath = Path.Combine(appPath, targetFileName);
                    DirectoryInfo parentFolder = Directory.GetParent(targetPath);
                    if (parentFolder != null && !parentFolder.Exists)
                    {
                        parentFolder.Create();
                    }
                    string backupFile = Path.Combine(backupDirectory, targetFileName);
                    File.Copy(backupFile, targetPath, overwrite: true);
                }
            }
            catch
            {
            }
        }
        #endregion
    }
}