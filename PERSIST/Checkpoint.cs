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
    public class Checkpoint
    {
        public Rectangle box
        { get; private set; }
        public Rectangle HitBox 
        { get { return new Rectangle(box.X + 2, box.Y + 14, box.Width - 4, box.Height - 14); } }
        private Rectangle frame = new Rectangle(0, 0, 16, 32);
        private Texture2D sprite;
        float animate_timer = 0f;
        private Level root;

        public Checkpoint(Rectangle box, Level root)
        {
            this.box = box;
            this.root = root;
        }

        public void Load(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public void DontAnimate(GameTime gameTime)
        {
            frame.X = 0;
        }

        public void Animate(GameTime gameTime)
        {
            animate_timer += 10 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            frame.X = 16 + (16 * ((int)animate_timer % 4));
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(sprite, box, frame, Color.White);
        }
    }

    public class FakeCheckpoint : Checkpoint
    {
        public FakeCheckpoint(Rectangle box, Level root) : base(box, root) { }

        new public Rectangle box
        { get; private set; }
    }
}
