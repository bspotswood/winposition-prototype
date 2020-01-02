using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Win32Interop.Methods;
using Win32Interop.Structs;
using Win32Interop.Enums;

namespace WinPosition
{

    enum DWMWINDOWATTRIBUTE : uint
    {
        NCRenderingEnabled = 1,
        NCRenderingPolicy,
        TransitionsForceDisabled,
        AllowNCPaint,
        CaptionButtonBounds,
        NonClientRtlLayout,
        ForceIconicRepresentation,
        Flip3DPolicy,
        ExtendedFrameBounds,
        HasIconicBitmap,
        DisallowPeek,
        ExcludedFromPeek,
        Cloak,
        Cloaked,
        FreezeRepresentation
    }

    public partial class ConfigForm : Form
    {
        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out RECT pvAttribute, int cbAttribute);


        HotKeyRegister leftKey;
        HotKeyRegister centerKey;
        HotKeyRegister rightKey;
        HotKeyRegister logKey;

        public ConfigForm()
        {
            InitializeComponent();

            leftKey = new HotKeyRegister(this.Handle, 0, KeyModifiers.Windows, Keys.NumPad1);
            centerKey = new HotKeyRegister(this.Handle, 1, KeyModifiers.Windows, Keys.NumPad2);
            rightKey = new HotKeyRegister(this.Handle, 2, KeyModifiers.Windows, Keys.NumPad3);
            logKey = new HotKeyRegister(this.Handle, 3, KeyModifiers.Windows, Keys.NumPad5);

            leftKey.HotKeyPressed += LeftKey_HotKeyPressed;
            centerKey.HotKeyPressed += CenterKey_HotKeyPressed;
            rightKey.HotKeyPressed += RightKey_HotKeyPressed;
            logKey.HotKeyPressed += LogKey_HotKeyPressed;
        }

        private void LogKey_HotKeyPressed(object sender, EventArgs e)
        {
            IntPtr hWnd = User32.GetForegroundWindow();

            RECT winRect    = new RECT();
            RECT clientRect = new RECT();

            User32.GetWindowRect(hWnd, out winRect);
            User32.GetClientRect(hWnd, out clientRect);

            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            User32.GetWindowPlacement(hWnd, ref placement);

            RECT dwmRect = new RECT();
            DwmGetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out dwmRect, System.Runtime.InteropServices.Marshal.SizeOf(dwmRect));

            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine(String.Format("DWM Rect: left={0}, right={1}, top={2}, bottom={3}", dwmRect.left, dwmRect.right, dwmRect.top, dwmRect.bottom));

            Console.WriteLine(String.Format("Win Rect: left={0}, right={1}, top={2}, bottom={3}", winRect.left, winRect.right, winRect.top, winRect.bottom));
            Console.WriteLine(String.Format("Clt Rect: left={0}, right={1}, top={2}, bottom={3}", clientRect.left, clientRect.right, clientRect.top, clientRect.bottom));

            Console.WriteLine(String.Format("Placement: left={0}, right={1}, top={2}, bottom={3}", placement.rcNormalPosition.left, placement.rcNormalPosition.right, placement.rcNormalPosition.top, placement.rcNormalPosition.bottom));
            Console.WriteLine(String.Format("Placement flags: {0}", placement.flags));
        }

        private void snapForegroundWindow(int left, int right, int top, int bottom)
        {
            IntPtr hWnd = User32.GetForegroundWindow();
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            
            RECT winRect = new RECT();
            RECT clientRect = new RECT();

            User32.GetWindowRect(hWnd, out winRect);
            User32.GetClientRect(hWnd, out clientRect);
            User32.GetWindowPlacement(hWnd, ref placement);
            
            RECT dwmRect = new RECT();
            DwmGetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out dwmRect, System.Runtime.InteropServices.Marshal.SizeOf(dwmRect));

            RECT padding = new RECT();
            int hzDiff = ((winRect.right - winRect.left) - (clientRect.right - clientRect.left));
            int vtDiff = ((winRect.bottom - winRect.top) - (clientRect.bottom - clientRect.top));
            padding.left   = -7; // - (hzDiff / 2);
            padding.right  =  0; //   (hzDiff / 2) + (hzDiff % 2);
            padding.top    =  0; // - (vtDiff / 2);
            padding.bottom =  7; //   (vtDiff / 2) + (vtDiff % 2);

            padding.left = - (dwmRect.left - winRect.left);
            padding.right = winRect.right - dwmRect.right;
            padding.top = dwmRect.top - winRect.top;
            padding.bottom = winRect.bottom - dwmRect.bottom;

            placement.rcNormalPosition.left   = left   + padding.left;
            placement.rcNormalPosition.top    = top    + padding.top;
            placement.rcNormalPosition.right  = right  + padding.right;
            placement.rcNormalPosition.bottom = bottom + padding.bottom;

            placement.showCmd = 1; // Normal
            
            User32.ShowWindow(hWnd, 1);
            //User32.SetWindowPlacement(hWnd, ref placement);

            
            User32.SetWindowPos(hWnd, IntPtr.Zero, placement.rcNormalPosition.left, placement.rcNormalPosition.top, placement.rcNormalPosition.right - placement.rcNormalPosition.left, placement.rcNormalPosition.bottom - placement.rcNormalPosition.top, 0);
            
        }

        private void LeftKey_HotKeyPressed(object sender, EventArgs e)
        {
            snapForegroundWindow(0, 960, 0, 1160);
        }

        private void CenterKey_HotKeyPressed(object sender, EventArgs e)
        {
            snapForegroundWindow(960, 960 + 1920, 0, 1160);
        }

        private void RightKey_HotKeyPressed(object sender, EventArgs e)
        {
            snapForegroundWindow(960 + 1920, 960 + 1920 + 960, 0, 1160);
        }

        private void ConfigForm_Resize(object sender, EventArgs e)
        {
            if(this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
            }
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }
        
        private void ConfigForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
