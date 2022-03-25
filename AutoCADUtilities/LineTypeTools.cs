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
    public static class LineTypeTools
    {
        /// <summary>
        /// 添加新的线型
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="typeName">线型名称</param>
        /// <returns>新加线型的ObjectId</returns>
        public static ObjectId AddLineType(this Database db, string typeName)
        {
            //打开线型表
            LinetypeTable lt = db.LinetypeTableId.GetObject(OpenMode.ForRead) as LinetypeTable;
            if (!lt.Has(typeName)) //如果不存在名typeName的线型表记录
            {
                lt.UpgradeOpen();//切换线型表为写
                //新建一个线型表记录
                LinetypeTableRecord ltr = new LinetypeTableRecord();
                ltr.Name = typeName;//设置线型表记录的名称
                lt.Add(ltr);//将新的线型表记录信息添加到线型表中
                db.TransactionManager.AddNewlyCreatedDBObject(ltr, true);//将新建的线型表记录添加到数据库中
                lt.DowngradeOpen();
            }
            return lt[typeName];
        }
        /// <summary>
        /// 从acad.lin文件中加载指定线型
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="typeName">线型名称</param>
        /// <returns>线型的ObjectId</returns>
        public static ObjectId LoadLineType(this Database db, string typeName)
        {
            //打开线型表
            LinetypeTable lt = db.LinetypeTableId.GetObject(OpenMode.ForRead) as LinetypeTable;
            if (!lt.Has(typeName)) //如果不存在名typeName的线型表记录
            {
                //加载typeName线型
                db.LoadLineTypeFile(typeName, "acad.lin");
            }
            return lt[typeName];
        }

    }
}
