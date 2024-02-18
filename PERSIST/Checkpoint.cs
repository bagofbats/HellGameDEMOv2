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
        public bool sideways
        { get; private set; } = false;
        public bool sideways_right
        { get; private set; } = false;
        public Wall side_wall
        { get; private set; } = null;

        private Rectangle DrawBox;

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
            frame.Y = 0;

            animate_timer += 10 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            frame.X = 16 + (16 * ((int)animate_timer % 4));

            if (sideways)
                frame.Y += 32;

            if (sideways_right)
                frame.Y += 32;
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            if (visible)
                _spriteBatch.Draw(sprite, DrawBox, frame, Color.White);
        }

        public void Interact()
        {
            if (visible)
                root.StartDialogue(root.dialogue_checkpoint, 0, 'c', 25f, true);
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
