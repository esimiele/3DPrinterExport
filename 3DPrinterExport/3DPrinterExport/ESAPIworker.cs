using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media.Media3D;


namespace _3DPrinterExport
{
    public class ESAPIworker
    {
        //data structure to hold all this crap
        public struct dataContainer
        {
            //data members
            public VMS.TPS.Common.Model.API.Application app;
            public MeshGeometry3D mesh;
            public int numIndices;
            public int numPositions;
            public int numNormals;
            public string filePath;
            
            //simple method to automatically assign/initialize the above data members
            public void construct(MeshGeometry3D mg, VMS.TPS.Common.Model.API.Application a, string export)
            {
                mesh = mg;
                numIndices = mesh.TriangleIndices.Count;
                numPositions = mesh.Positions.Count;
                numNormals = mesh.Normals.Count;
                filePath = export;
                app = a;
            }
        }

        //instance of dataContainer structure to copy the optimization parameters to thread-local memory
        public dataContainer data;
        public readonly Dispatcher _dispatcher;

        //constructor
        public ESAPIworker(dataContainer d)
        {
            //copy optimization parameters from main thread to new thread
            data = d;
            //copy the dispatcher assigned to the main thread (the optimization loop will run on the main thread)
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        //asynchronously execute the supplied task on the main thread
        public void DoWork(Action<dataContainer> a)
        {
            _dispatcher.BeginInvoke(a, data);
        }
    }
}
