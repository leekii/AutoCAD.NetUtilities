using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace DotNetARX
{
    public static class CircleTools
    {
        public static bool CreateCircle(this Circle circle, Point3d pt1,Point3d pt2,Point3d pt3)
        {
            //判断三点是否共线
            Vector3d va = pt1.GetVectorTo(pt2);
            Vector3d vb = pt1.GetVectorTo(pt3);
            if (va.GetAngleTo(vb) == 0 | va.GetAngleTo(vb) == Math.PI)
            {
                return false;
            }
            else
            {
                CircularArc3d geArc = new CircularArc3d(pt1, pt2, pt3);
                circle.Center = geArc.Center;
                circle.Radius = geArc.Radius;
                return true;
            }
        }  
    }
}
