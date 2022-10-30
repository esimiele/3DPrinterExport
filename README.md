# 3DPrinterExport
A simple tool to visualize structures in Eclipse and export them as stl files

updated 10/28/22
The purpose of this code is to visualize structures from Eclipse and export them as STL files for use in 3D printing. Similar projects can export Eclipse structures in STL format (e.g., https://github.com/esimiele/Varian-Code-Samples/tree/master/Eclipse%20Scripting%20API/projects/Export3D), but these previous works do not provide the functionality to view a 3D rendering of the structure prior to export. While Eclipse provides the functionality to view a 3D rendering of the structure, the lighting in Eclipse is ambient, which doesn't show the individual facets of the object that will be present in the final print. You could also simply export the object in stl format and open it in a 3D rendering program such as paint3D (slow!!) or 3D viewer, which are pre-installed on all windows 10 computers. However, this tool cuts out that additional step.
While this tool has limited functionality and is relatively simple, it was a fun project that provided a nice introduction to WPF 3D applications. This is a nice resource for anyone interested in 3D rendering applications. In addition, it also provides a nice example of multithreading where a new window will open displaying the export progress and then close once completed. Of course, there are other more powerful libraries that can be used for 3D rendering (e.g., DirectX and OpenGL), but that wasn't the purpose of this tool. 

How to run:
- From Eclipse:
-   open the structure set or plan in External beam planning
-   launch launch3DPrinterExport.cs from the /bin directory of the code
-   the patient, structure set, and structure fields should be automatically populated

- Outside Eclipse:
-   launch 3DPrinterExport.exe
-   If the computer does not have a local install of Aria, you will get a warning message. Ignore this and hit ok (it just says it couldn't connect to Aria)
-   If the computer does have a local install of Aria, enter the patient MRN and hit the open patient button
-   Select the structure set and structure you want to render

Viewport controls:
- pan --> left mouse click and drag
- rotate --> right mouse click and drag
- zoom --> mouse scroll

The help button also explains all of the above. Once a structure has been selected from the drop-down, it will automatically be rendered in the viewport. Changing to another structure in the dropdown will clear the viewport and render the selected structure. For testing and review, you can choose to open an existing stl file (ASCII format only!). The code will construct the 3D mesh and render the structure.

Open to suggestions if anyone is interested in contributing. If so, please open an issue or pull request on this page.
Thanks!
