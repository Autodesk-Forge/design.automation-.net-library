using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;

namespace Autodesk
{
    public class GeneralUtils
    {
        /// <summary>
        /// Bucket name in Amazon S3 to be used for uploading drawing
        /// </summary>
        public static String S3BucketName
        {
            get;
            set;
        }

        /// <summary>
        /// Identifies the commands exposed by a bundle represented by the "packageZipFilePath"
        /// </summary>
        /// <param name="packageZipFilePath">Path to the zip file of the bundle</param>
        /// <param name="localCommands">Returns the local commands identified from packagecontents.xml</param>
        /// <param name="globalCommands">Returns the global commands identified from packagecontents.xml</param>
        public static void FindListedCommands(String packageZipFilePath, ref StringCollection localCommands, ref StringCollection globalCommands)
        {
            String tempPath = System.IO.Path.GetTempPath();
            if (File.Exists(packageZipFilePath))
            {
                using (System.IO.Compression.ZipArchive za = System.IO.Compression.ZipFile.OpenRead(packageZipFilePath))
                {
                    foreach (ZipArchiveEntry entry in za.Entries)
                    {
                        if (entry.FullName.EndsWith("PackageContents.xml", StringComparison.OrdinalIgnoreCase))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(tempPath, entry.FullName)));
                            String packageContentsXmlFilePath = Path.Combine(tempPath, entry.FullName);

                            if (File.Exists(packageContentsXmlFilePath))
                                File.Delete(packageContentsXmlFilePath);

                            entry.ExtractToFile(packageContentsXmlFilePath);

                            localCommands.Clear();
                            globalCommands.Clear();

                            System.IO.TextReader tr = new System.IO.StreamReader(packageContentsXmlFilePath);
                            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(tr))
                            {
                                while (reader.ReadToFollowing("Command"))
                                {
                                    reader.MoveToFirstAttribute();
                                    if (reader.Name.Equals("Local"))
                                        localCommands.Add(reader.Value);
                                    else if (reader.Name.Equals("Global"))
                                        globalCommands.Add(reader.Value);

                                    while (reader.MoveToNextAttribute())
                                    {
                                        if (reader.Name.Equals("Local"))
                                            localCommands.Add(reader.Value);
                                        else if (reader.Name.Equals("Global"))
                                            globalCommands.Add(reader.Value);
                                    }
                                }
                            }
                            tr.Close();

                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Uploads the contents from "filePath" to the url provided
        /// </summary>
        /// <param name="url">Upload url</param>
        /// <param name="filePath">Local file path</param>
        /// <returns>true if uploaded, false otherwise</returns>
        public static bool UploadObject(string url, string filePath)
        {
            bool uploaded = false;
            System.Net.HttpWebRequest httpRequest = System.Net.HttpWebRequest.Create(url) as System.Net.HttpWebRequest;
            httpRequest.Method = "PUT";
            using (Stream dataStream = httpRequest.GetRequestStream())
            {
                byte[] buffer = new byte[4096];
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    int bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        dataStream.Write(buffer, 0, bytesRead);
                    }
                }
            }
            System.Net.HttpWebResponse response = httpRequest.GetResponse() as System.Net.HttpWebResponse;
            uploaded = (response.StatusCode == System.Net.HttpStatusCode.OK);
            return uploaded;
        }

        /// <summary>
        /// Downloads and saves the contents from the url in a local file
        /// </summary>
        /// <param name="url">Download url</param>
        /// <param name="localFilePath">Local file path to which the contents were saved</param>
        /// <returns>true if downloaded, false otherwise</returns>
        public static bool Download(String url, ref String localFilePath)
        {
            try
            {
                if (String.IsNullOrEmpty(url))
                    return false;

                // Load the url in web browser
                if (url.StartsWith("http://") || url.StartsWith("https://"))
                {
                    String filename = Path.GetFileName(new Uri(url).AbsolutePath);
                    localFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

                    using (System.Net.WebClient wc = new System.Net.WebClient())
                    {
                        wc.DownloadFile(url, localFilePath);
                    }

                    if (!String.IsNullOrEmpty(localFilePath))
                    {
                        return true;
                    }
                }
            }
            catch (System.UriFormatException)
            {
                Console.WriteLine(url + "could not be loaded.");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            localFilePath = String.Empty;
            return false;
        }

        /// <summary>
        /// Uploads the drawing to Amazon S3
        /// </summary>
        /// <param name="dwgFilePath"></param>
        /// <returns>Presigned Url of the uploaded drawing file in Amazon S3</returns>
        public static String UploadDrawingToS3(String dwgFilePath)
        {
            String s3URL = String.Empty;

            try
            {
                if (!System.IO.File.Exists(dwgFilePath))
                    return s3URL;

                String keyName = System.IO.Path.GetFileName(dwgFilePath);

                using (Amazon.S3.IAmazonS3 client = new Amazon.S3.AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1))
                {
                    Amazon.S3.Model.PutObjectRequest putRequest1 = new Amazon.S3.Model.PutObjectRequest
                    {
                        BucketName = S3BucketName,
                        Key = keyName,
                        ContentBody = "sample text"
                    };

                    Amazon.S3.Model.PutObjectResponse response1 = client.PutObject(putRequest1);

                    Amazon.S3.Model.PutObjectRequest putRequest2 = new Amazon.S3.Model.PutObjectRequest
                    {
                        BucketName = S3BucketName,
                        Key = keyName,
                        FilePath = dwgFilePath,
                        ContentType = "application/acad"
                    };
                    putRequest2.Metadata.Add("x-amz-meta-title", keyName);

                    Amazon.S3.Model.PutObjectResponse response2 = client.PutObject(putRequest2);

                    Amazon.S3.Model.GetPreSignedUrlRequest request1 = new Amazon.S3.Model.GetPreSignedUrlRequest
                    {
                        BucketName = S3BucketName,
                        Key = keyName,
                        Expires = DateTime.Now.AddMinutes(5)
                    };

                    s3URL = client.GetPreSignedURL(request1);

                    Console.WriteLine(s3URL);
                }
            }
            catch (Amazon.S3.AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Check the provided AWS Credentials.");
                    Console.WriteLine("For service sign up go to http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("Error occurred. Message:'{0}' when writing an object", amazonS3Exception.Message);
                }
            }
            return s3URL;
        }
    }
}
