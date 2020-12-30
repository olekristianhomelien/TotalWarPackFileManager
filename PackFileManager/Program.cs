using Common;
using System;
using System.IO;
using System.Windows.Forms;

namespace PackFileManager
{

    public class Program
    {
        public static PackFileManagerForm MainForm = null;

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logging.Create<Program>().Here().Fatal(e.ExceptionObject.ToString());
            MessageBox.Show(e.ExceptionObject.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        [STAThread]
        public static void Main(string[] args)
        {
            DirectoryHelper.EnsureCreated();
            Logging.Configure(Serilog.Events.LogEventLevel.Information);
            var logger = Logging.Create<Program>();

            System.Windows.Application app = new System.Windows.Application();

            logger.Here().Information("Application starting");
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.CurrentDomain_UnhandledException);
            Application.EnableVisualStyles();
            try
            {
                MainForm = new PackFileManagerForm(args);
                Application.Run(MainForm);
            }
            catch (Exception exception)
            {
                logger.Here().Fatal(exception.ToString());
                MessageBox.Show(exception.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

            logger.Information("Application terminated sucsessfully");
        }
    }
}

