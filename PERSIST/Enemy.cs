using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PERSIST
{
    public abstract class Enemy
    {
        // for enemies with only one HitBox, GetHitBox should just return the HitBox and ignore the input
        // for enemies with multiple HitBoxes, GetHitBox should use the input to compute which one to return
        public abstract Rectangle GetHitBox(Rectangle input);
        public abstract void LoadAssets(Texture2D sprite);
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void DebugDraw(SpriteBatch spriteBatch, Texture2D blue);
        public abstract void Damage();
        public abstract bool CheckCollision(Rectangle input);

        protected Level root;
        public Room room { get; set; }
        protected Vector2 pos;
        public Vector2 Pos { get => pos; }
        public bool hurtful { get; protected set; }
    }

    // regular enemies

    public class Slime : Enemy
    {
        Texture2D sprite;
        private int hp = 4;
        private Rectangle frame = new Rectangle(0, 0, 32, 32);
        private float vsp = 0f;
        private float grav = 0.12f;
        private float hspeed = 1f;
        private float bounce_counter = 2f;
        private int hdir = 1;
        private int h_oset = 8;
        private int v_oset = 20;
        private bool damaged = false;
        private float damaged_timer = 0f;
        private Random rnd = new Random();

        private SleepFX sleepFX;
        private bool sleep;

        public Slime(Vector2 pos, Level root)
        {
            this.pos = pos;
            this.root = root;
            hurtful = true;
        }

        public int HP
        { get { return hp; } set { hp = value; } }

        public Rectangle PositionRectangle
        { get { return new Rectangle((int)pos.X, (int)pos.Y, 32, 32); } }

        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X + h_oset, (int)pos.Y + v_oset, 32 - (h_oset * 2), 32 - v_oset); } }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return HitBox;
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;

            sleepFX = new SleepFX(root.particle_img, root, this);
        }

        public override void Update(GameTime gameTime)
        {
            Vector2 player_pos = new Vector2(root.player.HitBox.X, root.player.HitBox.Y);
            float dist_x = Math.Abs(pos.X - player_pos.X);
            float dist_y = Math.Abs(pos.Y - player_pos.Y);

            vsp += grav;

            if (bounce_counter >= 3)
            {
                vsp = -3;
            }

            float vsp_col_check = vsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            if (vsp_col_check > 0)
                vsp_col_check += 1;
            else
                vsp_col_check -= 1;

            Wall vcheck = root.SimpleCheckCollision(new Rectangle(HitBox.X, (int)(HitBox.Y + vsp_col_check), HitBox.Width, HitBox.Height));

            sleep = (dist_x > 100 || dist_y > 70) && vcheck != null;

            if (vcheck != null)
            {
                if (vsp < 0)
                    pos.Y = vcheck.bounds.Bottom - v_oset;
                else
                    pos.Y = vcheck.bounds.Top - 32;
                vsp = 0;

                if (!sleep)
                {
                    hdir = Math.Sign(root.player.HitBox.X - pos.X - 8);
                    if (hdir == 0)
                        hdir = 1;
                }
            }
            else
            {
                bounce_counter = 0;
                frame.X = 96;
                float x_displacement = hspeed * hdir;
                if (damaged)
                    x_displacement += (float)rnd.NextDouble() * (rnd.Next(0, 1) - 0.5f);

                float hsp_col_check = x_displacement * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
                if (hsp_col_check > 0)
                    hsp_col_check += 1;
                else
                    hsp_col_check -= 1;

                Wall hcheck = root.SimpleCheckCollision(new Rectangle((int)(HitBox.X + hsp_col_check), HitBox.Y, HitBox.Width, HitBox.Height));

                if (hcheck != null)
                {
                    if (hdir == -1)
                    {
                        pos.X = hcheck.bounds.Right - h_oset;
                    }
                    else
                    {
                        pos.X = hcheck.bounds.Left - 32 + h_oset;
                    }
                }
                else
                {
                    pos.X += x_displacement * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
                }
            }

            pos.Y += vsp;

            if (vcheck != null && !sleep)
            {
                frame.X = 32 * ((int)bounce_counter % 2);
                bounce_counter += 1.5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (!damaged)
            {
                frame.Y = 16 * -(hdir - 1);
            }
            else
            {
                frame.Y = 64 + (16 * -(hdir - 1));
                damaged_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (damaged_timer > 0.1)
                    damaged = false;
            }

            if (sleep)
            {
                sleepFX.Update(gameTime);
                frame.X = 64;
                if (hdir == 1)
                    frame.Y = 0;
                else
                    frame.Y = 32;
            }
            else
                sleepFX.ResetZs();
            
        }

        public override void Damage()
        {
            hp -= 1;
            if (hp <= 0)
            {
                root.RemoveEnemy(this);
                SlimeFX particle = new SlimeFX(new Vector2(PositionRectangle.X, PositionRectangle.Y), root.particle_img, root);
                root.AddFX(particle);
            }
                
            bounce_counter = 1;
            vsp = -1;
            hdir = -Math.Sign(root.player.HitBox.X - pos.X - 8);
            if (hdir == 0)
                hdir = 1;

            damaged = true;
            damaged_timer = 0;
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, PositionRectangle, frame, Color.White);
            if (sleep)
                sleepFX.Draw(spriteBatch);
        }

        public override bool CheckCollision(Rectangle input)
        {
            return HitBox.Intersects(input);
        }
    }

    public class EyeSwitch : Enemy
    {
        private Texture2D sprite;
        private Rectangle bounds;
        public bool two = true;

        public EyeSwitch(Rectangle bounds, Level root)
        {
            this.bounds = bounds;
            this.root = root;
            hurtful = false;
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
            this.room = root.RealGetRoom(new Vector2(bounds.X, bounds.Y));
        }

        public override void Update(GameTime gameTime)
        {
            // throw new NotImplementedException();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, bounds, Color.Red);
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            //throw new NotImplementedException();
        }

        public override void Damage()
        {
            root.Switch(room, two);
            // two = !two;
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return bounds;
        }

        public override bool CheckCollision(Rectangle input)
        {
            return bounds.Intersects(input);
        }
    }


    // bosses and mini-bosses

    public class BigSlime : Enemy
    {
        private Texture2D sprite;
        private Rectangle frame = new Rectangle(0, 128, 96, 64);
        private float bounce_counter = 0f;
        private float vsp = 0f;
        private float grav = 0.1f;
        private int hdir = 1;
        private float hspeed = 1f;
        public bool sleep = true;
        private Player player;
        private bool wakeup_ready = false;
        private Rectangle wakeup_rectangle = new Rectangle(880, 720, 16, 32);
        new private TutorialLevel root;
        private int hp = 22;
        private bool damaged = false;
        private float damaged_timer = 0f;
        private bool airborne = false;

        public BigSlime(Vector2 pos, Player player, TutorialLevel root)
        {
            this.pos = pos;
            this.player = player;
            this.root = root;
            hurtful = true;
        }

        public Rectangle PositionRectangle
        { get { return new Rectangle((int)pos.X - 48, (int)pos.Y - 8, 96, 64); } }

        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X - 32, (int)pos.Y + 20, 64, 28); } }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {
            if (sleep)
                Sleep(gameTime);
            else
                ActualUpdate(gameTime);
        }

        private void ActualUpdate(GameTime gameTime)
        {
            bounce_counter += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (bounce_counter >= 4f)
                bounce_counter = 0f;

            vsp += grav;

            if (bounce_counter >= 2)
                vsp = -3.1f;

            float vsp_col_check = vsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            if (vsp_col_check > 0)
                vsp_col_check += 1;
            else
                vsp_col_check -= 1;

            Wall vcheck = root.SimpleCheckCollision(new Rectangle(HitBox.X, (int)(HitBox.Y + vsp_col_check), HitBox.Width, HitBox.Height));
            airborne = vcheck == null;

            if (vcheck != null)
            {
                if (vsp < 0)
                    pos.Y = vcheck.bounds.Bottom - 16;
                else
                    pos.Y = vcheck.bounds.Top - 48;
                vsp = 0;

                hdir = Math.Sign(root.player.HitBox.X - pos.X);
                if (hdir == 0)
                    hdir = 1;

                frame.X = 96 * ((int)bounce_counter % 2);

                if (hdir == -1)
                    frame.Y = 192;
                else
                    frame.Y = 128;
            }
            else
            {
                bounce_counter = 0;

                if (hdir == -1)
                    frame.Y = 192;
                else
                    frame.Y = 128;

                frame.X = 192;

                float x_displacement = hspeed * hdir;

                float hsp_col_check = x_displacement * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
                if (hsp_col_check > 0)
                    hsp_col_check += 1;
                else
                    hsp_col_check -= 1;

                Wall hcheck = root.SimpleCheckCollision(new Rectangle((int)(HitBox.X + hsp_col_check), HitBox.Y, HitBox.Width, HitBox.Height));

                if (hcheck != null)
                {
                    if (hdir == -1)
                    {
                        pos.X = hcheck.bounds.Right + 32;
                    }
                    else
                    {
                        pos.X = hcheck.bounds.Left - 32;
                    }
                }
                else
                {
                    pos.X += x_displacement * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
                }
            }

            pos.Y += vsp;

            if (damaged)
            {
                damaged_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (damaged_timer > 0.1)
                    damaged = false;
            }
        }

        private void Sleep(GameTime gameTime)
        {
            frame.X = 192;
            frame.Y = 0;

            if (player.HitBox.Intersects(wakeup_rectangle))
                wakeup_ready = true;
            else if (wakeup_ready)
                root.WakeUpSlime(this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!damaged)
                spriteBatch.Draw(sprite, PositionRectangle, frame, Color.White);
            else
            {
                var temp = new Rectangle(frame.X, frame.Y + 128, frame.Width, frame.Height);
                spriteBatch.Draw(sprite, PositionRectangle, frame, Color.White * 0.76f);
                spriteBatch.Draw(sprite, PositionRectangle, temp, Color.White * 0.24f);
            }
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            if (airborne)
            {
                spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
                spriteBatch.Draw(blue, JumpingHB1, Color.Blue * 0.3f);
            }
            else if ((int)bounce_counter % 2 == 0)
            {
                // spriteBatch.Draw(blue, HurtBox, Color.Red * 0.3f);
                spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
                spriteBatch.Draw(blue, IdleHB1, Color.Blue * 0.3f);
                spriteBatch.Draw(blue, IdleHB2, Color.Blue * 0.3f);
                spriteBatch.Draw(blue, IdleHB3, Color.Blue * 0.3f);
            }
            else
            {
                spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
                spriteBatch.Draw(blue, SquishedHB1, Color.Blue * 0.3f);
                spriteBatch.Draw(blue, SquishedHB2, Color.Blue * 0.3f);
            }
        }

        public override void Damage()
        {
            hp -= 1;
            damaged = true;
            damaged_timer = 0;
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            if (input == new Rectangle(0, 0, 0, 0))
                return HitBox;

            if (airborne)
                return HitBox;
            else if ((int)bounce_counter % 2 == 0)
            {
                if (input.Intersects(IdleHB1))
                    return IdleHB1;
                if (input.Intersects(IdleHB3))
                    return IdleHB3;
                if (input.Intersects(HitBox))
                    return HitBox;
                return IdleHB2;
            }
            else
            {
                if (input.Intersects(SquishedHB1))
                    return SquishedHB1;
                if (input.Intersects(HitBox))
                    return HitBox;
                return SquishedHB2;
            }
        }

        public override bool CheckCollision(Rectangle input)
        {
            if (airborne)
                return HitBox.Intersects(input) || JumpingHB1.Intersects(input);
            else if ((int)bounce_counter % 2 == 0)
                return HitBox.Intersects(input) || IdleHB1.Intersects(input) || IdleHB2.Intersects(input) || IdleHB3.Intersects(input);
            else
                return HitBox.Intersects(input) || SquishedHB1.Intersects(input) || SquishedHB2.Intersects(input);
        }

        public Rectangle IdleHB1
        { get { return new Rectangle((int)pos.X - 18, (int)pos.Y + 8, 36, 12); } }

        public Rectangle IdleHB2
        { get { return new Rectangle((int)pos.X - 35, (int)pos.Y + 24, 70, 20); } }

        public Rectangle IdleHB3
        { get { return new Rectangle((int)pos.X - 24, (int)pos.Y + 12, 48, 8); } }

        public Rectangle SquishedHB1
        { get { return new Rectangle((int)pos.X - 23, (int)pos.Y + 14, 46, 14); } }

        public Rectangle SquishedHB2
        { get { return new Rectangle((int)pos.X - 37, (int)pos.Y + 26, 74, 16); } }

        public Rectangle JumpingHB1
        { get { return new Rectangle((int)pos.X - 28, (int)pos.Y, 56, 32); } }
    }

    public class Lukas_Tutorial : Enemy
    {
        public override void LoadAssets(Texture2D sprite)
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            throw new NotImplementedException();
        }

        public override void Damage()
        {
            throw new NotImplementedException();
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            throw new NotImplementedException();
        }

        public override bool CheckCollision(Rectangle input)
        {
            throw new NotImplementedException();
        }
    }
}
