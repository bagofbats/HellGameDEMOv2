using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
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

    public class SlimeFX : ParticleFX
    {
        private Rectangle frame = new Rectangle(0, 48, 32, 32);
        private Texture2D img;
        private Rectangle pos;
        private float animate_timer = 0;

        public SlimeFX(Vector2 pos, Texture2D img, Level root)
        {
            this.pos = new Rectangle((int)pos.X, (int)pos.Y, 32, 32);
            this.root = root;
            this.img = img;
        }

        public override void Update(GameTime gameTime)
        {
            animate_timer += 12 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            frame.X = 32 * ((int)animate_timer);

            if (frame.X >= 96)
                root.RemoveFX(this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(img, pos, frame, Color.White);
        }
    }

    public class SleepFX : ParticleFX
    {
        // private Rectangle frame = new Rectangle(0, 80, 16, 16);
        private Texture2D img;
        private Slime host;

        private float max_diff = 72f;

        private Rectangle z_one = new Rectangle(0, 0, 16, 16);
        private Rectangle z_one_frame = new Rectangle(32, 80, 16, 16);
        private float z_one_diff = 72f - 32f;
        private float z_one_xdiff = 0;
        private float z_one_transparency = 0f;

        private Rectangle z_two = new Rectangle(0, 0, 16, 16);
        private Rectangle z_two_frame = new Rectangle(32, 80, 16, 16);
        private float z_two_diff = 72f - 16f;
        private float z_two_xdiff = 0;
        private float z_two_transparency = 0f;

        private Rectangle z_three = new Rectangle(0, 0, 16, 16);
        private Rectangle z_three_frame = new Rectangle(0, 80, 16, 16);
        private float z_three_diff = 72f - 24f;
        private float z_three_xdiff = 0;
        private float z_three_transparency = 0f;

        

        public SleepFX(Texture2D img, Level root, Slime host)
        {
            this.root = root;
            this.img = img;
            this.host = host;
        }

        public override void Update(GameTime gameTime)
        {
            // z_one
            z_one.X = host.HitBox.X + (int)z_one_xdiff;
            z_one.Y = host.HitBox.Y - 12 + (int)z_one_diff;

            z_one_diff -= 0.5f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);
            z_one_diff = z_one_diff % max_diff;

            z_one_xdiff = 8 * (float)Math.Sin(z_one_diff / 4);

            z_one_transparency = 0.42f + (z_one_diff / 100);
            if (z_one_diff >= -5f)
                z_one_transparency = z_one_diff / -10;

            // z_two
            z_two.X = host.HitBox.X + (int)z_two_xdiff;
            z_two.Y = host.HitBox.Y - 12 + (int)z_two_diff;

            z_two_diff -= 0.5f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);
            z_two_diff = z_two_diff % max_diff;

            z_two_xdiff = 8 * (float)Math.Sin(z_two_diff / 4);

            z_two_transparency = 0.42f + (z_two_diff / 100);
            if (z_two_diff >= -5f)
                z_two_transparency = z_two_diff / -10;

            // z_three
            z_three.X = host.HitBox.X + (int)z_three_xdiff;
            z_three.Y = host.HitBox.Y - 12 + (int)z_three_diff;

            z_three_diff -= 0.5f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);
            z_three_diff = z_three_diff % max_diff;

            z_three_xdiff = 8 * (float)Math.Sin(z_three_diff / 4);

            z_three_transparency = 0.42f + (z_three_diff / 100);
            if (z_three_diff >= -5f)
                z_three_transparency = z_three_diff / -10;
        }

        public void ResetZs()
        {
            z_one_diff = max_diff - 32f;
            z_two_diff = max_diff - 16f;
            z_three_diff = max_diff - 24f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(img, z_one, z_one_frame, Color.White * z_one_transparency);
            spriteBatch.Draw(img, z_two, z_two_frame, Color.White * z_two_transparency);
            spriteBatch.Draw(img, z_three, z_three_frame, Color.White * z_three_transparency);
        }
    }
}