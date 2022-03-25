using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace DotNetARX
{
    public static class EllipseTools
    {
        public static void CreateEllipse(this Ellipse ellipse, Point3d pt1, Point3d pt2)
        {
            Point3d center = GeTools.MidPoint(pt1, pt2);
            Vector3d normal = Vector3d.ZAxis;
            Vector3d majorAxis = new Vector3d(Math.Abs(pt1.X - pt2.X) / 2, 0, 0);
            double ratio = Math.Abs((pt1.Y - pt2.Y) / (pt1.X - pt2.X));
            ellipse.Set(center, normal, majorAxis, ratio, 0, 2 * Math.PI);
        }       
    }
}
