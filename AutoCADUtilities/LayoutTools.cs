using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;


namespace DotNetARX
{
    public static class LayoutTools
    {
        /// <summary>
        /// 获取数据库的所有布局
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <returns>返回所有布局</returns>
        public static List<Layout> GetAllLayouts(this Database db)
        {
            List<Layout> layouts = new List<Layout>();
            BlockTable bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
            foreach (ObjectId id in bt)
            {
                BlockTableRecord btr = (BlockTableRecord)id.GetObject(OpenMode.ForRead);
                if (btr.IsLayout && btr.Name.ToUpper() != BlockTableRecord.ModelSpace.ToUpper())
                {
                    Layout layout = (Layout)btr.LayoutId.GetObject(OpenMode.ForRead);
                    layouts.Add(layout);
                }
            }
            return layouts.OrderBy(layout => layout.TabOrder).ToList();
        }
        /// <summary>
        /// 获得布局中的全部实体
        /// </summary>
        /// <param name="layout">布局名称</param>
        /// <param name="bIncludeFirstViewPort">是否包含第一个视口</param>
        /// <returns>所有实体的ObjectIdCollection</returns>
        public static ObjectIdCollection GetEntsInLayout(
            this Layout layout, bool bIncludeFirstViewPort)
        {
            ObjectIdCollection entIds = new ObjectIdCollection();
            ObjectId blkDefId = layout.BlockTableRecordId;
            BlockTableRecord btr = blkDefId.GetObject(OpenMode.ForRead) as BlockTableRecord;
            if (btr == null) return null;
            bool bFirstViewPort = true;
            foreach(ObjectId entId in btr)
            {
                Viewport vp = entId.GetObject(OpenMode.ForRead) as Viewport;
                if (vp != null && bFirstViewPort)
                {
                    if (bIncludeFirstViewPort) entIds.Add(entId);
                    bFirstViewPort = false;
                }
                else entIds.Add(entId);
                ObjectId dictId = vp.ExtensionDictionary;
                if(dictId.IsValid)
                {
                    DBDictionary dict = dictId.GetObject(OpenMode.ForWrite) as DBDictionary;
                    dict.TreatElementsAsHard = true;
                }
            }
            return entIds;
        }
        /// <summary>
        /// 获得布局的ObjectId并返回布局中的全部实体
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="name">布局名称</param>
        /// <param name="entIds">所有实体的ObjectIdCollection</param>
        /// <returns></returns>
        public static ObjectId GetLayoutId(
            this Database db, string name, ref ObjectIdCollection entIds)
        {
            ObjectId layoutId = new ObjectId();
            BlockTable bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
            foreach(ObjectId btrId in bt)
            {
                BlockTableRecord btr = btrId.GetObject(OpenMode.ForRead) as BlockTableRecord;
                if(btr.IsLayout)
                {
                    Layout layout = btr.LayoutId.GetObject(OpenMode.ForRead) as Layout;
                    if(layout.LayoutName.CompareTo(name) == 0)
                    {
                        layoutId = btr.LayoutId;
                        //获取布局中的全部实体
                        entIds = layout.GetEntsInLayout(true);
                        break;
                    }
                }
            }
            return layoutId;
        }
        /// <summary>
        /// 确保布局中的图纸显示在布局中的中间，而不需要使用缩放命令来显示
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="layoutName">布局的名称</param>
        public static void CenterLayoutViewport(this Database db,string layoutName)
        {
            Extents3d ext = db.GetAllEntsExtent();
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                BlockTable bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                foreach(ObjectId btrId in bt)
                {
                    BlockTableRecord btr = btrId.GetObject(OpenMode.ForRead) as BlockTableRecord;
                    if(btr.IsLayout)
                    {
                        Layout layout = btr.LayoutId.GetObject(OpenMode.ForRead) as Layout;
                        if(layout.LayoutName.CompareTo(layoutName) == 0)
                        {
                            int vpIndex = 0;
                            ObjectId firstViewportId = new ObjectId();
                            ObjectId secondViewportId = new ObjectId();
                            foreach(ObjectId entId in btr)
                            {
                                Entity ent = entId.GetObject(OpenMode.ForWrite) as Entity;
                                if(ent is Viewport)
                                {
                                    if(vpIndex == 0)
                                    {
                                        firstViewportId = entId;
                                        vpIndex++;
                                    }
                                    else if (vpIndex == 1)
                                    {
                                        secondViewportId = entId;
                                    }
                                }
                            }
                            //布局复制过来以后得到两个视口，第一个视口与模型空间关联，第二个视口则放在正确的位置上
                            if(firstViewportId.IsValid && secondViewportId.IsValid)
                            {
                                Viewport secondVp = secondViewportId.GetObject(OpenMode.ForRead) as Viewport;
                                secondVp.ColorIndex = 1;
                                secondVp.Erase();
                                Viewport firstVp = firstViewportId.GetObject(OpenMode.ForRead) as Viewport;
                                firstVp.CenterPoint = secondVp.CenterPoint;
                                firstVp.Height = secondVp.Height;
                                firstVp.Width = secondVp.Width;
                                firstVp.ColorIndex = 5;
                                Point3d midPt = GeTools.MidPoint(ext.MinPoint, ext.MaxPoint);
                                firstVp.ViewCenter = midPt.ToPoint2d();
                                double xScale = secondVp.Width / ((ext.MaxPoint.X - ext.MinPoint.X) * 1.1);
                                double yScale = secondVp.Height / ((ext.MaxPoint.Y - ext.MinPoint.Y) * 1.1);
                                firstVp.CustomScale = Math.Min(xScale, yScale);
                            }
                        }
                    }
                }
                trans.Commit();//执行事务处理
            }
        }
    }
}
