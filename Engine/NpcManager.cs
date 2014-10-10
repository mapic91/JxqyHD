using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine
{
    public class NpcManager
    {
        private List<Npc> _list = new List<Npc>(); 

        public NpcManager() { }

        public NpcManager(string filePath)
        {
            Load(filePath);
        }

        public bool Load(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.GetEncoding(936));
                Load(lines);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool Load(string[] lines)
        {
            _list.Clear();

            var count = lines.Count();
            for (var i = 0; i < count;)
            {
                var groups = Regex.Match(lines[i++], @"\[NPC([0-9]+)\]").Groups;
                if (groups[0].Success)
                {
                    var contents = new List<string>();
                    while (i < count && !string.IsNullOrEmpty(lines[i]))
                    {
                        contents.Add(lines[i]);
                        i++;
                    }
                    AddNpc(contents.ToArray());
                    i++;
                }
            }
            return true;
        }

        public void AddNpc(string[] lines)
        {
            var npc = new Npc();
            npc.Load(lines);
            AddNpc(npc);
        }

        public void AddNpc(Npc npc)
        {
            _list.Add(npc);
        }

        public void ClearAllNpc()
        {
            _list.Clear();
        }

        public void Update(GameTime gameTime)
        {
            foreach (var npc in _list)
            {
                npc.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var mouseState = Mouse.GetState();
            var mouseTilePosition = Map.ToTilePosition(Globals.TheCarmera.ToWorldPosition(new Vector2(mouseState.X, mouseState.Y)));
            foreach (var npc in _list)
            {
                if(mouseTilePosition == Map.ToTilePosition(npc.Figure.PositionInWorld))
                    npc.Draw(spriteBatch, Color.Red);
                else npc.Draw(spriteBatch);
            }
        }
    }
}
