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
    /// <summary>
    /// 3D实体操作类
    /// </summary>
    public static class Draw3DTools
    {
        /// <summary>
        /// 在UCS中添加长方体
        /// </summary>
        /// <param name="cornerPt">长方体的角点</param>
        /// <param name="lengthX">X方向尺寸</param>
        /// <param name="lengthY">Y方向尺寸</param>
        /// <param name="lengthZ">Z方向尺寸</param>
        /// <returns>长方体的ObjectId</returns>
        public static ObjectId AddBox(Point3d cornerPt,double lengthX,double lengthY,double lengthZ)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            if(Math.Abs(lengthX)<1e-5 || Math.Abs(lengthY) < 1e-5 || Math.Abs(lengthZ) < 1e-5)
            {
                ed.WriteMessage("\n参数不当，创建长方体失败！");
                return ObjectId.Null;
            }
            Solid3d ent = new Solid3d();
            ent.RecordHistory = true;
            ent.CreateBox(Math.Abs(lengthX), Math.Abs(lengthY), Math.Abs(lengthZ));
            //位置调整
            Point3d centPt = cornerPt + new Vector3d(0.5 * lengthX, 0.5 * lengthY, 0.5 * lengthZ);
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            mt = mt * Matrix3d.Displacement(centPt - Point3d.Origin);
            ent.TransformBy(mt);
            ObjectId entId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                entId = db.AddToModelSpace(ent);//将长方体添加到模型空间
                trans.Commit();
            }
            return entId;
        }
        /// <summary>
        /// 创建圆锥形
        /// </summary>
        /// <param name="bottomCentPt">圆锥底面圆心</param>
        /// <param name="radius">圆锥底面半径</param>
        /// <param name="height">圆锥高度</param>
        /// <returns>圆锥的ObjectId</returns>
        [CommandMethod("AddCone")]
        public static ObjectId AddCone(Point3d bottomCentPt, double radius,double height)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            if(radius < 1e-5 || Math.Abs(height) <1e-5)
            {
                ed.WriteMessage("\n参数不当，创建长方体失败！");
                return ObjectId.Null;
            }
            //创建
            Solid3d ent = new Solid3d();
            ent.RecordHistory = true;
            ent.CreateFrustum(Math.Abs(height), radius, radius, 0);
            //位置调整
            Point3d cenPt = bottomCentPt + new Vector3d(0, 0, 0.5 * height);
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);
            if(height < 0)
            {
                Plane miPlane = new Plane(bottomCentPt, bottomCentPt + new Vector3d(radius, 0, 0),
                    bottomCentPt + new Vector3d(0, radius, 0));
                Matrix3d mtMirroring = Matrix3d.Mirroring(miPlane);
                mt = mt * mtMirroring;
            }
            ent.TransformBy(mt);
            ObjectId entId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                entId = db.AddToModelSpace(ent);//将长方体添加到模型空间
                trans.Commit();
            }
            return entId;
        }
        /// <summary>
        /// 由底面中心点、半径和高度在UCS中创建圆柱体
        /// </summary>
        /// <param name="bottomCenPt">底面中心点</param>
        /// <param name="radius">底面半径</param>
        /// <param name="height">高度</param>
        /// <returns>返回创建的圆柱体的Id</returns>
        public static ObjectId AddCylinder(Point3d bottomCenPt, double radius, double height)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (radius < 0.00001 || Math.Abs(height) < 0.00001)
            {
                ed.WriteMessage("\n参数不当,创建圆柱体失败！");
                return ObjectId.Null;
            }

            // 创建
            Solid3d ent = new Solid3d();
            ent.RecordHistory = true;
            ent.CreateFrustum(Math.Abs(height), radius, radius, radius);

            // 位置调整
            Point3d cenPt = bottomCenPt + new Vector3d(0.0, 0.0, 0.5 * height);
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);
            ent.TransformBy(mt);

            ObjectId entId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                entId = db.AddToModelSpace(ent);
                tr.Commit();
            }
            return entId;
        }
        /// <summary>
        /// 由中心点和半径在UCS中创建球体
        /// </summary>
        /// <param name="cenPt">中心点</param>
        /// <param name="radius">半径</param>
        /// <returns>返回创建的球体的Id</returns>
        public static ObjectId AddSphere(Point3d cenPt, double radius)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (radius < 0.00001)
            {
                ed.WriteMessage("\n参数不当,创建球体失败！");
                return ObjectId.Null;
            }

            // 创建
            Solid3d ent = new Solid3d();
            ent.RecordHistory = true;
            ent.CreateSphere(radius);

            // 位置调整
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);
            ent.TransformBy(mt);

            ObjectId entId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                entId = db.AddToModelSpace(ent);
                tr.Commit();
            }
            return entId;
        }
        /// <summary>
        /// 由中心点、圆环半径和圆管半径在UCS中创建圆环体
        /// </summary>
        /// <param name="cenPt">中心点</param>
        /// <param name="majorRadius">圆环半径</param>
        /// <param name="minorRadius">圆管半径</param>
        /// <returns>返回创建的圆环体的Id</returns>
        public static ObjectId AddTorus(Point3d cenPt, double majorRadius, double minorRadius)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (Math.Abs(majorRadius) < 0.00001 || minorRadius < 0.00001)
            {
                ed.WriteMessage("\n参数不当,创建圆锥体失败！");
                return ObjectId.Null;
            }

            try
            {
                // 创建
                Solid3d ent = new Solid3d();
                ent.RecordHistory = true;
                ent.CreateTorus(majorRadius, minorRadius);

                // 位置调整
                Matrix3d mt = ed.CurrentUserCoordinateSystem;
                mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);
                ent.TransformBy(mt);

                ObjectId entId = ObjectId.Null;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    entId = db.AddToModelSpace(ent);
                    tr.Commit();
                }
                return entId;
            }
            catch
            {
                ed.WriteMessage("\n参数不当,创建圆锥体失败！");
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// 由角点、长度、宽度和高度在UCS中创建楔体
        /// </summary>
        /// <param name="cornerPt">角点</param>
        /// <param name="lengthAlongX">长度</param>
        /// <param name="lengthAlongY">宽度</param>
        /// <param name="lengthAlongZ">高度</param>
        /// <returns>返回创建的楔体的Id</returns>
        public static ObjectId AddWedge(Point3d cornerPt, double lengthAlongX,
            double lengthAlongY, double lengthAlongZ)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (Math.Abs(lengthAlongX) < 0.00001 || Math.Abs(lengthAlongX) < 0.00001 || Math.Abs(lengthAlongX) < 0.00001)
            {
                ed.WriteMessage("\n参数不当,创建楔体失败！");
                return ObjectId.Null;
            }

            Solid3d ent = new Solid3d();
            ent.RecordHistory = true;
            ent.CreateWedge(Math.Abs(lengthAlongX), Math.Abs(lengthAlongY), Math.Abs(lengthAlongZ));

            // 位置调整
            Point3d cenPt = cornerPt + new Vector3d(0.5 * lengthAlongX, 0.5 * lengthAlongY, 0.5 * lengthAlongZ);
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);
            ent.TransformBy(mt);

            ObjectId entId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                entId = db.AddToModelSpace(ent);
                tr.Commit();
            }
            return entId;
        }

        /// <summary>
        /// 由底面中心点、高度、棱数和底面外接圆半径在UCS中创建棱柱
        /// </summary>
        /// <param name="bottomCenPt">底面中心点</param>
        /// <param name="height">高度</param>
        /// <param name="sides">棱数</param>
        /// <param name="radius">底面外接圆半径</param>
        /// <returns>返回创建的棱柱的Id</returns>
        public static ObjectId AddPrism(Point3d bottomCenPt, double height, int sides, double radius)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (Math.Abs(height) < 0.00001 || radius < 0.00001 || sides < 3 || sides > 32)
            {
                ed.WriteMessage("\n参数不当,创建棱柱失败！");
                return ObjectId.Null;
            }

            // 创建
            Solid3d ent = new Solid3d();
            ent.RecordHistory = true;
            ent.CreatePyramid(Math.Abs(height), sides, radius, radius);

            // 位置调整
            Point3d cenPt = bottomCenPt + new Vector3d(0.0, 0.0, 0.5 * height);
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);
            ent.TransformBy(mt);

            ObjectId entId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                entId = db.AddToModelSpace(ent);
                tr.Commit();
            }
            return entId;
        }

        /// <summary>
        /// 由底面中心点、高度、棱数和底面外接圆半径创建棱锥
        /// </summary>
        /// <param name="bottomCenPt">底面中心点</param>
        /// <param name="height">高度</param>
        /// <param name="sides">棱数</param>
        /// <param name="radius">底面外接圆半径</param>
        /// <returns>返回创建的棱锥的Id</returns>
        public static ObjectId AddPyramid(Point3d bottomCenPt, double height, int sides, double radius)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (Math.Abs(height) < 0.00001 || radius < 0.00001 || sides < 3 || sides > 32)
            {
                ed.WriteMessage("\n参数不当,创建棱柱失败！");
                return ObjectId.Null;
            }

            // 创建
            Solid3d ent = new Solid3d();
            ent.RecordHistory = true;
            ent.CreatePyramid(Math.Abs(height), sides, radius, 0);

            // 位置调整
            Point3d cenPt = bottomCenPt + new Vector3d(0.0, 0.0, 0.5 * height);
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);

            if (height < 0)
            {
                Plane miPlane = new Plane(bottomCenPt, bottomCenPt + new Vector3d(radius, 0.0, 0.0),
                bottomCenPt + new Vector3d(0.0, radius, 0.0));
                Matrix3d mtMirroring = Matrix3d.Mirroring(miPlane);
                mt = mt * Matrix3d.Mirroring(miPlane);
            }

            ent.TransformBy(mt);

            ObjectId entId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                entId = db.AddToModelSpace(ent);
                tr.Commit();
            }
            return entId;
        }
        /// <summary>
        /// 按照拉伸高度创建拉伸体
        /// </summary>
        /// <param name="region">拉伸面</param>
        /// <param name="height">拉伸高度</param>
        /// <param name="taperAngle">拉伸角度</param>
        /// <returns>新建拉伸体的ObjectId</returns>
        public static ObjectId AddExtrudedSolid(Region region,double height, double taperAngle)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                Solid3d ent = new Solid3d();
                ent.Extrude(region, height, taperAngle);
                ObjectId entId = ObjectId.Null;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    entId = db.AddToModelSpace(ent);
                    trans.Commit();
                }
                return entId;
            }
            catch
            {
                ed.WriteMessage("\n参数不当，创建拉伸体失败！");
                return ObjectId.Null;
            }
        }
        /// <summary>
        /// 按照拉伸路径创建拉伸体
        /// </summary>
        /// <param name="region">拉伸面域</param>
        /// <param name="path">拉伸路径</param>
        /// <param name="taperAngle">拉伸角度</param>
        /// <returns>新建拉伸体的ObjectId</returns>
        public static ObjectId AddExtrudedSolid(Region region,Curve path,double taperAngle)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                Solid3d ent = new Solid3d();
                ent.ExtrudeAlongPath(region, path, taperAngle);
                ObjectId entId = ObjectId.Null;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    entId = db.AddToModelSpace(ent);
                    trans.Commit();
                }
                return entId;
            }
            catch
            {
                ed.WriteMessage("\n参数不当，创建拉伸体失败！");
                return ObjectId.Null;
            }
        }
        /// <summary>
        /// 创建旋转体
        /// </summary>
        /// <param name="region">旋转面域</param>
        /// <param name="axisPt1">旋转轴端点</param>
        /// <param name="axisPt2">旋转轴端点</param>
        /// <param name="angle">旋转角度</param>
        /// <returns>新建旋转体的ObjectId</returns>
        public static ObjectId AddRevolvedSolid(Region region,Point3d axisPt1,
            Point3d axisPt2,double angle)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                Solid3d ent = new Solid3d();
                ent.Revolve(region, axisPt1,axisPt2-axisPt1,angle);
                ObjectId entId = ObjectId.Null;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    entId = db.AddToModelSpace(ent);
                    trans.Commit();
                }
                return entId;
            }
            catch
            {
                ed.WriteMessage("\n参数不当，创建拉伸体失败！");
                return ObjectId.Null;
            }
        }
        /// <summary>
        /// 布尔运算
        /// </summary>
        /// <param name="boolType">布尔运算类型</param>
        /// <param name="solid3dId1">第一个实体对象</param>
        /// <param name="solid3dId2">第二个实体对象</param>
        /// <returns>布尔运算是否成功</returns>
        public static bool BoolSolid3dRegion(BooleanOperationType boolType,
            ObjectId solid3dId1,ObjectId solid3dId2)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    Entity ent1 = trans.GetObject(solid3dId1, OpenMode.ForWrite) as Entity;
                    Entity ent2 = trans.GetObject(solid3dId2, OpenMode.ForWrite) as Entity;
                    if(ent1 == null || ent2 == null)
                    {
                        ed.WriteMessage("\n布尔操作失败！");
                        return false;
                    }
                    if(ent1 is Solid3d && ent2 is Solid3d)
                    {
                        Solid3d solid3dEnt1 = ent1 as Solid3d;
                        Solid3d solid3dEnt2 = ent2 as Solid3d;
                        solid3dEnt1.BooleanOperation(boolType, solid3dEnt2);
                        ent2.Dispose();
                    }
                    if (ent1 is Region && ent2 is Region)
                    {
                        Region regionEnt1 = ent1 as Region;
                        Region regionEnt2 = ent2 as Region;
                        regionEnt1.BooleanOperation(boolType, regionEnt2);
                        ent2.Dispose();
                    }
                }
                catch
                {
                    ed.WriteMessage("\n布尔操作失败！");
                    return false;
                }
                trans.Commit();
                return true;
            }
        }
    }
}
