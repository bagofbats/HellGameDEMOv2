using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Gui.Controls;
using MonoGame.Extended.Timers;

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
        public abstract bool CheckCollision(Rectangle input);
        public virtual void Damage(float damage)
        {
            // this method is only needed for attackable enemies
            // non-attackable enemies (e.g. projectiles) should not override this empty function
        }
        public virtual void Interact()
        {
            // this method is only needed for non-hurtful interactable enemies
            // regular enemies should not override this empty function.
        }
        public virtual void FlashDestroy()
        {
            // this method is only for hurtful switch blocks
            // nobody else should override this empty function
        }

        public virtual void OnDamage()
        {
            // this method is intended for projectiles that detroy themselves when they hit the player
            // but really any enemy COULD overwrite this with something if they wanted to
            // if it is not overwritten, it is an empty function.
        }

        protected Level root;
        public Room room { get; set; }
        protected Vector2 pos;
        public Vector2 Pos { get => pos; }
        public bool hurtful { get; protected set; } = true;
        public bool pogoable { get; protected set; } = true;
        public bool super_pogo { get; protected set; } = false;
        public bool destroy_projectile { get; protected set; } = true;
    }

    // regular enemies
    
    // tutorial
    public class Slime : Enemy
    {
        Texture2D sprite;
        protected float hp = 4f;
        private Rectangle frame = new Rectangle(0, 0, 32, 32);
        protected float vsp = 0f;
        private float grav = 0.12f;
        private float hspeed = 1f;
        protected float bounce_counter = 2f;
        private float bounce_threshhold = 3f;
        protected int hdir = 1;
        private int h_oset = 8;
        private int v_oset = 20;
        protected bool damaged = false;
        protected float damaged_timer = 0f;
        private Random rnd = new Random();

        private SleepFX sleepFX;
        private bool sleep;
        protected bool sleep_possible = true;

        private bool airborne = false;

        public Slime(Vector2 pos, Level root)
        {
            this.pos = pos;
            this.root = root;
            hurtful = true;
            pogoable = true;
        }

        public float HP
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

            if (bounce_counter >= bounce_threshhold)
            {
                vsp = -3;
            }

            float vsp_col_check = vsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            if (vsp_col_check > 0)
                vsp_col_check += 1;
            else
                vsp_col_check -= 1;

            Wall vcheck = root.SimpleCheckCollision(new Rectangle(HitBox.X, (int)(HitBox.Y + vsp_col_check), HitBox.Width, HitBox.Height));

            sleep = (dist_x > 100 || dist_y > 100) && vcheck != null && sleep_possible;
            airborne = vcheck == null;

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

            pos.Y += vsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;

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
                sleepFX.trail_off = false;

                sleepFX.Update(gameTime);
                frame.X = 64;
                if (hdir == 1)
                    frame.Y = 0;
                else
                    frame.Y = 32;
            }
            else if (!sleepFX.trailed_off)
            {
                sleepFX.Update(gameTime);
                sleepFX.trail_off = true;
            }
            else
            {
                sleepFX.ResetZs();
            }
                

        }

        public override void Damage(float damage)
        {
            hp -= damage;
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

            root.audio_manager.PlaySound("hit");
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
            else if (!sleepFX.trailed_off && !airborne)
                sleepFX.Draw(spriteBatch);
        }

        public override bool CheckCollision(Rectangle input)
        {
            return HitBox.Intersects(input);
        }

        public void SetSpeed(float speed)
        {
            hspeed = speed;
        }

        public void SetTimer(float timer)
        {
            bounce_threshhold = timer;
        }
    }

    public class EyeSwitch : Enemy
    {
        private Texture2D sprite;
        private Rectangle bounds;
        public bool two = true;
        private Rectangle eye = new Rectangle(0, 0, 3, 3);
        private Player player;

        private Color drawColor = Color.Red;
        private Color pupilColor = Color.DarkRed;
        private bool damaged = false;
        private float dmg_timer = 0f;

        public bool disabled
        { get; private set; }

        public EyeSwitch(Rectangle bounds, Player player, Level root)
        {
            this.bounds = bounds;
            this.root = root;
            hurtful = false;
            pogoable = true;
            this.player = player;
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
            this.room = root.RealGetRoom(new Vector2(bounds.X, bounds.Y));
        }

        public override void Update(GameTime gameTime)
        {
            // animate the pupil
            var temp = player.GetPos();
            int xdiff = bounds.X + 2 + (int)(0.13f * (temp.X + 16 - bounds.X + 6));
            int ydiff = bounds.Y + 4 + (int)(0.13f * (temp.Y + 16 - bounds.Y + 6));

            eye.X = xdiff;
            eye.Y = ydiff;

            eye.X = Math.Clamp(eye.X, bounds.X + 1, bounds.X + bounds.Width - 1 - eye.Width);
            eye.Y = Math.Clamp(eye.Y, bounds.Y + 1, bounds.Y + bounds.Width - 1 - eye.Width);


            // flash white when damaged
            if (damaged)
            {
                drawColor = Color.White;
                pupilColor = Color.Blue;

                dmg_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (dmg_timer >= 0.08f)
                {
                    damaged = false;
                    dmg_timer = 0;
                }    
            }
            else if (disabled)
            {
                drawColor = Color.Black;
                pupilColor = Color.Black;
            }
            else
            {
                drawColor = Color.Red;
                pupilColor = Color.DarkRed;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, bounds, drawColor);
            spriteBatch.Draw(sprite, eye, pupilColor);
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            //throw new NotImplementedException();
        }

        public override void Damage(float damage)
        {
            if (disabled)
                return;

            root.Switch(room, two);
            damaged = true;
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return bounds;
        }

        public override bool CheckCollision(Rectangle input)
        {
            return bounds.Intersects(input);
        }

        public void SetDisabled(bool d)
        {
            disabled = d;
        }
    }

    // styx
    public class Walker : Enemy
    {
        Texture2D sprite;
        Texture2D vfx_sprite;

        private int h_oset = 9;
        private int v_oset = 15;
        private float animation_timer = 0f;
        private bool flash = false;
        private float flash_timer = 0f;

        private Rectangle frame = new Rectangle(0, 0, 32, 32);

        private int dir = -1;
        private float hsp = 1f;
        private float grav = 0.211f;
        private float vsp = 0f;

        private float hp = 5f;

        private int shake_xoset = 0;
        private int shake_yoset = 0;

        private Random rnd = new Random();

        public Rectangle PositionRectangle
        { get { return new Rectangle((int)pos.X, (int)pos.Y, 32, 32); } }

        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X + h_oset, (int)pos.Y + v_oset, 32 - (h_oset * 2), 32 - v_oset); } }

        public Rectangle front_check
        { get { return new Rectangle(HitBox.X + (HitBox.Width * (dir + 1) / 2) + dir, HitBox.Y + HitBox.Height + 1, 1, 1); } }

        public Rectangle back_check
        { get { return new Rectangle(HitBox.X + (HitBox.Width * (dir - 1) / -2) + dir, HitBox.Y + HitBox.Height + 1, 1, 1); } }

        public Walker(Vector2 pos, Level root)
        {
            this.pos = pos;
            this.root = root;
            hurtful = true;
            pogoable = true;
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
            vfx_sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {
            //Room current = root.RealGetRoom(temp);
            Room home = root.RealGetRoom(pos);

            if (home != null)
                if (home.bounds.Intersects(root.player.HitBox))
                    ActualUpdate(gameTime);
        }

        public override void Damage(float damage)
        {
            flash = true;
            flash_timer = 0;

            //root.AddFX(new MushroomFX(new Vector2(pos.X + 16, pos.Y + 16), vfx_sprite, root, new Vector2(-0.64f ,-0.8f)));
            //root.AddFX(new MushroomFX(new Vector2(pos.X + 16, pos.Y + 16), vfx_sprite, root, new Vector2(0.6f, -0.9f)));
            //root.AddFX(new MushroomFX(new Vector2(pos.X + 16, pos.Y + 16), vfx_sprite, root, new Vector2(0.2f, -0.8f)));

            hp -= damage;

            if (hp <= 0)
            {
                root.RemoveEnemy(this);

                root.AddFX(new MushroomFX(new Vector2(pos.X + 16, pos.Y + 16), vfx_sprite, root, new Vector2(-0.64f, -0.8f)));
                root.AddFX(new MushroomFX(new Vector2(pos.X + 16, pos.Y + 16), vfx_sprite, root, new Vector2(0.6f, -0.9f)));
                root.AddFX(new MushroomFX(new Vector2(pos.X + 16, pos.Y + 16), vfx_sprite, root, new Vector2(0.2f, -0.8f)));
            }

        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
            spriteBatch.Draw(blue, front_check, Color.Blue * 0.3f);
            spriteBatch.Draw(blue, back_check, Color.Blue * 0.3f);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle draw_rectangle = new Rectangle(PositionRectangle.X + shake_xoset, PositionRectangle.Y, PositionRectangle.Width, PositionRectangle.Height);

            spriteBatch.Draw(sprite, draw_rectangle, frame, Color.White);

            if (flash)
            {
                var flash_frame = new Rectangle(frame.X, frame.Y + 64, frame.Width, frame.Height);
                spriteBatch.Draw(sprite, draw_rectangle, flash_frame, Color.White * 0.4f);
            }
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return HitBox;
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(HitBox);
        }

        private void Animate(GameTime gameTime)
        {
            animation_timer += 8 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            frame.X = 32 * ((int)animation_timer % 4);

            frame.Y = 16 * (dir + 1);
        }

        private void ActualUpdate(GameTime gameTime)
        {
            Wall ledge_check = root.SimpleCheckCollision(front_check);
            Wall ledge_check_back = root.SimpleCheckCollision(back_check);

            if (ledge_check == null && ledge_check_back != null)
                dir *= -1;


            float xdiff = dir * hsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;

            Wall hcheck = root.SimpleCheckCollision(new Rectangle((int)(HitBox.X + xdiff), HitBox.Y, HitBox.Width, HitBox.Height));

            if (hcheck != null)
            {
                if (dir == 1)
                    pos.X = hcheck.bounds.Left - 32 + h_oset;
                if (dir == -1)
                    pos.X = hcheck.bounds.Right - h_oset;
                dir *= -1;
            }
            else
                pos.X += xdiff;

            float vdiff = vsp + (grav * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);

            if (vdiff > 0)
                vdiff += 1;
            else
                vdiff -= 1;

            Wall vcheck = root.SimpleCheckCollision(new Rectangle(HitBox.X, (int)(HitBox.Y + vdiff), HitBox.Width, HitBox.Height));

            if (vcheck != null)
            {
                pos.Y = vcheck.bounds.Top - 32;
                vsp = 0;
            }
            else
            {
                vsp += grav;
                pos.Y += vsp;
            }

            if (flash)
            {
                flash_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (flash_timer > 0.1f)
                {
                    flash = false;
                    flash_timer = 0f;
                }

                shake_xoset = (int)((rnd.Next(0, 2) - 0.5f) * 2);
                shake_yoset = (int)((rnd.Next(0, 2) - 0.5f) * 2);
            }

            else
            {
                shake_xoset = 0;
                shake_yoset = 0;
            }


            Animate(gameTime);
        }
    }

    public class Trampoline : Enemy
    {
        Texture2D sprite;
        private int h_oset = 4;
        private int v_oset = 22;
        private float flash_timer = 0f;

        private Rectangle frame = new Rectangle(128, 0, 32, 32);
        private Random rnd = new Random();

        public bool flash
        { get; set; } = false;

        public Rectangle PositionRectangle
        { get { return new Rectangle((int)pos.X, (int)pos.Y, 32, 32); } }

        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X + h_oset, (int)pos.Y + v_oset - 3, 32 - (h_oset * 2), 32 - v_oset); } }


        public Trampoline(Vector2 pos, Level root)
        {
            this.pos = pos;
            this.root = root;
            hurtful = true;
            pogoable = true;
            super_pogo = true;
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {
            if (flash)
            {
                flash_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (flash_timer > 0.1f)
                    flash = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, PositionRectangle, frame, Color.White);

            if (flash)
                spriteBatch.Draw(sprite, PositionRectangle, new Rectangle(frame.X + 32, frame.Y, frame.Width, frame.Height), Color.White * 0.4f);
        }

        public override void Damage(float damage)
        {
            flash = true;
            flash_timer = 0f;

            int particle_oset = rnd.Next(0, 5);

            root.AddFX(new MushroomFX(new Vector2(pos.X + 15, pos.Y + 18), sprite, root, new Vector2(-1.2f, -1.1f)));
            root.AddFX(new MushroomFX(new Vector2(pos.X + 17, pos.Y + 16), sprite, root, new Vector2(0.8f, -1.5f)));
            root.AddFX(new MushroomFX(new Vector2(pos.X + 14 + particle_oset, pos.Y + 18), sprite, root, new Vector2(0.2f, -1.3f)));
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, HitBox, Color.Red * 0.3f);
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(HitBox);
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return HitBox;
        }
    }

    public class GhostBlock : Enemy
    {
        private Rectangle frame;
        private Texture2D img;
        private Texture2D black;

        private Rectangle HitBox;

        private bool flash = true;
        private bool self_destruct = false;
        private float flash_timer = 0f;

        public GhostBlock(Vector2 pos, Level root, Rectangle frame)
        {
            this.pos = pos;
            this.root = root;
            this.frame = frame;
            hurtful = true;
            pogoable = false;

            HitBox = new Rectangle((int)pos.X, (int)pos.Y, 16, 16);
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return HitBox;
        }
        public override void LoadAssets(Texture2D sprite)
        {
            img = sprite;
            this.black = root.black;
        }
        public override void Update(GameTime gameTime)
        {
            if (flash)
            {
                flash_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (flash_timer >= 0.08f)
                {
                    flash = false;
                    flash_timer = 0f;
                    if (self_destruct)
                        root.RemoveEnemy(this);
                }
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (root.cutscene)
                flash = false;

            spriteBatch.Draw(img, HitBox, frame, Color.White);

            if (flash)
                spriteBatch.Draw(black, HitBox, Color.White * 0.6f);
        }
        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {

        }
        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(HitBox);
        }
        public override void FlashDestroy()
        {
            flash = true;
            self_destruct = true;
            flash_timer = 0f;
        }
    }


    // bosses and mini-bosses

    // tutorial
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
        private Rectangle wakeup_rectangle = new Rectangle(880, 960, 24, 32);
        new private TutorialLevel root;
        private float hp = 21f;
        private int max_hp = 21;
        private bool damaged = false;
        private float damaged_timer = 0f;
        private bool airborne = false;
        public bool shake = false;
        private Random rnd = new Random();
        private SleepFX sleepFX;
        public bool up = false;

        public BigSlime(Vector2 pos, Player player, TutorialLevel root)
        {
            this.pos = pos;
            this.player = player;
            this.root = root;
            pogoable = true;
            hurtful = true;
        }

        public Rectangle PositionRectangle
        { get { return new Rectangle((int)pos.X - 48, (int)pos.Y - 8, 96, 64); } }

        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X - 32, (int)pos.Y + 20, 64, 28); } }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
            sleepFX = new SleepFX(root.particle_img, root, this);
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
            root.GetBossHP(hp, max_hp);
            sleepFX.trail_off = true;
            sleepFX.Update(gameTime);

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
            else if (wakeup_ready && player.GetPos().X > wakeup_rectangle.X)
                root.WakeUpSlime(this, gameTime);
            else if (wakeup_ready)
                wakeup_ready = false;

            sleepFX.Update(gameTime);
            sleepFX.trail_off = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (shake)
            {
                Rectangle draw_rectangle = new Rectangle(PositionRectangle.X + (int)(rnd.Next(0, 6) - 3f) * 2,
                                                         PositionRectangle.Y + (int)(rnd.Next(0, 2) - 0.5f) * 2,
                                                         PositionRectangle.Width,
                                                         PositionRectangle.Height);
                spriteBatch.Draw(sprite, draw_rectangle, frame, Color.White);
            }
            else if (!damaged)
                spriteBatch.Draw(sprite, PositionRectangle, frame, Color.White);
            else
            {
                var temp = new Rectangle(frame.X, frame.Y + 128, frame.Width, frame.Height);
                spriteBatch.Draw(sprite, PositionRectangle, frame, Color.White * 0.76f);
                spriteBatch.Draw(sprite, PositionRectangle, temp, Color.White * 0.24f);
            }

            if (sleep)
                sleepFX.Draw(spriteBatch);
            else if (!sleepFX.trailed_off)
                sleepFX.Draw(spriteBatch);
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

        public override void Damage(float damage)
        {
            hp -= damage;
            damaged = true;
            damaged_timer = 0;

            if (hp <= 0)
            {
                //root.DefeatSime(this);
                root.SplitSlime(this);
                root.ResetBossHP();
            }
        }

        public void UpdateSleepFX(GameTime gameTime)
        {
            if (sleep)
                sleepFX.Update(gameTime);
            else if (!sleepFX.trailed_off)
            {
                sleepFX.Update(gameTime);
                sleepFX.trail_off = true;
            }
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

    public class BabySlime : Slime
    {
        TutorialLevel tut_root;

        public BabySlime(Vector2 pos, Level root, TutorialLevel tut_root) : base(pos, root)
        {
            this.tut_root = tut_root;

            sleep_possible = false;
            hdir = -Math.Sign(root.player.HitBox.X - pos.X - 8);
            if (hdir == 0)
                hdir = 1;
        }

        public override void Damage(float damage)
        {
            hp -= damage;
            if (hp <= 0)
            {
                root.RemoveEnemy(this);
                SlimeFX particle = new SlimeFX(new Vector2(PositionRectangle.X, PositionRectangle.Y), root.particle_img, root);
                root.AddFX(particle);
                tut_root.DefeatSime();
            }

            bounce_counter = 1;
            vsp = -1;
            hdir = -Math.Sign(root.player.HitBox.X - pos.X - 8);
            if (hdir == 0)
                hdir = 1;

            damaged = true;
            damaged_timer = 0;
        }
    }

    public class Lukas_Tutorial : Enemy
    {
        private Texture2D sprite;
        private Rectangle frame = new Rectangle(0, 0, 32, 32);
        private Player player;
        private float hp = 22;
        private int max_hp = 22;
        new private TutorialLevel root;

        private List<Lukas_Projectile> projectiles = new List<Lukas_Projectile>();

        // movement fields
        private float hsp = 0f;
        private float vsp = 0f;
        private int loc_one = 0;
        private int loc_two = 0;
        private int y_one = 0;
        private int y_two = 0;
        private bool teleported = true;
        private bool hurt = false;
        private float hurt_timer = 0f;
        public bool sleep
        { get; set; } = true;
        private Rectangle wakeup_rectangle = new Rectangle(2400, 368, 32, 64);
        private bool wakeup_ready = false;

        // animation fields
        private float timer = 0f;
        private int frame_reset = 4;
        private bool left = false;
        private bool right = true;
        private bool flash = false;
        private float flash_timer = 0f;
        private bool teleporting = false;
        private float teleport_threshhold = 2.2f;
        private bool teleport_flash = false;
        private float teleport_flash_timer = 0f;

        // attack fields
        private float atk_timer = 1.4f;
        private bool attacking = false;
        private float atk_counter = 0f;

        public Rectangle PositionRectangle
        { get { return new Rectangle((int)pos.X, (int)pos.Y, 32, 32); } }

        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X + 11, (int)pos.Y + 8, 10, 18); } }

        public Lukas_Tutorial(Vector2 pos, Player player, TutorialLevel root)
        {
            this.pos = pos;
            this.player = player;
            this.root = root;
            pogoable = true;
            hurtful = false;

            loc_one = (int)pos.X + 20;
            loc_two = (int)pos.X - 100;

            y_one = (int)pos.Y;
            y_two = (int)pos.Y + 32;

            room = root.RealGetRoom(pos);
        }

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

        public void ActualUpdate(GameTime gameTime)
        {
            if (!root.GetRoom(player.GetPos() + new Vector2(16, 16)).Intersects(HitBox) && !teleporting)
                return;

            root.GetBossHP(hp, max_hp);

            float elapsed_time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            timer += elapsed_time;

            if (!hurt)
            {
                UpdateNormal(elapsed_time);
                AnimateNormal();
            }

            else
            {
                UpdateHurt(elapsed_time);
            }


            // animation stuff

            vsp = 0f;

            if (!teleporting)
            {
                frame.X = 32 * ((int)(timer * 10) % frame_reset);
                vsp = 0.1f * (float)Math.Sin(timer * 2) + 0.04f * Math.Sign(Math.Sin(timer * 2));
            }
                
            if (flash)
            {
                flash_timer += elapsed_time;
                if (flash_timer > 0.08f)
                    flash = false;
            }

            if (teleport_flash)
            {
                if (hurt)
                {
                    teleport_flash_timer = 0f;
                    teleport_flash = false;
                }
                else
                {
                    teleport_flash_timer += elapsed_time;
                    if (teleport_flash_timer > 0.08f)
                        teleport_flash = false;
                    frame.X = 256;
                }
            }

            float vsp_shift_down = 0f;

            if (hurt && hurt_timer < teleport_threshhold)
                vsp_shift_down = Math.Max(0, (y_two - pos.Y) / 10);

            pos.Y += vsp + vsp_shift_down;
            pos.X += hsp;

            for (int i = projectiles.Count - 1; i >= 0; i--)
                projectiles[i].Update(gameTime);
        }

        public void Sleep(GameTime gameTime)
        {
            if (player.HitBox.Intersects(wakeup_rectangle))
                wakeup_ready = true;
            else if (wakeup_ready && player.GetPos().X > wakeup_rectangle.X)
                root.FightLukas(this, gameTime);
            else if (wakeup_ready)
                wakeup_ready = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, PositionRectangle, frame, Color.White);

            if (flash && !teleporting)
                spriteBatch.Draw(sprite, PositionRectangle, new Rectangle(frame.X + 128, frame.Y, frame.Width, frame.Height), Color.White * 0.5f);

            if (flash && teleporting)
                spriteBatch.Draw(sprite, PositionRectangle, new Rectangle(frame.X, frame.Y + 32, frame.Width, frame.Height), Color.White * 0.5f);

            for (int i = projectiles.Count - 1; i >= 0; i--)
                projectiles[i].Draw(spriteBatch);
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
        }

        public override void Damage(float damage)
        {
            if (!hurt)
            {
                hurt = true;
                timer = 0f;
            }

            flash = true;
            flash_timer = 0f;

            hp -= damage;

            if (hp <= 0)
                Die();
        }

        // lukas-specific functions
        public void AddProjectile(float x, float y, string type, Rectangle room, bool dir)
        {
            var temp = new Lukas_Projectile(new Vector2(x, y), type, this, root, room, dir);
            temp.LoadAssets(sprite);
            projectiles.Add(temp);
            root.AddEnemy(temp);
        }

        public void RemoveProjectile(Lukas_Projectile proj)
        {
            projectiles.Remove(proj);
            root.RemoveEnemy(proj);
        }

        public Vector2 GetPlayerPos()
        {
            return player.GetPos();
        }

        private void UpdateNormal(float elapsed_time)
        {
            atk_timer += elapsed_time;

            if ((atk_timer > 2 && !attacking) || (atk_timer > 2.8 && attacking))
            {
                attacking = !attacking;
                atk_timer = 0;
                atk_counter = 0;
            }

            if (!attacking && atk_timer > 1.9 && !teleported)
                Teleport();

            if (attacking)
                Attack(0, atk_timer);
        }

        private void AnimateNormal()
        {
            right = Math.Abs(pos.X - loc_one) < Math.Abs(pos.X - loc_two);
            left = !right;


            if (attacking)
            {
                teleported = false;
                frame_reset = 6;
                frame.Y = 64;
            }

            else
            {
                frame_reset = 4;
                frame.Y = 0;
            }

            if (left)
                frame.Y += 32;

            Vector2 diff = GetPlayerPos() - pos + new Vector2(16, 8);

            if ((diff.X < 0 && left) || (diff.X > 0 && right))
                frame.Y += 160;
        }

        private void UpdateHurt(float elapsed_time)
        {
            hurt_timer += elapsed_time;
            frame.Y = 128;
            frame_reset = 4;

            // this function is kind of a mess
            // animation code and update code are mixed together here

            // -- teleporting: marker to check if currently doing teleport animation
            //                 not to be confused with teleportED which checks if Lukas already teleported in UpdateNormal
            //                 so he doesn't teleport every frame

            if (hurt_timer > 1.7f)
            {
                if (!teleporting)
                    timer = 0f;

                teleporting = true;
                frame.Y = 288;
                frame.X = 32 * ((int)(timer * 15) % 8);

                if (hurt_timer > teleport_threshhold && frame.X == 0)
                    TeleportOut();
            }

            if (hurt_timer > 2.7f)
            {
                Teleport();
                hurt_timer = 0;
                hurt = false;
                teleported = true;
                atk_timer = 2.5f;
                attacking = false;
                teleporting = false;

                frame.Y = 0;
                frame.X = 0;

                right = Math.Abs(pos.X - loc_one) < Math.Abs(pos.X - loc_two);
                left = !right;

                if (left)
                    frame.Y += 32;
            }
        }

        private void Attack(int type, float timer)
        {
            if (type == 0)
            {
                // aimed attacks
                if (timer > atk_counter)
                {
                    int offset = 22;
                    if (left)
                        offset = 22 - 26;
                    AddProjectile(pos.X + offset, pos.Y - 0, "aim", room.bounds, left);
                    atk_counter += 0.8f;
                }
                    
            }
        }

        private void Teleport()
        {
            Random rnd = new Random();

            if (Math.Abs(pos.X - loc_one) < Math.Abs(pos.X - loc_two))
                pos.X = loc_two + rnd.Next(-16, 32);
            else
                pos.X = loc_one + rnd.Next(-32, 16); 
            teleported = true;

            // timer = 0f;
            pos.Y = y_one;

            teleport_flash = true;
        }

        private void TeleportOut()
        {
            pos.Y = y_one - 200;
        }

        private void Die()
        {
            for (int i = projectiles.Count() - 1; i >= 0; i--)
                RemoveProjectile(projectiles[i]);

            root.ResetBossHP();
            root.RemoveEnemy(this);
        }


        // trivial functions
        public override Rectangle GetHitBox(Rectangle input)
        {
            return HitBox;
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(HitBox);
        }
    }

    public class Lukas_Projectile : Enemy 
    {
        private Texture2D sprite;
        private string type;
        private Lukas_Tutorial boss;

        private Rectangle frame = new Rectangle(212, 80, 12, 12);
        private float speed = 2f;
        private Vector2 move;
        private Vector2 diff;
        private Rectangle room_bounds;

        private bool flash = true;
        private float flash_timer = 0f;

        private bool backwards = true;
        private bool dir;

        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X, (int)pos.Y, 12, 12); } }

        public Lukas_Projectile(Vector2 pos, string type, Lukas_Tutorial boss, Level root, Rectangle room_bounds, bool dir) 
        {
            pogoable = false;
            destroy_projectile = false;

            this.pos = pos;
            this.type = type;
            this.root = root;
            this.boss = boss;
            this.room_bounds = room_bounds;
            this.dir = dir;

            Vector2 player_pos = boss.GetPlayerPos();
            diff = player_pos - pos + new Vector2(16, 8);
            diff = Vector2.Normalize(diff);

            if (type == "aim")
            {
                Vector2 initial_mov = new Vector2(-1, 1);
                if (dir)
                    initial_mov = new Vector2(1, 1);
                move = Vector2.Normalize(initial_mov) * speed * -1.1f;
            }
                
                
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (flash)
                spriteBatch.Draw(root.black, HitBox, Color.White);
            else
                spriteBatch.Draw(sprite, HitBox, frame, Color.White);
        }

        public override void Update(GameTime gameTime)
        {
            //var temp = root.SimpleCheckCollision(HitBox);

            //if (temp != null)
            if (!HitBox.Intersects(room_bounds))
                boss.RemoveProjectile(this);

            pos += move;

            if (type == "aim" && move.Length() < 5)
            {
                if (move.Length() < 0.05f)
                {
                    backwards = false;
                    Vector2 player_pos = boss.GetPlayerPos();
                    diff = player_pos - pos + new Vector2(16, 8);
                    diff = Vector2.Normalize(diff);
                }
                    
                if (backwards)
                {
                    Vector2 initial_mov = new Vector2(-1, 1);
                    if (dir)
                        initial_mov = new Vector2(1, 1);
                    move += Vector2.Normalize(initial_mov) * 0.08f;
                }
                    

                else
                    move += diff * 0.08f;
                    
            }

            if (flash)
            {
                flash_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (flash_timer > 0.08f)
                    flash = false;
            }
        }

        // obligatory
        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
        }
        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(HitBox);
        }
        public override Rectangle GetHitBox(Rectangle input)
        {
            return HitBox;
        }
    }

    // styx
    public class Kanna_Boss : Enemy
    {
        private Player player;
        private float hp = 24;
        private int max_hp = 24;
        new private StyxLevel root;
        private Texture2D sprite;

        private bool flash = false;
        private float flash_timer = 0f;
        private float flash_limit = 0.1f;
        private int state = 1;
        private float state_timer = 0f;
        private bool state_change = false;
        private bool cooldown = false;
        private bool short_cooldown = false;
        private float cooldown_timer = 0f;
        private int num_states = 3;
        private Random rd = new Random();
        private int player_dir = 1;
        private float hspeed = 2.3f;
        private int jmp_dst = 108;
        private float vsp = 0f;
        private float grav = 0.12f;
        private float grav_max = 5f;
        private float jmp_vsp = -3.7f;
        private float air_time;
        private float atk_zero_duration = 0.3f;
        private bool shots_fired = false;
        private float walk_timer = 0f;

        private bool triggered = false;
        private bool trigger_watch = false;
        public bool mask = true;

        private Rectangle frame = new Rectangle(256, 64, 32, 32);

        private int h_oset = 11;
        private int v_oset = 14;

        private GameTime gt_copy;

        public Rectangle PositionRectangle
        { get { return new Rectangle((int)pos.X, (int)pos.Y, 32, 32); } }

        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X + h_oset, (int)pos.Y + v_oset, 32 - (h_oset * 2), 32 - v_oset); } }

        public Kanna_Boss(Vector2 pos, Player player, StyxLevel root)
        {
            this.pos = pos;
            this.player = player;
            this.root = root;

            hurtful = false;

            // i have no idea how i came up with this LMAOOO
            air_time = jmp_vsp * jmp_vsp / grav;
        }



        public override void Update(GameTime gameTime)
        {
            gt_copy = gameTime;

            if (triggered && !root.prog_manager.kanna_defeated)
            {
                ActualUpdate(gameTime);

                return;
            }

            else if (!root.prog_manager.kanna_defeated)
            {
                if (player.HitBox.Intersects(root.kanna_trigger))
                    trigger_watch = true;

                else if (player.HitBox.Y > root.kanna_trigger.Y && trigger_watch)
                    root.FightKanna(this, gameTime);
            }

            else
            {
                //HandleFlash(gameTime);
                pogoable = false;
            }
        }

        private void ActualUpdate(GameTime gameTime)
        {
            root.GetBossHP(hp, max_hp);

            if (cooldown)
                Cooldown(gameTime);

            else
                CycleAttacks(gameTime);

            HandleFlash(gameTime);
            
        }

        private void CycleAttacks(GameTime gameTime)
        {
            if (state_change)
            {

                // change the state, save the player_dir, do attack specific setup
                state_change = false;

                shots_fired = false;

                int next_state = rd.Next(num_states);

                while (next_state == state)
                    next_state = rd.Next(num_states);

                state = next_state;

                // player_dir
                player_dir = Math.Sign(player.HitBox.X + (player.HitBox.Width / 2) - (HitBox.X + (HitBox.Width / 2)));

                if (player_dir == 0)
                    player_dir = 1;


                // atk specific setup
                if (state == 2)
                {
                    vsp = jmp_vsp;
                    if (!root.kanna_zone.Contains(new Vector2(pos.X + (jmp_dst * player_dir) + 16, pos.Y + 16)))
                        player_dir *= -1;
                }

                if (state == 0)
                {
                    if (!root.kanna_zone.Contains(new Vector2(pos.X + (hspeed * atk_zero_duration * 60 * player_dir * -1) + 16, pos.Y + 16)))
                        player_dir *= -1;
                }

            }



            if (state == 0)
                AtkZero(gameTime);
            else if (state == 1)
                AtkOne(gameTime);
            else if (state == 2)
                AtkTwo(gameTime);
        }

        private void AtkZero(GameTime gameTime)
        {

            /****** walk around ******/

            float dist = hspeed * (float)gameTime.ElapsedGameTime.TotalSeconds * 60 * player_dir * -1;

            //if (!root.kanna_zone.Contains(new Vector2(pos.X + dist, pos.Y)))
            //{
            //    player_dir *= -1;
            //    dist *= -1;
            //}
                

            pos.X += dist;

            // animate
            frame.X = 288;
            frame.Y = 0;

            if (player_dir == -1)
                frame.Y = 64;


            // change state
            state_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (state_timer > atk_zero_duration)
            {
                state_timer = 0f;
                state_change = true;
                cooldown = true;
                short_cooldown = true;
            }
        }

        private void AtkOne(GameTime gameTime)
        {
            /****** fire shots ******/

            frame.X = 320;
            frame.Y = 0;

            if (shots_fired)
                frame.X += 96;

            if (player_dir == -1)
                frame.Y = 64;

            state_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (state_timer > 0.4f && !shots_fired)
            {
                shots_fired = true;
                var shot = new Kanna_Projectile(new Vector2(HitBox.X + (HitBox.Width/2), HitBox.Y + (HitBox.Height/2) - 2), 
                                                root, 
                                                sprite, 
                                                player_dir == 1, 
                                                false
                                                );
                root.AddEnemy(shot);

            }

            if (state_timer > 0.86f)
            {
                state_timer = 0f;
                state_change = true;
                cooldown = true;
            }
        }

        private void AtkTwo(GameTime gameTime)
        {
            /****** jump & shoot ******/

            vsp += grav;

            if (vsp > grav_max)
                vsp = grav_max;

            float vsp_col_check = vsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            if (vsp_col_check > 0)
                vsp_col_check += 1;
            else
                vsp_col_check -= 1;

            Wall vcheck = root.SimpleCheckCollision(new Rectangle(HitBox.X, (int)(HitBox.Y + vsp_col_check), HitBox.Width, HitBox.Height));

            if (vcheck != null)
            {
                if (vsp < 0)
                    pos.Y = vcheck.bounds.Bottom - v_oset;
                else
                    pos.Y = vcheck.bounds.Top - PositionRectangle.Height;
                vsp = 0;

                // change state
                state_timer = 0f;
                state_change = true;
                cooldown = true;
            }

            pos.Y += vsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;

            pos.X += jmp_dst / air_time * player_dir * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;


            state_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (state_timer > 0.4f && !shots_fired)
            {
                shots_fired = true;

                int shot_xoset = - 4;
                if (player_dir == -1)
                    shot_xoset = 10;

                var shot = new Kanna_Projectile(new Vector2(HitBox.X + (HitBox.Width / 2) - shot_xoset, HitBox.Y + (HitBox.Height / 2) - 1),
                                                root,
                                                sprite,
                                                player_dir == 1,
                                                true
                                                );
                root.AddEnemy(shot);

            }


            // animate

            frame.X = 384;
            frame.Y = 0;

            if (shots_fired)
                frame.X += 96;

            if (player_dir == -1)
                frame.Y = 64;
            
            
        }

        private void Cooldown(GameTime gameTime)
        {
            frame.X = 256;
            frame.Y = 0;

            if (player_dir == -1)
                frame.Y = 64;

            cooldown_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (cooldown_timer > 0.57f || (cooldown_timer > 0.48f && short_cooldown))
            {
                cooldown_timer = 0f;
                cooldown = false;
            }
        }

        public void Trigger()
        {
            triggered = true;
            player_dir = Math.Sign(player.HitBox.X + (player.HitBox.Width / 2) - (HitBox.X + (HitBox.Width / 2)));
        }

        public void HandleFlash(GameTime gameTime)
        {
            if (flash)
            {
                flash_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (flash_timer > flash_limit)
                {
                    flash = false;
                    flash_timer = 0f;
                }
            }
        }


        // for cutscene animation stuff
        public void DoPhysics(GameTime gameTime)
        {
            vsp += grav;

            if (vsp > grav_max)
                vsp = grav_max;

            float vsp_col_check = vsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            if (vsp_col_check > 0)
                vsp_col_check += 1;
            else
                vsp_col_check -= 1;

            Wall vcheck = root.SimpleCheckCollision(new Rectangle(HitBox.X, (int)(HitBox.Y + vsp_col_check), HitBox.Width, HitBox.Height));

            if (vcheck != null)
            {
                if (vsp < 0)
                    pos.Y = vcheck.bounds.Bottom - v_oset;
                else
                    pos.Y = vcheck.bounds.Top - PositionRectangle.Height;
                vsp = 0;

                frame.X = 256;
                frame.Y = 0;

                player_dir = Math.Sign(player.HitBox.X + (player.HitBox.Width / 2) - (HitBox.X + (HitBox.Width / 2)));

                if (player_dir == -1)
                    frame.Y = 64;
            }

            pos.Y += vsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
        }

        public void DoWalk(GameTime gameTime)
        {
            pos.X += 1.6f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;

            frame.X = 256 + (32 * ((int)walk_timer % 8));
            frame.Y = 128;

            walk_timer += 14 * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        // end cutscene animation stuff

        public override void Damage(float damage)
        {
            if (!root.prog_manager.kanna_defeated)
            {
                hp -= damage;
                flash = true;

                if (hp < 8)
                {
                    root.RemoveArrows();
                    root.ResetBossHP();
                    root.DefeatKanna(this, gt_copy);

                    flash = false;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            int mask_oset = 0;

            if (!mask)
                mask_oset -= 256;

            Rectangle temp_frame = new Rectangle(frame.X + mask_oset, frame.Y, frame.Width, frame.Height);

            spriteBatch.Draw(sprite, PositionRectangle, temp_frame, Color.White);

            if (flash)
            {
                Rectangle flash_frame = new Rectangle(frame.X + mask_oset, frame.Y + 32, frame.Width, frame.Height);
                spriteBatch.Draw(sprite, PositionRectangle, flash_frame, Color.White * 0.5f);
            }
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);

            
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return HitBox;
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(HitBox);
        }
    }

    public class Kanna_Projectile : Enemy
    {
        private Texture2D img;

        private Rectangle frame_left = new Rectangle(64, 283, 13, 5);
        private Rectangle frame_right = new Rectangle(64, 267, 13, 5);
        private Rectangle frame_diag_left = new Rectangle(80, 264, 8, 8);
        private Rectangle frame_diag_right = new Rectangle(80, 280, 8, 8);

        private bool right;
        private bool diag;

        private float hspeed = 6f / 2;
        private float diagspeed = 5f / 2;

        private Rectangle bounds;


        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X, (int)pos.Y, 3, 3); } }

        public Rectangle DrawBox_Left
        { get { return new Rectangle((int)pos.X - 1, (int)pos.Y - 1, 13, 5); } }

        public Rectangle DrawBox_Right
        { get { return new Rectangle((int)pos.X - 9, (int)pos.Y - 1, 13, 5); } }

        public Rectangle DrawBox_DiagLeft
        { get { return new Rectangle((int)pos.X, (int)pos.Y - 5, 8, 8); } }

        public Rectangle DrawBox_DiagRight
        { get { return new Rectangle((int)pos.X - 5, (int)pos.Y - 5, 8, 8); } }

        public Kanna_Projectile(Vector2 pos, StyxLevel root, Texture2D img, bool right, bool diag)
        {
            this.pos = pos;
            this.root = root;
            this.img = img;
            this.right = right;
            this.diag = diag;

            //hurtful = false;
            destroy_projectile = false;
            pogoable = false;

            bounds = root.RealGetRoom(pos).bounds;
        }

        public override void LoadAssets(Texture2D sprite)
        {
            // nothing lol
        }

        public override void Update(GameTime gameTime)
        {

            float frame_factor = (float)gameTime.ElapsedGameTime.TotalSeconds * 60;

            if (!diag && right)
                pos.X += hspeed * frame_factor;

            else if (!diag && !right)
                pos.X += hspeed * -1 * frame_factor;

            else if (diag && right)
            {
                pos.X += diagspeed * frame_factor;
                pos.Y += diagspeed * frame_factor;
            }

            else if (diag && !right)
            {
                pos.X += diagspeed * -1 * frame_factor;
                pos.Y += diagspeed * frame_factor;
            }

            Wall check = root.SimpleCheckCollision(HitBox);

            if (check != null)
            {
                int fx_xpos = check.bounds.Right - 8;
                int fx_ypos = HitBox.Y - 7;
                if (right && !diag)
                    fx_xpos = check.bounds.Left - 8;
                if (diag)
                {
                    fx_xpos = HitBox.X;
                    fx_ypos = check.bounds.Top - 8;
                }


                RangedFX particle = new RangedFX(new Vector2(fx_xpos, fx_ypos), root.particle_img, root, true);

                root.AddFX(particle);


                root.RemoveEnemy(this);
            }

            if (!bounds.Contains(pos))
                root.RemoveEnemy(this);
                
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (diag && right)
                spriteBatch.Draw(img, DrawBox_DiagRight, frame_diag_right, Color.White);

            else if (!diag && right)
                spriteBatch.Draw(img, DrawBox_Right, frame_right, Color.White);

            else if (!diag && !right)
                spriteBatch.Draw(img, DrawBox_Left, frame_left, Color.White);

            else if (diag && !right)
                spriteBatch.Draw(img, DrawBox_DiagLeft, frame_diag_left, Color.White);
        }

        public override void OnDamage()
        {
            int xoset = 0;
            if (!right)
                xoset = -8;

            RangedFX particle = new RangedFX(new Vector2(pos.X + xoset, pos.Y - 7), root.particle_img, root, true);
            root.AddFX(particle);
            root.RemoveEnemy(this);
        }









        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return HitBox;
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(HitBox);
        }
    }

    public class Mushroom_Boss : Enemy
    {
        /*
         * BOSS OVERVIEW:
         *  - this boss is a GIANT MUSHROOM with one hand
         *  - it attacks by slamming its hand into the ground, releasing a SHOCK WAVE towards the player
         *  - after it slams the player can POGO off the hand like a TRAMPOLINE
         *  - the player can POGO off the boss itself to deal damage
         *  - if the player is above the boss too long, it releases a SPORE CLOUD
         *  - the player will have to POGO on top of the boss to hurt it
         *    and then back off when it releases SPORES
         */


        private Player player;
        private float hp = 24;
        private int max_hp = 24;
        new private StyxLevel root;
        private Texture2D sprite;
        private Random rnd;
        private Rectangle frame = new Rectangle(72, 128, 72, 72);

        private Mushroom_Hand right_hand;
        private Mushroom_Body body;

        private bool sleep = true;
        private bool trigger_watch = false;

        private float atk_timer = 0f;
        private float atk0_threshold = 2.7f;
        private float atk1_threshold = 1.8f;
        private float atk999_threshold = 3f;
        private float atk0_smash_threshold = 0.6f;
        private float atk999_spore_threshold = 1f;
        private int atk = 0;
        private int dir = 1;
        private bool state_change = false;
        private int move_dist = 72;
        private bool smashed = false;
        private bool hand_right = true;

        private int h_oset = 9;
        private int v_oset = 16;
        private int[] spore_osets = { 10, 20, 30, 40, 50, 60 };
        private int spore_index = 0;
        private int spore_index_old = 0;

        public Rectangle PositionRectangle
        { get { return new Rectangle((int)pos.X, (int)pos.Y, 72, 72); } }

        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X + h_oset, (int)pos.Y + v_oset, 72 - (h_oset * 2), 16); } }

        public Mushroom_Boss(Vector2 pos, Player player, StyxLevel root)
        {
            this.pos = pos;
            this.player = player;
            this.root = root;

            hurtful = true;
            super_pogo = true;

            right_hand = new Mushroom_Hand(new Vector2(pos.X + 72, pos.Y + 40), player, root);
            body = new Mushroom_Body(new Vector2(pos.X, pos.Y), player, root);
            root.AddEnemy(right_hand);
            root.AddEnemy(body);

            rnd = new();
        }


        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {

            body.SetPosX(pos.X + 0);
            body.SetPosY(pos.Y + 8);


            if (sleep)
            {
                Sleep(gameTime);
                return;
            }





            // *** not asleep, boss fight time!!! ***

            if (state_change)
            {
                state_change = false;
                smashed = false;
                atk_timer = 0f;

                CalculateDir();

                if (player.GetPos().Y < HitBox.Y && PositionRectangle.Contains(new Vector2(player.GetPos().X + (player.DrawBox.Width / 2), HitBox.Y + 4)))
                {
                    // spore attack
                    atk = 999;
                }

                else
                {
                    if (atk == 1)
                        atk = 0;
                    else
                        atk = 1;
                }


                if (atk == 1 && !root.mushroom_zone.Contains(new Rectangle(HitBox.X + (dir * move_dist),
                                                                           HitBox.Y, HitBox.Width, HitBox.Height)))
                {
                    dir *= -1;
                }
            }

            root.GetBossHP(hp, max_hp);

            atk_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (atk == 0)
                AtkZero(gameTime);
            if (atk == 1)
                AtkOne(gameTime);
            if (atk == 999)
                Atk999(gameTime);
        }

        private void CalculateDir()
        {
            if ((int)player.GetPos().X < (int)pos.X + 18)
                dir = -1;
            else if ((int)player.GetPos().X > (int)pos.X + 18)
                dir = 1;
            else
                dir = 0;
        }

        private void AtkZero(GameTime gameTime)
        {
            // SMASH !!!

            if (atk_timer > atk0_threshold)
            {
                state_change = true;
            }

            if (atk_timer > atk0_smash_threshold && !smashed)
            {
                smashed = true;

                right_hand.SetPosY(pos.Y + 40);

                var shock = new Shockwave(
                    new Vector2(right_hand.HitBox.X + (right_hand.HitBox.Width / 2), right_hand.HitBox.Y + right_hand.HitBox.Height - 10),
                    root,
                    sprite,
                    dir == 1
                    );

                root.AddEnemy(shock);
            }

            if (dir == -1)
                right_hand.SetPosX(pos.X - 32);
            else
                right_hand.SetPosX(pos.X + 72);


            if (smashed)
                right_hand.SetPosY(pos.Y + 40);
            else
                right_hand.SetPosY(pos.Y);

            if (smashed)
                frame.X = 72 * 2;
            else
                frame.X = 0;
        }

        private void AtkOne(GameTime gameTime)
        {
            // walk around

            if (atk_timer > atk1_threshold)
            {
                state_change = true;
            }

            pos.X += ((float)move_dist / (atk1_threshold * 60)) * dir * 60 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            frame.X = 72;



            if ((int)player.GetPos().X < (int)pos.X + 08 && hand_right)
                hand_right = false;

            if ((int)player.GetPos().X > (int)pos.X + 28 && !hand_right)
                hand_right = true;


            if (!hand_right)
                right_hand.SetPosX(pos.X - 32);
            else
                right_hand.SetPosX(pos.X + 72);

            right_hand.SetPosY(pos.Y + 10);
        }

        private void Atk999(GameTime gameTime)
        {
            // spore attack

            if (atk_timer > atk999_threshold)
            {
                state_change = true;
            }

            if (atk_timer > atk999_spore_threshold)
            {
                if ((int)(atk_timer * 60) % 10 == 0)
                {

                    while(spore_index == spore_index_old)
                        spore_index = rnd.Next(spore_osets.Length);

                    spore_index_old = spore_index;

                    var spore = new Mushroom_Spore(
                        new Vector2(pos.X + spore_osets[spore_index], pos.Y + v_oset),
                        root,
                        sprite
                    );

                    root.AddEnemy(spore);
                }
            }

            right_hand.SetPosX(pos.X + 18);
            right_hand.SetPosY(pos.Y + 18);
        }

        public void Sleep(GameTime gameTime)
        {
            if (player.HitBox.Intersects(root.mushroom_trigger))
                trigger_watch = true;

            else if (player.HitBox.X < root.mushroom_trigger.X && trigger_watch)
                sleep = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, PositionRectangle, frame, Color.White);
        }

        public override void Damage(float damage)
        {
            hp -= damage;
        }



        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
            //spriteBatch.Draw(blue, PositionRectangle, Color.Blue * 0.3f);
        }


        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(HitBox);
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return HitBox;
        }

        
    }

    public class Mushroom_Body : Enemy
    {
        private Player player;
        new private StyxLevel root;
        private Texture2D sprite;

        private int h_oset = 11;
        private int v_oset = 32;

        public Rectangle PositionRectangle
        { get { return new Rectangle((int)pos.X, (int)pos.Y, 72, 72); } }

        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X + h_oset, (int)pos.Y + v_oset, 72 - (h_oset * 2), 64 - v_oset); } }

        public Mushroom_Body(Vector2 pos, Player player, StyxLevel root)
        {
            this.pos = pos;
            this.player = player;
            this.root = root;

            hurtful = true;
            super_pogo = false;
        }


        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {
            // throw new NotImplementedException();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // throw new NotImplementedException();
        }

        public void SetPosX(float x)
        {
            pos.X = x;
        }

        public void SetPosY(float y)
        {
            pos.Y = y;
        }




        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
            //spriteBatch.Draw(blue, PositionRectangle, Color.Blue * 0.3f);
        }


        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(HitBox);
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return HitBox;
        }


    }

    public class Mushroom_Hand : Enemy
    {
        private Player player;
        new private StyxLevel root;
        private Texture2D sprite;

        private int h_oset = 5;
        private int v_oset = 3;

        private Rectangle frame = new Rectangle(192, 0, 32, 32);

        public Rectangle PositionRectangle
        { get { return new Rectangle((int)pos.X, (int)pos.Y, 32, 32); } }

        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X + h_oset, (int)pos.Y + v_oset, 32 - (h_oset * 2), 32 - v_oset); } }

        public Mushroom_Hand(Vector2 pos, Player player, StyxLevel root)
        {
            this.pos = pos;
            this.player = player;
            this.root = root;

            hurtful = true;
            super_pogo = true;
        }


        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, PositionRectangle, frame, Color.White);
        }

        public void SetPosX(float x)
        {
            pos.X = x;
        }

        public void SetPosY(float y)
        {
            pos.Y = y;
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(HitBox);
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return HitBox;
        }


    }

    public class Mushroom_Spore : Enemy
    {

        private Texture2D img;
        private Rectangle bounds;

        private float speed = -1.7f;

        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X - 3, (int)pos.Y - 3, 6, 6); } }

        public Mushroom_Spore(Vector2 pos, StyxLevel root, Texture2D img)
        {
            this.pos = pos;
            this.root = root;
            this.img = img;

            hurtful = true;
            destroy_projectile = false;
            pogoable = false;

            bounds = root.RealGetRoom(pos).bounds;
        }


        public override void Update(GameTime gameTime)
        {
            pos.Y += speed * 60 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!bounds.Contains(HitBox))
            {
                root.RemoveEnemy(this);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // nothing (for now)
        }

        public override void LoadAssets(Texture2D sprite)
        {
            // nothing lol
        }


        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return HitBox;
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(HitBox);
        }
    }



    // generic projectiles
    public class Shockwave : Enemy
    {
        private Texture2D img;
        private Rectangle bounds;
        private bool right;
        private float hspeed = 6f / 2;

        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X, (int)pos.Y, 16, 10); } }

        public Shockwave(Vector2 pos, StyxLevel root, Texture2D img, bool right)
        {
            this.pos = pos;
            this.root = root;
            this.img = img;
            this.right = right;

            hurtful = true;
            destroy_projectile = false;
            pogoable = false;

            bounds = root.RealGetRoom(pos).bounds;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // nothing (yet)
        }

        public override void Update(GameTime gameTime)
        {
            float frame_factor = (float)gameTime.ElapsedGameTime.TotalSeconds * 60;

            if (right)
                pos.X += hspeed * frame_factor;

            else
                pos.X += hspeed * -1 * frame_factor;

            Wall check = root.SimpleCheckCollision(HitBox);

            if (check != null)
            {
                root.RemoveEnemy(this);
            }

            if (!bounds.Contains(HitBox))
            {
                root.RemoveEnemy(this);
            }
        }

        public override void LoadAssets(Texture2D sprite)
        {
            // nothing lol
        }






        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return HitBox;
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(HitBox);
        }
    }

    // miscellaneous/weird cases

    public class DeadGuy : Enemy
    {
        Rectangle loc;
        Texture2D sprite;
        Rectangle frame = new Rectangle(192, 64, 32, 32);
        Rectangle hitbox;
        ProgressionManager progMan;

        DialogueStruct[] dialogue_deadguy;

        int counter = 0;

        public DeadGuy(Rectangle loc, DialogueStruct[] dialogue_deadguy, ProgressionManager progMan, Level root)
        {
            this.loc = loc;
            hurtful = false;
            pogoable = true;
            hitbox = new Rectangle(loc.X + 8, loc.Y + 16, 13, 16);
            this.dialogue_deadguy = dialogue_deadguy;
            this.root = root;
            this.progMan = progMan;

            if (progMan.knife)
                frame.X += 32;
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {
            // nothing xd (for now)
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, loc, frame, Color.White);
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, hitbox, Color.Blue * 0.3f);
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(hitbox);
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return hitbox;
        }

        public override void Interact()
        {
            if (!progMan.knife)
            {
                root.StartDialogue(dialogue_deadguy, 0, 'c', 25f, true);
            }
            else if (counter == 1)
            {
                root.StartDialogue(dialogue_deadguy, 3, 'c', 25f, true);
                counter++;
                frame.X = 192 + 32;
            }
            else
            {
                root.StartDialogue(dialogue_deadguy, 5, 'c', 25f, true);
                frame.X = 192 + 32;
            }
                

        }

        public void GetKnife()
        {
            progMan.GetKnife();
            counter++;
            frame.X += 32;
        }
    }

    public class DeadGuyTwo : Enemy
    {
        Rectangle loc;
        Texture2D sprite;
        Rectangle frame = new Rectangle(240, 80, 32, 32);
        Rectangle hitbox;
        ProgressionManager progMan;

        DialogueStruct[] dialogue_deadguy;

        public DeadGuyTwo(Rectangle loc, DialogueStruct[] dialogue_deadguy, ProgressionManager progMan, Level root)
        {
            this.loc = loc;
            hurtful = false;
            pogoable = true;
            hitbox = new Rectangle(loc.X + 8, loc.Y + 16, 13, 16);
            this.dialogue_deadguy = dialogue_deadguy;
            this.root = root;
            this.progMan = progMan;
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {
            // nothing xd (for now)
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, loc, frame, Color.White);
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, hitbox, Color.Blue * 0.3f);
        }

        public override bool CheckCollision(Rectangle input)
        {
            return input.Intersects(hitbox);
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return hitbox;
        }

        public override void Interact()
        {
            if (!progMan.dash)
            {
                root.StartDialogue(dialogue_deadguy, 0, 'c', 25f, true);
                LearnDash();
            }
            else
            {
                root.StartDialogue(dialogue_deadguy, 7, 'c', 25f, true);
            }


        }

        public void LearnDash()
        {
            progMan.LearnDash();
        }
    }

    public class Lukas_Cutscene : Enemy
    {

        new private StyxLevel root;
        private Texture2D sprite;

        private float timer = 0f;
        private int frame_reset = 4;
        private int frame_base = 0;
        private string type;

        private Rectangle frame = new Rectangle(0, 352, 32, 32);

        public Rectangle PositionRectangle
        { get { return new Rectangle((int)pos.X, (int)pos.Y, 32, 32); } }

        public bool looking
        { get; set; } = false;

        public bool magic
        { get; set; } = false;

        public Lukas_Cutscene(Vector2 pos, StyxLevel root, String type)
        {
            this.pos = pos;
            this.root = root;
            this.type = type;

            hurtful = false;
            pogoable = false;
        }


        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {
            if (looking)
                frame_base = 128;
            else
                frame_base = 0;

            if (magic)
            {
                frame.Y = 384;
                frame_base = 0;
                frame_reset = 6;
            }
            else
            {
                frame.Y = 352;
                frame_reset = 4;
            }
                

            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            frame.X = frame_base + (32 * ((int)(timer * 10) % frame_reset));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, PositionRectangle, frame, Color.White);
        }

        public override bool CheckCollision(Rectangle input)
        {
            return PositionRectangle.Intersects(input);
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            //throw new NotImplementedException();
        }

        public override Rectangle GetHitBox(Rectangle input)
        {
            return PositionRectangle;
        }

        
    }

    public class Kanna_Cutscene : Enemy
    {
        public override bool CheckCollision(Rectangle input)
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

        public override Rectangle GetHitBox(Rectangle input)
        {
            throw new NotImplementedException();
        }

        public override void LoadAssets(Texture2D sprite)
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
