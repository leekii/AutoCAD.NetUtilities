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
    /// 字典工具
    /// </summary>
    public static class DictTools
    {
        /// <summary>
        /// 为对象添加扩展字典
        /// </summary>
        /// <param name="id">对象的Id</param>
        /// <param name="searchKey">扩展记录关键字</param>
        /// <param name="values">扩展纪录数据</param>
        /// <returns>新加扩展记录的Id</returns>
        public static ObjectId AddXrecord(this ObjectId id, string searchKey, TypedValueList values)
        {
            DBObject obj = id.GetObject(OpenMode.ForRead);//打开对象
            if (obj.ExtensionDictionary.IsNull)//如果对象无扩展字典
            {
                obj.UpgradeOpen();//切换对象为写状态
                obj.CreateExtensionDictionary();//为对象创建扩展字典
                obj.DowngradeOpen();//为安全起见，将对象修改为读状态
            }
            //打开对象的扩展字典
            DBDictionary dict = obj.ExtensionDictionary.GetObject(OpenMode.ForRead) as DBDictionary;
            //如果扩展字典中已包括指定的扩展记录对象，则返回
            if (dict.Contains(searchKey)) return ObjectId.Null;
            Xrecord xrec = new Xrecord();//为对象新建一个扩展记录
            xrec.Data = values;//指定扩展记录的内容
            dict.UpgradeOpen();//将扩展字典切换为写的状态
                               //在扩展字典中加入新建的扩展记录，并指定搜索关键字
            ObjectId idXrec = dict.SetAt(searchKey, xrec);
            id.Database.TransactionManager.AddNewlyCreatedDBObject(xrec, true);
            dict.DowngradeOpen();//安全起见，将扩展字典设为读模式
            return idXrec;//返回添加扩展记录的Id
        }
        /// <summary>
        /// 为对象修改扩展字典，若没有扩展字典或没有对应键，则添加
        /// </summary>
        /// <param name="id">对象的Id</param>
        /// <param name="searchKey">扩展记录关键字</param>
        /// <param name="values">扩展纪录数据</param>
        /// <returns>新加扩展记录的Id</returns>
        public static ObjectId SetXrecord(this ObjectId id, string searchKey, TypedValueList values)
        {
            DBObject obj = id.GetObject(OpenMode.ForRead);//打开对象
            if (obj.ExtensionDictionary.IsNull)//如果对象无扩展字典
            {
                obj.UpgradeOpen();//切换对象为写状态
                obj.CreateExtensionDictionary();//为对象创建扩展字典
                obj.DowngradeOpen();//为安全起见，将对象修改为读状态
            }
            //打开对象的扩展字典
            DBDictionary dict = obj.ExtensionDictionary.GetObject(OpenMode.ForRead) as DBDictionary;
            ObjectId idXrec = new ObjectId();
            //如果扩展字典中已包括指定的扩展记录对象，则修改
            if (dict.Contains(searchKey))
            {
                idXrec = dict.GetAt(searchKey);//获取该键值下的Xrecord的ObjectId
                Xrecord xrec = idXrec.GetObject(OpenMode.ForWrite) as Xrecord;//获取Xrecord对象
                xrec.Data = values;//修改Xrecord数据
            }
            else
            {
                idXrec = id.AddXrecord(searchKey, values);
            }            
            return idXrec;//返回添加扩展记录的Id
        }
        /// <summary>
        /// 获取对象扩展记录数据
        /// </summary>
        /// <param name="id">对象的Id</param>
        /// <param name="searchKey">关键字</param>
        /// <returns>对应关键字下的扩展记录数据</returns>
        public static TypedValueList GetXrecord(this ObjectId id, string searchKey)
        {
            DBObject obj = id.GetObject(OpenMode.ForRead);//打开对象
            ObjectId dictId = obj.ExtensionDictionary;//获取对象字典
            if (dictId.IsNull) return null;//若对象没有扩展字典，则返回
            DBDictionary dict = dictId.GetObject(OpenMode.ForRead) as DBDictionary;
            if(!dict.Contains(searchKey)) return null;//若字典中没有对应键值，则返回
            ObjectId xrecordId = dict.GetAt(searchKey);//获取扩展记录的ObjectId
            //打开扩展记录并获取扩展记录的内容
            Xrecord xrecord = xrecordId.GetObject(OpenMode.ForRead) as Xrecord;
            return xrecord.Data;//返回扩展记录的内容
        }

        /// <summary>
        /// 为图形添加有名字典
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="searchKey">关键字</param>
        /// <returns>新加有名对象字典项的Id</returns>
        public static ObjectId AddNamedDictionary(this Database db, string searchKey)
        {
            ObjectId id = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //打开数据库的有名对象字典
                DBDictionary dicts = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForRead) as DBDictionary;
                if (!dicts.Contains(searchKey))//如果不存在指点关键字的字典项
                {
                    DBDictionary dict = new DBDictionary();//新建字典项
                    dicts.UpgradeOpen();//切换有名字典项为写状态
                    id = dicts.SetAt(searchKey, dict);//设置新建字典项的关键字
                    dicts.DowngradeOpen();//为了安全起见，将有名字典切换为读状态
                    //将新建字典项添加到事务处理中
                    db.TransactionManager.AddNewlyCreatedDBObject(dict, true);
                }
                trans.Commit();//执行事务处理
            }            
            return id;
        }
        /// <summary>
        /// 根据DBDictionary对象的ObjectId添加Xrecord
        /// </summary>
        /// <param name="dictId">DBDictionary对象的ObjectId</param>
        /// <param name="searchKey">键值</param>
        /// <param name="values">data值，表示为自定义的TypedValueList类</param>
        /// <returns>新添加的Xrecord的ObjectId</returns>
        public static ObjectId AddXrecord2DBDict(this ObjectId dictId, string searchKey, TypedValueList values)
        {
            if (dictId.ObjectClass == RXObject.GetClass(typeof(DBDictionary)))
            {
                Database db = dictId.Database;
                DBDictionary dict = dictId.GetObject(OpenMode.ForRead) as DBDictionary;
                //如果字典中已包括指定的扩展记录对象，则返回
                if (dict.Contains(searchKey)) return ObjectId.Null;
                Xrecord xrec = new Xrecord();//为对象新建一个扩展记录
                xrec.Data = values;//指定扩展记录的内容
                dict.UpgradeOpen();//将扩展字典切换为写的状态
                //在扩展字典中加入新建的扩展记录，并指定搜索关键字
                ObjectId idXrec = dict.SetAt(searchKey, xrec);
                db.TransactionManager.AddNewlyCreatedDBObject(xrec, true);
                dict.DowngradeOpen();//安全起见，将扩展字典设为读模式
                return idXrec;//返回添加扩展记录的Id
            }
            else
            {
                return ObjectId.Null;
            }
        }
        /// <summary>
        /// 根据DBDictionary对象的ObjectId更新Xrecord
        /// </summary>
        /// <param name="dictId">DBDictionary对象的ObjectId</param>
        /// <param name="searchKey">键值</param>
        /// <param name="values">data值，表示为自定义的TypedValueList类</param>
        /// <returns>新添加的Xrecord的ObjectId</returns>
        public static ObjectId UpdateXrecord2DBDict(this ObjectId dictId, string searchKey, TypedValueList values)
        {
            if (dictId.ObjectClass == RXObject.GetClass(typeof(DBDictionary)))
            {
                Database db = dictId.Database;
                ObjectId idXrec = ObjectId.Null;
                DBDictionary dict = dictId.GetObject(OpenMode.ForRead) as DBDictionary;
                if (dict.Contains(searchKey))//已包含该键值
                {
                    dict.UpgradeOpen();//将有名对象字典切换为写状态
                    idXrec = dict.GetAt(searchKey);//获得searchKey对应xrecord的ObjectId
                    Xrecord xrec = idXrec.GetObject(OpenMode.ForWrite) as Xrecord;//获得Xrecord
                    xrec.Data = values;//设置新值
                    xrec.DowngradeOpen();//安全起见将xrec降为读模式
                    dict.DowngradeOpen();//安全起见将dict降为读模式
                }
                else//未包含该键值则添加
                {
                    Xrecord xrec = new Xrecord();//为对象新建一个扩展记录
                    xrec.Data = values;//指定扩展记录的内容
                    dict.UpgradeOpen();//将扩展字典切换为写的状态
                    //在扩展字典中加入新建的扩展记录，并指定搜索关键字
                    idXrec = dict.SetAt(searchKey, xrec);
                    db.TransactionManager.AddNewlyCreatedDBObject(xrec, true);
                    dict.DowngradeOpen();//安全起见，将扩展字典设为读模式
                }
                return idXrec;//返回添加扩展记录的Id
            }
            else
            {
                return ObjectId.Null;
            }
        }
    }
}
