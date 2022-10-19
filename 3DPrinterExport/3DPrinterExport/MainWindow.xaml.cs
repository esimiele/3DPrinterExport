using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VMS.TPS.Common.Model.API;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using System.Threading;


namespace _3DPrinterExport
{
    public partial class MainWindow : Window
    {
        string mrn, ss;
        public VMS.TPS.Common.Model.API.Application app = null;
        Patient pi = null;
        public StructureSet selectedSS = null;
        public Structure renderStructure = null;
        private bool _leftMouseDown = false;
        private bool _rightMouseDown = false;
        public bool doNotUpdate = false;
        //string exportDir = @"C:\Users\Eric Simiele\Documents\GitHub\stlExport\exports";
        string exportDir = @"\\vfs0006\RadData\oncology\ESimiele\Research\stlExport\exports";
        Point leftDownPos;
        Point rightDownPos;
        double TotalDx = 0;
        double TotalDy = 0;
        GeometryModel3D myGeometryModel;

        public MainWindow(StartupEventArgs e)
        {
            InitializeComponent();
            if (!Directory.Exists(exportDir)) exportDir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            try { app = VMS.TPS.Common.Model.API.Application.CreateApplication(); }
            catch (Exception except) { MessageBox.Show(String.Format("Warning! Could not generate Aria application instance because: {0}", except.Message)); }
            if (app != null)
            {
                if (e.Args.Length == 2)
                {
                    //called from Eclipse
                    //can open patient and structure set and populate comboBoxes
                    mrn = e.Args.ElementAt(0);
                    ss = e.Args.ElementAt(1);
                    if (!string.IsNullOrEmpty(mrn) && !string.IsNullOrWhiteSpace(mrn))
                    {
                        mrnTB.Text = mrn;
                        openPatient();
                    }
                    if (pi != null) loadSSCBandSelectSS(ss);
                }
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            message += string.Format("This script provides functionality to render and export structures as stl files") + System.Environment.NewLine;
            message += string.Format("Some notes:") + System.Environment.NewLine;
            message += string.Format("1. Structures will automatically render in the window upon structure selection") + System.Environment.NewLine;
            message += string.Format("2. You can zoom in on the object in the window by scrolling the mouse wheel") + System.Environment.NewLine;
            message += string.Format("3. You can pan the object by clicking and dragging with the left mouse button") + System.Environment.NewLine;
            message += string.Format("4. You can rotate the object by clicking and dragging with the right mouse button") + System.Environment.NewLine;
            MessageBox.Show(message);
        }

        private void OpenPatient_Click(object sender, RoutedEventArgs e)
        {
            if (app == null) return;
            if (!string.IsNullOrEmpty(mrnTB.Text) && !string.IsNullOrWhiteSpace(mrnTB.Text))
            {
                if (mrn != mrnTB.Text)
                {
                    app.ClosePatient();
                    mrn = mrnTB.Text;
                    openPatient();
                    if (pi != null) loadSSCBandSelectSS();
                }
            }
        }

        private void openPatient()
        {
            try { pi = app.OpenPatientById(mrn); }
            catch (Exception exceptPI) { MessageBox.Show(string.Format("Error! Could not open patient because: {0}! Please try again!", exceptPI.Message)); pi = null; mrn = ""; }
        }

        private void loadSSCBandSelectSS(string requestedSS = "")
        {
            foreach (StructureSet itr in pi.StructureSets) ssCB.Items.Add(itr.Id);
            if (requestedSS == "") ssCB.SelectedIndex = 0;
            else ssCB.Text = requestedSS;
            selectedSS = pi.StructureSets.FirstOrDefault(x => x.Id == ssCB.SelectedItem.ToString());
        }

        private void loadStructureCB()
        {
            foreach (Structure itr in selectedSS.Structures) structureCB.Items.Add(itr.Id);
            structureCB.SelectedIndex = 0;
        }

        private void loadSTLBTN_Click(object sender, RoutedEventArgs e)
        {
            //load a stl file (only works for ASCI stl files, not Binary)
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = exportDir;
            openFileDialog.Filter = "stl files (*.stl)|*.stl|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog().Value) loadSTL(openFileDialog.FileName);
        }

        private void ssCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (app == null) return;
            doNotUpdate = true;
            structureCB.Items.Clear();
            selectedSS = pi.StructureSets.FirstOrDefault(x => x.Id == ssCB.SelectedItem.ToString());
            if (selectedSS == null) { MessageBox.Show("No structure set found! Exiting!"); return; }
            doNotUpdate = false;
            loadStructureCB();
        }

        private void structureCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (app == null) return;
            if (doNotUpdate) return;
            renderStructure = selectedSS.Structures.FirstOrDefault(x => x.Id == structureCB.SelectedItem.ToString());
            if (renderStructure == null || renderStructure.IsEmpty) { MessageBox.Show("No structure found or structure is empty! Exiting!"); return; }
            TotalDx = TotalDy = 0;
            renderStructureInView(renderStructure.MeshGeometry);
        }

        private void renderStructureInView(MeshGeometry3D mesh)
        {
            Point3D centerPoint = new Point3D((mesh.Positions.Max(x => x.X) + mesh.Positions.Min(x => x.X)) / 2, (mesh.Positions.Max(x => x.Y) + mesh.Positions.Min(x => x.Y)) / 2, (mesh.Positions.Max(x => x.Z) + mesh.Positions.Min(x => x.Z)) / 2);
            Point3DCollection pts = new Point3DCollection(mesh.Positions);
            mesh.Positions.Clear();
            for (int i = 0; i < pts.Count; i++) mesh.Positions.Add(new Point3D(pts.ElementAt(i).X - centerPoint.X, pts.ElementAt(i).Y - centerPoint.Y, pts.ElementAt(i).Z - centerPoint.Z));

            //clear the viewport and initialize
            myViewport3D.Children.Clear();
            Model3DGroup myModel3DGroup = new Model3DGroup();
            myGeometryModel = new GeometryModel3D();
            ModelVisual3D myModelVisual3D = new ModelVisual3D();

            // Specify where in the 3D scene the camera is.
            double maxDistance = 0.0;
            if (Math.Abs(pts.Min(x => x.Z) - centerPoint.Z) >= Math.Abs(pts.Max(x => x.Z) - centerPoint.Z)) maxDistance = Math.Abs(pts.Min(x => x.Z) - centerPoint.Z);
            else maxDistance = Math.Abs(pts.Max(x => x.Z) - centerPoint.Z);
            myPCamera.Position = new Point3D(0, -maxDistance / Math.Tan(20 * 3.14 / 180), 0);

            // Specify the direction that the camera is pointing.
            myPCamera.LookDirection = new Vector3D(0, 1, 0);

            //which way is up
            myPCamera.UpDirection = new Vector3D(0, 0, 1);

            // Define camera's horizontal field of view in degrees.
            myPCamera.FieldOfView = 60;

            // Asign the camera to the viewport
            myViewport3D.Camera = myPCamera;

            // Define the lights cast in the scene. Without light, the 3D object cannot
            // be seen. Note: to illuminate an object from additional directions, create
            // additional lights.
            DirectionalLight myDirectionalLight = new DirectionalLight();
            myDirectionalLight.Color = Colors.White;
            myDirectionalLight.Direction = new Vector3D(0.61, 0.5, 0.61);
            myModel3DGroup.Children.Add(myDirectionalLight);

            //AmbientLight ambientLight = new AmbientLight();
            //ambientLight.Color = Colors.White;
            //myModel3DGroup.Children.Add(ambientLight);

            //normals (not supplied by default in Eclipse)
            if (!mesh.Normals.Any()) mesh.Normals = getNormals(mesh);

            //// Create a collection of vertex positions for the MeshGeometry3D.
            //Point3DCollection myPositionCollection = new Point3DCollection();
            //myPositionCollection.Add(new Point3D(-0.5, -0.5, 0.5));
            //myPositionCollection.Add(new Point3D(0.5, -0.5, 0.5));
            //myPositionCollection.Add(new Point3D(0.5, 0.5, 0.5));
            //myPositionCollection.Add(new Point3D(0.5, 0.5, 0.5));
            //myPositionCollection.Add(new Point3D(-0.5, 0.5, 0.5));
            //myPositionCollection.Add(new Point3D(-0.5, -0.5, 0.5));
            //myMeshGeometry3D.Positions = myPositionCollection;

            //// Create a collection of texture coordinates for the MeshGeometry3D.
            //PointCollection myTextureCoordinatesCollection = new PointCollection();
            //myTextureCoordinatesCollection.Add(new Point(0, 0));
            //myTextureCoordinatesCollection.Add(new Point(1, 0));
            //myTextureCoordinatesCollection.Add(new Point(1, 1));
            //myTextureCoordinatesCollection.Add(new Point(1, 1));
            //myTextureCoordinatesCollection.Add(new Point(0, 1));
            //myTextureCoordinatesCollection.Add(new Point(0, 0));
            //myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;

            //not sure if this is doing anything useful...
            mesh.TextureCoordinates = Ab3d.Utilities.MeshUtils.GeneratePlanarTextureCoordinates(mesh, new Vector3D(0, 1, 0), new Vector3D(0, 0, 1), false, false, false);

            // Apply the mesh to the geometry model.
            myGeometryModel.Geometry = mesh;
            //need this here otherwise the rotations WILL NOT WORK
            myGeometryModel.Transform = new Transform3DGroup();

            //apply a solid material color to the object
            myGeometryModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));

            // Apply a transform to the object. In this sample, a rotation transform is applied,
            // rendering the 3D object rotated.
            //RotateTransform3D myRotateTransform3D = new RotateTransform3D();
            //AxisAngleRotation3D myAxisAngleRotation3d = new AxisAngleRotation3D();
            //myAxisAngleRotation3d.Axis = new Vector3D(0, 3, 0);
            //myAxisAngleRotation3d.Angle = 40;
            //myRotateTransform3D.Rotation = myAxisAngleRotation3d;
            //myGeometryModel.Transform = myRotateTransform3D;

            // Add the geometry model to the model group.
            myModel3DGroup.Children.Add(myGeometryModel);

            // Add the group of models to the ModelVisual3d.
            myModelVisual3D.Content = myModel3DGroup;

            myViewport3D.Children.Add(myModelVisual3D);
        }

        public Vector3DCollection getNormals(MeshGeometry3D mesh)
        {
            Vector3DCollection normals = new Vector3DCollection();
            Int32Collection indices = mesh.TriangleIndices;
            Point3D p0, p1, p2;
            Vector3D u, v, w;
            for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
            {
                p0 = mesh.Positions[indices[i]];
                p1 = mesh.Positions[indices[i + 1]];
                p2 = mesh.Positions[indices[i + 2]];
                u = p1 - p0;
                v = p2 - p0;
                w = Vector3D.CrossProduct(u, v);
                w.Normalize();
                normals.Add(w);
            }

            return normals;
        }

        public Point3DCollection getPositions(MeshGeometry3D mesh) { if (mesh != null) return mesh.Positions; else return new Point3DCollection(); }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            myPCamera.Position = new Point3D(myPCamera.Position.X - e.Delta * myPCamera.LookDirection.X / 10.0, myPCamera.Position.Y - e.Delta * myPCamera.LookDirection.Y / 10.0, myPCamera.Position.Z - e.Delta * myPCamera.LookDirection.Z / 10.0);
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _leftMouseDown = true;
            leftDownPos = e.GetPosition(myViewport3D);
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _leftMouseDown = false;
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _rightMouseDown = true;
            rightDownPos = e.GetPosition(myViewport3D);
        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _rightMouseDown = false;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (_leftMouseDown)
            {
                double newX, newY, newZ;
                newX = myPCamera.Position.X + (((e.GetPosition(myViewport3D).X - leftDownPos.X) * Vector3D.CrossProduct(myPCamera.UpDirection, myPCamera.LookDirection).X) + ((e.GetPosition(myViewport3D).Y - leftDownPos.Y) * myPCamera.UpDirection.X)) / 1.5;
                newY = myPCamera.Position.Y + (((e.GetPosition(myViewport3D).X - leftDownPos.X) * Vector3D.CrossProduct(myPCamera.UpDirection, myPCamera.LookDirection).Y) + ((e.GetPosition(myViewport3D).Y - leftDownPos.Y) * myPCamera.UpDirection.Y)) / 1.5;
                newZ = myPCamera.Position.Z + (((e.GetPosition(myViewport3D).X - leftDownPos.X) * Vector3D.CrossProduct(myPCamera.UpDirection, myPCamera.LookDirection).Z) + ((e.GetPosition(myViewport3D).Y - leftDownPos.Y) * myPCamera.UpDirection.Z)) / 1.5;
                myPCamera.Position = new Point3D(newX, newY, newZ);
                leftDownPos = e.GetPosition(myViewport3D);
            }
            else if (_rightMouseDown)
            {
                //solution from: https://www.vbforums.com/showthread.php?637168-WPF-3D-orbiting-camera-(pitch-yaw-rotation-only)
                //https://www.codeproject.com/Articles/23332/WPF-3D-Primer
                Point pos = Mouse.GetPosition(myViewport3D);
                double dx = pos.X - rightDownPos.X;
                double dy = pos.Y - rightDownPos.Y;

                TotalDx += dx;
                TotalDy += dy;

                double theta = TotalDx / 3;
                double phi = TotalDy / 3;
                Vector3D thetaAxis = myPCamera.UpDirection;
                Vector3D phiAxis = Vector3D.CrossProduct(myPCamera.LookDirection, myPCamera.UpDirection);

                Transform3DGroup group = myGeometryModel.Transform as Transform3DGroup;
                group.Children.Clear();
                QuaternionRotation3D r;
                r = new QuaternionRotation3D(new Quaternion(thetaAxis, theta));
                group.Children.Add(new RotateTransform3D(r));
                r = new QuaternionRotation3D(new Quaternion(phiAxis, phi));
                group.Children.Add(new RotateTransform3D(r));

                rightDownPos = e.GetPosition(myViewport3D);
            }
        }

        //method to create the new thread, set the apartment state, set the new thread to be a background thread, and execute the action supplied to this method
        private void RunOnNewThread(Action a)
        {
            Thread t = new Thread(() => a());
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();
        }

        private void exportSTL_Click(object sender, RoutedEventArgs e)
        {
            ESAPIworker.dataContainer d = new ESAPIworker.dataContainer();
            d.construct(renderStructure.MeshGeometry, app);
            ESAPIworker slave = new ESAPIworker(d);
            //create a new frame (multithreading jargon)
            DispatcherFrame frame = new DispatcherFrame();
            RunOnNewThread(() =>
            {
                //pass the progress window the newly created thread and this instance of the optimizationLoop class.
                exportProgressWindow pw = new exportProgressWindow(slave, this);
                pw.ShowDialog();

                //tell the code to hold until the progress window closes.
                frame.Continue = false;
            });

            Dispatcher.PushFrame(frame);

            return;
            if (app == null) { MessageBox.Show("Not connected to Eclipse! Exiting!"); return; }
            if (renderStructure == null) { MessageBox.Show("No Structure to export! Exiting!"); return; }
            MeshGeometry3D mesh = renderStructure.MeshGeometry;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                InitialDirectory = exportDir,
                Title = "Choose text file output",
                CheckPathExists = true,

                DefaultExt = "stl",
                Filter = "txt files (*.txt)|*.stl",
                FilterIndex = 2,
                RestoreDirectory = true,
            };
            string fileName = "";
            if (saveFileDialog1.ShowDialog() == saveFileDialog1.CheckPathExists) fileName = saveFileDialog1.FileName;
            else return;
            Vector3DCollection normals = getNormals(mesh);
            Point3D p0, p1, p2;
            File.AppendAllText(fileName, string.Format("solid {0}", renderStructure.Id) + System.Environment.NewLine);
            string output;
            Int32Collection indices = mesh.TriangleIndices;
            int index = 0;
            for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
            {
                p0 = mesh.Positions[indices[i]];
                p1 = mesh.Positions[indices[i + 1]];
                p2 = mesh.Positions[indices[i + 2]];
                output = string.Format("facet normal {0} {1} {2}", normals[index].X, normals[index].Y, normals[index].Z) + System.Environment.NewLine;
                output += "outer loop" + System.Environment.NewLine;
                output += string.Format("vertex {0} {1} {2}", p0.X, p0.Y, p0.Z) + System.Environment.NewLine;
                output += string.Format("vertex {0} {1} {2}", p1.X, p1.Y, p1.Z) + System.Environment.NewLine;
                output += string.Format("vertex {0} {1} {2}", p2.X, p2.Y, p2.Z) + System.Environment.NewLine;
                output += "endloop" + System.Environment.NewLine;
                output += "endfacet" + System.Environment.NewLine;
                File.AppendAllText(fileName, output);
                index++;
            }
            File.AppendAllText(fileName, string.Format("endsolid {0}", renderStructure.Id) + System.Environment.NewLine);
            MessageBox.Show("finished writing stl file!");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (app == null) return;
            if (pi != null) app.ClosePatient();
            app.Dispose();
        }

        private void loadSTL(string stlFile)
        {
            try
            {
                using (StreamReader reader = new StreamReader(stlFile))
                {
                    string line;
                    string solidName = "";
                    MeshGeometry3D mesh = new MeshGeometry3D();
                    int index = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        List<string> stringList = line.Split(' ').ToList();
                        if (line.Contains("solid")) solidName = stringList.ElementAt(1);
                        if (line.Contains("normal")) mesh.Normals.Add(new Vector3D(double.Parse(stringList.ElementAt(2)), double.Parse(stringList.ElementAt(3)), double.Parse(stringList.ElementAt(4))));
                        else if (line.Contains("vertex"))
                        {
                            mesh.Positions.Add(new Point3D(double.Parse(stringList.ElementAt(1)), double.Parse(stringList.ElementAt(2)), double.Parse(stringList.ElementAt(3))));
                            mesh.TriangleIndices.Add(index++);
                        }
                    }
                    //MessageBox.Show(String.Format("{0}, {1}, {2}", mesh.Positions.Count, mesh.Normals.Count, mesh.TriangleIndices.Count));
                    if (mesh.Positions.Count > 0 && mesh.Normals.Count > 0 && mesh.TriangleIndices.Count > 0)
                    {
                        TotalDx = TotalDy = 0;
                        renderStructureInView(mesh);
                    }
                    else MessageBox.Show(String.Format("Error in reading file {0}!", stlFile));
                }
            }
            catch (Exception e) { MessageBox.Show(e.Message); }
        }
    }
}