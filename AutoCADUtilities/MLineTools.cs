using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace DotNetARX
{
    /// <summary>
    /// 多线工具
    /// </summary>
    public static class MLineTools
    {
        /// <summary>
        /// 创建多线样式
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="styleName">多线样式名称</param>
        /// <param name="elements">多线样式元素</param>
        /// <returns>新建多线样式的Id</returns>
         public static ObjectId CreateMLineStyle(
             this Database db, string styleName, List<MlineStyleElement> elements)
        {
            //打开当前数据库的多线样式字典对象
            DBDictionary dict = db.MLStyleDictionaryId.GetObject(OpenMode.ForRead) as DBDictionary;
            if (dict.Contains(styleName))//如果已经存在多项样式
                return (ObjectId)dict[styleName];//返回该多线样式的Id
            MlineStyle mStyle = new MlineStyle();
            mStyle.Name = styleName;//设置多线样式的名称
            //为多线样式添加新的元素
            foreach(var element in elements)
            {
                mStyle.Elements.Add(element, true);
            }
            //设置多线样式字典为写状态
            dict.UpgradeOpen();
            //在多线样式字典中加入新创建的样式，并制定关键字为styleName
            dict.SetAt(styleName, mStyle);
            //通知事务处理完成多线样式的加入
            db.TransactionManager.AddNewlyCreatedDBObject(mStyle, true);
            dict.DowngradeOpen();//为安全起见，将多线样式字典切换为读
            return mStyle.ObjectId;//返回新创建的多线样式Id
        }
        /// <summary>
        /// 删除多线样式
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="styleName">要删除的多线样式</param>
        public static void RemoveMlineStyle(this Database db,string styleName)
        {
            //打开当前数据库的多线样式字典对象
            DBDictionary dict = db.MLStyleDictionaryId.GetObject(OpenMode.ForRead) as DBDictionary;
            //在多线样式字典中搜索关键字为styleName的多线样式
            if(dict.Contains(styleName))
            {
                dict.UpgradeOpen();
                dict.Remove(styleName);
                dict.DowngradeOpen();
            }
        }
    }
}
