using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace YuanShen_Window_Adapter
{
    internal partial class Cover : Form
    {
        internal Cover(IntPtr parent, string[] args)
        {
            InitializeComponent();
            string reg = (string)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Cursors", "Hand", "");
            if (string.IsNullOrEmpty(reg)) throw new Win32Exception("读取注册表键值(鼠标指针文件路径)失败。");
            IntPtr regptr = LoadCursorFromFile(reg);
            if (regptr == IntPtr.Zero) throw new Win32Exception("加载鼠标指针文件失败。");
            this.Cursor = new Cursor(regptr);
            this.parent = parent;
            this.args = args;
            this.Shown += Cover_Shown;
            this.toolTip.Draw += ToolTip_Draw;
            this.MouseUp += Cover_MouseUp;
        }

        private void ToolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            ToolTip toolTip = (ToolTip)sender;
            PropertyInfo info = toolTip.GetType().GetProperty("Handle", BindingFlags.NonPublic | BindingFlags.Instance);
            IntPtr handle = (IntPtr)info.GetValue(toolTip, null);
            Point location = this.PointToScreen(new Point(240 - e.Bounds.Width, - e.Bounds.Height - 10));
            MoveWindow(handle, location.X, location.Y, e.Bounds.Width, e.Bounds.Height, false);
            Color forecolor = Color.FromArgb(0xFF, Color.FromArgb(0xE9E9E9));
            Color backcolor = Color.FromArgb(0xFF, Color.FromArgb(0x202020));
            e.Graphics.FillRectangle(new SolidBrush(backcolor), e.Bounds);
            StringFormat format = StringFormat.GenericTypographic;
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            e.Graphics.DrawString(
                e.ToolTipText,
                this.Font,
                new SolidBrush(forecolor),
                e.Bounds.Width / 2 - 1,
                e.Bounds.Height / 2 + 15 + 1,
                format);
            Rectangle title = e.Bounds;
            title.Offset(0, 10 + 1);
            title.Height = 20;
            e.Graphics.FillRectangle(new HatchBrush(HatchStyle.ForwardDiagonal, backcolor, Color.DarkCyan), title);
            title.Offset(-2, -2);
            title.Size += new Size(4, 4);
            e.Graphics.DrawRectangle(new Pen(Color.Gray, 2), title);
            GraphicsPath path = new GraphicsPath();
            path.AddString("——  重  要  信  息  说  明  ——", this.Font.FontFamily, (int)this.Font.Style, 13.5F, title, format);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawPath(new Pen(forecolor, 2), path);
            e.Graphics.FillPath(new SolidBrush(backcolor), path);
            path.Dispose();
            e.DrawBorder();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000/*WS_EX_NOACTIVATE*/;
                return cp;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct NMHDR
        {
            [FieldOffset(0)]
            internal IntPtr hwndFrom;
            [FieldOffset(8)]
            internal uint idFrom;
            [FieldOffset(16)]
            internal uint code;
            internal NMHDR(IntPtr hwndFrom, uint idFrom, uint code)
            {
                this.hwndFrom = hwndFrom;
                this.idFrom = idFrom;
                this.code = code;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            internal int Left;
            internal int Top;
            internal int Right;
            internal int Bottom;
            internal RECT(int Left, int Top, int Right, int Bottom)
            {
                this.Left = Left;
                this.Top = Top;
                this.Right = Right;
                this.Bottom = Bottom;
            }
            internal Rectangle ToRectangle()
            {
                return new Rectangle(Left, Top, Right - Left, Bottom - Top);
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct NMTTCUSTOMDRAW
        {
            [FieldOffset(0)]
            internal NMHDR hdr;
            [FieldOffset(24)]
            internal uint dwDrawStage;
            [FieldOffset(32)]
            internal IntPtr hdc;
            [FieldOffset(40)]
            internal RECT rc;
            internal NMTTCUSTOMDRAW(NMHDR hdr, uint dwDrawStage, IntPtr hdc, RECT rc)
            {
                this.hdr = hdr;
                this.dwDrawStage = dwDrawStage;
                this.hdc = hdc;
                this.rc = rc;
            }
        }

        private Size ToolTipSize = Size.Empty;

        private bool Rightalign = false;

        protected override void WndProc(ref Message m)
        {
            const int WM_NOTIFY = 0x004E;
            uint NM_CUSTOMDRAW;
            uint TTN_SHOW;
            unchecked
            {
                const uint NM_FIRST = 0U - 0U;
                NM_CUSTOMDRAW = NM_FIRST - 12;
                const uint TTN_FIRST = 0U - 520U;
                TTN_SHOW = TTN_FIRST - 1;
            }
            if (m.Msg == WM_NOTIFY && m.WParam == this.Handle)
            {
                NMHDR nmhdr = (NMHDR)m.GetLParam(typeof(NMHDR));
                //Debug.Print(m.LParam.ToString("X8"));
                //Debug.Print($"{nmhdr.hwndFrom.ToString("X8")} / {nmhdr.idFrom:X8} / {nmhdr.code:X8}");
                //string codex = string.Empty;
                //for (int i = 0; i < Marshal.SizeOf(nmhdr); i++)
                //{
                //    codex += $"{Marshal.ReadByte(m.LParam, i):X2} ";
                //}
                //Debug.Print(codex);
                if (nmhdr.code == NM_CUSTOMDRAW)
                {
                    NMTTCUSTOMDRAW nmttcustomdraw = (NMTTCUSTOMDRAW)m.GetLParam(typeof(NMTTCUSTOMDRAW));
                    ToolTipSize = nmttcustomdraw.rc.ToRectangle().Size;
                    Screen screen = Screen.FromPoint(MousePosition);
                    Rightalign = screen.WorkingArea.X + screen.WorkingArea.Width - MousePosition.X < ToolTipSize.Width + 11;
                }
                if (nmhdr.code == TTN_SHOW)
                {
                    Point location = this.PointToScreen(new Point(0, 0 - ToolTipSize.Height - this.Height));
                    if (Rightalign)
                    {
                        location.X -= ToolTipSize.Width - this.Width + 9;
                        location.Y += 12;
                    }
                    else
                    {
                        location.X -= 16;
                        location.Y += 12;
                    }
                    SetWindowPos(nmhdr.hwndFrom, IntPtr.Zero, location.X, location.Y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
                    m.Result = new IntPtr(1);
                    ToolTipSize = Size.Empty;
                    Rightalign = false;

                }
                return;
            }
            base.WndProc(ref m);
        }

        private readonly IntPtr parent = IntPtr.Zero;

        private readonly string[] args = null;

        private bool isworking = false;

        private void Cover_Shown(object sender, EventArgs e)
        {
            this.Location = new Point(926, 626);
            this.Size = new Size(202, 64);
            SetParent(this.Handle, parent);
            this.Opacity = 1D;
        }

        private void Cover_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (isworking && FindGame() != null) return;
                    isworking = true;
                    if (FindGame() == null) SendClick(parent, this.PointToClient(MousePosition));
                    Thread working = new Thread(() =>
                    {
                        AutoResetEvent timer = new AutoResetEvent(false);
                        Process game = null;
                        do
                        {
                            timer.WaitOne(1);
                            game = FindGame();
                        } while (game == null);
                        do
                        {
                            timer.WaitOne(1);
                        } while (game.MainWindowHandle == IntPtr.Zero);
                        timer.WaitOne(500);
                        Program.MainFunction(game, args);
                        isworking = false;
                    })
                    { IsBackground = true };
                    working.Start();
                    break;
                case MouseButtons.None:
                    goto default;
                case MouseButtons.Right:
                    SendClick(parent, this.PointToClient(MousePosition));
                    break;
                case MouseButtons.Middle:
                    goto default;
                case MouseButtons.XButton1:
                    goto default;
                case MouseButtons.XButton2:
                    goto default;
                default:
                    break;
            }
        }

        private Process FindGame()
        {
            string local = Environment.CurrentDirectory;
            Process[] ppool = Process.GetProcessesByName("YuanShen");
            Process ptargate = null;
            if (ppool == null) return ptargate;
            foreach (Process p in ppool)
            {
                try
                {
                    if (p.MainModule.FileName == $"{local}\\Genshin Impact Game\\YuanShen.exe")
                    {
                        ptargate = p;
                        break;
                    }
                    continue;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return ptargate;
        }

        private void SendClick(IntPtr hwnd, Point point)
        {
            int lparm = ((626 + point.Y) << 16) + 926 + point.X;
            PostMessage(hwnd, WM_LBUTTONDOWN, 1, lparm);
            PostMessage(hwnd, WM_LBUTTONUP, 0, lparm);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursorFromFile(string lpFileName);

        private const uint WM_LBUTTONDOWN = 0x0201;

        private const uint WM_LBUTTONUP = 0x0202;

        [DllImport("user32.dll")]
        private static extern int PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("User32.dll")]
        private static extern bool MoveWindow(IntPtr h, int x, int y, int width, int height, bool redraw);

        private const uint SWP_NOSIZE = 0x0001;

        private const uint SWP_NOZORDER = 0x0004;

        [DllImport("User32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    }
}
