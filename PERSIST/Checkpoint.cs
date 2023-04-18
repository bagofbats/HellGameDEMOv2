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
        private Rectangle frame = new Rectangle(0, 0, 16, 32);
        private Texture2D sprite;

        public Checkpoint(Rectangle box)
        {
            this.box = box;
        }

        public void Load(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(sprite, box, frame, Color.White);
        }
    }
}
