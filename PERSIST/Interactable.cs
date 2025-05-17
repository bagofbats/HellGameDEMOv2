using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended.Gui.Controls;
using System.Diagnostics.Metrics;

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

    public class KeyPickup : Interactable
    {
        private Texture2D sprite;
        protected Rectangle pos;
        private ProgressionManager progman;
        private DialogueStruct[] dialogue;
        private int dialogue_num;

        private Rectangle frame = new Rectangle(240, 64, 16, 16);

        private float timer = 0f;

        public KeyPickup(Vector2 pos, Level root, ProgressionManager progman, DialogueStruct[] dialogue, int dialogue_num)
        {
            this.pos = new Rectangle((int)pos.X, (int)pos.Y, 16, 16);
            this.root = root;
            this.progman = progman;
            this.dialogue = dialogue;
            this.dialogue_num = dialogue_num;
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {
            frame.X = 240 + (16 * ((int)(timer * 10) % 6));
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, pos, frame, Color.White);
        }

        public override void Interact()
        {
            if (!progman.locks)
            {
                root.StartDialogue(dialogue, dialogue_num, 'c', 25f, true);
                progman.Unlock();
                root.RemoveInteractable(this);
            }
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(pos);
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, pos, Color.Blue * 0.3f);
        }
    }

    public class ShadePickup : Interactable
    {
        private Texture2D sprite;
        protected Rectangle pos;
        private ProgressionManager progman;
        private DialogueStruct[] dialogue;
        private int dialogue_num;

        private Rectangle frame = new Rectangle(240, 48, 16, 16);

        private float timer = 0f;

        public ShadePickup(Vector2 pos, Level root, ProgressionManager progman, DialogueStruct[] dialogue, int dialogue_num)
        {
            this.pos = new Rectangle((int)pos.X, (int)pos.Y, 16, 16);
            this.root = root;
            this.progman = progman;
            this.dialogue = dialogue;
            this.dialogue_num = dialogue_num;
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {
            frame.X = 240 + (16 * ((int)(timer * 10) % 6));
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, pos, frame, Color.White);
        }

        public override void Interact()
        {
            if (!progman.jump_blocks)
            {
                root.StartDialogue(dialogue, dialogue_num, 'c', 25f, true);
                progman.ShadeBlocks();
                root.RemoveInteractable(this);
            }
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(pos);
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, pos, Color.Blue * 0.3f);
        }
    }

    public class Furniture : Interactable
    {
        protected Texture2D sprite;
        protected Rectangle pos;
        protected DialogueStruct[] dialogue;

        protected int counter = 0;
        protected int[] breakpoints;

        public Furniture(Rectangle pos, Level root)
        {
            this.pos = pos;
            this.root = root;
            // this.dialogue = dialogue;
            // this.frame = frame;
        }

        public void SetType(DialogueStruct[] dialogue, int[] breakpoints)
        { 
            this.dialogue = dialogue;
            this.breakpoints = breakpoints;
        }

        public override void Interact()
        {
            root.StartDialogue(dialogue, breakpoints[counter], 'c', 25f, true);

            if (counter < breakpoints.Length - 1)
                counter++;
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(pos);
        }

        public override void Update(GameTime gameTime)
        {
            // throw new NotImplementedException();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // spriteBatch.Draw(sprite, pos, frame, Color.White);
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, pos, Color.Blue * 0.3f);
        }
    }

    public class SecretDesk : Furniture
    {
        ProgressionManager progman;

        public SecretDesk(Rectangle pos, Level root, ProgressionManager progman) : base(pos, root)
        {
            this.progman = progman;
        }

        public override void Interact()
        {
            if (progman.journal_secret && counter == 0)
                counter = 1;

            root.StartDialogue(dialogue, breakpoints[counter], 'c', 25f, true);

            if (counter < breakpoints.Length - 1 && progman.journal_secret)
                counter++;
        }
    }
}
