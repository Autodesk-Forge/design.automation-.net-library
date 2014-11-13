library-autocadio-dotnet
========================

Library with helper methods to perform tasks related to AutoCAD IO 

Following methods are provided by this library :

	<member name="Autodesk.AcadIOUtils.SetupAutoCADIOContainer(System.String,System.String)">
		<summary>
		Does setup of AutoCAD IO. 
		This method will need to be invoked once before any other methods of this
		utility class can be invoked.
		</summary>
		<param name="autocadioclientid">AutoCAD IO Client ID - can be obtained from developer.autodesk.com</param>
		<param name="autocadioclientsecret">AutoCAD IO Client Secret - can be obtained from developer.autodesk.com</param>
	</member>
	
	<member name="Autodesk.AcadIOUtils.GetActivityDetails">
		<summary>
		Get the activity name and script associated with the activities
		</summary>
		<returns>Key Value pair of the activity names and script associated with each activity</returns>
	</member>
	
	<member name="Autodesk.AcadIOUtils.GetAppPackageDetails">
		<summary>
		Get the appPackage name and resource associated with the appPackages
		</summary>
		<returns>Key Value pair of the appPackage name and resource url associated with each appPackage</returns>
	</member>
	
	<member name="Autodesk.AcadIOUtils.CreateActivity(System.String,System.String)">
		<summary>
		Creates a new activity
		</summary>
		<param name="activityId">Unique name identifying the activity</param>
		<param name="script">AutoCAD Script that is associated with the activity</param>
		<returns>true if activity was created, false otherwise</returns>
	</member>
	
	<member name="Autodesk.AcadIOUtils.DeleteActivity(System.String)">
		<summary>
		Removes an existing activity
		</summary>
		<param name="activityId">Unique name identifying the activity to be removed. Activity with this name must already exist.</param>
		<returns>true if activity was removed, false otherwise</returns>
	</member>
	
	<member name="Autodesk.AcadIOUtils.UpdateActivity(System.String,System.String)">
		<summary>
		Updates an existing activity script
		</summary>
		<param name="activityId">Unique name identifying the activity to be updated. Activity with this name must already exist.</param>
		<param name="script">Script to replace the existing one associated with the activity</param>
		<returns>true if activity was updated, false otherwise</returns>
	</member>
	
	<member name="Autodesk.AcadIOUtils.SubmitWorkItem(System.String,System.String)">
		<summary>
		Creates a new WorkItem
		</summary>
		<param name="activityId">ActivityId that is associated with the new workItem</param>
		<param name="hostDwgS3Url">Url of the drawing after it has been uploaded to Amazon S3</param>
		<returns>true if WorkItem was created, false otherwise</returns>
	</member>
	
	<member name="Autodesk.AcadIOUtils.CreateAppPackageFromBundle(System.String,System.String)">
		<summary>
		Creates a new AppPackage 
		</summary>
		<param name="packageId">Unique name identifying the appPackage to be created. AppPackage must not already exist with the same name.</param>
		<param name="bundleFolderPath">Local folder path to the autoloader bundle. This path must contain the PackageContents.xml</param>
		<returns>true if appPackage was created, false otherwise</returns>
	</member>
	
	<member name="Autodesk.AcadIOUtils.CreateAppPackageFromZip(System.String,System.String)">
		<summary>
		Creates a new AppPackage
		</summary>
		<param name="packageId">Unique name identifying the appPackage to be created. AppPackage must not already exist with the same name.</param>
		<param name="packageZipFilePath">Local path to the autoloader bundle after it has been zipped.</param>
		<returns>true if appPackage was created, false otherwise</returns>
	</member>
	
	<member name="Autodesk.AcadIOUtils.LinkAppPackage2Activity(System.String,System.String)">
		<summary>
		Links an appPakage with an existing activity
		</summary>
		<param name="activityId">Unique name identifying the activity to be linked with an AppPackage. Activity with this name must already exist.</param>
		<param name="packageId">Unique name identifying the package to be linked with the activity. AppPackage with this name must already exist.</param>
		<returns>true if appPackage was linked with the activity, false otherwise</returns>
	</member>
	
	<member name="Autodesk.AcadIOUtils.DeletePackage(System.String)">
		<summary>
		Removes an existing appPackage
		</summary>
		<param name="activityId">Unique name identifying the appPackage to be removed. appPackage with this name must already exist.</param>
		<returns>true if appPackage was removed, false otherwise</returns>
	</member>
	
	<member name="Autodesk.GeneralUtils.FindListedCommands(System.String,System.Collections.Specialized.StringCollection@,System.Collections.Specialized.StringCollection@)">
		<summary>
		Identifies the commands exposed by a bundle represented by the "packageZipFilePath"
		</summary>
		<param name="packageZipFilePath">Path to the zip file of the bundle</param>
		<param name="localCommands">Returns the local commands identified from packagecontents.xml</param>
		<param name="globalCommands">Returns the global commands identified from packagecontents.xml</param>
	</member>
	
	<member name="Autodesk.GeneralUtils.UploadObject(System.String,System.String)">
		<summary>
		Uploads the contents from "filePath" to the url provided
		</summary>
		<param name="url">Upload url</param>
		<param name="filePath">Local file path</param>
		<returns>true if uploaded, false otherwise</returns>
	</member>
	
	<member name="Autodesk.GeneralUtils.Download(System.String,System.String@)">
		<summary>
		Downloads and saves the contents from the url in a local file
		</summary>
		<param name="url">Download url</param>
		<param name="localFilePath">Local file path to which the contents were saved</param>
		<returns>true if downloaded, false otherwise</returns>
	</member>
	
	<member name="Autodesk.GeneralUtils.UploadDrawingToS3(System.String)">
		<summary>
		Uploads the drawing to Amazon S3
		</summary>
		<param name="dwgFilePath"></param>
		<returns>Presigned Url of the uploaded drawing file in Amazon S3</returns>
	</member>
	
	<member name="Autodesk.GeneralUtils.S3BucketName">
		<summary>
		Bucket name in Amazon S3 to be used for uploading drawing
		</summary>
	</member>