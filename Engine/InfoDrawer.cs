using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    static class InfoDrawer
    {
        public static void DrawRectangle(SpriteBatch spriteBatch, Rectangle coords, Color color)
        {
            spriteBatch.Draw(TextureGenerator.GetColorTexture(color,1,1), coords, color);
        }

        public static Color EnemyLifeColor = new Color(163, 18, 21)*0.9f;
        public static Color FriendLifeColor = new Color(16, 165, 28)*0.9f;
        public static Color LifeLoseColor = Color.Black * 0.7f;
        public static Color NameColor = Color.White*0.8f;
        public static Color EnemyBossNameColor = new Color(200, 200, 10) * 0.9f;
        public static void DrawLife(SpriteBatch spriteBatch, Npc npc)
        {
            Color drawColor;
            Color nameColor = NameColor;
            if (npc.IsEnemy) 
            {
                drawColor = EnemyLifeColor;
                if (npc.ExpBonus > 0)
                {
                    nameColor = EnemyBossNameColor;
                }
            }
            else if (npc.IsFighterFriend) drawColor = FriendLifeColor;
            else return;

            const int width = 300;
            const int height = 25;
            const int topLeftY = 50;
            var topLeftX = Globals.WindowWidth / 2 - width / 2;
            float percent = 0f;
            if (npc.LifeMax > 0 && npc.Life >= 0)
            {
                percent = npc.Life / (float)npc.LifeMax;
                if (percent > 1f) percent = 1f;
            }
            if (npc.LifeMax <= 0)
            {
                percent = 1f;
            }
            var lifeLength = (int)(width * percent);
            var lifeRegion = new Rectangle(topLeftX, topLeftY, lifeLength, height);
            var lifeLoseRegion = new Rectangle(topLeftX + lifeLength, topLeftY, width - lifeLength, height);
            DrawRectangle(spriteBatch, lifeRegion, drawColor);
            DrawRectangle(spriteBatch, lifeLoseRegion, LifeLoseColor);
            if (!string.IsNullOrEmpty(npc.Name))
            {
                var measure = Globals.FontSize12.MeasureString(npc.Name);
                var namePosition = new Vector2((float)Globals.WindowWidth / 2 - measure.X / 2,
                    topLeftY + (height - measure.Y) / 2 + 1);
                spriteBatch.DrawString(Globals.FontSize12, npc.Name, namePosition, nameColor);
            }
        }
    }
}
