using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.IO;
using Microsoft.Win32;

namespace _3DPrinterExport
{
    public partial class exportProgressWindow : Window
    {
        ESAPIworker slave;
        MainWindow mainWindow;
        public int calcItems;
        public exportProgressWindow(ESAPIworker e, MainWindow mw)
        {
            InitializeComponent();
            slave = e;
            mainWindow = mw;
            calcItems = slave.data.numIndices / 3;
            doStuff();
            //< ScrollViewer x: Name = "scroller" Height = "380" Width = "400" >
      
            //      < TextBlock x: Name = "progressTB" TextWrapping = "Wrap" Background = "White" ></ TextBlock >
            
            //        </ ScrollViewer >
        }

        public void doStuff()
        {
            slave.DoWork(d =>
            {
                /*
                Dispatcher.BeginInvoke((Action)(() => { provideUpdate("Running"); }));
                Dispatcher.BeginInvoke((Action)(() => { provideUpdate(string.Format("solid {0}", "test")); }));
                Dispatcher.BeginInvoke((Action)(() => { provideUpdate("Solid information:"); }));
                Dispatcher.BeginInvoke((Action)(() => { provideUpdate(String.Format("Number of positions: {0}", d.numPositions)); }));
                Dispatcher.BeginInvoke((Action)(() => { provideUpdate(String.Format("Number of normals: {0}", d.numNormals)); }));
                Dispatcher.BeginInvoke((Action)(() => { provideUpdate(String.Format("Number of vertices: {0}", d.numIndices)); }));
                */
                SaveFileDialog saveFileDialog1 = new SaveFileDialog
                {
                    InitialDirectory = @"\\vfs0006\RadData\oncology\ESimiele\Research\3DPrinterExport\exports",
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
                Vector3DCollection normals = mainWindow.getNormals(d.mesh);
                Point3DCollection points = mainWindow.getPositions(d.mesh);
                Point3D p0, p1, p2;
                File.AppendAllText(fileName, string.Format("solid {0}", "test") + System.Environment.NewLine);
                string output;
                Int32Collection indices = d.mesh.TriangleIndices;
                int index = 0;
                for (int i = 0; i < d.mesh.TriangleIndices.Count; i += 3)
                {
                    p0 = d.mesh.Positions[indices[i]];
                    p1 = d.mesh.Positions[indices[i + 1]];
                    p2 = d.mesh.Positions[indices[i + 2]];
                    output = string.Format("facet normal {0} {1} {2}", normals[index].X, normals[index].Y, normals[index].Z) + System.Environment.NewLine;
                    output += "outer loop" + System.Environment.NewLine;
                    output += string.Format("vertex {0} {1} {2}", p0.X, p0.Y, p0.Z) + System.Environment.NewLine;
                    output += string.Format("vertex {0} {1} {2}", p1.X, p1.Y, p1.Z) + System.Environment.NewLine;
                    output += string.Format("vertex {0} {1} {2}", p2.X, p2.Y, p2.Z) + System.Environment.NewLine;
                    output += "endloop" + System.Environment.NewLine;
                    output += "endfacet" + System.Environment.NewLine;
                    File.AppendAllText(fileName, output);
                   // Dispatcher.BeginInvoke((Action)(() => { provideUpdate((int)(100 * (++index) / calcItems), String.Format("Printed facet: {0}", index)); }));
                    Dispatcher.BeginInvoke((Action)(() => { provideUpdate((int)(100 * (++index) / calcItems)); }));
                    //index++;
                }
                File.AppendAllText(fileName, string.Format("endsolid {0}", "test") + System.Environment.NewLine);
                //Dispatcher.BeginInvoke((Action)(() => { provideUpdate("Finished!"); }));
            });
        }

        public void provideUpdate(int percentComplete, string message)
        {
            progress.Value = percentComplete;
            //progressTB.Text += message + System.Environment.NewLine;
            //scroller.ScrollToBottom();
            //updateLogFile(message);
        }

        public void provideUpdate(int percentComplete) { progress.Value = percentComplete; }

       // public void provideUpdate(string message) { progressTB.Text += message + System.Environment.NewLine; scroller.ScrollToBottom(); }
    }
}
