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
            if (!progman.GetFlag(FLAGS.ranged))
            {
                root.StartDialogue(dialogue, 0, 'c', 25f, true);
                progman.SetFlag(FLAGS.ranged);
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

        protected new StyxLevel root;

        

        public KeyPickup(Vector2 pos, StyxLevel root, ProgressionManager progman, DialogueStruct[] dialogue, int dialogue_num)
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
            if (!progman.GetFlag(FLAGS.locks))
            {
                root.StartDialogue(dialogue, dialogue_num, 'c', 25f, true);
                progman.SetFlag(FLAGS.locks);
                root.RemoveInteractable(this);
                root.RemoveLukas();
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
        private bool cutscene = false;
        private GameTime saved_gameTime;
        private bool transformed = false;

        private Rectangle frame = new Rectangle(240, 32, 16, 16);

        private float timer = 0f;
        private float flash_timer = 0f;

        private float normal_y = 0f;
        private float floating_y = 0f;
        private float drawing_y = 0f;

        public bool floating
        { get; set; } = false;

        public ShadePickup(Vector2 pos, Level root, ProgressionManager progman, DialogueStruct[] dialogue, int dialogue_num)
        {
            this.pos = new Rectangle((int)pos.X, (int)pos.Y, 16, 16);
            this.root = root;
            this.progman = progman;
            this.dialogue = dialogue;
            this.dialogue_num = dialogue_num;

            normal_y = pos.Y;
            floating_y = pos.Y - 64;

            drawing_y = normal_y;
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {
            frame.X = 240 + (16 * ((int)(timer * 10) % 6));
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            saved_gameTime = gameTime;

            if (frame.Y == 80)
            {
                flash_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (flash_timer > 0.08)
                    frame.Y = 48;
            }

            if (floating)
            {
                // current_y += (target.Y - current_y) / speed;

                drawing_y += (floating_y - drawing_y) / 17;

                if (Math.Abs(drawing_y - floating_y) <= 0.3)
                    drawing_y = floating_y;
            }

            else
            {
                drawing_y += (normal_y - drawing_y) / 17;

                if (Math.Abs(drawing_y - normal_y) <= 0.3)
                    drawing_y = normal_y;

                //drawing_y = normal_y;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle draw_rect = new Rectangle(pos.X, (int)drawing_y, pos.Width, pos.Height);

            spriteBatch.Draw(sprite, draw_rect, frame, Color.White);
        }

        public override void Interact()
        {
            if (!progman.GetFlag(FLAGS.jump_blocks))
            {
                //root.StartDialogue(dialogue, dialogue_num, 'c', 25f, true);
                //progman.ShadeBlocks();
                //root.RemoveInteractable(this);

                if (!transformed)
                {
                    cutscene = true;
                    root.HandleCutscene("lukaspickup|empty|empty|empty|empty|empty", saved_gameTime, true);
                }
                else
                {
                    root.StartDialogue(dialogue, dialogue_num, 'c', 25f, true);
                    progman.SetFlag(FLAGS.jump_blocks);
                    root.RemoveInteractable(this);
                }
            }
        }

        public void Transform()
        {
            frame.Y = 80; //frame.Y = 48;
            transformed = true;
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
            if (progman.GetFlag(FLAGS.journal_secret) && counter == 0)
                counter = 1;

            root.StartDialogue(dialogue, breakpoints[counter], 'c', 25f, true);

            if (counter < breakpoints.Length - 1 && progman.GetFlag(FLAGS.journal_secret))
                counter++;
        }
    }

    public class InteractableGuy : Furniture
    {
        private Rectangle frame = new();
        private Rectangle hitbox = new();
        private bool animate = false;
        private float animate_speed = new();
        private int num_frames = 1;

        private float animate_timer = 0f;
        private Rectangle draw_frame = new();


        public InteractableGuy(Rectangle pos, Level root) : base(pos, root)
        {

        }

        public void SetGuyInfo(Rectangle hitbox, Rectangle frame, bool animate=false, float animate_speed=0f, int num_frames=1)
        {
            this.frame = frame;
            this.hitbox = hitbox;
            this.animate = animate;
            this.animate_speed = animate_speed;
            this.num_frames = num_frames;

            draw_frame = frame;
        }

        public override void Update(GameTime gameTime)
        {
            if (animate)
            {
                draw_frame.X = frame.X + (frame.Width * ((int)(animate_timer * animate_speed) % num_frames));
                animate_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, pos, draw_frame, Color.White);
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(hitbox);
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, pos, Color.Blue * 0.3f);
            spriteBatch.Draw(blue, hitbox, Color.Red * 0.3f);
        }
    }
}
