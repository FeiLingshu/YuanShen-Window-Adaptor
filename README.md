# YuanShen窗口化适配工具

**通过对原神游戏窗口进行调整以适配宽屏和无边框窗口模式的适配工具。**

# 运行环境

**程序需要在安装有 .Net Framework 4.0 的 x64构架 系统上运行。**

**（只要你的电脑能运行原神这款游戏，一定能运行YuanShen窗口化适配工具）**

> **Releases中提供了 .Net Framework 4.0 的安装包和下载地址。**

# 使用说明

**程序只包含单个应用程序，请在观察到原神游戏主窗口完全显示后再启动本程序。**

**程序检查到当前状态无法/无需执行时，不会产生提示信息(只有在抛出异常时会进行提示)，如遇操作无效请自行分辨。**

**更多使用注意事项及使用方法详细信息，请[前往这里](https://www.bilibili.com/video/av000000000)进行查看。**

> **由于原神游戏主进程以管理员身份运行，本程序需要申请管理员(UAC)权限！**

# 来自作者的补充说明

**请尽量不要在GitHub上进行反馈，作者不会经常访问GitHub查看反馈信息，反馈请[前往这里](https://www.bilibili.com/video/av000000000)的评论区进行反馈。**

**本程序在 BSD-3-Clause license 下开源，程序不会对任何系统及用户进程进行侵入性代码植入。**

**为了防止其他个人或组织对程序代码进行更改导致危害您的计算机系统或数据，请在且仅在本项目仓库下载本程序，作者可以保证本程序不会导致任何数据的损坏或丢失！**

　
　

> # 关于程序自动化处理的相关信息

> **程序使用命令行参数识别修改模式，使用时请根据情况创建快捷方式并指定命令行参数。【使用命令行参数时，需进入快捷方式的属性窗口，调整快捷方式选项卡下的目标栏数据，在原有数据后添加空格并键入相应的命令行参数】**

> **  —— 参数1：无参数 【执行默认的无边框窗口化全屏模式，需要游戏分辨率和屏幕分辨率保持一致，适配显卡设置了超分辨率显示的情况】  
      —— 参数2：(PC) 【执行窗口化宽屏适配模式，适配分辨率以标准2K分辨率为基准，显示分辨率为2520×1080】  
      —— 参数3：(PHONE) 【执行窗口化宽屏适配模式，适配分辨率以标准2K分辨率为基准，保持与当前大多数手机屏幕分辨率相同，显示分辨率为2400×1080】**
    
> **【注意：命令行参数包含半角括号】  
    【所有分辨率说明均是在基准分辨率下的数值实现，在其他分辨率的设备下，实际分辨率同该分辨率数值呈正相关】**
    
> ### **_执行程序前，必须保证游戏处于窗口化模式，否则无法进行相应自动化处理！_**

> ### **_若要处理原神游戏窗口以外的窗口，请下载源代码进行修改，并自行编译！_**

　
　

# 

> ### **_本程序由作者本人（即 FeiLingshu）原创编写。_**
