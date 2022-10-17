using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VMS.TPS.Common.Model.API;
using Microsoft.Win32;
using System.IO;

namespace _3DPrinterExport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
        Point leftDownPos;
        Point rightDownPos;
        public MainWindow(StartupEventArgs e)
        {
            InitializeComponent();
            try { app = VMS.TPS.Common.Model.API.Application.CreateApplication(); }
            catch (Exception except) { MessageBox.Show(String.Format("Warning! Could not generate Aria application instance because: {0}", except.Message)); }
            if(app != null)
            {
                if(e.Args.Length == 2)
                {
                    //called from Eclipse
                    //can open patient and structure set and populate comboBoxes
                    mrn = e.Args.ElementAt(0);
                    ss = e.Args.ElementAt(1);
                    if(!string.IsNullOrEmpty(mrn) && !string.IsNullOrWhiteSpace(mrn))
                    {
                        mrnTB.Text = mrn;
                        try { pi = app.OpenPatientById(mrn); }
                        catch (Exception exceptPI) { MessageBox.Show(string.Format("Error! Could not open patient because: {0}! Please try again!", exceptPI.Message)); pi = null; }
                    }
                    if (pi != null)
                    {
                        foreach (StructureSet itr in pi.StructureSets) ssCB.Items.Add(itr.Id);
                        ssCB.Text = ss;
                        selectedSS = pi.StructureSets.FirstOrDefault(x => x.Id == ss);
                    }
                    if(selectedSS != null)
                    {
                        foreach (Structure itr in selectedSS.Structures) structureCB.Items.Add(itr.Id);
                        structureCB.SelectedIndex = 0;
                    }
                }
            }
        }

        private void OpenPatient_Click(object sender, RoutedEventArgs e)
        {
            if (app == null) return;
            if (!string.IsNullOrEmpty(mrnTB.Text) && !string.IsNullOrWhiteSpace(mrnTB.Text))
            {
                if(mrn != mrnTB.Text)
                {
                    app.ClosePatient();
                    mrn = mrnTB.Text;
                }
                try { pi = app.OpenPatientById(mrn); }
                catch (Exception exceptPI) { MessageBox.Show(string.Format("Error! Could not open patient because: {0}! Please try again!", exceptPI.Message)); pi = null; mrn = ""; }
                if (pi != null)
                {
                    foreach (StructureSet itr in pi.StructureSets) ssCB.Items.Add(itr.Id);
                    ssCB.SelectedIndex = 0;
                    selectedSS = pi.StructureSets.FirstOrDefault(x => x.Id == ssCB.SelectedItem.ToString());
                }
                if (selectedSS != null)
                {
                    foreach (Structure itr in selectedSS.Structures) structureCB.Items.Add(itr.Id);
                    structureCB.SelectedIndex = 0;
                }
            }
        }

        private void ssCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (app == null) return;
            doNotUpdate = true;
            structureCB.Items.Clear();
            selectedSS = pi.StructureSets.FirstOrDefault(x => x.Id == ssCB.SelectedItem.ToString());
            if(selectedSS == null) { MessageBox.Show("No structure set found! Exiting!"); return; }
            doNotUpdate = false;
            foreach (Structure itr in selectedSS.Structures) structureCB.Items.Add(itr.Id);
            structureCB.SelectedIndex = 0;
        }

        private void structureCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (app == null) return;
            if (doNotUpdate) return;
            renderStructure = selectedSS.Structures.FirstOrDefault(x => x.Id == structureCB.SelectedItem.ToString());
            if (renderStructure == null) { MessageBox.Show("No structure found! Exiting!"); return; }
            renderStructureInView();
        }

        private void renderStructureInView()
        {
            MeshGeometry3D mesh = renderStructure.MeshGeometry;
            //Viewport3D myViewport3D = new Viewport3D();
            myViewport3D.Children.Clear();
            Model3DGroup myModel3DGroup = new Model3DGroup();
            GeometryModel3D myGeometryModel = new GeometryModel3D();
            ModelVisual3D myModelVisual3D = new ModelVisual3D();
            // Defines the camera used to view the 3D object. In order to view the 3D object,
            // the camera must be positioned and pointed such that the object is within view
            // of the camera.
            //PerspectiveCamera myPCamera = new PerspectiveCamera();

            // Specify where in the 3D scene the camera is.
            //myPCamera.Position = new Point3D(0, 0, 2);
            //MessageBox.Show(String.Format("{0}, {1}",Math.Abs(mesh.Positions.Max(x => x.Z) - theStructure.CenterPoint.z), Math.Abs(mesh.Positions.Min(x => x.Z) - theStructure.CenterPoint.z)));
            double maxDistance = 0.0;
            if (Math.Abs(mesh.Positions.Min(x => x.Z) - renderStructure.CenterPoint.z) >= Math.Abs(mesh.Positions.Max(x => x.Z) - renderStructure.CenterPoint.z)) maxDistance = Math.Abs(mesh.Positions.Min(x => x.Z) - renderStructure.CenterPoint.z);
            else maxDistance = Math.Abs(mesh.Positions.Max(x => x.Z) - renderStructure.CenterPoint.z);
            myPCamera.Position = new Point3D(renderStructure.CenterPoint.x, renderStructure.CenterPoint.y - maxDistance / Math.Tan(20 * 3.14 / 180), renderStructure.CenterPoint.z);

            // Specify the direction that the camera is pointing.
            myPCamera.LookDirection = new Vector3D(0, 1, 0);

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

            // The geometry specifes the shape of the 3D plane. In this sample, a flat sheet
            // is created.
            //MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            //// Create a collection of normal vectors for the MeshGeometry3D.
            //Vector3DCollection myNormalCollection = new Vector3DCollection();
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            mesh.Normals = getNormals(mesh);

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
            mesh.TextureCoordinates = Ab3d.Utilities.MeshUtils.GeneratePlanarTextureCoordinates(mesh, new Vector3D(0, 1, 0), new Vector3D(0, 0, 1), false, false, false);

            //// Create a collection of triangle indices for the MeshGeometry3D.
            //Int32Collection myTriangleIndicesCollection = new Int32Collection();
            //myTriangleIndicesCollection.Add(0);
            //myTriangleIndicesCollection.Add(1);
            //myTriangleIndicesCollection.Add(2);
            //myTriangleIndicesCollection.Add(3);
            //myTriangleIndicesCollection.Add(4);
            //myTriangleIndicesCollection.Add(5);
            //myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            // MessageBox.Show(String.Format("{0}, {1}, {2}, {3}", mesh.Positions.Count, mesh.TextureCoordinates.Count, mesh.TriangleIndices.Count, mesh.Normals.Count));

            // Apply the mesh to the geometry model.
            myGeometryModel.Geometry = mesh;

            // The material specifies the material applied to the 3D object. In this sample a
            // linear gradient covers the surface of the 3D object.

            // Create a horizontal linear gradient with four stops.
            //LinearGradientBrush myHorizontalGradient = new LinearGradientBrush();
            //myHorizontalGradient.StartPoint = new Point(0, 0.5);
            //myHorizontalGradient.EndPoint = new Point(1, 0.5);
            //myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
            //myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Red, 0.25));
            //myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Blue, 0.75));
            //myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.LimeGreen, 1.0));

            //// Define material and apply to the mesh geometries.
            //DiffuseMaterial myMaterial = new DiffuseMaterial(myHorizontalGradient);
            //myGeometryModel.Material = myMaterial;
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

            // Apply the viewport to the page so it will be rendered.
            //this.Content = myViewport3D;

        }

        private Vector3DCollection getNormals(MeshGeometry3D mesh)
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
                //Vector3D orientation = Vector3D.CrossProduct(myPCamera.UpDirection, myPCamera.LookDirection);
                //Vector3D update = new Vector3D((e.GetPosition(myViewport3D).X - leftDownPos.X) * Vector3D.CrossProduct(myPCamera.UpDirection, myPCamera.LookDirection).X, (e.GetPosition(myViewport3D).X - leftDownPos.X) * Vector3D.CrossProduct(myPCamera.UpDirection, myPCamera.LookDirection).Y, (e.GetPosition(myViewport3D).X - leftDownPos.X) * Vector3D.CrossProduct(myPCamera.UpDirection, myPCamera.LookDirection).Z);
                double newX, newY, newZ;
                newX = myPCamera.Position.X + ((e.GetPosition(myViewport3D).X - leftDownPos.X) * Vector3D.CrossProduct(myPCamera.UpDirection, myPCamera.LookDirection).X) + ((e.GetPosition(myViewport3D).Y - leftDownPos.Y) * myPCamera.UpDirection.X);
                newY = myPCamera.Position.Y + ((e.GetPosition(myViewport3D).X - leftDownPos.X) * Vector3D.CrossProduct(myPCamera.UpDirection, myPCamera.LookDirection).Y) + ((e.GetPosition(myViewport3D).Y - leftDownPos.Y) * myPCamera.UpDirection.Y);
                newZ = myPCamera.Position.Z + ((e.GetPosition(myViewport3D).X - leftDownPos.X) * Vector3D.CrossProduct(myPCamera.UpDirection, myPCamera.LookDirection).Z) + ((e.GetPosition(myViewport3D).Y - leftDownPos.Y) * myPCamera.UpDirection.Z);
                myPCamera.Position = new Point3D(newX, newY, newZ);
                leftDownPos = e.GetPosition(myViewport3D);
            }
            else if (_rightMouseDown)
            {
                //Vector3D thetaAxis = Vector3D.CrossProduct(myPCamera.UpDirection, myPCamera.LookDirection);
                //Vector3D phiAxis = myPCamera.UpDirection;
                //double dx, dy;

                //rightDownPos = e.GetPosition(myViewport3D);
            }
        }

        private void exportSTL_Click(object sender, RoutedEventArgs e)
        {
            if (renderStructure == null) MessageBox.Show("No Structure to export! Exiting!");
            MeshGeometry3D mesh = renderStructure.MeshGeometry;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                InitialDirectory = @"\\vfs0006\RadData\oncology\ESimiele\Research\stlExport\exports",
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
    }
}
