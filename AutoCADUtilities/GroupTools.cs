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
    public static class GroupTools
    {
        /// <summary>
        /// 创建组
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="groupName">组名</param>
        /// <param name="ids">添加进组的实体的Id集合</param>
        /// <returns>返回组的Id</returns>
        public static ObjectId CreateGroup(this Database db, string groupName,ObjectIdCollection ids)
        {
            //打开当前数据库的组字典对象
            DBDictionary groupDict = db.GroupDictionaryId.GetObject(OpenMode.ForRead) as DBDictionary;
            //如果已经存在制定名称的组，则返回
            if (groupDict.Contains(groupName)) return ObjectId.Null;
            Group group = new Group(groupName, true);//新建一个组对象
            groupDict.UpgradeOpen();//切换组字典为写状态
            //在组字典中加入新创建的组对象，并制定它的关键字为groupName
            groupDict.SetAt(groupName, group);
            //通过事务处理完成组对象的加入
            db.TransactionManager.AddNewlyCreatedDBObject(group, true);
            group.Append(ids);//在租对象中加入实体对象
            groupDict.DowngradeOpen();//为了安全起见，将组字典切换为写状态
            return group.ObjectId;
        }
        public static ObjectId CreateGroup(this Database db, string groupName, params ObjectId[] ids)
        {
            ObjectIdCollection entIds = new ObjectIdCollection();
            foreach (ObjectId id in ids)
                entIds.Add(id);
            ObjectId groupId= CreateGroup(db, groupName, entIds);
            return groupId;
        }
        /// <summary>
        /// 组中添加实体
        /// </summary>
        /// <param name="groupId">组的Id</param>
        /// <param name="ids"></param>
        public static void AppendEntityToGroup(this ObjectId groupId,ObjectIdCollection ids)
        {
            Group group = groupId.GetObject(OpenMode.ForRead) as Group;
            if (group == null) return;
            group.UpgradeOpen();
            group.Append(ids);
            group.DowngradeOpen();
        }
        public static void AppendEntityToGroup(this ObjectId groupId, params ObjectId[] ids)
        {
            Group group = groupId.GetObject(OpenMode.ForRead) as Group;
            if (group == null) return;
            group.UpgradeOpen();
            ObjectIdCollection entIds = new ObjectIdCollection();
            foreach (ObjectId id in ids)
                entIds.Add(id);
            group.Append(entIds);
            group.DowngradeOpen();
        }
        /// <summary>
        /// 获取实体所在组的id
        /// </summary>
        /// <param name="entId">实体的id</param>
        /// <returns>所在组的id</returns>
        public static IEnumerable<ObjectId> GetGroups(this ObjectId entId)
        {
            DBObject obj = entId.GetObject(OpenMode.ForRead);//打开实体
            //获取实体对象所用有的永久反应器（组也属于永久反应器之一）
            ObjectIdCollection ids = obj.GetPersistentReactorIds();
            if (ids != null && ids.Count > 0)
            {
                //对实体的永久反应器筛选，只返回组
                var groupIds = from ObjectId id in ids
                                   //获取永久反应器
                               let reactor = id.GetObject(OpenMode.ForRead)
                               //筛选条件设置为Group类
                               where reactor is Group
                               select id;
                if (groupIds.Count() > 0)//如果实体属于组
                    return groupIds;
            }
            return null;
        }
    }
}
