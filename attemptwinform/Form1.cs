using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ClipboardShare
{
    public partial class Form1 : Form
    {
        IntPtr nextClipboardViewer;
        NetworkService ns;
        public Form1()
        {
            InitializeComponent();
            ns = new NetworkService();
            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)
                         this.Handle);
        }

        [DllImport("User32.dll")]
        protected static extern int SetClipboardViewer(int hWndNewViewer);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool
               ChangeClipboardChain(IntPtr hWndRemove,
                                    IntPtr hWndNewNext);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg,
                                             IntPtr wParam,
                                             IntPtr lParam);
        protected override void
          WndProc(ref System.Windows.Forms.Message m)
        {
            // defined in winuser.h
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:
                    //DisplayClipboardData();
                    IDataObject iData = Clipboard.GetDataObject(); 
                    if (iData.GetDataPresent(DataFormats.Text))
                    {
                        string text = (string)iData.GetData(DataFormats.Text);      // Clipboard text
                        Message msg = new Message(Message.TEXT, text);
                        this.textBox1.Text = text;
                        ns.SendMessage(msg.ToString());
                        // do something with it
                    } else if(iData.GetDataPresent(DataFormats.FileDrop)){

                    }
                    
                    SendMessage(nextClipboardViewer, m.Msg, m.WParam,
                                m.LParam);
                    break;

                case WM_CHANGECBCHAIN:
                    if (m.WParam == nextClipboardViewer)
                        nextClipboardViewer = m.LParam;
                    else
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam,
                                    m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ns.SendMessage("test");
        }
    }

}
