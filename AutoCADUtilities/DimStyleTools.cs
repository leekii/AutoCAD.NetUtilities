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
    public static class DimStyleTools
    {
        /// <summary>
        /// 创建新标注样式
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="styleName">新标注样式名</param>
        /// <returns>新标注样式的ObjectId</returns>
        public static ObjectId AddDimStyle(this Database db, string styleName)
        {
            //打开标注样式表
            DimStyleTable dst = db.DimStyleTableId.GetObject(OpenMode.ForRead) as DimStyleTable;
            if(!dst.Has(styleName))//如果不存在名为styleName的标注样式，则创建新的样式
            {
                //定义一个新的标注样式表记录
                DimStyleTableRecord dstr = new DimStyleTableRecord();
                dstr.Name = styleName; //设置样式名
                dst.UpgradeOpen(); //切换为写模式以添加新创建的标注样式
                dst.Add(dstr);
                //将标注样式记录添加到事务处理中
                db.TransactionManager.AddNewlyCreatedDBObject(dstr,true);
                dst.DowngradeOpen();//为了数据安全，降为读模式
            }
            return dst[styleName];//返回新添加标注样式的ObjectId
        }
    }
}
