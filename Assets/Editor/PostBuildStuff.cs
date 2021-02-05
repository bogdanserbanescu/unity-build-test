using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public class PostBuildStuff
{

    private const string awsBucketName = "ow-build-artifacts";
    private static string awsAccessKey;
    private static string awsSecretKey;
    private static string awsURLBaseVirtual = "";

    public static void UploadBuild(string exportPath)
    {
        Debug.Log($"Finished build: {exportPath}");
        awsAccessKey = Environment.GetEnvironmentVariable("AWSACCESSKEY");
        awsSecretKey = Environment.GetEnvironmentVariable("AWSSECRETKEY");
#if UNITY_ANDROID
        UploadAndroidBuild(exportPath);
#else
        UploadWebglBuild(exportPath);
#endif

    }

    private static void UploadAndroidBuild(string exportPath)
    {
        UploadFileToAWS3($"unity/master/dev/LuckyLandSlots.apk", exportPath);
    }
    private static void UploadWebglBuild(string exportPath)
    {
        var dirFiles = Directory.EnumerateFiles($"{exportPath}/Build");
        foreach (var file in dirFiles)
        {
            var fileName = file.Substring(file.LastIndexOf('/') + 1);
            var extension = fileName.Substring(fileName.IndexOf('.'));
            Debug.Log($"Found file {fileName} - {extension} in build output : {file}");
            UploadFileToAWS3($"unity/master/dev/webgl/build{extension}", file);
        }
    }


    [MenuItem("Tools/AWS Upload", false, 68)]
    public static void Upload()
    {
        UploadFileToAWS3("unity/master/dev/luckylandslots.apk", "D:\\git\\luckylandcasino\\client\\lls2020.2.apk");
    }
    public static void UploadFileToAWS3(string FileName, string FilePath)
    {
        awsURLBaseVirtual = "https://" +
          awsBucketName +
          ".s3.amazonaws.com/";
        string currentAWS3Date =
            System.DateTime.UtcNow.ToString(
                "ddd, dd MMM yyyy HH:mm:ss ") +
                "GMT";
        string canonicalString =
            "PUT\n\n\n\nx-amz-date:" +
            currentAWS3Date + "\n/" +
            awsBucketName + "/" + FileName;
        UTF8Encoding encode = new UTF8Encoding();
        HMACSHA1 signature = new HMACSHA1();
        signature.Key = encode.GetBytes(awsSecretKey);
        byte[] bytes = encode.GetBytes(canonicalString);
        byte[] moreBytes = signature.ComputeHash(bytes);
        string encodedCanonical = Convert.ToBase64String(moreBytes);
        string aws3Header = "AWS " +
            awsAccessKey + ":" +
            encodedCanonical;
        string URL3 = awsURLBaseVirtual + FileName;
        WebRequest requestS3 = (HttpWebRequest)WebRequest.Create(URL3);
        requestS3.Headers.Add("Authorization", aws3Header);
        requestS3.Headers.Add("x-amz-date", currentAWS3Date);
        byte[] fileRawBytes = File.ReadAllBytes(FilePath);
        requestS3.ContentLength = fileRawBytes.Length;
        requestS3.Method = "PUT";
        Stream S3Stream = requestS3.GetRequestStream();

        S3Stream.Write(fileRawBytes, 0, fileRawBytes.Length);
        Debug.Log("Sent bytes: " +
            requestS3.ContentLength +
            ", for file: " +
            FileName);
        S3Stream.Close();
    }
}
