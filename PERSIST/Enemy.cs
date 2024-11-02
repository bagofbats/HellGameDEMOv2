using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
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
        public virtual void Damage()
        {
            // this method is only needed for attackable enemies
            // non-attackable enemies (e.g. projectiles) should not override this empty function
        }
        public virtual void Interact()
        {
            // this method is only needed for non-hurtful interactable enemies
            // regular enemies should not override this empty function.
        }

        protected Level root;
        public Room room { get; set; }
        protected Vector2 pos;
        public Vector2 Pos { get => pos; }
        public bool hurtful { get; protected set; } = true;
        public bool pogoable { get; protected set; } = true;
    }

    // regular enemies

    public class Slime : Enemy
    {
        Texture2D sprite;
        protected int hp = 4;
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

        public override void Damage()
        {
            if (disabled)
                return;

            root.Switch(room, two);
            damaged = true;
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

        public void SetDisabled(bool d)
        {
            disabled = d;
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
        private Rectangle wakeup_rectangle = new Rectangle(880, 960, 24, 32);
        new private TutorialLevel root;
        private int hp = 22;
        private int max_hp = 22;
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

        public override void Damage()
        {
            hp -= 1;
            damaged = true;
            damaged_timer = 0;

            if (hp == 0)
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

        public override void Damage()
        {
            hp -= 1;
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

        private List<Lukas_Projectile> projectiles = new List<Lukas_Projectile>();

        // movement fields
        private float hsp = 0f;
        private float vsp = 0f;

        // animation fields
        private float timer = 0f;
        private int frame_reset = 4;

        // attack fields
        private float atk_timer = 0f;
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

            room = root.RealGetRoom(pos);
        }

        public override void LoadAssets(Texture2D sprite)
        {
            this.sprite = sprite;
        }

        public override void Update(GameTime gameTime)
        {
            if (!root.GetRoom(player.GetPos() + new Vector2(16, 16)).Intersects(HitBox))
                return;

            float elapsed_time = (float)gameTime.ElapsedGameTime.TotalSeconds;
            timer += elapsed_time;
            atk_timer += elapsed_time;

            if (atk_timer > 3)
            {
                attacking = !attacking;
                atk_timer = 0;
                atk_counter = 0;
            }

            if (attacking)
            {
                frame_reset = 6;
                frame.Y = 64;
                Attack(0, atk_timer);
            } 

            else
            {
                frame_reset = 4;
                frame.Y = 0;
            } 
                

            frame.X = 32 * ((int)(timer * 10) % frame_reset);

            vsp = 0.1f * (float)Math.Sin(timer * 2) + 0.04f * Math.Sign(Math.Sin(timer * 2));

            pos.Y += vsp;
            pos.X += hsp;

            for (int i = projectiles.Count - 1; i >= 0; i--)
                projectiles[i].Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, PositionRectangle, frame, Color.White);

            for (int i = projectiles.Count - 1; i >= 0; i--)
                projectiles[i].Draw(spriteBatch);
        }

        public override void DebugDraw(SpriteBatch spriteBatch, Texture2D blue)
        {
            spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
        }

        public override void Damage()
        {
            
        }

        // lukas-specific functions
        public void AddProjectile(float x, float y, string type, Rectangle room)
        {
            var temp = new Lukas_Projectile(new Vector2(x, y), type, this, root, room);
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

        
        public void Attack(int type, float timer)
        {
            if (type == 0)
            {
                // aimed attacks
                if (timer > atk_counter)
                {
                    AddProjectile(pos.X + 22, pos.Y - 0, "aim", room.bounds);
                    atk_counter += 0.8f;
                }
                    
            }
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

        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X, (int)pos.Y, 12, 12); } }

        public Lukas_Projectile(Vector2 pos, string type, Lukas_Tutorial boss, Level root, Rectangle room_bounds) 
        {
            pogoable = false;
            this.pos = pos;
            this.type = type;
            this.root = root;
            this.boss = boss;
            this.room_bounds = room_bounds;

            Vector2 player_pos = boss.GetPlayerPos();
            diff = player_pos - pos + new Vector2(16, 8);
            diff = Vector2.Normalize(diff);

            if (type == "aim")
                move = Vector2.Normalize(new Vector2(-1, 1)) * speed * -1.1f;
                //move = diff * speed * -1.1f;
                
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
                    move += Vector2.Normalize(new Vector2(-1, 1)) * 0.08f;

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
}
