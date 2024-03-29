﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using YuanShen_Window_Adapter.Watcher;
using static YuanShen_Window_Adapter.Func.MsgBeep;
using UnhandledExceptionEventArgs = System.UnhandledExceptionEventArgs;

namespace YuanShen_Window_Adapter
{
    /// <summary>
    /// 应用程序基础类
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// 全局guid常数
        /// </summary>
        private static readonly string GUID = "92C7038B-7A22-427D-974A-ACA274FEE347";
        /// <summary>
        /// 全局线程同步事件
        /// </summary>
        private static EventWaitHandle ProgramStarted;
        /// <summary>
        /// 应用程序的主入口点
        /// </summary>
        /// <param name="args">应用程序启动参数</param>
        private static void Main(string[] args)
        {
            // 绑定异常处理程序
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            // 尝试创建一个命名事件
            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, GUID, out bool createNew);
            // 如果该命名事件已经存在(存在有前一个运行实例)，则发事件通知并退出
            if (!createNew)
            {
                ProgramStarted.Set();
                return;
            }
            // 为命名事件添加回调委托
            ThreadPool.RegisterWaitForSingleObject(ProgramStarted, OnProgramStarted, null, -1, false);
            
            // 运行应用程序主功能方法
            string local = Environment.CurrentDirectory;
            bool checkout = File.Exists($"{local}\\launcher.exe")
                && Directory.Exists($"{local}\\Genshin Impact Game")
                && File.Exists($"{local}\\Genshin Impact Game\\YuanShen.exe");
            if (!checkout) throw new ArithmeticException("程序所在目录不正确。\n备注：请将程序置于游戏启动器(launcher.exe)相同目录下。");
            Process[] ppool = Process.GetProcessesByName("launcher");
            Process ptargate = null;
            if (ppool != null)
            {
                foreach (Process p in ppool)
                {
                    try
                    {
                        if (p.MainModule.FileName == $"{local}\\launcher.exe")
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
            }
            if (ptargate == null)
            {
                ptargate = Process.Start($"{local}\\launcher.exe");
            }
            AutoResetEvent timer = new AutoResetEvent(false);
            do
            {
                timer.WaitOne(1);
            } while (ptargate.MainWindowHandle == IntPtr.Zero);
            Cover coverform = new Cover(ptargate.MainWindowHandle, args);
            coverform.ShowDialog();
        }

        /// <summary>
        /// 应用程序主功能
        /// </summary>
        /// <param name="game">应用程序本体路径</param>
        /// <param name="args">应用程序启动参数</param>
        /// <exception cref="ArithmeticException">主功能计算出现严重错误</exception>
        /// <exception cref="Win32Exception">主功能进行Win32操作时出现错误</exception>
        internal static void MainFunction(Process game, string[] args)
        {
            #region 应用程序主功能

            #region 已弃用功能
            // 获取窗口句柄
            if (false)
            {
                #pragma warning disable CS0162 // 检测到无法访问的代码
                IntPtr hWnd = IntPtr.Zero;
                Process process = null;
                IntPtr startHandle = IntPtr.Zero;
                do
                {
                    IntPtr fwe = FindWindowEx(IntPtr.Zero, startHandle, "UnityWndClass", "原神");
                    if (fwe == IntPtr.Zero)
                    {
                        break;
                    }
                    GetWindowThreadProcessId(fwe, out int pid);
                    try
                    {
                        if (Process.GetProcessById(pid).ProcessName != "YuanShen")
                        {
                            startHandle = fwe;
                            continue;
                        };
                    }
                    catch
                    {
                        startHandle = fwe;
                        continue;
                    }
                    hWnd = fwe;
                    process = Process.GetProcessById(pid);
                    startHandle = IntPtr.Zero;
                    break;
                } while (hWnd == IntPtr.Zero);
                if (hWnd == IntPtr.Zero)
                {
                    do
                    {
                        IntPtr fwe = FindWindowEx(IntPtr.Zero, startHandle, "UnityWndClass", "原神 · 宽屏适配模块相关进程已挂载(F12=启用/禁用位置锁定)");
                        if (fwe == IntPtr.Zero)
                        {
                            break;
                        }
                        GetWindowThreadProcessId(fwe, out int pid);
                        try
                        {
                            if (Process.GetProcessById(pid).ProcessName != "YuanShen")
                            {
                                startHandle = fwe;
                                continue;
                            };
                        }
                        catch
                        {
                            startHandle = fwe;
                            continue;
                        }
                        hWnd = fwe;
                        process = Process.GetProcessById(pid);
                        startHandle = IntPtr.Zero;
                        break;
                    } while (hWnd == IntPtr.Zero);
                    if (hWnd != IntPtr.Zero)
                    {
                        SendMessage(hWnd, WM_SETTEXT, IntPtr.Zero, "原神");
                    }
                }
                if (hWnd == IntPtr.Zero) return;
                #pragma warning restore CS0162 // 检测到无法访问的代码
            }
            #endregion

            // 获取窗口win32信息

            bool report = true;
            report = GetWindowRect(game.MainWindowHandle, out RECT window_r);
            report = GetClientRect(game.MainWindowHandle, out RECT client_r);
            Point point_e = Point.Empty;
            report = ClientToScreen(game.MainWindowHandle, ref point_e);
            if (!report) return;
            Size size_e = new Size()
            {
                Width = window_r.ToRectangle().Width - client_r.ToRectangle().Width,
                Height = window_r.ToRectangle().Height - client_r.ToRectangle().Height
            };
            RECT rect_e = new RECT()
            {
                left = point_e.X - window_r.ToRectangle().Location.X,
                top = point_e.Y - window_r.ToRectangle().Location.Y,
                right = size_e.Width,
                bottom = size_e.Height
            };
            rect_e.right -= rect_e.left;
            rect_e.bottom -= rect_e.top;

            // 判断命令行参数选择执行模式

            if (args.Length == 1)
            {
                Size targatesize = Size.Empty; // 对两种预置显示模式进行匹配
                if (args[0].ToUpper() == "(PC)") // 21:9
                {
                    targatesize = new Size(2520, 1080);
                }
                if (args[0].ToUpper() == "(PHONE)") // 20:9
                {
                    targatesize = new Size(2400, 1080);
                }
                if (targatesize == Size.Empty) return;
                Screen main_s = Screen.FromRectangle(window_r.ToRectangle()); // 进行屏幕检测
                Size screensize = main_s.Bounds.Size;
                int screenw = screensize.Width; // 对各种纵横比的屏幕区域进行相关检测
                int screenh = screensize.Height;
                int? pick = null;
                if ((double)screensize.Width / screensize.Height > 16D / 9D)
                {
                    pick = screenh;
                } // 21:9...
                if ((double)screensize.Width / screensize.Height < 16D / 9D)
                {
                    pick = screenw;
                } // 4:3...
                Size windowsize = Size.Empty; // 适配并计算实际的显示区域大小
                if (pick == null)
                {
                    double zoomw = screenw / 2560D;
                    double zoomh = screenh / 1440D;
                    if (zoomw == zoomh)
                    {
                        windowsize = new Size()
                        {
                            Width = (int)Math.Round(targatesize.Width * zoomw, MidpointRounding.AwayFromZero),
                            Height = (int)Math.Round(targatesize.Height * zoomh, MidpointRounding.AwayFromZero)
                        };
                    }
                    else throw new ArithmeticException("算数运算出现严重的未知错误。");
                }
                if (pick.GetValueOrDefault() == screenw)
                {
                    windowsize = new Size()
                    {
                        Width = (int)Math.Round(targatesize.Width * (screenw / 2560D), MidpointRounding.AwayFromZero),
                        Height = 0
                    };
                    windowsize.Height = (int)Math.Round((double)windowsize.Width / targatesize.Width * targatesize.Height, MidpointRounding.AwayFromZero);
                }
                if (pick.GetValueOrDefault() == screenh)
                {
                    windowsize = new Size()
                    {
                        Width = 0,
                        Height = (int)Math.Round(targatesize.Height * (screenh / 1440D), MidpointRounding.AwayFromZero)
                    };
                    windowsize.Width = (int)Math.Round((double)windowsize.Height / targatesize.Height * targatesize.Width, MidpointRounding.AwayFromZero);
                }
                Point lction_p = main_s.Bounds.Location; // 计算并应用新的窗口坐标
                lction_p.Offset(new Point()
                {
                    X = (screensize.Width - windowsize.Width) / 2,
                    Y = screensize.Height - windowsize.Height - (int)((screensize.Height - windowsize.Height) * ((double)targatesize.Height / (targatesize.Width + targatesize.Height)))
                });
                lction_p.Offset(-rect_e.left, -rect_e.top);
                RECT rect = new RECT()
                {
                    left = lction_p.X,
                    top = lction_p.Y,
                    right = lction_p.X + windowsize.Width + size_e.Width,
                    bottom = lction_p.Y + windowsize.Height + size_e.Height
                };
                Rectangle rectangle = rect.ToRectangle();
                SizeMove();
                ChangeTitle();
                bool movestate = true; // 额外附加操作，冻结窗口位置
                ChangeMenu(false);
                WatcherForm watcher = new WatcherForm() // 挂接挂钩过程
                {
                    Opacity = 0
                };
                bool runblocker = true;
                Func<int, int, IntPtr, int> Win32CallBack = // 在首层方法体中内联声明Win32委托，以便执行内存固定
                (int nCode, int wParam, IntPtr lParam) =>
                {
                    KeyBoardHookStruct keyBoardHookStruct = new KeyBoardHookStruct();
                    try
                    {
                        keyBoardHookStruct = (KeyBoardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyBoardHookStruct));
                    }
                    finally
                    {
                        if (keyBoardHookStruct == null)
                        {
                            ExceptionList.Add($"[{DateTime.Now:yyyy/MM/dd HH:mm:ss}] - LocalLowHook捕获发生异常。");
                        }
                    }
                    if (wParam == WM_KEYUP)
                    {
                        if (runblocker)
                        {
                            if (keyBoardHookStruct.vkCode == VK_F12 && GetForegroundWindow() == game.MainWindowHandle)
                            {
                                ChangeMenu(!movestate);
                            }
                            if (keyBoardHookStruct.vkCode == VK_F11 && GetForegroundWindow() == game.MainWindowHandle)
                            {
                                SizeMove();
                                ChangeTitle();
                            }
                        }
                    }
                    return CallNextHookEx((int)HOOKhwnd, nCode, wParam, lParam);
                };
                HOOKPROC Win32HookProc = new HOOKPROC(Win32CallBack);
                watcher.Shown += (object es, EventArgs ee) =>
                {
                    HOOKhwnd = SetWindowsHookEx(WH_KEYBOARD_LL, Win32HookProc, IntPtr.Zero, 0);
                    if (HOOKhwnd == IntPtr.Zero)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error(),
                            $"挂接全局HOOK挂钩过程中出现Win32异常({Marshal.GetLastWin32Error().ToString().PadLeft(4, '0')})。");
                    }
                    GotoChecking();
                    GotoWaiting();
                };
                watcher.FormClosing += (object es, FormClosingEventArgs ee) => // 卸载挂钩过程
                {
                    runblocker = false;
                    GC.KeepAlive(Win32HookProc);
                    GC.KeepAlive(Win32CallBack);
                    if (!UnhookWindowsHookEx((int)HOOKhwnd))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error(),
                            $"挂接全局HOOK挂钩过程中出现Win32异常({Marshal.GetLastWin32Error().ToString().PadLeft(4, '0')})。");
                    };
                };
                void ChangeMenu(bool restore) // 修改窗口菜单选项
                {
                    if (!restore)
                    {
                        IntPtr menuhwnd = GetSystemMenu(game.MainWindowHandle, false);
                        if (menuhwnd != IntPtr.Zero)
                        {
                            RemoveMenu(menuhwnd, SC_MOVE, MF_BYCOMMAND);
                            movestate = false;
                            SizeMove(); // 恢复冻结后重新定位窗口坐标
                        };
                    }
                    else
                    {
                        IntPtr menuhwnd = GetSystemMenu(game.MainWindowHandle, true);
                        movestate = true;
                    }
                }
                bool SizeMove() // 调整窗口属性
                {
                    if (!GetWindowRect(game.MainWindowHandle, out RECT checkout))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error(),
                            $"调整窗口属性过程中出现Win32异常({Marshal.GetLastWin32Error().ToString().PadLeft(4, '0')})。");
                    };
                    if (checkout.ToRectangle().Location != rectangle.Location || checkout.ToRectangle().Size != rectangle.Size)
                    {
                        SendMessage(game.MainWindowHandle, 0x0231/*WM_ENTERSIZEMOVE*/, IntPtr.Zero, IntPtr.Zero);

                        bool final_in = SetWindowPos(game.MainWindowHandle, IntPtr.Zero,
                        rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height,
                        SWP_FRAMECHANGED | SWP_NOCOPYBITS | SWP_NOZORDER);
                        if (!final_in)
                        {
                            throw new Win32Exception(Marshal.GetLastWin32Error(),
                                $"调整窗口属性过程中出现Win32异常({Marshal.GetLastWin32Error().ToString().PadLeft(4, '0')})。");
                        };
                        
                        IntPtr param = Marshal.AllocHGlobal(Marshal.SizeOf(rect));
                        Marshal.StructureToPtr(rect, param, false);
                        SendMessage(game.MainWindowHandle, 0x0216/*WM_MOVING*/, IntPtr.Zero, param);
                        Marshal.FreeHGlobal(param);

                        SendMessage(game.MainWindowHandle, 0x0232/*WM_EXITSIZEMOVE*/, IntPtr.Zero, IntPtr.Zero);

                        SendMessage(game.MainWindowHandle, 0x0112, (IntPtr)0xF020, (IntPtr)0);
                        SendMessage(game.MainWindowHandle, 0x0112, (IntPtr)0xF120, (IntPtr)(rectangle.Y << 16 | rectangle.X));
                        return true;
                    } // IMP:以上方法体操作中，为避免由于游戏内分辨率数据未同步造成的画面拉伸，通过SendMessage函数进行窗口移动模拟是必须的操作
                    return false;
                }
                bool ChangeTitle() // 修改程序标题
                {
                    StringBuilder title = new StringBuilder(GetWindowTextLength(game.MainWindowHandle) + 1);
                    GetWindowText(game.MainWindowHandle, title, title.Capacity);
                    if (title.ToString() != "原神 · 宽屏适配模块相关进程已挂载(F11=刷新窗口设置)(F12=启用/禁用位置锁定)")
                    {
                        string titlestr = "原神 · 宽屏适配模块相关进程已挂载(F11=刷新窗口设置)(F12=启用/禁用位置锁定)";
                        SendMessage(game.MainWindowHandle, WM_SETTEXT, IntPtr.Zero, titlestr);
                        return true;
                    }
                    return false;
                }
                void GotoChecking() // 修改程序标题，向用户指示主要操作已经完成
                {
                    ChangeTitle();
                    Thread checking = new Thread((object e_time) =>
                    {
                        AutoResetEvent timer = new AutoResetEvent(false);
                        int counter = 0;
                        int counter_limit = 30000 - (int)e_time + 1000; // 通过挂接目标进程运行时间和标准时间计算需要进行的循环数
                                                                        // 添加1000次循环(消耗1s)，保证循环次数足量
                        StringBuilder checksb = new StringBuilder(1);
                        do
                        {

                            checksb.Capacity += GetWindowTextLength(game.MainWindowHandle);
                            GetWindowText(game.MainWindowHandle, checksb, checksb.Capacity);
                            if (checksb.ToString() != "原神 · 宽屏适配模块相关进程已挂载(F11=刷新窗口设置)(F12=启用/禁用位置锁定)")
                            {
                                timer.WaitOne(500); // 添加500ms标准延时，防止更新速度过快导致过度闪烁
                                counter += 500;
                                if (ChangeTitle()) break;
                            }
                            checksb.Clear();
                            checksb.Capacity = 1;
                            counter++;
                            if (counter == counter_limit) break; // 由于挂接目标进程在启动过程中会重置一次窗口属性，故循环30s进行标题判断
                                                                 // 循环体30s循环时间，是由于挂接目标进程自启动到可以进行交互时所消耗的时间大致为30s
                            timer.WaitOne(1);
                        } while (true);
                    })
                    {
                        IsBackground = true
                    };
                    int time = (int)new TimeSpan((DateTime.Now - game.StartTime).Ticks).TotalMilliseconds; // 检测挂接目标进程运行时间
                    if (time < 30000) checking.Start(time);
                }
                void GotoWaiting() // 等待程序退出
                {
                    Thread waiting = new Thread(() =>
                    {
                        game.WaitForExit();
                        watcher.Invoke(new Action(() =>
                        {
                            watcher.Close();
                        }));
                    })
                    {
                        IsBackground = true
                    };
                    waiting.Start();
                }
                watcher.ShowDialog();
                GC.KeepAlive(Win32CallBack); // 固定相关Win32委托内存，防止垃圾回收机制回收方法体导致抛出【未将对象引用设置到对象的实例】异常
                GC.KeepAlive(Win32HookProc);
                if (ExceptionList.Count != 0) // 检测是否存在未影响程序运行的异常并按需显示提示信息
                {
                    string message = "程序运行过程中发生了一个或多个异常，但未影响程序正常运行。";
                    foreach (string msg in ExceptionList)
                    {
                        message += $"    {Environment.NewLine}低影响度异常：{msg}";
                    }
                    Exception_throw(new AggregateException(message));
                }
            } // 执行窗口化宽屏模式

            else
            {
                return;
            } // 匹配无效的启动参数

            #endregion
        }

        #region win32互操作声明

        /// <summary>
        /// 查找指定特征的窗口
        /// </summary>
        /// <param name="parentHandle">指定查找窗口的父窗口</param>
        /// <param name="childAfter">指定查找的起始窗口</param>
        /// <param name="className">指定查找窗口的类名称</param>
        /// <param name="windowTitle">指定查找窗口的标题字符串</param>
        /// <returns>返回窗口查找结果</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        /// <summary>
        /// 获取拥有指定窗口的进程ID和线程ID
        /// </summary>
        /// <param name="hWnd">目标窗口句柄</param>
        /// <param name="lpdwProcessId">out - 拥有目标窗口的进程ID</param>
        /// <returns>返回拥有目标窗口的线程ID</returns>
        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        /// <summary>
        /// 获取窗口标题字符串长度
        /// </summary>
        /// <param name="hwnd">目标窗口句柄</param>
        /// <returns>返回窗口标题字符串的长度</returns>
        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hwnd);

        /// <summary>
        /// 获取窗口标题字符串
        /// </summary>
        /// <param name="hwnd">目标窗口句柄</param>
        /// <param name="lpString">存储字符串的对象</param>
        /// <param name="nMaxCount">获取字符的最大长度</param>
        /// <returns>返回获取到的字符串长度</returns>
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hwnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// 获取窗口矩形
        /// </summary>
        /// <param name="hWnd">目标窗口句柄</param>
        /// <param name="lpRect">out - 目标窗口的窗口矩形</param>
        /// <returns>返回操作是否成功</returns>
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// 获取客户矩形
        /// </summary>
        /// <param name="hWnd">目标窗口句柄</param>
        /// <param name="lpRect">out - 目标窗口的客户矩形</param>
        /// <returns>返回操作是否成功</returns>
        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// 将客户区坐标转换为屏幕坐标
        /// </summary>
        /// <param name="hWnd">目标窗口句柄</param>
        /// <param name="lpPoint">要转换的客户区坐标(标准值为0,0)</param>
        /// <returns>返回操作是否成功</returns>
        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        /// <summary>
        /// 将 WM_NCCALCSIZE 消息发送到窗口
        /// </summary>
        private const int SWP_FRAMECHANGED = 0x0020;
        /// <summary>
        /// 指示不改变窗口Z序
        /// </summary>
        private const int SWP_NOZORDER = 0x0004;
        /// <summary>
        /// 丢弃工作区的整个内容
        /// </summary>
        private const int SWP_NOCOPYBITS = 0x0100;
        /// <summary>
        /// 调整窗口空间信息
        /// </summary>
        /// <param name="hwnd">目标窗口坐标</param>
        /// <param name="hWndInsertAfter">指示窗口Z序如何变化</param>
        /// <param name="x">窗口左上角横坐标</param>
        /// <param name="y">窗口左上角纵坐标</param>
        /// <param name="cx">窗口宽度</param>
        /// <param name="cy">窗口高度</param>
        /// <param name="wFlags">窗口空间信息修改规则</param>
        /// <returns>返回操作是否成功</returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        /// <summary>
        /// 声明win32结构RECT
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            /// <summary>
            /// 距左边界距离
            /// </summary>
            internal int left;
            /// <summary>
            /// 距上边界距离
            /// </summary>
            internal int top;
            /// <summary>
            /// 距右边界距离
            /// </summary>
            internal int right;
            /// <summary>
            /// 距下边界距离
            /// </summary>
            internal int bottom;

            /// <summary>
            /// 实现将win32结构RECT转换为C#结构Rectangle
            /// </summary>
            /// <returns></returns>
            internal Rectangle ToRectangle()
            {
                return new Rectangle(left, top, right - left, bottom - top);
            }
        }

        /// <summary>
        /// 需要设置窗口的标题文本
        /// </summary>
        private const int WM_SETTEXT = 0x000C;
        /// <summary>
        /// 向指定窗口发送指定的Win32消息
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="Msg">消息常量</param>
        /// <param name="wParam">消息参数</param>
        /// <param name="lParam">消息参数</param>
        /// <returns>返回消息的处理结果</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        /// <summary>
        /// 向指定窗口发送指定的Win32消息(仅用于发送字符串消息)
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="Msg">消息常量</param>
        /// <param name="wParam">消息参数</param>
        /// <param name="lParam">消息参数(仅传递字符串对象)</param>
        /// <returns>返回消息的处理结果</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

        /// <summary>
        /// 获取窗口菜单句柄
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="bRevert">是否恢复到保存的窗口菜单副本</param>
        /// <returns>窗口菜单句柄</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        /// <summary>
        /// 表示窗口移动菜单项
        /// </summary>
        private const int SC_MOVE = 0xF010;
        /// <summary>
        /// 表示通过命令常量查找菜单项
        /// </summary>
        private const int MF_BYCOMMAND = 0;
        /// <summary>
        /// 移除窗口菜单的菜单项
        /// </summary>
        /// <param name="hMenu">窗口菜单句柄</param>
        /// <param name="nPos">窗口菜单项的索引/常量</param>
        /// <param name="flags">指示查找菜单项的方式</param>
        /// <returns>返回操作是否成功</returns>
        [DllImport("user32.dll")]
        private static extern bool RemoveMenu(IntPtr hMenu, int nPos, int flags);

        /// <summary>
        /// 获取当前正在前台显示的窗口句柄
        /// </summary>
        /// <returns>返回当前正在前台显示的窗口句柄</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// 控制窗口显示行为
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="nCmdShow">控制显示行为的标志</param>
        /// <returns>指示操作是否成功</returns>
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        #endregion

        #region 全局HOOK挂钩互操作代码块

        /// <summary>
        /// 全局HOOK挂钩句柄
        /// </summary>
        private static IntPtr HOOKhwnd = IntPtr.Zero;

        /// <summary>
        /// 声明键盘挂钩的封装结构类型
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private class KeyBoardHookStruct
        {
            /// <summary>
            /// 触发挂钩过程的键盘按键的键值
            /// </summary>
            internal int vkCode;
            /// <summary>
            /// 挂钩过程的扫描代码(unchecked)
            /// </summary>
            internal int scanCode;
            /// <summary>
            /// 挂钩过程的位标志(unchecked)
            /// </summary>
            internal int flags;
            /// <summary>
            /// 挂钩过程触发的时间(unchecked)
            /// </summary>
            internal int time;
            /// <summary>
            /// 挂钩过程包含的状态详细信息(unchecked)
            /// </summary>
            internal int dwExtraInfo;
        }

        /// <summary>
        /// 键盘按键抬起事件常量
        /// </summary>
        private const int WM_KEYUP = 0x0101;

        /// <summary>
        /// 表示键盘F12按键
        /// </summary>
        private const int VK_F12 = 0x7B;

        /// <summary>
        /// 表示键盘F11按键
        /// </summary>
        private const int VK_F11 = 0x7A;

        /// <summary>
        /// 用于执行挂钩过程的委托函数
        /// </summary>
        /// <param name="nCode">通知下个挂钩过程如何处理挂钩信息</param>
        /// <param name="wParam">主要挂钩数据</param>
        /// <param name="lParam">挂钩事件参数的相关标志信息的位组合数据</param>
        /// <returns>返回当前挂钩处理结果</returns>
        private delegate int HOOKPROC(int nCode, int wParam, IntPtr lParam);

        /// <summary>
        /// 表示用于监视低级别键盘输入事件的挂钩过程
        /// </summary>
        private const int WH_KEYBOARD_LL = 13;

        /// <summary>
        /// 安装全局HOOK挂钩
        /// <para>【对于低级别全局HOOK挂钩，显式设置dwThreadId参数将会引发Win32Error(1429)_只能全局设置该挂接过程】</para>
        /// </summary>
        /// <param name="idHook">要安装的挂钩过程的类型</param>
        /// <param name="lpfn">指向挂钩过程的指针</param>
        /// <param name="hmod">指向挂钩过程的dll的句柄，若挂接线程由当前进程创建，并且挂钩过程位于与当前进程关联的代码中，则必须为null</param>
        /// <param name="dwThreadId">要挂接的线程的线程标识符</param>
        /// <returns>返回挂接的挂钩句柄</returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HOOKPROC lpfn, IntPtr hmod, int dwThreadId);

        /// <summary>
        /// 传递挂钩数据并执行下一个挂钩函数
        /// </summary>
        /// <param name="idHook">此参数被忽略</param>
        /// <param name="nCode">确认如何处理挂钩信息</param>
        /// <param name="wParam">主要挂钩数据</param>
        /// <param name="lParam">挂钩事件参数的相关标志信息的位组合数据</param>
        /// <returns>返回值由链中的下一个挂钩过程返回</returns>
        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        /// <summary>
        /// 卸载全局HOOK挂钩
        /// </summary>
        /// <param name="idHook">要卸载的挂钩的句柄</param>
        /// <returns>返回操作是否成功执行</returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(int idHook);

        #endregion

        /// <summary>
        /// 当收到第二进程的通知时，响应消息
        /// </summary>
        /// <param name="state">消息参数</param>
        /// <param name="timeout">超时时间</param>
        private static void OnProgramStarted(object state, bool timeout)
        {
            return; // 由于进程没有UI界面，且只有在抛出异常时才会显示弹出式窗口，故不对重复启动进行提示
        }

        /// <summary>
        /// 捕获UI线程所有异常
        /// </summary>
        /// <param name="sender">抛出异常的?(object)</param>
        /// <param name="e">异常相关信息</param>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Exception_throw(e.Exception);
        }

        /// <summary>
        /// 捕获后台线程所有异常
        /// </summary>
        /// <param name="sender">抛出异常的?(object)</param>
        /// <param name="e">异常相关信息</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception_throw((Exception)e.ExceptionObject);
        }

        /// <summary>
        /// 用于保存未导致应用程序运行错误的异常信息
        /// </summary>
        internal static List<string> ExceptionList = new List<string> { };

        /// <summary>
        /// 封装异常处理程序
        /// </summary>
        /// <param name="e">异常相关信息</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static void Exception_throw(Exception e)
        {
            // 异常处理过程开始前首先尝试卸载全局HOOK挂钩
            if (HOOKhwnd != IntPtr.Zero) UnhookWindowsHookEx((int)HOOKhwnd);
            // 播放提示音
            Beep(UType.MB_ICONHAND);
            // 匹配已知未知异常
            MessageBox.Show($"[应用程序内部异常] [{DateTime.Now:yyyy/MM/dd HH:mm:ss}]"
                + $"\n\n根命名空间:{e.Source}"
                + $"\n方法体:{e.TargetSite}"
                + $"\n详细信息:{e.GetType().Name}\n{e.Message}"
                + $"{(Regex.IsMatch(e.Message, @"\n\z") ? string.Empty : "\n")}"
                + $"\n位置:\n{e.StackTrace}"
                + "\n\nYuanShen窗口化适配工具 - Exceptions Processed By FeiLingshu");
            // 强制退出程序进程
            Environment.Exit(0);
        }
    }
}
