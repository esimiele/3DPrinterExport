using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.IO;
using System.Threading;

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
        }

        public void doStuff()
        {
            slave.DoWork(d =>
            {
                Dispatcher.BeginInvoke((Action)(() => { provideUpdate("Running"); }));
                Dispatcher.BeginInvoke((Action)(() => { provideUpdate(string.Format("solid {0}", "test")); }));
                Dispatcher.BeginInvoke((Action)(() => { provideUpdate("Solid information:"); }));
                Dispatcher.BeginInvoke((Action)(() => { provideUpdate(String.Format("Number of positions: {0}", d.numPositions)); }));
                Dispatcher.BeginInvoke((Action)(() => { provideUpdate(String.Format("Number of normals: {0}", d.numNormals)); }));
                Dispatcher.BeginInvoke((Action)(() => { provideUpdate(String.Format("Number of vertices: {0}", d.numIndices)); }));

                Vector3DCollection normals = mainWindow.getNormals(d.mesh);
                Point3DCollection points = mainWindow.getPositions(d.mesh);
                Point3D p0, p1, p2;
                File.WriteAllText(d.filePath, string.Format("solid {0}", "test") + System.Environment.NewLine);
                string output;
                Int32Collection indices = d.mesh.TriangleIndices;
                int index = 0;
                for (int i = 0; i < d.numIndices; i += 3)
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
                    File.AppendAllText(d.filePath, output);
                    if(++index % 10 == 0) Dispatcher.BeginInvoke((Action)(() => { provideUpdate((int)(100 * index / calcItems), String.Format("Printed facet: {0}", index)); }));
                   // Dispatcher.BeginInvoke((Action)(() => { provideUpdate((int)(100 * (++index) / calcItems)); }));
                    //index++;
                }
                File.AppendAllText(d.filePath, string.Format("endsolid {0}", "test") + System.Environment.NewLine);
                Dispatcher.BeginInvoke((Action)(() => { provideUpdate((int)(100 * index / calcItems), String.Format("Printed facet: {0}", index)); }));
                Dispatcher.BeginInvoke((Action)(() => { provideUpdate("Finished!"); }));
                Thread.Sleep(2000);
                Dispatcher.BeginInvoke((Action)(() => { this.Close(); }));

            });
        }

        public void provideUpdate(int percentComplete, string message)
        {
            progress.Value = percentComplete;
            progressTB.Text += message + System.Environment.NewLine;
            scroller.ScrollToBottom();
            //updateLogFile(message);
        }

        public void provideUpdate(int percentComplete) { progress.Value = percentComplete; }

        public void provideUpdate(string message) { progressTB.Text += message + System.Environment.NewLine; scroller.ScrollToBottom(); }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SizeToContent = SizeToContent.WidthAndHeight;
        }
    }
}
