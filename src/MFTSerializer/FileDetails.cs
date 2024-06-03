using System;
using System.IO;
using System.Text.Json;

namespace MFTSerializer;

public class FileDetails(string fullPath) : IComparable<FileDetails>
{
    public string FullPath { get; set; } = fullPath;
    public string FileName { get; set; } = System.IO.Path.GetFileName(fullPath);
    public DateTime LastModified { get; set; } = File.GetLastWriteTime(fullPath);
    public DateTime DateCreated { get; set; } = File.GetCreationTime(fullPath);

    public new string ToString()
    {
        return JsonSerializer.Serialize(this);
    }

    public string ToSQLString()
    {
        return @$"('{FileName}', '{FullPath.Replace("\\", "\\\\")}', '{LastModified.ToString()}', '{DateCreated.ToString()}')";
    }

    public int CompareTo(FileDetails other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return LastModified.CompareTo(other.LastModified);
    }
}