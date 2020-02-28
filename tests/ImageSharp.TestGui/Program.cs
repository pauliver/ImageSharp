using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageSharp.TestGui
{
    static class Program
    {
        public static string Jekyll_data_Folder = "_data";
        public static string Jekyll_data_File = "galleryjson.json";
        public static string THUMBNAILS = "Thumbnails";
        public static string GENERATED = "\\Generated";
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
