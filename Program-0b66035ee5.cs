using System;
using System.Windows.Forms;
using YaotouPartyManager.UI;
using YaotouPartyManager.Data;

namespace YaotouPartyManager
{
    /// <summary>
    /// 程序入口类
    /// </summary>
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // 初始化数据库
                DatabaseInitializer.InitializeDatabase();

                // 启动主窗体
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"系统启动失败:\n\n{ex.Message}\n\n请联系系统管理员。",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
