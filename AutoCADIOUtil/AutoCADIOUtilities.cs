using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.IO;

using Newtonsoft.Json;

namespace Autodesk
{
    public class AcadIOUtils
    {
        private static AcadIO.Container container = null;
        private static String _accessToken = String.Empty;
        private static RestSharp.RestClient restClient = new RestSharp.RestClient("https://developer.api.autodesk.com/autocad.io/v1/");

        /// <summary>
        /// Does setup of AutoCAD IO. 
        /// This method will need to be invoked once before any other methods of this
        /// utility class can be invoked.
        /// </summary>
        /// <param name="autocadioclientid">AutoCAD IO Client ID - can be obtained from developer.autodesk.com</param>
        /// <param name="autocadioclientsecret">AutoCAD IO Client Secret - can be obtained from developer.autodesk.com</param>
        public static void SetupAutoCADIOContainer(String autocadioclientid, String autocadioclientsecret)
        {
            try
            {
                String clientId = autocadioclientid;
                String clientSecret = autocadioclientsecret;

                Uri uri = new Uri("https://developer.api.autodesk.com/autocad.io/v1/");
                container = new AcadIO.Container(uri);
                container.Format.UseJson();

                using (var client = new HttpClient())
                {
                    var values = new List<KeyValuePair<string, string>>();
                    values.Add(new KeyValuePair<string, string>("client_id", clientId));
                    values.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
                    values.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
                    var requestContent = new FormUrlEncodedContent(values);
                    var response = client.PostAsync("https://developer.api.autodesk.com/authentication/v1/authenticate", requestContent).Result;
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var resValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                    _accessToken = resValues["token_type"] + " " + resValues["access_token"];

                    if (!string.IsNullOrEmpty(_accessToken))
                    {
                        container.SendingRequest2 += (sender, e) => e.RequestMessage.SetHeader("Authorization", _accessToken);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(String.Format("Error while connecting to https://developer.api.autodesk.com/autocad.io/v1/", ex.Message));
                container = null;
                throw;
            }
        }

        /// <summary>
        /// Get the activity name and script associated with the activities
        /// </summary>
        /// <returns>Key Value pair of the activity names and script associated with each activity</returns>
        public static Dictionary<String, String> GetActivityDetails()
        {
            Dictionary<String, String> activityDetails = new Dictionary<string, string>();
            try
            {
                foreach (AcadIO.Activity act in container.Activities)
                {
                    String userId = act.UserId;
                    String activityId = act.Id;
                    AcadIO.Instruction activityInstruction = act.Instruction;
                    activityDetails.Add(String.Format("{0}", activityId), act.Instruction.Script);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return activityDetails;
        }

        /// <summary>
        /// Get the appPackage name and resource associated with the appPackages
        /// </summary>
        /// <returns>Key Value pair of the appPackage name and resource url associated with each appPackage</returns>
        public static Dictionary<String, String> GetAppPackageDetails()
        {
            Dictionary<String, String> packageDetails = new Dictionary<string, string>();
            try
            {
                foreach (AcadIO.AppPackage appPackage in container.AppPackages)
                {
                    String userId = appPackage.UserId;
                    String packageId = appPackage.Id;
                    packageDetails.Add(String.Format("{0}", packageId), appPackage.Resource);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return packageDetails;
        }

        /// <summary>
        /// Creates a new activity
        /// </summary>
        /// <param name="activityId">Unique name identifying the activity</param>
        /// <param name="script">AutoCAD Script that is associated with the activity</param>
        /// <returns>true if activity was created, false otherwise</returns>
        public static bool CreateActivity(String activityId, String script)
        {
            if (String.IsNullOrEmpty(activityId) || String.IsNullOrEmpty(script))
                return false;

            bool created = false;
            try
            {
                foreach (AcadIO.Activity act1 in container.Activities)
                {
                    if (activityId.Equals(act1.Id))
                    {
                        // Activity already exists !
                        Console.WriteLine(String.Format("Activity with name {0} already exists !", act1.Id));
                        return false;
                    }
                }

                // Identify the result file in the script.
                // The result file name can be any name of your choice, AutoCADIO does not have a restriction on that.
                // But just to make the code generic and to have it identify the result file automatically from the script,
                // we look for anything that sounds like result.pdf, Result.dwf, RESULT.DWG etc.
                String resultLocalFileName = String.Empty;
                foreach (Match m in Regex.Matches(script, "(?i)result.[a-z][a-z][a-z]"))
                {
                    resultLocalFileName = m.Value;
                }

                if (String.IsNullOrEmpty(resultLocalFileName))
                {// Script did not have file name like Result.pdf, Result.dwg ....
                    Console.WriteLine(String.Format("Could not identify the result output file in the provided script ! Please use result.* as the output of the script."));
                    return false;
                }

                // Create a new activity
                AcadIO.Activity act = new AcadIO.Activity()
                {
                    UserId = "", // User id will be our AutoCADIO client Id
                    Id = activityId,
                    Version = 1,
                    Instruction = new AcadIO.Instruction()
                    {
                        Script = script
                    },
                    Parameters = new AcadIO.Parameters()
                    {
                        InputParameters = { new AcadIO.Parameter() { Name = "HostDwg", LocalFileName = "$(HostDwg)" } },
                        OutputParameters = { new AcadIO.Parameter() { Name = "Result", LocalFileName = resultLocalFileName } }
                    },
                    RequiredEngineVersion = "20.0"
                };
                container.AddToActivities(act);
                container.SaveChanges();

                created = true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return created;
        }

        /// <summary>
        /// Removes an existing activity
        /// </summary>
        /// <param name="activityId">Unique name identifying the activity to be removed. Activity with this name must already exist.</param>
        /// <returns>true if activity was removed, false otherwise</returns>
        public static bool DeleteActivity(String activityId)
        {
            bool deleted = false;

            try
            {
                foreach (AcadIO.Activity act1 in container.Activities)
                {
                    if (activityId.Equals(act1.Id))
                    {
                        if (act1.UserId.Equals("Shared"))
                            return false; // Dont attempt deleting the shared ones. Those are provided by AutoCAD IO.

                        RestSharp.RestRequest req = new RestSharp.RestRequest();
                        req.Resource = String.Format("Activities(UserId='{0}',Id='{1}')", act1.UserId, act1.Id);
                        req.Method = RestSharp.Method.DELETE;
                        req.AddParameter("Authorization", _accessToken, RestSharp.ParameterType.HttpHeader);

                        RestSharp.IRestResponse resp = restClient.Execute(req);
                        String response = resp.Content;
                        if (resp.ResponseStatus == RestSharp.ResponseStatus.Completed)
                            deleted = true;

                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return deleted;
        }

        /// <summary>
        /// Updates an existing activity script
        /// </summary>
        /// <param name="activityId">Unique name identifying the activity to be updated. Activity with this name must already exist.</param>
        /// <param name="script">Script to replace the existing one associated with the activity</param>
        /// <returns>true if activity was updated, false otherwise</returns>
        public static bool UpdateActivity(String activityId, String script)
        {
            if (String.IsNullOrEmpty(activityId) || String.IsNullOrEmpty(script))
                return false;

            // Identify the result file in the script.
            // The result file name can be any name of your choice, AutoCAD IO does not have a restriction on that.
            // But just to make the code generic and to have it identify the result file automatically from the script,
            // we look for anything that sounds like result.pdf, Result.dwf, RESULT.DWG etc.
            String resultLocalFileName = String.Empty;
            foreach (Match m in Regex.Matches(script, "(?i)result.[a-z][a-z][a-z]"))
            {
                resultLocalFileName = m.Value;
            }

            if (String.IsNullOrEmpty(resultLocalFileName))
            {// Script did not have file name like Result.pdf, Result.dwg ....
                Console.WriteLine(String.Format("Could not identify the result output file in the provided script ! Please use result.* as the output of the script."));
                return false;
            }

            bool activityUpdated = false;

            try
            {
                foreach (AcadIO.Activity act1 in container.Activities)
                {
                    if (activityId.Equals(act1.Id))
                    {
                        // Activity already exists 
                        var ins = new AcadIO.Instruction();
                        ins.Script = script;
                        act1.Instruction = ins;
                        container.UpdateObject(act1);
                        container.SaveChanges(System.Data.Services.Client.SaveChangesOptions.PatchOnUpdate);
                        activityUpdated = true;
                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return activityUpdated;
        }

        /// <summary>
        /// Creates a new WorkItem
        /// </summary>
        /// <param name="activityId">ActivityId that is associated with the new workItem</param>
        /// <param name="hostDwgS3Url">Url of the drawing after it has been uploaded to Amazon S3</param>
        /// <returns>true if WorkItem was created, false otherwise</returns>
        public static String SubmitWorkItem(String activityId, String hostDwgS3Url)
        {
            try
            {
                String userId = String.Empty;
                foreach (AcadIO.Activity act1 in container.Activities)
                {
                    if (activityId.Equals(act1.Id))
                    {
                        // Found the activity using which a WorkItem is to be submitted to AutoCAD IO
                        userId = act1.UserId;
                        break;
                    }
                }

                AcadIO.WorkItem wi = new AcadIO.WorkItem()
                {
                    UserId = "",
                    Id = "",
                    Arguments = new AcadIO.Arguments(),
                    Version = 1,
                    ActivityId = new AcadIO.EntityId() { UserId = userId, Id = activityId }
                };

                // Drawing
                wi.Arguments.InputArguments.Add(new AcadIO.Argument()
                {
                    Name = "HostDwg",
                    Resource = hostDwgS3Url,
                    StorageProvider = "Generic",
                    HttpVerb = "GET"
                });

                wi.Arguments.OutputArguments.Add(new AcadIO.Argument()
                {
                    Name = "Result",
                    Resource = null,
                    StorageProvider = "Generic",
                    HttpVerb = "POST",
                });

                container.MergeOption = System.Data.Services.Client.MergeOption.OverwriteChanges;
                container.AddToWorkItems(wi);
                container.SaveChanges();

                Console.WriteLine("Submitted WorkItem. WorkItem UserId = {0}, WorkItem Id= {1}", wi.UserId, wi.Id);
                Console.WriteLine("Checking WorkItem status...");

                do
                {
                    System.Threading.Thread.Sleep(5000);
                    wi = container.WorkItems.Where(p => p.UserId == wi.UserId && p.Id == wi.Id).SingleOrDefault();
                    Console.WriteLine("WorkItem Status : {0}", wi.Status);
                } while (wi.Status == "Pending" || wi.Status == "InProgress");

                Console.WriteLine("WorkItem Status : {0}", wi.Status);
                Console.WriteLine("The result is downloadable at {0}", wi.Arguments.OutputArguments.First().Resource);

                return wi.Arguments.OutputArguments.First().Resource;
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine(String.Format("WorkItem using {0} activity was not processed. {1}", activityId, ex.Message));
            }
            catch (System.Data.Services.Client.DataServiceRequestException ex)
            {
                Console.WriteLine(ex.InnerException.Message);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(String.Format("WorkItem using {0} activity was not processed. {1}", activityId, ex.Message));
            }

            return String.Empty;
        }

        /// <summary>
        /// Creates a new AppPackage 
        /// </summary>
        /// <param name="packageId">Unique name identifying the appPackage to be created. AppPackage must not already exist with the same name.</param>
        /// <param name="bundleFolderPath">Local folder path to the autoloader bundle. This path must contain the PackageContents.xml</param>
        /// <returns>true if appPackage was created, false otherwise</returns>
        public static bool CreateAppPackageFromBundle(String packageId, string bundleFolderPath)
        {
            bool appPackageCreated = false;

            if (String.IsNullOrEmpty(packageId))
                return appPackageCreated;

            // Check if the selected folder contains "PackageContents.xml" as a check for 
            // a valid bundle folder that we can zip and upload
            if (File.Exists(Path.Combine(bundleFolderPath, "PackageContents.xml")) == false)
            {
                Console.WriteLine(String.Format("{0} is not a bundle folder. Please select a valid bundle.", bundleFolderPath));
                return appPackageCreated;
            }

            String bundleName = Path.GetFileName(bundleFolderPath);

            String tempPath = System.IO.Path.GetTempPath();
            String packageZipFilePath = Path.Combine(tempPath, String.Format("{0}.zip", bundleName));
            if (File.Exists(packageZipFilePath))
            {
                // Delete existing zip file if any from the temp folder
                File.Delete(packageZipFilePath);
            }

            System.IO.Compression.ZipFile.CreateFromDirectory(bundleFolderPath, packageZipFilePath, System.IO.Compression.CompressionLevel.Optimal, true);

            if (File.Exists(packageZipFilePath))
            {
                // Zip was created. Create a App Package using it.
                if (CreateAppPackageFromZip(packageId, packageZipFilePath))
                {// App Package created ok
                    appPackageCreated = true;
                    Console.WriteLine(String.Format("Created new app package."));
                }
                else
                {
                    Console.WriteLine(String.Format("Sorry, could not create new app package."));
                    appPackageCreated = false;
                }
            }

            return appPackageCreated;
        }

        /// <summary>
        /// Creates a new AppPackage
        /// </summary>
        /// <param name="packageId">Unique name identifying the appPackage to be created. AppPackage must not already exist with the same name.</param>
        /// <param name="packageZipFilePath">Local path to the autoloader bundle after it has been zipped.</param>
        /// <returns>true if appPackage was created, false otherwise</returns>
        public static bool CreateAppPackageFromZip(String packageId, string packageZipFilePath)
        {
            if (String.IsNullOrEmpty(packageId) || !File.Exists(packageZipFilePath))
                return false;

            AcadIO.AppPackage appPackage = null;

            foreach (AcadIO.AppPackage pack in container.AppPackages)
            {
                if (pack.Id.Equals(packageId))
                {
                    appPackage = pack;
                    break;
                }
            }

            if (appPackage == null)
            {
                try
                {
                    // First step -- query for the url to upload the AppPackage file
                    UriBuilder builder = new UriBuilder(container.BaseUri);
                    builder.Path += "AppPackages/GenerateUploadUrl";
                    var url = container.Execute<string>(builder.Uri, "POST", true, null).First();

                    // Second step -- upload AppPackage file
                    if (GeneralUtils.UploadObject(url, packageZipFilePath))
                    {
                        // third step -- after upload, create the AppPackage
                        appPackage = new AcadIO.AppPackage()
                        {
                            UserId = "", // Will automatically assigned by AutoCAD IO 
                            Id = packageId,
                            Version = 1,
                            RequiredEngineVersion = "20.0",
                            Resource = url
                        };
                        container.AddToAppPackages(appPackage);
                        container.SaveChanges();
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return (appPackage != null);
        }

        /// <summary>
        /// Links an appPakage with an existing activity
        /// </summary>
        /// <param name="activityId">Unique name identifying the activity to be linked with an AppPackage. Activity with this name must already exist.</param>
        /// <param name="packageId">Unique name identifying the package to be linked with the activity. AppPackage with this name must already exist.</param>
        /// <returns>true if appPackage was linked with the activity, false otherwise</returns>
        public static bool LinkAppPackage2Activity(String activityId, String packageId)
        {
            if (String.IsNullOrEmpty(activityId) || String.IsNullOrEmpty(packageId))
                return false;

            //Link the activity with the app package
            foreach (AcadIO.Activity act in container.Activities)
            {
                if (activityId.Equals(act.Id))
                {
                    foreach (AcadIO.AppPackage pack in container.AppPackages)
                    {
                        if (packageId.Equals(pack.Id))
                        {
                            try
                            {
                                container.AddLink(act, "AppPackages", pack);
                                container.SaveChanges();
                                return true;
                            }
                            catch (System.Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Removes an existing appPackage
        /// </summary>
        /// <param name="activityId">Unique name identifying the appPackage to be removed. appPackage with this name must already exist.</param>
        /// <returns>true if appPackage was removed, false otherwise</returns>
        /// 
        public static bool DeletePackage(string packageId)
        {
            if (String.IsNullOrEmpty(packageId))
                return false;

            bool deleted = false;
            try
            {
                foreach (AcadIO.AppPackage pack in container.AppPackages)
                {
                    if (packageId.Equals(pack.Id))
                    {
                        RestSharp.RestRequest req = new RestSharp.RestRequest();
                        req.Resource = String.Format("AppPackages(UserId='{0}',Id='{1}')", pack.UserId, pack.Id);
                        req.Method = RestSharp.Method.DELETE;
                        req.AddParameter("Authorization", _accessToken, RestSharp.ParameterType.HttpHeader);

                        RestSharp.IRestResponse resp = restClient.Execute(req);
                        String response = resp.Content;
                        if (resp.ResponseStatus == RestSharp.ResponseStatus.Completed)
                            deleted = true;

                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return deleted;
        }
    }
}
