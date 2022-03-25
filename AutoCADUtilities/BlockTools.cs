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
    public enum DynBlockPropTypeCode
    {
        String =1,
        Real =40,
        Short =70,
        Long = 90
    }
    public static class BlockTools
    {
        /// <summary>
        /// 创建新块
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="blockName">新加块的名称</param>
        /// <param name="ents">块中的实体</param>
        /// <returns>新加块的ObjectId</returns>
        public static ObjectId AddBlockTableRecord(
            this Database db,string blockName,List<Entity> ents)
        {
            //打开块表
            BlockTable bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
            if(!bt.Has(blockName))  //判断是否存在blockName的块
            {
                //创建一个BlockTableRecord对象，表示所要创建的块
                BlockTableRecord btr = new BlockTableRecord();
                btr.Name = blockName;  //设置块名
                //将列表中的实体加入到新建的BlockTableRecord中
                ents.ForEach(ent => btr.AppendEntity(ent));
                bt.UpgradeOpen();  //切换块表为写状态
                bt.Add(btr);  //在块表中加入blockName块
                db.TransactionManager.AddNewlyCreatedDBObject(btr, true);
                bt.DowngradeOpen();  //为了安全，将块表状态修改为读
            }
            return bt[blockName];
        }
        /// <summary>
        /// 创建新块
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="blockName">新加块的名称</param>
        /// <param name="ents">块中的实体</param>
        /// <returns>新加块的ObjectId</returns>
        public static ObjectId AddBlockTableRecord(
            this Database db, string blockName, params Entity[] ents)
        {
            return AddBlockTableRecord(db, blockName, ents.ToList());
        }
        /// <summary>
        /// 插入块参照
        /// </summary>
        /// <param name="spaceId">空间（模型还是布局）的ObjectId</param>
        /// <param name="layer">插入图层</param>
        /// <param name="blockName">块名</param>
        /// <param name="position">插入位置</param>
        /// <param name="scale">缩放比例</param>
        /// <param name="rotateAngle">旋转角度</param>
        /// <returns>插入块参照的ObjectId</returns>
        public static ObjectId InsertBlockReference(
            this ObjectId spaceId, string layer, string blockName, Point3d position, Scale3d scale, double rotateAngle)
        {
            ObjectId blockRefId;//存储块参照的Id
            Database db = spaceId.Database;//获取数据库对象
            //以读方式打开块表
            BlockTable bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
            //如果没有blockName块，则返回
            if (!bt.Has(blockName)) return ObjectId.Null;
            //以写的方式打开空间（模型空间或者图纸空间）
            BlockTableRecord space = spaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
            //创建一个快参照，并设置插入点
            BlockReference br = new BlockReference(position, bt[blockName]);
            br.ScaleFactors = scale;//设置块参照的缩放比例
            br.Layer = layer;//设置图层
            br.Rotation = rotateAngle;//设置旋转角度
            blockRefId = space.AppendEntity(br);//在空间中加入新建的块参照
            //通知事务处理加入创建的块参照
            db.TransactionManager.AddNewlyCreatedDBObject(br, true);
            space.DowngradeOpen();//为了安全，将块表状态改为读
            return blockRefId;
        }
        /// <summary>
        /// 插入带属性的块参照
        /// </summary>
        /// <param name="spaceId">空间（模型还是布局）的ObjectId</param>
        /// <param name="layer">图层</param>
        /// <param name="blockName">块名</param>
        /// <param name="position">插入位置</param>
        /// <param name="scale">缩放比例</param>
        /// <param name="rotateAngle">旋转角度</param>
        /// <param name="attNameValues">属性的名称和值</param>
        /// <returns>插入块参照的ObjectId</returns>
        public static ObjectId InsertBlockReference(
            this ObjectId spaceId,string layer,string blockName,Point3d position,
            Scale3d scale,double rotateAngle, Dictionary<string,string> attNameValues)
        {
            Database db = spaceId.Database;//获取数据库对象
            //以读方式打开块表
            BlockTable bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
            //如果没有blockName表示的块，则程序返回
            if (!bt.Has(blockName)) return ObjectId.Null;
            //以写的方式打开空间（模型空间或图纸空间）
            BlockTableRecord space = spaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
            ObjectId btrId = bt[blockName];//获取块表记录的Id
            //打开块表记录
            BlockTableRecord record = btrId.GetObject(OpenMode.ForRead) as BlockTableRecord;
            //创建一个快参照并设置插入点
            BlockReference br = new BlockReference(position, btrId);//新建块参照，设置插入点和块定义
            br.ScaleFactors = scale;//设置缩放比例
            br.Layer = layer;//设置图层
            br.Rotation = rotateAngle;//设置旋转角度
            space.AppendEntity(br);//在空间中添加块参照
            if(record.HasAttributeDefinitions)//判断块表记录是否含有属性定义
            {
                foreach(ObjectId id in record)//存在属性定义的话，遍历快定义中的实体，并对其中的属性进行处理
                {
                    //检查是否为属性
                    AttributeDefinition attDef = id.GetObject(OpenMode.ForRead) as AttributeDefinition;
                    if(attDef != null)
                    {
                        //创建一个新的属性对象
                        AttributeReference attribute = new AttributeReference();
                        //从属性定义获得属性对象的对象特性
                        attribute.SetAttributeFromBlock(attDef, br.BlockTransform);
                        //设置属性对象的其他特性
                        attribute.Position = attDef.Position.TransformBy(br.BlockTransform);
                        attribute.Rotation = attDef.Rotation;
                        attribute.AdjustAlignment(db);
                        //判断是否包含指定的属性名称
                        if(attNameValues.ContainsKey(attDef.Tag.ToUpper()))
                        {
                            attribute.TextString = attNameValues[attDef.Tag.ToUpper()];//设置属性值
                        }
                        //向块参照添加属性对象
                        br.AttributeCollection.AppendAttribute(attribute);
                        db.TransactionManager.AddNewlyCreatedDBObject(attribute, true);
                    }
                }

            }
            db.TransactionManager.AddNewlyCreatedDBObject(br, true);
            return br.ObjectId;
        }
        /// <summary>
        /// 为块定义添加属性
        /// </summary>
        /// <param name="blockId">块定义的ObjectId</param>
        /// <param name="atts">待添加的属性</param>
        public static void AddAttsToBlock(this ObjectId blockId,List<AttributeDefinition> atts)
        {
            Database db = blockId.Database;
            //以写状态打开块表记录
            BlockTableRecord btr = blockId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
            foreach(AttributeDefinition att in atts)
            {
                btr.AppendEntity(att);//为块表记录添加属性
                db.TransactionManager.AddNewlyCreatedDBObject(att, true);
            }
            btr.DowngradeOpen();
        }
        /// <summary>
        /// 为块定义添加属性
        /// </summary>
        /// <param name="blockId">块定义的ObjectId</param>
        /// <param name="atts">待添加的属性</param>
        public static void AddAttsToBlock(this ObjectId blockId,params AttributeDefinition[] atts)
        {
            blockId.AddAttsToBlock(atts.ToList());
        }
        /// <summary>
        /// 更新块参照的属性
        /// </summary>
        /// <param name="blockRefId">块参照的ObjectId</param>
        /// <param name="attNameValues">待更新的属性</param>
        public static void UpdateAttributesInBlock(
            this ObjectId blockRefId,Dictionary<string,string> attNameValues)
        {
            //获取块参照对象
            BlockReference blockRef = blockRefId.GetObject(OpenMode.ForRead) as BlockReference;
            if(blockRef!=null)
            {
                //遍历块参照中的属性
                foreach(ObjectId id in blockRef.AttributeCollection)
                {
                    //获取属性
                    AttributeReference attRef = id.GetObject(OpenMode.ForRead) as AttributeReference;
                    //判断是否包含指定的属性名称
                    if(attNameValues.ContainsKey(attRef.Tag.ToUpper()))
                    {
                        attRef.UpgradeOpen();
                        attRef.TextString = attNameValues[attRef.Tag.ToUpper()].ToString();
                        attRef.DowngradeOpen();
                    }
                }
            }
        }
        /// <summary>
        /// 获取动态属性
        /// </summary>
        /// <param name="blockId">块参照的ObjectId</param>
        /// <param name="propName">要获取的动态属性名</param>
        /// <returns>动态属性值</returns>
        public static string GetDynBlockValue(this ObjectId blockId, string propName)
        {
            string propValue = null; //用于返回动态属性值的变量
            var props = blockId.GetDynProperties();//获取动态块的所有动态属性
            //遍历动态属性
            foreach(DynamicBlockReferenceProperty prop in props)
            {
                //如果动态属性名称与输入相同
                if(prop.PropertyName == propName)
                {
                    //获取动态属性值并结束遍历
                    propValue = prop.Value.ToString();
                    break;
                }
            }
            return propValue;
        }
        /// <summary>
        /// 获取块参照的动态属性集
        /// </summary>
        /// <param name="blockId">块参照的ObjectId</param>
        /// <returns>块参照的动态属性集</returns>
        public static DynamicBlockReferencePropertyCollection GetDynProperties(this ObjectId blockId)
        {
            //获取块参照
            BlockReference br = blockId.GetObject(OpenMode.ForRead) as BlockReference;
            if (br == null && !br.IsDynamicBlock) return null;//不是动态块则返回
            return br.DynamicBlockReferencePropertyCollection;//返回动态块的动态属性集合
        }
        /// <summary>
        /// 设置动态属性
        /// </summary>
        /// <param name="blockId">块参照的ObjectId</param>
        /// <param name="propName">待修改的动态属性名</param>
        /// <param name="value">待修改的动态属性值</param>
        public static void SetDynBlockValue(this ObjectId blockId,string propName,object value)
        {
            var props = blockId.GetDynProperties();//获取动态块的所有动态属性
            //遍历动态属性
            foreach(DynamicBlockReferenceProperty prop in props)
            {
                //判断动态属性的名称是否存在，且该属性是否为可读
                if(prop.ReadOnly  == false && prop.PropertyName == propName)
                {
                    //判断动态属性的类型，并通过类型转化设置正确的动态属性值
                    switch (prop.PropertyTypeCode)
                    {
                        case (short)DynBlockPropTypeCode.Short://短整型
                            prop.Value = Convert.ToInt16(value);
                            break;
                        case (short)DynBlockPropTypeCode.Long://长整型
                            prop.Value = Convert.ToInt64(value);
                            break;
                        case (short)DynBlockPropTypeCode.Real://实数型
                            prop.Value = Convert.ToDouble(value);
                            break;
                        default://其他
                            prop.Value = value;
                            break;
                    }
                }
                break;
            }
        }
        /// <summary>
        /// 获取块参照的名称
        /// </summary>
        /// <param name="bRef">块参照</param>
        /// <returns>块名</returns>
        public static string GetBlockName(this BlockReference bRef)
        {
            string blockName;//存储块名
            if (bRef == null) return null;
            if (bRef.IsDynamicBlock)//如果是动态块
            {
                //获取动态块所属的动态块表记录
                ObjectId idDyn = bRef.DynamicBlockTableRecord;
                //打开动态块表记录
                BlockTableRecord btr = idDyn.GetObject(OpenMode.ForRead) as BlockTableRecord;
                blockName = btr.Name;
            }
            else//非动态块
                blockName = bRef.Name;
            return blockName;
        }
        /// <summary>
        /// 从Dwg文件导入块定义
        /// </summary>
        /// <param name="destDb">导入的目标数据库</param>
        /// <param name="sourceFileName">源文件</param>
        public static void ImportBlocksFromDwg(this Database destDb, string sourceFileName)
        {
            //创建一个新的数据库对象，作为源数据库，以读入外部文件中的对象
            Database sourceDb = new Database(false, true);
            try
            {
                //把Dwg文件读入到一个临时的数据库中
                sourceDb.ReadDwgFile(sourceFileName, System.IO.FileShare.Read, true, null);
                //创建一个变量用于储存ObjectId列表
                ObjectIdCollection blockIds = new ObjectIdCollection();
                //获取缘数据库的事务处理管理器
                Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = sourceDb.TransactionManager;
                //在源数据库中开始事务处理
                using(Transaction myT = tm.StartTransaction())
                {
                    //打开源数据库中的块表
                    BlockTable bt = tm.GetObject(sourceDb.BlockTableId, OpenMode.ForRead, false) as BlockTable;
                    //遍历每个块
                    foreach(ObjectId btrId in bt)
                    {
                        
                        BlockTableRecord btr = tm.GetObject(btrId, OpenMode.ForRead, false) as BlockTableRecord;
                        //只加入命名块和非布局块到复制列表中
                        if (!btr.IsAnonymous && !btr.IsLayout)
                        {
                            blockIds.Add(btrId);
                        }
                        btr.Dispose();
                    }
                    bt.Dispose();
                }
                //定义一个IdMapping对象
                IdMapping mapping = new IdMapping();
                //从源数据库向目标数据库复制块表记录
                sourceDb.WblockCloneObjects(blockIds, destDb.BlockTableId, mapping,
                    DuplicateRecordCloning.Replace, false);
            }
            catch(Autodesk.AutoCAD.Runtime.Exception ex) //出错处理
            {
                Application.ShowAlertDialog("复制错误：" + ex.Message);
            }
            //操作完成后销毁源数据库
            sourceDb.Dispose();
        }
        /// <summary>
        /// 获取块参照的属性名和属性值
        /// </summary>
        /// <param name="blockReferenceId">块参照的Id</param>
        /// <returns>返回块参照的属性名和属性值</returns>
        public static SortedDictionary<string, string> GetAttributesInBlockReference(this ObjectId blockReferenceId)
        {
            SortedDictionary<string, string> attributes = new SortedDictionary<string, string>();
            Database db = blockReferenceId.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 获取块参照
                BlockReference bref = (BlockReference)trans.GetObject(blockReferenceId, OpenMode.ForRead);
                // 遍历块参照的属性，并将其属性名和属性值添加到字典中
                foreach (ObjectId attId in bref.AttributeCollection)
                {
                    AttributeReference attRef = (AttributeReference)trans.GetObject(attId, OpenMode.ForRead);
                    attributes.Add(attRef.Tag, attRef.TextString);
                }
                trans.Commit();
            }
            return attributes; // 返回块参照的属性名和属性值
        }

        /// <summary>
        /// 获取指定名称的块属性值
        /// </summary>
        /// <param name="blockReferenceId">块参照的Id</param>
        /// <param name="attributeName">属性名</param>
        /// <returns>返回指定名称的块属性值</returns>
        public static string GetAttributeInBlockReference(this ObjectId blockReferenceId, string attributeName)
        {
            string attributeValue = string.Empty; // 属性值
            Database db = blockReferenceId.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 获取块参照
                BlockReference bref = (BlockReference)trans.GetObject(blockReferenceId, OpenMode.ForRead);
                // 遍历块参照的属性
                foreach (ObjectId attId in bref.AttributeCollection)
                {
                    // 获取块参照属性对象
                    AttributeReference attRef = (AttributeReference)trans.GetObject(attId, OpenMode.ForRead);
                    //判断属性名是否为指定的属性名
                    if (attRef.Tag.ToUpper() == attributeName.ToUpper())
                    {
                        attributeValue = attRef.TextString;//获取属性值
                        break;
                    }
                }
                trans.Commit();
            }
            return attributeValue; //返回块属性值
        }
    }
}
