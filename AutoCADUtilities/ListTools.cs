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
    public class TypedValueList: List<TypedValue>
    {
        /// <summary>
        /// 接受可变参数的构造函数
        /// </summary>
        /// <param name="args">TypedValue对象</param>
        public TypedValueList(params TypedValue[] args)
        {
            AddRange(args);
        }

        /// <summary>
        /// 添加DXF组码及对应的类型
        /// </summary>
        /// <param name="typecode">DXF组码</param>
        /// <param name="value">类型</param>
        public void Add(int typecode, object value)
        {
            base.Add(new TypedValue(typecode, value));
        }

        /// <summary>
        /// 添加DXF组码
        /// </summary>
        /// <param name="typecode">DXF组码</param>
        public void Add(int typecode)
        {
            base.Add(new TypedValue(typecode));
        }

        /// <summary>
        /// 添加DXF组码及对应的类型
        /// </summary>
        /// <param name="typecode">DXF组码</param>
        /// <param name="value">类型</param>
        public void Add(DxfCode typecode, object value)
        {
            base.Add(new TypedValue((int)typecode, value));
        }

        /// <summary>
        /// 添加DXF组码
        /// </summary>
        /// <param name="typecode">DXF组码</param>
        public void Add(DxfCode typecode)
        {
            base.Add(new TypedValue((int)typecode));
        }

        /// <summary>
        /// 添加图元类型,DXF组码缺省为0
        /// </summary>
        /// <param name="entityType">图元类型</param>
        public void Add(Type entityType)
        {
            base.Add(new TypedValue(0, RXClass.GetClass(entityType).DxfName));
        }
        /// <summary>
        /// TypedValueList隐式转换为SelectionFilter
        /// </summary>
        /// <param name="src">要转换的TypedValueList对象</param>
        /// <returns>返回对应的SelectionFilter类对象</returns>
        public static implicit operator SelectionFilter(TypedValueList src)
        {
            return src != null ? new SelectionFilter(src) : null;
        }

        /// <summary>
        /// TypedValueList隐式转换为ResultBuffer
        /// </summary>
        /// <param name="src">要转换的TypedValueList对象</param>
        /// <returns>返回对应的ResultBuffer对象</returns>
        public static implicit operator ResultBuffer(TypedValueList src)
        {
            return src != null ? new ResultBuffer(src) : null;
        }

        /// <summary>
        /// TypedValueList隐式转换为TypedValue数组
        /// </summary>
        /// <param name="src">要转换的TypedValueList对象</param>
        /// <returns>返回对应的TypedValue数组</returns>
        public static implicit operator TypedValue[] (TypedValueList src)
        {
            return src != null ? src.ToArray() : null;
        }

        /// <summary>
        /// TypedValue数组隐式转换为TypedValueList
        /// </summary>
        /// <param name="src">要转换的TypedValue数组</param>
        /// <returns>返回对应的TypedValueList</returns>
        public static implicit operator TypedValueList(TypedValue[] src)
        {
            return src != null ? new TypedValueList(src) : null;
        }

        /// <summary>
        /// SelectionFilter隐式转换为TypedValueList
        /// </summary>
        /// <param name="src">要转换的SelectionFilter</param>
        /// <returns>返回对应的TypedValueList</returns>
        public static implicit operator TypedValueList(SelectionFilter src)
        {
            return src != null ? new TypedValueList(src.GetFilter()) : null;
        }

        /// <summary>
        /// ResultBuffer隐式转换为TypedValueList
        /// </summary>
        /// <param name="src">要转换的ResultBuffer</param>
        /// <returns>返回对应的TypedValueList</returns>
        public static implicit operator TypedValueList(ResultBuffer src)
        {
            return src != null ? new TypedValueList(src.AsArray()) : null;
        }
    }
}
