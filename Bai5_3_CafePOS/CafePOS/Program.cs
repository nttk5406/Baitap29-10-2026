using System;
using System.Windows.Forms;
using CafePOS.GUI;

namespace CafePOS
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
