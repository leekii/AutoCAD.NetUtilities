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
    /// 标注中的箭头符号
    /// </summary>
    public struct DimArrowBlock
    {
        /// <summary>
        /// 实心闭合
        /// </summary>
        public static readonly string ClosedFilled = "";
        /// <summary>
        /// 点
        /// </summary>
        public static readonly string Dot = "_DOT";
        /// <summary>
        /// 小点
        /// </summary>
        public static readonly string DotSmall = "_DOTSMALL";
        /// <summary>
        /// 空心点
        /// </summary>
        public static readonly string DotBlank = "_DOTBLANK";
        /// <summary>
        /// 原点标记
        /// </summary>
        public static readonly string Origin = "_ORIGIN";
        /// <summary>
        /// 原点标记2
        /// </summary>
        public static readonly string Origin2 = "_ORIGIN2";
        /// <summary>
        /// 打开
        /// </summary>
        public static readonly string Open = "_OPEN";
        /// <summary>
        /// 直角
        /// </summary>
        public static readonly string RightAngle = "_OPEN90";
        /// <summary>
        /// 30度角
        /// </summary>
        public static readonly string Angle30 = "_OPEN30";
        /// <summary>
        /// 闭合
        /// </summary>
        public static readonly string Closed = "_CLOSED";
        /// <summary>
        /// 空心小点
        /// </summary>
        public static readonly string DotSmallBlank = "_SMALL";
        /// <summary>
        /// 无
        /// </summary>
        public static readonly string None = "_NONE";
        /// <summary>
        /// 倾斜
        /// </summary>
        public static readonly string Oblique = "_OBLIQUE";
        /// <summary>
        /// 实心框
        /// </summary>
        public static readonly string BoxFilled = "_BOXFILLED";
        /// <summary>
        /// 框
        /// </summary>
        public static readonly string Box = "_BOXBLANK";
        /// <summary>
        /// 空心闭合
        /// </summary>
        public static readonly string ClosedBlank = "_CLOSEDBLANK";
        /// <summary>
        /// 实心基准三角形
        /// </summary>
        public static readonly string TriangleFilled = "_DATUMFILLED";
        /// <summary>
        /// 基准三角形
        /// </summary>
        public static readonly string Triangle = "_DATUMBLANK";
        /// <summary>
        /// 积分
        /// </summary>
        public static readonly string Integral = "_INTEGRAL";
        /// <summary>
        /// 建筑标记
        /// </summary>
        public static readonly string ArchitecturalTick = "_ARCHTICK";
    }
    public static class DimTools
    {
        public static string ArrowBlock
        {
            //获取DIMBLK系统变量值，表示尺寸线末端的箭头块
            get { return Application.GetSystemVariable("DIMBLK").ToString(); }
            set { Application.SetSystemVariable("DIMBLK", value); }
        }
        public static ObjectId GetArrowObjectId(this Database db, string arrowName)
        {
            ObjectId arrId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (bt.Has(arrowName)) arrId = bt[arrowName];
                trans.Commit();
            }
            return arrId;
        }
        /// <summary>
        /// 创建圆弧半径标注
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="arc">待标注圆弧</param>
        /// <param name="scale">缩放比例</param>
        /// <returns>标注的ObjectId</returns>
        public static ObjectId ArrowRadiusDim(this Database db, CircularArc3d arc,double scale)
        {
            ObjectId txtId = ObjectId.Null;
            //1.获得各点向量及圆弧所在平面
            Vector3d refVec = arc.ReferenceVector;//获取圆弧参考向量
            Vector3d startVec = refVec.RotateBy(arc.StartAngle, arc.Normal);//起点向量
            Vector3d endVec = refVec.RotateBy(arc.EndAngle, arc.Normal);//终点向量
            Vector3d midVec = (startVec + endVec) / 2;//中点向量
            Plane arcPlane = new Plane();//圆弧所在平面
            double incline = midVec.AngleOnPlane(arcPlane);
            if (incline > Math.PI)//大于180度的表示为负角度更为方便
                incline -= 2 * Math.PI;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //2.创建圆弧箭头
                Polyline arrow = new Polyline();
                Point3d pt1 = arc.Center + arc.Radius / midVec.Length * midVec;//箭头起点为圆弧中点
                arrow.AddVertexAt(0, pt1.ToPoint2d(), 0, 0, 0.4 * scale);//箭头宽度为0~0.4个单位
                Point3d pt2 = pt1 - 1.25 * scale / midVec.Length * midVec;//箭头长度为1.25个单位
                arrow.AddVertexAt(1, pt2.ToPoint2d(), 0, 0, 0);//添加第二个端点
                //3.创建圆弧文字
                DBText radiusString = new DBText();
                radiusString.TextString = "R="+arc.Radius.ToString("F0");//文字内容
                radiusString.Height = 3 * scale;//文字高度为3个单位
                radiusString.TextStyleId = db.Textstyle;//文字样式与CAD当前样式相同
                radiusString.WidthFactor = ((TextStyleTableRecord)
                    db.Textstyle.GetObject(OpenMode.ForRead)).XScale;//文字宽度比例与当前样式相同
                radiusString.Position = pt2 - 0.6 * scale * midVec;//文字位置为箭头尾部向圆心侧移动0.6个单位
                radiusString.HorizontalMode = TextHorizontalMode.TextLeft;//水平方向中对齐
                radiusString.VerticalMode = TextVerticalMode.TextBottom;//竖直方向底对齐
                radiusString.AlignmentPoint = radiusString.Position;//对齐点即为插入点
                double txtLength = (radiusString.GeometricExtents.MaxPoint.X -
                    radiusString.GeometricExtents.MinPoint.X);//获取文字的范围
                radiusString.Rotation = incline + Math.PI;//文字旋转角度
                //4.创建引线              
                Point3d pt3 = pt2 - txtLength / midVec.Length * midVec;//直线另一端点，直线长为10个单位
                Line line = new Line(pt2,pt3);
                db.AddToModelSpace(radiusString, arrow, line);//将新建图素都加入模型空间
                txtId = radiusString.ObjectId;//返回文字的ObjectId，方便修改
                trans.Commit();
            }
            return txtId;//返回文字的ObjectId，方便修改
        }
        /// <summary>
        /// 标注圆弧长度
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="arc">几何圆弧</param>
        /// <param name="scale">缩放比例</param>
        /// <returns>标注的ObjectId</returns>
        public static ObjectId ArcLengthDim(this Database db, CircularArc3d arc, double scale)
        {
            ObjectId txtId = ObjectId.Null;
            //1.获得各点向量及圆弧所在平面
            Vector3d refVec = arc.ReferenceVector;//获取圆弧参考向量
            Plane arcPlane = new Plane();//圆弧所在平面
            Vector3d startVec = refVec.RotateBy(arc.StartAngle, arc.Normal);//起点向量
            Vector3d endVec = refVec.RotateBy(arc.EndAngle, arc.Normal);//终点向量
            Vector3d midVec = (startVec + endVec) / 2;//中点向量
            double incline = midVec.AngleOnPlane(arcPlane);//中点倾角
            if (incline > Math.PI)//大于180度的表示为负角度更为方便
                incline -= 2 * Math.PI;
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //钢束线的长度
                DBText length = new DBText();
                //长度
                length.TextString = Math.Abs(arc.Radius * (arc.EndAngle - arc.StartAngle)).ToString("F0");
                //文字高度
                length.Height = 3 * scale;
                //样式为当前样式
                length.TextStyleId = db.Textstyle;
                //文字宽度比例与当前文字样式相同
                length.WidthFactor = ((TextStyleTableRecord)
                    db.Textstyle.GetObject(OpenMode.ForRead)).XScale;
                //位置为中点垂线以下0.5个单位
                length.Position = GeTools.PolarPoint(arc.Center, incline, arc.Radius + 0.5 * scale);
                //旋转角度
                length.Rotation = (incline > 0) ? incline - Math.PI / 2 : incline + Math.PI / 2;
                //水平对齐位置
                length.HorizontalMode = TextHorizontalMode.TextCenter;
                //竖直对齐位置
                length.VerticalMode = (incline > 0) ? TextVerticalMode.TextBottom : TextVerticalMode.TextTop;
                //对齐点即为插入点
                length.AlignmentPoint = length.Position;
                db.AddToModelSpace(length);//将新建图素都加入模型空间
                txtId = length.ObjectId;//返回文字的ObjectId，方便修改
                trans.Commit();//执行事务处理
            }
            return txtId;//返回文字的ObjectId，方便修改
        }
        /// <summary>
        /// 添加水平连续标注
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="pts">标注几何点</param>
        /// <param name="pos">标注线位置</param>
        /// <param name="scale">标注比例</param>
        /// <returns>标注的ObjectId集合</returns>
        public static ObjectIdCollection ContinuedHorizontalDims(this Database db, 
            List<Point3d> pts, Point3d pos, double scale)
        {
            if (pts.Count < 2) return null;//少于两个点，则返回
            ObjectIdCollection dimIds = new ObjectIdCollection();
            var ptsOrderedX = (from pt in pts
                              orderby pt.X
                              select pt).ToList();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                for(int i = 0; i < pts.Count-1; i++)
                {
                    RotatedDimension dimH = new RotatedDimension();
                    dimH.XLine1Point = ptsOrderedX[i];    //第一条尺寸边线
                    dimH.XLine2Point = ptsOrderedX[i+1];  //第二条尺寸边线
                    dimH.DimLinePoint = pos;              //尺寸线位置
                    dimH.DimensionStyle = db.Dimstyle;    //尺寸样式为当前样式
                    dimH.Dimscale = scale;                 //设置尺寸全局比例
                    db.AddToModelSpace(dimH);//将新建尺寸添加到模型空间
                    dimIds.Add(dimH.ObjectId);//添加新建标注至返回集中
                }
                trans.Commit();
            }
            return dimIds;//返回新建标注集的ObjectId集便于修改
        }
        /// <summary>
        /// 添加竖直连续标注
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="pts">标注几何点</param>
        /// <param name="pos">标注线位置</param>
        /// <param name="scale">标注比例</param>
        /// <returns>标注的ObjectId集合</returns>
        public static ObjectIdCollection ContinuedVerticalDims(this Database db,
            List<Point3d> pts, Point3d pos, double scale)
        {
            if (pts.Count < 2) return null;//少于两个点，则返回
            ObjectIdCollection dimIds = new ObjectIdCollection();
            var ptsOrderedX = (from pt in pts
                               orderby pt.Y
                               select pt).ToList();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < pts.Count - 2; i++)
                {
                    RotatedDimension dimV = new RotatedDimension();
                    dimV.XLine1Point = ptsOrderedX[i];      //第一条尺寸边线
                    dimV.XLine2Point = ptsOrderedX[i + 1];  //第二条尺寸边线
                    dimV.DimLinePoint = pos;              //尺寸线位置
                    dimV.DimensionStyle = db.Dimstyle;    //尺寸样式为当前样式
                    dimV.Dimscale = scale;                 //设置尺寸全局比例
                    dimV.Rotation = Math.PI / 2;//旋转90度
                    db.AddToModelSpace(dimV);//将新建尺寸添加到模型空间
                    dimIds.Add(dimV.ObjectId);//添加新建标注至返回集中
                }
                trans.Commit();
            }
            return dimIds;//返回新建标注集的ObjectId集便于修改
        }
        /// <summary>
        /// 标注直线段长度
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="line">直线段</param>
        /// <param name="scale">标注比例</param>
        /// <returns>标注的ObjectId</returns>
        public static ObjectId LineLengthDim(this Database db, LineSegment3d line, double scale)
        {
            ObjectId txtId = ObjectId.Null;
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
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //标注线段长度
                DBText length = new DBText();
                //长度
                length.TextString = lineDim.Length.ToString("F0");
                //文字高度为3个单位
                length.Height = 3 * scale;
                //样式为当前样式
                length.TextStyleId = db.Textstyle;
                //文字宽度比例与当前文字样式相同
                length.WidthFactor = ((TextStyleTableRecord)
                    db.Textstyle.GetObject(OpenMode.ForRead)).XScale;
                //位置为中点垂线以上0.5个单位
                length.Position = GeTools.PolarPoint(lineDim.MidPoint, incline + Math.PI / 2, 0.5 * scale);
                //旋转角度同直线段倾角
                length.Rotation = incline;
                //对齐位置为中下
                length.HorizontalMode = TextHorizontalMode.TextCenter;
                length.VerticalMode = TextVerticalMode.TextBottom;
                //对齐点即为插入点
                length.AlignmentPoint = length.Position;
                db.AddToModelSpace(length);//将新建标注添加到模型空间
                txtId = length.ObjectId;//返回文字的ObjectId，方便修改
                trans.Commit();//执行事务处理
            }
            return txtId;//返回文字的ObjectId，方便修改
        }
        /// <summary>
        /// 标注直线倾角
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="line">直线段</param>
        /// <param name="isDimStart">是否在直线段左端标注</param>
        /// <param name="scale">标注比例</param>
        /// <returns>标注的ObjectId</returns>
        public static ObjectId LineAngleDim(this Database db,LineSegment3d line,bool isDimStart, double scale)
        {
            ObjectId dimId = ObjectId.Null;
            LineSegment3d lineDim = line;
            
            if (line.StartPoint.X > line.EndPoint.X) //如果StartPoint在EndPoint的后端
            {
                //则新建一个头尾调换的线段
                lineDim = new LineSegment3d(line.EndPoint, line.StartPoint);
            }
            Vector2d vec = new Vector2d(lineDim.EndPoint.X - lineDim.StartPoint.X,
                           lineDim.EndPoint.Y - lineDim.StartPoint.Y);
            double incline = vec.Angle;//获取直线的倾角
            if (Math.Abs(incline) < 1e-5) return ObjectId.Null;//角度太小就忽略
            if (incline > Math.PI)//大于180度的表示为负角度更为方便
                incline -= 2 * Math.PI;
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                
                Point3AngularDimension dimA = new Point3AngularDimension();
                if (isDimStart == true)//在起点标注
                {
                    dimA.CenterPoint = lineDim.StartPoint;//角度中心
                    //角度起点和终点
                    dimA.XLine1Point = GeTools.PolarPoint(dimA.CenterPoint, incline, 1000);//第一条线段的终点
                    dimA.XLine2Point = GeTools.PolarPoint(dimA.CenterPoint, 0, 1000);//水平向距离为1m的点
                    //弧点到角度中心距离为600
                    dimA.ArcPoint = GeTools.PolarPoint(dimA.CenterPoint, incline / 2, 600);
                }
                else//在终点标注
                {
                    dimA.CenterPoint = lineDim.EndPoint;//角度中心
                    //角度起点和终点
                    dimA.XLine1Point = GeTools.PolarPoint(dimA.CenterPoint, incline, -1000);//第一条线段的终点
                    dimA.XLine2Point = GeTools.PolarPoint(dimA.CenterPoint, 0, -1000);//水平向距离为1m的点
                    //弧点到角度中心距离为600
                    dimA.ArcPoint = GeTools.PolarPoint(dimA.CenterPoint, incline / 2, -600);
                }
                dimA.DimensionStyle = db.Dimstyle;//尺寸样式为当前样式
                dimA.Dimscale = scale;//设置尺寸全局比例
                db.AddToModelSpace(dimA);//将新建标注添加到模型空间
                ObjectId arrowId = db.GetArrowObjectId(DimArrowBlock.ClosedFilled);//角度标注为实心箭头较为美观                
                if (arrowId == ObjectId.Null)
                {
                    ArrowBlock = ".";//如果当前数据库尚未添加实心箭头则添加
                    arrowId = db.GetArrowObjectId(DimArrowBlock.ClosedFilled);
                }
                dimA.Dimblk = arrowId;//设置实心箭头
                dimA.Dimasz = 1.25;//箭头大小1.25
                dimId = dimA.ObjectId;//返回新建标注的ObjectId便于修改
                trans.Commit();//执行事务处理
            }
            return dimId;//返回新建标注的ObjectId便于修改
        }
        /// <summary>
        /// 标注直线段斜率
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="line">标注直线段</param>
        /// <param name="scale">标注比例</param>
        /// <param name="isY2X">是否表示为“1：x”的形式，false时为“x:1”形式</param>
        /// <returns>斜率标注文字的ObjectId</returns>
        public static ObjectId LineSlopeDim(this Database db, LineSegment3d line, double scale, bool isY2X=true)
        {
            ObjectId txtId = ObjectId.Null;
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
            double slopeY2X = Math.Abs(line.EndPoint.Y - line.StartPoint.Y) / Math.Abs(line.EndPoint.X - line.StartPoint.X);//Y:X斜率
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //标注线段长度
                DBText slope = new DBText();
                //长度
                slope.TextString = isY2X ? "1:" + slopeY2X.ToString("F3"): (1 / slopeY2X).ToString("F3") + ":1";
                //文字高度为3个单位
                slope.Height = 3 * scale;
                //样式为当前样式
                slope.TextStyleId = db.Textstyle;
                //文字宽度比例与当前文字样式相同
                slope.WidthFactor = ((TextStyleTableRecord)
                    db.Textstyle.GetObject(OpenMode.ForRead)).XScale;
                //位置为中点垂线以上0.5个单位
                slope.Position = GeTools.PolarPoint(lineDim.MidPoint, incline + Math.PI / 2, 0.5 * scale);
                //旋转角度同直线段倾角
                slope.Rotation = incline;
                //对齐位置为中下
                slope.HorizontalMode = TextHorizontalMode.TextCenter;
                slope.VerticalMode = TextVerticalMode.TextBottom;
                //对齐点即为插入点
                slope.AlignmentPoint = slope.Position;
                db.AddToModelSpace(slope);//将新建标注添加到模型空间
                txtId = slope.ObjectId;//返回文字的ObjectId，方便修改
                trans.Commit();//执行事务处理
            }
            return txtId;//返回文字的ObjectId，方便修改
        }
    }
}
