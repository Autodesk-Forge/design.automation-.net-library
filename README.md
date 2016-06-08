Design Automation .NET Library
========================
(Formely AutoCAD I/O)

[![.net](https://img.shields.io/badge/.net-4.5-green.svg)](http://www.microsoft.com/en-us/download/details.aspx?id=30653)
[![odata](https://img.shields.io/badge/odata-4.0-yellow.svg)](http://www.odata.org/documentation/)
[![ver](https://img.shields.io/badge/Design%20Automation%20API-2.0-blue.svg)](https://developer.autodesk.com/api/autocadio/v2/)
[![visual studio](https://img.shields.io/badge/Visual%20Studio-2012%7C2013-brightgreen.svg)](https://www.visualstudio.com/)
[![License](http://img.shields.io/:license-mit-red.svg)](http://opensource.org/licenses/MIT)

##Description
This is a library with helper methods to perform tasks related to Design Automation. It also provides the some workflows of AWS S3 such as uploading objects to bucket.

##Dependencies
* Visual Studio 2012. 2013 or 2015 should be also fine, but has not yet been tested.

##Setup/Usage Instructions
* Open the PlotToPDFService sample project in Visual Studio 2012
* Restore the packages of the project by [NuGet](https://www.nuget.org/). The simplest way is to Projects tab >> Enable NuGet Package Restore. Then right click the project>>"Manage NuGet Packages for Solution" >> "Restore" (top right of dialog)
* Add other missing references
* Build the library project to generate the class dll.
* Test with other client projects such as 
  * [design.automation-windows-services-sample](https://github.com/Developer-Autodesk/design.automation-windows-services-sample)
  * [design.automation-workflow-winform-sample] (https://github.com/Developer-Autodesk/design.automation-workflow-winform-sample)
  
