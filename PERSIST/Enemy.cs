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
    public abstract class Enemy
    {
        public abstract Rectangle GetHitBox();
        public abstract void LoadAssets(Texture2D sprite);
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void DebugDraw(SpriteBatch spriteBatch, Texture2D blue);
        public abstract void Damage();
    }

    public class Slime : Enemy
    {
        Texture2D sprite;

        public override Rectangle GetHitBox()
        {
            throw new NotImplementedException();
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public override void Damage()
        {
            throw new NotImplementedException();
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            throw new NotImplementedException();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }
    }
}
