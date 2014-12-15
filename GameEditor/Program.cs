using System;
using System.Windows.Forms;
using Engine;
using Jxqy;

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
            var form = new GameEditor();
            form.Show();
            form.TheGame = new Game(
                form.DrawSurface.Handle,
                form,
                form.DrawSurface);

            //Register log event
            Globals.TheMessageSender.OnFunctionCall += form.FunctionRunStateAppendLine;
            Globals.TheMessageSender.OnScriptFileChange += form.SetScriptFileContent;

            form.TheGame.Run();
        }
    }
#endif
}

