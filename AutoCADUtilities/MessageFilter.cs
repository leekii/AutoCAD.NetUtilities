using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetARX
{
    public class MessageFilter : IMessageFilter
    {
        public const int WM_KEYDOWN = 0x0100;
        public Keys KeyName { get; set; }
        //在调度消息之前将其筛选出来
        public bool PreFilterMessage(ref Message m)
        {
            if(m.Msg == WM_KEYDOWN)//如果调度信息为按键
            {
                //设置键名
                KeyName = (Keys)(int)m.WParam & Keys.KeyCode;
                //返回true表示调度的是按键消息
                return true;
            }
            return false;//返回false表示非按键信息
        }
    }
}
