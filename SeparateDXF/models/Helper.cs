using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using netDxf;

namespace SeparateDXF.models
{
    
    public static class Helper
    {
        public const double ANGLE_2_RADIAN = Math.PI / 180.0;
        public const double RADIAN_2_ANGLE = 180.0 / Math.PI;
        public const double ARC_TESSELLATION_ANGLE = 10.0  / 180 * Math.PI;
        public const int MIN_ARC_TESSELLATION_SUBDIVISIONS = 8;
        public const double NUMBER_MIN_VALUE = 0.0001;
        public const  int SPLINE_SUBDIVISION = 4;
        public const double CONNECT_TOLERANCE = 0.01;
        public const double BOXCROSS_TOLERANCE = 0.001;


        public static bool IsSameVertice(Vector3 v1, Vector3 v2, double tolerval= CONNECT_TOLERANCE)
        {
            if (Math.Abs(v1.X - v2.X) < tolerval && Math.Abs(v1.Y - v2.Y) < tolerval && Math.Abs(v1.Z - v2.Z) < tolerval)
            {
                return true;
            }
            else
            {
                return false;
            }
        }






        public static bool CheckLoopVerticesInclude(List<Vector3> verticesMain, List<Vector3> verticesSub)
        {
            foreach (Vector3 vertice in verticesSub)
            {
                if (IsOnEdge(vertice, verticesMain)) { continue; }
                if (!IsInside(vertice, verticesMain)) { return false; }
            }
            return true;
        }

        public static bool IsInside(Vector3 vertice, List<Vector3> verticesMain)
        {
            double x = vertice.X, y = vertice.Y;

            var inside = false;
            for (int i = 0, j = verticesMain.Count - 1; i < verticesMain.Count; j = i++)
            {
                double xi = verticesMain[i].X, yi = verticesMain[i].Y;
                double xj = verticesMain[j].X, yj = verticesMain[j].Y;

                var intersect = ((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi);
                if (intersect) inside = !inside;
            }
            return inside;
        }

        public static bool IsOnEdge(Vector3 vertice, List<Vector3> verticesMain)
        {
            double x = vertice.X, y = vertice.Y;

            foreach (Vector3 tmpvertice in verticesMain)
            {
                if (Math.Abs(tmpvertice.X - x) < CONNECT_TOLERANCE && Math.Abs(tmpvertice.Y - y) < CONNECT_TOLERANCE)
                {
                    //on the edge
                    return true;
                }
            }

            for (int i = 0, j = verticesMain.Count - 1; i < verticesMain.Count; j = i++)
            {
                double xi = verticesMain[i].X, yi = verticesMain[i].Y;
                double xj = verticesMain[j].X, yj = verticesMain[j].Y;

                if (x >= Math.Min(xi, xj) - CONNECT_TOLERANCE &&
                    x <= Math.Max(xi, xj) + CONNECT_TOLERANCE &&
                    y >= Math.Min(yi, yj) - CONNECT_TOLERANCE &&
                    y <= Math.Max(yi, yj) + CONNECT_TOLERANCE &&
                    Math.Abs((y - yi) * (xj - xi) - (yj - yi) * (x - xi)) < CONNECT_TOLERANCE)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
