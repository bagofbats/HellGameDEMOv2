using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PERSIST
{
    public abstract class Attack
    {
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch _spriteBatch);
        public abstract void DebugDraw(SpriteBatch _spriteBatch, Texture2D blue);
    }

    public class Slash : Attack
    {
        private Player player;
        private char type;
        private Texture2D img;
        private Rectangle pos;
        private Rectangle frame = new Rectangle(0, 0, 32, 32);
        private float animateTimer = 0;
        private Level level;
        private bool pogoed = false;

        private List<Enemy> enemies_struck = new List<Enemy>();
        private List<Breakable> specials = new List<Breakable>();

        public Slash(Player player, char type, Texture2D img, bool slash_dir, Level level)
        {
            this.player = player;
            this.type = type;
            this.img = img;
            this.level = level;
            pos = player.DrawBox;
            pos.X = player.HitBox.X + 16;
            pos.Y = player.HitBox.Y;
            if (type == 'l')
                frame.Y = 32;
            else if (type == 'u')
                frame.Y = 64;
            else if (type == 'd')
                frame.Y = 96;
            if (!slash_dir)
                frame.Y += 128;
        }

        public Rectangle HitBox
        {
            get
            {
                if (type == 'u')
                    return new Rectangle(pos.X + 2, pos.Y, 12, 20);
                else if (type == 'd')
                    return new Rectangle(pos.X + 2, pos.Y + 2, 12, 18);
                else if (type == 'l')
                    return new Rectangle(pos.X + 11, pos.Y + 1, 22, 14);
                else
                    return new Rectangle(pos.X - 1, pos.Y + 1, 22, 14);
            }
        }

        public override void Update(GameTime gameTime)
        {
            animateTimer += 50 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            frame.X = 32 * ((int)animateTimer % 9);
            // repositioning
            if (type == 'l')
            {
                pos.X = player.HitBox.X - 20 - player.HitBox.Width;
                pos.Y = player.HitBox.Y;
            }
            else if (type == 'u')
            {
                pos.X = player.HitBox.X - 1;
                pos.Y = player.HitBox.Y - 22;
            }
            else if (type == 'd')
            {
                pos.X = player.HitBox.X - 1;
                pos.Y = player.HitBox.Y + 14;
            }
            else
            {
                pos.X = player.HitBox.X + 16;
                pos.Y = player.HitBox.Y;
            }

            if (frame.X >= 256)
            {
                // enemies_struck = null;
                player.FinishAttack(this);
            }
        }

        public override void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(img, pos, frame, Color.White);
        }

        public override void DebugDraw(SpriteBatch _spriteBatch, Texture2D blue)
        {
            throw new NotImplementedException();
        }
    }
}
