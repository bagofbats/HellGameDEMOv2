using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PERSIST
{
    public abstract class Interactable
    {
        protected Level root;

        public abstract void Interact();
        public abstract void LoadAssets(Texture2D sprite);
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void DebugDraw(SpriteBatch spriteBatch, Texture2D blue);
        public abstract bool CheckCollision(Rectangle input);
    }

    public class RangerPickup : Interactable
    {
        private Texture2D sprite;
        protected Rectangle pos;
        private ProgressionManager progman;
        private DialogueStruct[] dialogue;

        private Rectangle frame = new Rectangle(192, 96, 16, 16);

        private float timer = 0f;

        public RangerPickup(Vector2 pos, Level root, ProgressionManager progman, DialogueStruct[] dialogue)
        {
            this.pos = new Rectangle((int)pos.X, (int)pos.Y, 16, 16);
            this.root = root;
            this.progman = progman;
            this.dialogue = dialogue;
        }


        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {
            frame.X = 192 + (16 * ((int)(timer * 10) % 6));
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, pos, frame, Color.White);
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, pos, Color.Blue * 0.3f);
        }

        public override void Interact()
        {
            if (!progman.ranged)
            {
                root.StartDialogue(dialogue, 0, 'c', 25f, true);
                progman.GetRanged();
                root.RemoveInteractable(this);
            }
            
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(pos);
        }
    }
}
