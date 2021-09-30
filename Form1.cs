using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyboardHookMonitor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InstallKeyboardHook();

            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReleaseKeyboardHook();
        }
        private static bool bHooked = false;

        public void InstallKeyboardHook()
        {
            KeyboardHook.OnKeyboardEvent = onKeyboardDownHandler;
            KeyboardHook.Hook();
            bHooked = true;
            Console.WriteLine("Keyboard Hooked");
        }

        public void ReleaseKeyboardHook()
        {
            if (bHooked) {
                bHooked = false;
                KeyboardHook.UnHook();
                Console.WriteLine("Keyboard Unhooked");
            }
        }

        DateTime LastDt = DateTime.MaxValue.AddDays(-1);

        private void onKeyboardDownHandler(KeyboardHook.WM_KEY_STATUS wmKeyStatus, int vkey, int scanCode, int flags, int time, int extraInfo)
        {
            richTextBox1.Focus();
            var dtNow = DateTime.Now;
            if (LastDt.AddMilliseconds(100) < dtNow) richTextBox1.AppendText("\r\n");
            LastDt = dtNow;
            richTextBox1.AppendText($"{DateTime.Now.ToString("HH:mm:ss.fff")}: {wmKeyStatus} KeyCode={vkey:x}, ScanCode={scanCode:x}, Flags={flags:x}, Time={time}, extraInfo={extraInfo}\r\n");
        }

    }
}
