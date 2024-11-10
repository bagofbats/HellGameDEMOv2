using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private Rectangle glow_frame = new Rectangle(0, 96, 80, 48);
        private Texture2D sprite;
        float animate_timer = 0f;
        private int dialogue_num = 0;
        public bool sideways
        { get; private set; } = false;
        public bool sideways_right
        { get; private set; } = false;
        public Wall side_wall
        { get; private set; } = null;

        private Rectangle DrawBox;
        private Rectangle GlowBox;
        private bool Active = false;

        public Level root
        { get; private set; }

        public bool visible
        { get; set; }

        public Checkpoint(Rectangle box, Level root)
        {
            this.box = box;
            this.root = root;
            visible = true;
            DrawBox = box;

            GlowBox = new Rectangle(box.X - 32, box.Y, 80, 48);
        }

        public void Load(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public void DontAnimate(GameTime gameTime)
        {
            frame.X = 0;
            Active = false;
        }

        public void Animate(GameTime gameTime)
        {
            frame.Y = 0;

            animate_timer += 10 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            frame.X = 16 + (16 * ((int)animate_timer % 4));

            if (sideways)
                frame.Y += 32;

            if (sideways_right)
                frame.Y += 32;

            Active = true;

            glow_frame.Y = 96 + (48 * ((int)(animate_timer / 8) % 2));
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            if (visible)
            {
                if (Active && !sideways)
                    _spriteBatch.Draw(sprite, GlowBox, glow_frame, Color.White * 0.05f);

                _spriteBatch.Draw(sprite, DrawBox, frame, Color.White);
            }
                
        }

        public void Interact()
        {
            if (visible)
            {
                root.StartDialogue(root.dialogue_checkpoint, dialogue_num, 'c', 25f, true);
                dialogue_num = root.dialogue_second_index;
            }
                
        }

        public void SetSideways(bool sideways, string dir)
        {
            this.sideways = sideways;
            if (dir == "right")
                sideways_right = true;
            

            if (sideways)
                frame.Y += 32;

            if (sideways_right)
                frame.Y += 32;

            if (sideways)
                DrawBox.X += 1;

            if (sideways_right)
                DrawBox.X -= 2;
        }

        public void GetSidewaysWall()
        {
            if (sideways && !sideways_right)
            {
                side_wall = root.SimpleCheckCollision(new Rectangle(HitBox.X + 24, HitBox.Y, HitBox.Width, HitBox.Height));
            }
            else if (sideways && sideways_right)
                side_wall = root.SimpleCheckCollision(new Rectangle(HitBox.X - 24, HitBox.Y, HitBox.Width, HitBox.Height));
        }
    }
}
