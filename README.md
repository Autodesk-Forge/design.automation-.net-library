Design Automation .NET Library
========================
(Formely AutoCAD I/O)

[![.net](https://img.shields.io/badge/.net-4.5-green.svg)](http://www.microsoft.com/en-us/download/details.aspx?id=30653)
[![odata](https://img.shields.io/badge/odata-4.0-yellow.svg)](http://www.odata.org/documentation/)
[![Design Automation](https://img.shields.io/badge/Design%20Automation-v2-green.svg)](http://developer.autodesk.com/)
[![visual studio](https://img.shields.io/badge/Visual%20Studio-2012%7C2013%7C2015-blue.svg)](https://www.visualstudio.com/)
[![License](http://img.shields.io/:license-mit-red.svg)](http://opensource.org/licenses/MIT)

##Description
This is a library with helper methods to perform tasks related to Design Automation. It also provides the some workflows of AWS S3 such as uploading objects to bucket. 

See full [Design Automation API v2 Documentation](https://developer.autodesk.com/en/docs/design-automation/v2/overview/)

##Dependencies
* Visual Studio 2012, 2013, 2015. The latest test is on VS2015.

##Setup/Usage Instructions

* Restore the packages of the project by [NuGet](https://www.nuget.org/). The simplest way is
  * VS2012: Projects tab >> Enable NuGet Package Restore. Then right click the project>>"Manage NuGet Packages for Solution" >> "Restore" (top right of dialog)
  * VS2013/VS2015:  right click the project>>"Manage NuGet Packages for Solution" >> "Restore" (top right of dialog)
* Add other missing references
* Build the library project to generate the dll.
* Test with other client projects such as 
  * [design.automation-windows-services-sample](https://github.com/Developer-Autodesk/design.automation-windows-services-sample)
  * [design.automation-workflow-winform-sample](https://github.com/Developer-Autodesk/design.automation-workflow-winform-sample)
  
* important tip: be sure to connect to the endpoint which is the same region of your bucket!  line 168 of [GeneralUtilities.cs](./AutoCADIOUtil/GeneralUtilities.cs)
  ![thumbnail](./readme/AWS-region.png)
