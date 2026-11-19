using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HellGame
{
    public abstract class Touchable
    {
        public abstract Rectangle GetHitBox(Rectangle input);
        public abstract void LoadAssets(Texture2D sprite);
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void DebugDraw(SpriteBatch spriteBatch, Texture2D blue);
        public abstract bool CheckCollision(Rectangle input);
        public abstract void OnTouch();
    }
}
