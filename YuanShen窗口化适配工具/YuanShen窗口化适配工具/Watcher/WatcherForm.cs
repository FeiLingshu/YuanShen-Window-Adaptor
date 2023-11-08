using System.Windows.Forms;

namespace YuanShen_Window_Adapter.Watcher
{
    /// <summary>
    /// 后台窗口消息循环模拟基础类
    /// </summary>
    internal partial class WatcherForm : Form
    {
        /// <summary>
        /// WatcherForm类默认交互逻辑
        /// </summary>
        internal WatcherForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 表示窗口在创建后用不获取焦点的扩展风格
        /// </summary>
        private const int WS_EX_NOACTIVATE = 0x08000000;
        /// <summary>
        /// 在窗口创建时设置相关扩展风格
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                return cp;
            }
        }
    }
}
