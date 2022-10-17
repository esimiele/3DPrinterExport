-------------------------------------------------
-----         Ab3d.PowerToys Readme         -----
-------------------------------------------------

Ab3d.PowerToys is an ultimate WPF and WinForms 3D toolkit library 
that greatly simplifies developing desktop applications with 
scientific, technical, CAD or other 3D graphics.

Ab3d.PowerToys is using WPF 3D rendering engine (DirectX 9).
Check Ab3d.DXEngine for super fast DirectX 11 rendering engine
that can render the existing WPF 3D scene much faster and 
with better visual quality.


Explore features of the library with checking sample projects:
https://github.com/ab4d/Ab3d.PowerToys.Wpf.Samples      (.Net 4.5, Core3 and .Net 5.0 solutions)
https://github.com/ab4d/Ab3d.PowerToys.WinForms.Samples (.Net 4.5 solution)

https://github.com/ab4d/Ab3d.DXEngine.Wpf.Samples


Homepage:
https://www.ab4d.com/PowerToys.aspx

Online reference help:
https://www.ab4d.com/help/PowerToys/html/R_Project_Ab3d_PowerToys.htm

Change log:
https://www.ab4d.com/PowerToys-history.aspx


This version of Ab3d.PowerToys can be used as an evaluation and as a commercial version.

Evaluation usage:
On the first usage of the library, a dialog to start a 60-days evaluation is shown.
The evaluation version offers full functionality of the library but displays an evaluation
info dialog once a day and occasionally shows a "Ab3d.PowerToys evaluation" watermark text.
When the evaluation is expired, you can ask for evaluation extension or restart 
the evaluation period when a new version of the library is available.

You can see the prices of the library and purchase it on 
https://www.ab4d.com/Purchase.aspx#PowerToys

Commercial usage:
In case you have purchased a license, you can get the license parameters
from your User Account web page (https://www.ab4d.com/UserLogIn.aspx).
Then set the parametes with adding the following code before the library is used:

Ab3d.Licensing.PowerToys.LicenseHelper.SetLicense(licenseOwner: "[CompanyName]", 
                                                  licenseType: "[LicenseType]", 
                                                  license: "[LicenseText]");

Note that the version that is distributed as NuGet package uses a different licensing
mechanism then the commercial version that is distributed with a windows installer. 
Also the LicenseText that is used as a parameter to the SetLicense method is different 
then the license key used in the installer.