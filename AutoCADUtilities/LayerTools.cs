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
using Autodesk.AutoCAD.Colors;

namespace DotNetARX
{
    public static class LayerTools
    {
        /// <summary>
        /// 添加新图层
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="layerName">新加图层名</param>
        /// <returns>新加图层的ObjectId</returns>
        public static ObjectId AddLayer(this Database db, string layerName)
        {
            //打开层表
            LayerTable lt = db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable;
            if(!lt.Has(layerName))
            {
                LayerTableRecord ltr = new LayerTableRecord();
                ltr.Name = layerName;
                lt.UpgradeOpen(); //将图层表升级为写模式
                lt.Add(ltr);
                //把层表记录添加到事务处理中
                //之前创建了添加实体到模型空间的函数，尚未建立添加其他元素的函数
                db.TransactionManager.AddNewlyCreatedDBObject(ltr, true);
                lt.DowngradeOpen();//为了数据安全，降级为写模式
            }
            return lt[layerName];//返回新建图层的ObjectId
        }
        /// <summary>
        /// 设置图层颜色
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="layerName">图层名</param>
        /// <param name="colorIndex">颜色索引号</param>
        /// <returns>是否修改成功</returns>
        public static bool SetLayerColor(this Database db, string layerName, short colorIndex)
        {
            //打开层表
            LayerTable lt = db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable;
            if (!lt.Has(layerName)) return false;
            ObjectId layerId = lt[layerName];
            LayerTableRecord ltr = layerId.GetObject(OpenMode.ForWrite) as LayerTableRecord;
            ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
            ltr.DowngradeOpen();//为数据安全，将层表记录的打开方式将为读
            return true;
        }
        /// <summary>
        /// 设置当前图层
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="layerName">设为当前图层的图层名</param>
        /// <returns>设置是否成功</returns>
        public static bool SetCurrentLayer(this Database db,string layerName)
        {
            //打开层表
            LayerTable lt = db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable;
            if (!lt.Has(layerName)) return false;
            ObjectId layerId = lt[layerName];
            if (db.Clayer == layerId) return false;
            db.Clayer = layerId;
            return true;
        }
        /// <summary>
        /// 获取全部图层信息
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <returns>包含全部图层信息的List</returns>
        public static List<LayerTableRecord> GetAllLayers(this Database db)
        {
            //打开层表
            LayerTable lt = db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable;
            //用于返回层表记录的List
            List<LayerTableRecord> ltrs = new List<LayerTableRecord>();
            foreach(ObjectId id in lt)
            {
                //打开层表记录
                LayerTableRecord ltr = id.GetObject(OpenMode.ForRead) as LayerTableRecord;
                ltrs.Add(ltr);
            }
            return ltrs;
        }
        /// <summary>
        /// 删除图层
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="layerName">待删除图层名</param>
        /// <returns>删除是否成功</returns>
        public static bool DeleteLayer(this Database db, string layerName)
        {
            //打开层表
            LayerTable lt = db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable;
            if (!lt.Has(layerName)) return false;
            if (layerName == "0" || layerName == "Defpoints") return false;
            ObjectId layerId = lt[layerName]; //获取待删除图层的ObjectId
            if (layerId == db.Clayer) return false;//当前图层无法删除

            //打开待删除图层，如果被使用（包含对象、外部参照），则删除失败
            LayerTableRecord ltr = layerId.GetObject(OpenMode.ForRead) as LayerTableRecord;
            lt.GenerateUsageData();
            if (ltr.IsUsed) return false;

            ltr.UpgradeOpen();//升级为写模式以进行删除操作
            ltr.Erase(true);
            return true;
        }
        /// <summary>
        /// 获取数据库中全部图层的ObjectId
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <returns>全部图层的ObjectId组成的List</returns>
        public static List<ObjectId> GetAllLayerObjectIds(this Database db)
        {
            //打开层表
            LayerTable lt = db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable;
            //用于返回层表记录的List
            List<ObjectId> layerIds = new List<ObjectId>();
            foreach (ObjectId id in lt)
            {
                layerIds.Add(id);
            }
            return layerIds;
        }
    }
}
