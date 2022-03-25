using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Customization;
using System.IO;
using System.Collections.Specialized;

namespace DotNetARX
{
    /// <summary>
    /// 操作CUI的类
    /// </summary>
    public static class CUITools
    {
        /// <summary>
        /// 获取并打开主CUI文件（定义为Document类的扩展函数）
        /// </summary>
        /// <param name="doc">文档对象</param>
        /// <returns>主CUI</returns>
        public static CustomizationSection GetMainCustomizationSection(this Document doc)
        {
            //获取主CUI文件所在的位置
            string mainCuiFile = Application.GetSystemVariable("MENUNAME") + ".cuix";
            //打开主CUI文件
            return new CustomizationSection(mainCuiFile);
        }
        /// <summary>
        /// 创建局部Cui文件
        /// </summary>
        /// <param name="doc">文档对象</param>
        /// <param name="cuiFile">CUI文件名</param>
        /// <param name="menuGroupName">菜单组名称</param>
        /// <returns>局部Cui</returns>
        public static CustomizationSection AddCui(this Document doc, string cuiFile,string menuGroupName)
        {
            CustomizationSection cs;//声明CUI文件对象
            if (!File.Exists(cuiFile))//如果要创建的文件不存在
            {
                cs = new CustomizationSection();//创建CUI对象
                cs.MenuGroupName = menuGroupName;//指定菜单组名称
                cs.SaveAs(cuiFile);//保存CUI文件
            }
            //如果存在指定的Cui文件，则打开该文件
            else cs = new CustomizationSection(cuiFile);
            return cs;
        }
        /// <summary>
        /// 装载指定的局部CUI文件
        /// </summary>
        /// <param name="cs">局部Cui</param>
        public static void LoadCui(this CustomizationSection cs)
        {
            if (cs.IsModified) cs.Save();//如果CUI文件被修改，则保存
            //保存CMDECHO及FILEDIA系统变量
            //获取当前活动文档
            Document doc = Application.DocumentManager.MdiActiveDocument;
            //获取主CUI文件
            CustomizationSection mainCs = doc.GetMainCustomizationSection();
            //如果已存在CUI文件，则先卸载
            if (mainCs.PartialCuiFiles.Contains(cs.CUIFileName))
                Application.UnloadPartialMenu(cs.CUIFileBaseName);
            //装载cui文件，注意文件名必须是带路径的
            Application.LoadPartialMenu(cs.CUIFileName);
        }
        /// <summary>
        /// 在Cui菜单项中添加宏
        /// </summary>
        /// <param name="source">Cui对应的CustomizationSection</param>
        /// <param name="name">宏的显示名称</param>
        /// <param name="command">宏中的命令</param>
        /// <param name="tag">宏的标识符</param>
        /// <param name="helpString">宏的提示信息</param>
        /// <param name="imagePath">宏的图标</param>
        /// <returns>新加宏对象</returns>
        public static MenuMacro AddMacro(this CustomizationSection source, string name,
            string command, string tag, string helpString, string imagePath)
        {
            MenuGroup menuGroup = source.MenuGroup;//获取Cui中的菜单组
            //判断菜单组中是否已经定义与菜单组名相同的宏集合
            MacroGroup mg = menuGroup.FindMacroGroup(menuGroup.Name);
            if (mg == null) //如果宏集合没有定义，则创建一个与菜单组同名的宏集合
                mg = new MacroGroup(menuGroup.Name, menuGroup);
            //如果宏已经被定义，则返回
            foreach (MenuMacro macro in mg.MenuMacros)
                if (macro.ElementID == tag) return null;
            //在宏集合中创建一个命令宏
            MenuMacro menuMacro = new MenuMacro(mg, name, command, tag);
            //指定命令宏的说明信息，在状态栏中显示
            menuMacro.macro.HelpString = helpString;
            //指定命令宏大小图像的路径
            menuMacro.macro.LargeImage = imagePath;
            menuMacro.macro.SmallImage = imagePath;
            return menuMacro;
        }
        /// <summary>
        /// 添加下拉菜单
        /// </summary>
        /// <param name="menuGroup">包含菜单的菜单组</param>
        /// <param name="name">菜单名</param>
        /// <param name="aliasList">菜单的别名</param>
        /// <param name="tag">菜单的标识符</param>
        /// <returns>新添的下拉菜单</returns>
        public static PopMenu AddPopMenu(this MenuGroup menuGroup, string name,
            StringCollection aliasList, string tag)
        {
            PopMenu pm = null;//声明下拉菜单对象
            //如果菜单组中没有名称为name的下拉菜单
            if(menuGroup.PopMenus.IsNameFree(name))
            {
                //为下拉菜单指定显示名称、别名、标识符和所属的菜单组
                pm = new PopMenu(name, aliasList, tag, menuGroup);
            }
            return pm;//返回下拉菜单对象
        }
        /// <summary>
        /// 添加菜单项
        /// </summary>
        /// <param name="parentMenu">菜单项所属菜单</param>
        /// <param name="index">菜单项在菜单中的位置</param>
        /// <param name="name">菜单项的显示名</param>
        /// <param name="macroId">菜单项的命令宏Id</param>
        /// <returns>新添的菜单项</returns>
        public static PopMenuItem AddMenuItem(this PopMenu parentMenu, int index,
            string name, string macroId)
        {
            PopMenuItem newPmi = null;
            //如果存在名为name的菜单项，则返回
            foreach(PopMenuItem pmi in parentMenu.PopMenuItems)
            {
                if (pmi.Name == name && pmi.Name != null)
                    return newPmi;
            }
            //定义一个菜单项对象，指定所属菜单及位置
            newPmi = new PopMenuItem(parentMenu, index);
            //如果name不为空，则使用name作为显示名，否则使用命令宏的名称
            if (name != null)
                newPmi.Name = name;
            newPmi.MacroID = macroId;//菜单项命令宏的Id
            return newPmi;//返回菜单项对象
        }
        /// <summary>
        /// 添加子菜单
        /// </summary>
        /// <param name="parentMenu">子菜单上级菜单</param>
        /// <param name="index">子菜单在上级菜单中的位置</param>
        /// <param name="name">子菜单显示名</param>
        /// <param name="tag">子菜单标识符</param>
        /// <returns>新添的子菜单</returns>
        public static PopMenu AddSubMenu(this PopMenu parentMenu, int index, string name,string tag)
        {
            PopMenu pm = null;//声明子菜单对象（属于下拉菜单类）
            //如果菜单组中没有名为name的子菜单
            if(parentMenu.CustomizationSection.MenuGroup.PopMenus.IsNameFree(name))
            {
                //为子菜单指定显示名称，标识符和所属的菜单项，别名设为null
                pm = new PopMenu(name, null, tag, parentMenu.CustomizationSection.MenuGroup);
                //为子菜单选择其所属的菜单
                PopMenuRef menuRef = new PopMenuRef(pm, parentMenu, index);
            }
            return pm;//返回子菜单
        }
        /// <summary>
        /// 菜单中添加分割条
        /// </summary>
        /// <param name="parentMenu">要添加分隔条的菜单</param>
        /// <param name="index">添加分隔条的位置</param>
        /// <returns>新添的分隔条</returns>
        public static PopMenuItem AddSeparator(this PopMenu parentMenu, int index)
        {
            return new PopMenuItem(parentMenu, index);//定义一个分隔条并返回
        }
        /// <summary>
        /// 在菜单组中添加工具栏
        /// </summary>
        /// <param name="menuGroup">菜单组</param>
        /// <param name="name">工具栏显示名</param>
        /// <returns>新添工具栏</returns>
        public static Toolbar AddToolBar(this MenuGroup menuGroup,string name)
        {
            Toolbar tb = null;//声明一个工具栏对象
            //如果菜单组中没有名称为name的工具栏
            if(menuGroup.Toolbars.IsNameFree(name))
            {
                tb = new Toolbar(name, menuGroup);//为工具栏指定名称和所属菜单组
                tb.ToolbarOrient = ToolbarOrient.floating;//设置工具栏为浮动工具栏
                tb.ToolbarVisible = ToolbarVisible.show;//显示工具栏
            }
            return tb;
        }
        /// <summary>
        /// 在工具栏中添加按钮
        /// </summary>
        /// <param name="parent">工具栏</param>
        /// <param name="index">添加按钮的位置</param>
        /// <param name="name">按钮显示名</param>
        /// <param name="macroId">执行的宏Id</param>
        /// <returns>新添的按钮</returns>
        public static ToolbarButton AddToolBarButton(this Toolbar parent,int index,string name,string macroId)
        {
            //创建一个工具栏按钮对象，指定其命令宏id，显示名称，所属工具栏和位置
            ToolbarButton button = new ToolbarButton(macroId, name, parent, index);
            return button;//返回按钮对象
        }
        /// <summary>
        /// 为工具条添加弹出式工具条
        /// </summary>
        /// <param name="parent">父工具条</param>
        /// <param name="index">弹出式工具条在父工具条中的位置</param>
        /// <param name="toolbarRef">弹出工具条</param>
        public static void AttachToolbarToFlyout(this Toolbar parent,int index,Toolbar toolbarRef)
        {
            //创建一个弹出式工具栏，指定其所属的工具栏和位置
            ToolbarFlyout flyout = new ToolbarFlyout(parent, index);
            //指定弹出式工具栏所引用的工具栏
            flyout.ToolbarReference = toolbarRef.Name;
            toolbarRef.ToolbarVisible = ToolbarVisible.hide;//引用工具栏不可见，点击后才可见
        }
    }
}
