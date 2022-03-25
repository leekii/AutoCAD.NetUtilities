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
    public static class DocumentTools
    {
        /// <summary>
        /// 确定文档中是否有未保存的修改
        /// </summary>
        /// <param name="doc">文档</param>
        /// <returns>是否有未保存的修改</returns>
        public static bool Saved(this Document doc)
        {
            //获取DBMOD系统变量，用来指示系统的修改状态
            object dbmod = Application.GetSystemVariable("DBMOD");
            //若DBMOD不为0，则表示图形已被修改但还未保存
            if (Convert.ToInt16(dbmod) != 0) return true;
            else return false;
        }
        /// <summary>
        /// 保存文档
        /// </summary>
        /// <param name="doc">文件</param>
        public static void Save(this Document doc)
        {
            doc.Database.SaveAs(doc.Name,DwgVersion.Current);
        }

    }
}
