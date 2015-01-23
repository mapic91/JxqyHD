using System.Windows.Forms;
using Engine;

namespace GameEditor
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new GameEditor();
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
        }
    }
#endif
}

