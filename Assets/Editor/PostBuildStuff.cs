using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PostBuildStuff 
{
    public static void UploadBuild(string exportPath)
    {
        Debug.Log($"Finished build: {exportPath}");
        var dirFiles = Directory.EnumerateFiles(exportPath);
        foreach(var file in dirFiles)
        {
            Debug.Log($"Found file {file}");
        }
        var dirs = Directory.EnumerateDirectories(exportPath);
        foreach (var dir in dirs)
        {
            Debug.Log($"Found directory {dir}");
        }
    }
}
