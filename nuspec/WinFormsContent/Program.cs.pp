using System;
using System.Windows.Forms;
using MugenMvvmToolkit.WinForms.Infrastructure;

namespace $rootnamespace$
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var bootstrapper = new Bootstrapper<Core.App>(new IIocContainer());
            bootstrapper.Start();
        }
    }
}