using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Engine;

namespace GameEditor
{
#if WINDOWS || XBOX
    static class Program
    {
        public static bool Restart = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new GameEditor();

            Settings.LoadSettings();
            var x = Settings.GetInt("X", 0);
            var y = Settings.GetInt("Y", 0);
            var w = Settings.GetInt("Width", 0);
            var h = Settings.GetInt("Height", 0);
            var maximized = Settings.IsMaximized();

            if (maximized)
            {
                form.WindowState = FormWindowState.Maximized;
            }
            else
            {
                if (x > 0 && y > 0)
                {
                    form.Location = new Point(x, y);
                }
                if (w > 0 && h > 0)
                {
                    form.Size = new Size(w, h);
                }
            }

            form.Show();
            form.TheJxqyGame = new JxqyGame(
                form.DrawSurface.Handle,
                form,
                form.DrawSurface);

            //Register log event
            Globals.TheMessageSender.OnFunctionCall += form.FunctionRunStateAppendLine;
            Globals.TheMessageSender.OnScriptFileChange += form.SetScriptFileContent;
            Globals.TheMessageSender.OnScriptVariables += form.OnScriptVariables;
            Globals.TheMessageSender.OnLog += form.OnLog;

            form.TheJxqyGame.Run();

            if (Restart)
            {
                Process.Start("GameEditor.exe");
            }
        }
    }
#endif
}

