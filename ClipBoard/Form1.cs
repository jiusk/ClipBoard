using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
namespace ClipBoard
{
    public partial class Form1 : Form
    {
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern IntPtr SetClipboardViewer(IntPtr hwnd);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern IntPtr ChangeClipboardChain(IntPtr hwnd, IntPtr hWndNext);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
        const int WM_DRAWCLIPBOARD = 0x308;
        const int WM_CHANGECBCHAIN = 0x30D;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //获得观察链中下一个窗口句柄
            NextClipHwnd = SetClipboardViewer(this.Handle);
        }
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            Console.WriteLine(m.Msg);
            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:
                    //将WM_DRAWCLIPBOARD消息传递到下一个观察链中的窗口
                    SendMessage(NextClipHwnd, m.Msg, m.WParam, m.LParam);
                    IDataObject iData = Clipboard.GetDataObject();
                    //检测文本
                    if (iData.GetDataPresent(DataFormats.Text) | iData.GetDataPresent(DataFormats.OemText))
                    {
                        this.richTextBox1.Text = (String)iData.GetData(DataFormats.Text);
                    }
                    //检测图像
                    if (iData.GetDataPresent(DataFormats.Bitmap))
                    {
                        pictureBox1.Image = Clipboard.GetImage();
                        MyItem item = new MyItem();
                        item.CopyToClipboard();
                    }
                    //检测自定义类型
                    if (iData.GetDataPresent(typeof(MyItem).FullName))
                    {
                        // MyItem item = (MyItem)iData.GetData(typeof(MyItem).FullName);
                        MyItem item = GetFromClipboard();
                        if (item != null)
                        {
                            this.richTextBox1.Text = item.ItemName;
                        }
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
        private void Form1_Closed(object sender, System.EventArgs e)
        {
            //从观察链中删除本观察窗口(第一个参数：将要删除的窗口的句柄；
            //第二个参数：观察链中下一个窗口的句柄 )
            ChangeClipboardChain(this.Handle, NextClipHwnd);
            //将变动消息WM_CHANGECBCHAIN消息传递到下一个观察链中的窗口
            SendMessage(NextClipHwnd, WM_CHANGECBCHAIN, this.Handle, NextClipHwnd);
        }
        private void button1_Click(object sender, System.EventArgs e)
        {

        }
        IntPtr NextClipHwnd;
        protected static MyItem GetFromClipboard()
        {
            MyItem item = null;
            IDataObject dataObj = Clipboard.GetDataObject();
            string format = typeof(MyItem).FullName;
            if (dataObj.GetDataPresent(format))
            {
                item = dataObj.GetData(format) as MyItem;
            }
            return item;
        }
    }
    [Serializable]
    public class MyItem
    {
        public MyItem()
        {
            itemName = "This is a Custom Item";
        }
        public string ItemName
        {
            get { return itemName; }
        }
        private string itemName;

        public void CopyToClipboard()
        {
            DataFormats.Format format = DataFormats.GetFormat(typeof(MyItem).FullName);
            IDataObject dataObj = new DataObject();
            dataObj.SetData(format.Name, false, this);
            Clipboard.SetDataObject(dataObj, false);
        }
    }
    public enum MessageMsg
    {

    }
}

