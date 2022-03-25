using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace DotNetARX
{
    public static class GeTools
    {
        /// <summary>
        /// 获取源点到目标点向量与X轴正向的夹角
        /// </summary>
        /// <param name="pt1">源点</param>
        /// <param name="pt2">目标点</param>
        /// <returns>夹角，表示为rad</returns>
        public static double AngleFromXAxis(this Point3d pt1,Point3d pt2)
        {
            Vector2d vector = new Vector2d(pt1.X - pt2.X, pt1.Y - pt2.Y);
            return vector.Angle;
        }
        /// <summary>
        /// 获取两个点的中点
        /// </summary>
        /// <param name="pt1">第一个端点</param>
        /// <param name="pt2">第二个端点</param>
        /// <returns>中点</returns>
        public static Point3d MidPoint(Point3d pt1, Point3d pt2)
        {
            Point3d midPoint = new Point3d((pt1.X + pt2.X) / 2,
                (pt1.Y + pt2.Y) / 2, (pt1.Z + pt2.Z) / 2);
            return midPoint;
        }
        /// <summary>
        /// 获取两个点的中点
        /// </summary>
        /// <param name="pt1">第一个端点</param>
        /// <param name="pt2">第二个端点</param>
        /// <returns>中点</returns>
        public static Point2d MidPoint(Point2d pt1, Point2d pt2)
        {
            Point2d midPoint = new Point2d((pt1.X + pt2.X) / 2,(pt1.Y + pt2.Y) / 2);
            return midPoint;
        }
        /// <summary>
        /// 获取源点偏转一定角度延伸一定距离后的点
        /// </summary>
        /// <param name="point">源点</param>
        /// <param name="angle">旋转角度</param>
        /// <param name="dist">延伸距离</param>
        /// <returns>目标点</returns>
        public static Point3d PolarPoint(this Point3d point,double angle,double dist)
        {
            return new Point3d(point.X + dist * Math.Cos(angle), point.Y + dist * Math.Sin(angle), point.Z);
        }
        /// <summary>
        /// 获取源点偏转一定角度延伸一定距离后的点
        /// </summary>
        /// <param name="point">源点</param>
        /// <param name="angle">旋转角度</param>
        /// <param name="dist">延伸距离</param>
        /// <returns>目标点</returns>
        public static Point2d PolarPoint(this Point2d point, double angle, double dist)
        {
            return new Point2d(point.X + dist * Math.Cos(angle), point.Y + dist * Math.Sin(angle));
        }
        /// <summary>
        /// 把Point3d类转换为Point2d类
        /// </summary>
        /// <param name="pt3d">待转换的Point3d对象</param>
        /// <returns>转换好的Point2d类</returns>
        public static Point2d ToPoint2d(this Point3d pt3d)
        {
            Point2d pt2d = new Point2d(pt3d.X, pt3d.Y);
            return pt2d;
        }
        /// <summary>
        /// 把Point2d类转换为Point3d类
        /// </summary>
        /// <param name="pt2d">待转换的Point2d对象</param>
        /// <returns>转换好的Point3d类</returns>
        public static Point3d ToPoint3d(this Point2d pt2d)
        {
            Point3d pt3d = new Point3d(pt2d.X, pt2d.Y,0);
            return pt3d;
        }
        /// <summary>
        /// 把Point2d类转换为Point3d类
        /// </summary>
        /// <param name="pt2d">待转换的Point2d对象</param>
        /// <param name="elevation">z坐标</param>s
        /// <returns>待转换的Point2d对象</returns>
        public static Point3d ToPoint3d(this Point2d pt2d,double elevation)
        {
            Point3d pt3d = new Point3d(pt2d.X, pt2d.Y, elevation);
            return pt3d;
        }
        /// <summary>
        /// 弧度转角度
        /// </summary>
        /// <param name="rad">弧度值</param>
        /// <returns>角度值</returns>
        public static double Rad2Deg(double rad)
        {
            double deg = rad * 180 / Math.PI;
            return deg;
        }
        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="deg">角度值</param>
        /// <returns>弧度值</returns>
        public static double Deg2Rad(double deg)
        {
            double rad = deg * Math.PI / 180;
            return rad;
        }
        /// <summary>
        /// 获得向量方向的单位向量
        /// </summary>
        /// <param name="vec">源向量</param>
        /// <returns>单位向量</returns>
        public static Vector3d GetUnitVector(this Vector3d vec)
        {
            Vector3d unitVec = vec / vec.Length;
            return unitVec;
        }
        /// <summary>
        /// 获得向量方向的单位向量
        /// </summary>
        /// <param name="vec">源向量</param>
        /// <returns>单位向量</returns>
        public static Vector2d GetUnitVector(this Vector2d vec)
        {
            Vector2d unitVec = vec / vec.Length;
            return unitVec;
        }
        /// <summary>
        /// 计算线段的倾角，表示为弧度，-pi~pi
        /// </summary>
        /// <param name="line">x线段</param>
        /// <returns>倾角</returns>
        public static double GetAngleOfLineSeg(this LineSegment3d line)
        {
            LineSegment3d lineDim = line;
            if (line.StartPoint.X > line.EndPoint.X) //如果StartPoint在EndPoint的后端
            {
                //则新建一个头尾调换的线段
                lineDim = new LineSegment3d(line.EndPoint, line.StartPoint);
            }
            Vector2d vec = new Vector2d(lineDim.EndPoint.X - lineDim.StartPoint.X,
                           lineDim.EndPoint.Y - lineDim.StartPoint.Y);
            double incline = vec.Angle;//获取直线的倾角
            if (incline > Math.PI)//大于180度的表示为负角度更为方便
                incline -= 2 * Math.PI;
            return incline;
        }
    }
}
