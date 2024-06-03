using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace MFTSerializer;

public class FileDetails(string fullPath) : IComparable<FileDetails>
{
    public enum FileDetailsType
    {
        FILE,
        FOLDER,
        APPLICATION
    }
    public string FullPath { get; set; } = fullPath;
    public string FileName { get; set; } = System.IO.Path.GetFileName(fullPath);
    public DateTime LastModified { get; set; } = File.GetLastWriteTime(fullPath);
    public DateTime DateCreated { get; set; } = File.GetCreationTime(fullPath);
    public FileDetailsType DetailsType {
        get
        {
            if (Path.HasExtension(FileName))
            {
                if (Path.GetExtension(FullPath) == ".exe")
                {
                    return FileDetailsType.APPLICATION;
                }
                return FileDetailsType.FILE;
            }
            return FileDetailsType.FOLDER;
        }
    }

    public new string ToString()
    {
        return JsonSerializer.Serialize(this);
    }

    public string ToSQLString()
    {
        if (this.DetailsType == FileDetailsType.APPLICATION)
        {
            FileVersionInfo info = System.Diagnostics.FileVersionInfo.GetVersionInfo(FullPath);
            string name = String.IsNullOrWhiteSpace(info.FileName)
                ? Path.GetFileNameWithoutExtension(FileName)
                : info.FileName;
            string desc = String.IsNullOrWhiteSpace(info.FileDescription)
                ? Path.GetFileNameWithoutExtension(FileName)
                : info.FileName;
            return $@"('{name}', '{desc}', '{FullPath.Replace("\\", "\\\\")}')";
        }
        return @$"('{FileName}', '{FullPath.Replace("\\", "\\\\")}', '{LastModified.ToString()}', '{DateCreated.ToString()}')";
    }

    public int CompareTo(FileDetails other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return LastModified.CompareTo(other.LastModified);
    }
}