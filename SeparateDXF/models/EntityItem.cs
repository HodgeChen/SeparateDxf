using SeparateDXF.models;
using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using netDxf;
using netDxf.Entities;
using System.Windows.Controls;
using System.Transactions;
using System.IO;

namespace SeparateDXF.models
{
    internal class EntityItem
    {

        public EntityObject Entity { get; }
        public List<Vector3> Vertices { get; }
        public RectBox Rectbox { get; }

        public EntityItem(EntityObject entity)
        {
            Entity = entity;
            Vertices = new List<Vector3>();
            Rectbox = new RectBox();
            GenerateGeom2Vertices(Entity);
            getVerticesRectBox(Vertices);
            //GenerateVerticesLog()

        }

        private void GenerateGeom2Vertices(EntityObject entity)
        {
            switch (entity.Type)
            {
                case EntityType.Line:
                    Vertices.Add(((Line)entity).StartPoint);
                    Vertices.Add(((Line)entity).EndPoint);
                    break;
                case EntityType.Arc:
                    GenerateArcVertices(((Arc)entity).Center, ((Arc)entity).Radius, ((Arc)entity).StartAngle * Helper.ANGLE_2_RADIAN, ((Arc)entity).EndAngle * Helper.ANGLE_2_RADIAN);
                    break;
                case EntityType.Circle:
                    GenerateArcVertices(((Circle)entity).Center, ((Circle)entity).Radius, 0, Math.PI * 2);
                    break;
                case EntityType.Polyline2D:
                    GeneratePolyLineVertices((Polyline2D)entity);
                    break;
                case EntityType.Spline:
                    GenerateSplineVertices((Spline)entity);
                    break;
                case EntityType.Text:
                    GenerateTextVertices((Text)entity);
                    break;
                case EntityType.MText:
                    GenerateMTextVertices((MText)entity);
                    break;
            }
        }





        private void GenerateArcVertices(Vector3 center, double radius, double startangle, double endangle)
        {
            while (endangle <= startangle)
            {
                endangle += Math.PI * 2;
            }

            double arcAngle = endangle - startangle;
            double numSegments = Math.Floor(arcAngle / Helper.ARC_TESSELLATION_ANGLE);
            if (numSegments == 0)
            {
                numSegments = 1;
            }


            double step = arcAngle / numSegments;

            for (int i = 0; i <= numSegments; i++)
            {
                double a = startangle + i * step;
                Vertices.Add(new Vector3(center.X + radius * Math.Cos(a), center.Y + radius * Math.Sin(a), 0.0));
            }
        }

        private void GeneratePolyLineVertices(Polyline2D polyentity)
        {
            List<Polyline2DVertex> polyvertices = polyentity.Vertexes;
            int vertexesCount = polyvertices.Count;


            for (int i = 0; i < vertexesCount; i++)
            {
                if (i > 0 && polyvertices[i - 1].Bulge != 0)
                {
                    GenerateBulgeVertices(polyvertices[i - 1], polyvertices[i], polyvertices[i - 1].Bulge);
                }
                else
                {
                    Vertices.Add(new Vector3(polyvertices[i].Position.X, polyvertices[i].Position.Y, 0.0));
                }
            }

            if (polyentity.IsClosed == true)
            {
                Vertices.Add(new Vector3(polyvertices[0].Position.X, polyvertices[0].Position.Y, 0.0));
            }
        }


        private void GenerateBulgeVertices(Polyline2DVertex startVtx, Polyline2DVertex endVtx, double bulge)
        {
            if (bulge == 0)
            {
                Vertices.Add(new Vector3(endVtx.Position.X, endVtx.Position.Y, 0.0));
            }

            double a = 4 * Math.Atan(bulge);
            double aAbs = Math.Abs(a);
            if (aAbs < Helper.ARC_TESSELLATION_ANGLE)
            {
                Vertices.Add(new Vector3(endVtx.Position.X, endVtx.Position.Y, 0.0));
            }

            double ha = a / 2;
            double sha = Math.Sin(ha);
            double cha = Math.Cos(ha);
            Vector2 d = new Vector2(endVtx.Position.X - startVtx.Position.X, endVtx.Position.Y - startVtx.Position.Y);
            double dSq = d.X * d.X + d.Y * d.Y;
            if (dSq < Helper.NUMBER_MIN_VALUE * 2)
            {
                /* No vertex is pushed since end vertex is duplicate of start vertex. */
                return;
            }
            double D = Math.Sqrt(dSq);
            double R = D / 2 / sha;
            d.X /= D;
            d.Y /= D;
            Vector2 center = new Vector2((d.X * sha - d.Y * cha) * R + startVtx.Position.X, (d.X * cha + d.Y * sha) * R + startVtx.Position.Y);


            double numSegments = Math.Floor(aAbs / Helper.ARC_TESSELLATION_ANGLE);
            if (numSegments < Helper.MIN_ARC_TESSELLATION_SUBDIVISIONS)
            {
                numSegments = Helper.MIN_ARC_TESSELLATION_SUBDIVISIONS;
            }
            if (numSegments > 1)
            {
                double startAngle = Math.Atan2(startVtx.Position.Y - center.Y, startVtx.Position.X - center.X);
                double step = a / numSegments;
                if (a < 0)
                {
                    R = -R;
                }
                for (int i = 1; i < numSegments; i++)
                {
                    double aang = startAngle + i * step;
                    Vertices.Add(new Vector3(center.X + R * Math.Cos(aang), center.Y + R * Math.Sin(aang), 0.0));
                }
                Vertices.Add(new Vector3(endVtx.Position.X, endVtx.Position.Y, 0.0));


            }
        }

        private void GenerateSplineVertices(Spline splineentity)
        {
            if (splineentity.ControlPoints.Length == 0)
            {
                //XXX knots or fit points not supported yet
                return;
            }

            double subdivisions = splineentity.ControlPoints.Length * Helper.SPLINE_SUBDIVISION;
            double step = 1 / subdivisions;
            for (int i = 0; i <= subdivisions; i++)
            {
                (double pt_x, double pt_y) = InterpolateSpline(i * step, splineentity.Degree, splineentity.ControlPoints,
                                                       splineentity.Knots, splineentity.Weights);
                Vertices.Add(new Vector3(pt_x, pt_y, 0.0));
            }
        }


        private void GenerateTextVertices(Text textentity)
        {
            Vertices.Add(new Vector3(textentity.Position.X, textentity.Position.Y, 0.0));
            Vertices.Add(new Vector3(textentity.Position.X + textentity.Width, textentity.Position.Y + textentity.Height, 0.0));

        }


        private void GenerateMTextVertices(MText mtextentity)
        {
            Vertices.Add(new Vector3(mtextentity.Position.X, mtextentity.Position.Y, 0.0));
            Vertices.Add(new Vector3(mtextentity.Position.X + mtextentity.RectangleWidth, mtextentity.Position.Y - mtextentity.Height, 0.0));
        }


        private void getVerticesRectBox(List<Vector3> vertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 v = vertices[i];
                if (i == 0)
                {
                    Rectbox.SetRect(v.X, v.Y, v.X, v.Y);
                }
                else
                {
                    if (v.X < Rectbox.Min_X)
                    {
                        Rectbox.Min_X = v.X;
                    }
                    else if (v.X > Rectbox.Max_X)
                    {
                        Rectbox.Max_X = v.X;
                    }
                    if (v.Y < Rectbox.Min_Y)
                    {
                        Rectbox.Min_Y = v.Y;
                    }
                    else if (v.Y > Rectbox.Max_Y)
                    {
                        Rectbox.Max_Y = v.Y;
                    }

                }
            }

        }


        private (double pt_x, double pt_y) InterpolateSpline(double t, short degree, Vector3[] points, double[] knots, double[] weights)
        {
            int i, s, l;        // function-scoped iteration variables
            int n = points.Length;// points count

            if (degree < 1)
            {
                //throw new Error("Degree must be at least 1 (linear)")
                return (0, 0);
            }
            if (degree > (n - 1))
            {
                //throw new Error("Degree must be less than or equal to point count - 1")
                return (0, 0);
            }

            if (weights.Length == 0)
            {
                // build weight vector of length [n]
                weights = new double[n];
                for (i = 0; i < n; i++)
                {
                    weights[i] = 1;
                }
            }


            if (knots.Length == 0)
            {
                // build knot vector of length [n + degree + 1]
                knots = new double[n + degree + 1];
                for (i = 0; i < n + degree + 1; i++)
                {
                    knots[i] = i;
                }
            }
            else
            {
                if (knots.Length != n + degree + 1)
                {
                    //throw new Error("Bad knot vector length")
                    return (0, 0);
                }
            }

            int[] domain = new int[] {
                degree,
                knots.Length - 1 - degree
            };

            // remap t to the domain where the spline is defined
            double low = knots[domain[0]];
            double high = knots[domain[1]];
            t = t * (high - low) + low;


            if (t < low)
            {
                t = low;
            }
            else if (t > high)
            {
                t = high;
            }

            // find s (the spline segment) for the [t] value provided
            for (s = domain[0]; s < domain[1]; s++)
            {
                if (t >= knots[s] && t <= knots[s + 1])
                {
                    break;
                }
            }

            // convert points to homogeneous coordinates
            List<Vector3> v = new List<Vector3>();
            for (i = 0; i < n; i++)
            {
                Vector3 vpt = new Vector3();
                vpt.X = points[i].X * weights[i];
                vpt.Y = points[i].Y * weights[i];
                vpt.Z = weights[i];
                v.Add(vpt);
            }

            // l (level) goes from 1 to the curve degree + 1
            double alpha;
            for (l = 1; l <= degree + 1; l++)
            {
                // build level l of the pyramid
                for (i = s; i > s - degree - 1 + l; i--)
                {
                    alpha = (t - knots[i]) / (knots[i + degree + 1 - l] - knots[i]);

                    Vector3 vpt = v[i];
                    vpt.X = (1 - alpha) * v[i - 1].X + alpha * v[i].X;
                    vpt.Y = (1 - alpha) * v[i - 1].Y + alpha * v[i].Y;
                    vpt.Z = (1 - alpha) * v[i - 1].Z + alpha * v[i].Z;

                    v[i] = vpt;



                }
            }

            // convert back to cartesian and return
            return (v[s].X / v[s].Z, v[s].Y / v[s].Z);
        }


        //for testing only
        private void GenerateVerticesLog()
        {
            StreamWriter log = File.AppendText("d://log.txt");
            for (int i = 0; i < Vertices.Count; i++)
            {
                string Gcode = "";
                if (i == 0) { Gcode += "G00 "; }
                else { Gcode += "G01 "; }
                Gcode += "X" + Vertices[i].X + " Y" + Vertices[i].Y;
                log.WriteLine(Gcode);
            }
            log.Close();
        }
    }
}
