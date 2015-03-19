using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace arbitrage
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form2());
            //Form1 flogin = new Form1();
            //flogin.ShowDialog();
            //if (flogin.DialogResult == DialogResult.OK)
            //{
            //    Application.Run(new Form2());

            //}
            //else
            //{
            //    return;
            //}
        }
    }
}
