using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PERSIST
{
    public abstract class ParticleFX
    {
        protected Level root;

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
    }

    public class RangedFX : ParticleFX
    {
        private Rectangle frame = new Rectangle(0, 0, 16, 16);
        private Texture2D img;
        private Rectangle pos;
        private float animate_timer = 0;

        public RangedFX(Vector2 pos, Texture2D img, Level root, bool fourway)
        {
            this.pos = new Rectangle((int)pos.X, (int)pos.Y, 16, 16);
            this.root = root;
            this.img = img;

            if (fourway)
                frame.Y = 16;
        }

        public override void Update(GameTime gameTime)
        {
            animate_timer += 36 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            frame.X = 16 * ((int)animate_timer);

            if (frame.X >= 80)
                root.RemoveFX(this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(img, pos, frame, Color.White);
        }
    }
}