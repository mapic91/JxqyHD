using System;
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
            GameEditor form = new GameEditor();
            form.Show();
            form.TheGame = new Game(
                form.DrawSurface.Handle,
                form,
                form.DrawSurface);
            form.TheGame.Run();
        }
    }
#endif
}

