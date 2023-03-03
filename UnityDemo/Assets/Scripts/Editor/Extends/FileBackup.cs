using System;
using System.IO;
using System.Linq;

public class FileBackup
{
    public readonly static string CODE_GENERATE = "CodeGenerate";

    string _rootPath;
    string _backupPathRoot;

    public FileBackup(string backupType, string rootPath = null)
    {
        if (string.IsNullOrEmpty(rootPath))
        {
            rootPath = Directory.GetCurrentDirectory();
        }

        var now = DateTime.Now;

        _rootPath = System.IO.Path.GetFullPath(rootPath);
        _backupPathRoot = Path.Combine(_rootPath, 
            "Backup", backupType,
            $"{now.Year}-{now.Month}-{now.Day}", 
            $"{now.Hour}-{now.Minute}-{now.Second}");

        if (Directory.Exists(_backupPathRoot))
        {
            int i = 0;

            string newBackupPathRoot = $"{_backupPathRoot}_{i}";

            while (Directory.Exists(newBackupPathRoot))
                newBackupPathRoot = $"{_backupPathRoot}_{++i}";

            _backupPathRoot = newBackupPathRoot;
        }
    }

    static readonly char[] sep = new char[] { '\\', '/' };

    public string ToHrefText(string filepath)
    {
        filepath = filepath.Replace("\\", "/");
        return $"<a href=\"file:///{filepath}/\">{Path.GetFileName(filepath)}</a>";
    }

    void CreateDirectoryRecursively(string path)
    {
        string[] pathParts = path.Split(sep);

        for (int i = 0; i < pathParts.Length; i++)
        {
            if (i > 0)
                pathParts[i] = $"{pathParts[i - 1]}/{pathParts[i]}";

            if (!Directory.Exists(pathParts[i]))
                Directory.CreateDirectory(pathParts[i]);
        }
    }

    /// <summary>
    /// ���ݴ���
    /// </summary>
    /// <param name="sourceFilepath"></param>
    /// <returns></returns>
    public string Backup(string sourceFilepath)
    {
        string fullPath = Path.GetFullPath(sourceFilepath);

        if (!fullPath.StartsWith(Directory.GetCurrentDirectory()))
            throw new Exception($"���ܱ����� {Directory.GetCurrentDirectory()} ��Ŀ¼�µ��ļ�������Ŀ�����: {fullPath}"); 

        fullPath = fullPath.Replace(Directory.GetCurrentDirectory(), "");

        if (fullPath.StartsWith("\\") || fullPath.StartsWith("/"))
            fullPath = fullPath.Substring(1);

        string dstFileName = Path.Combine(_backupPathRoot, fullPath);

        string dstDir = Path.GetDirectoryName(dstFileName);

        if (!Directory.Exists(dstDir))
        {
            CreateDirectoryRecursively(dstDir);
        }

        File.Copy(sourceFilepath, dstFileName);

        return dstFileName;
    }
}