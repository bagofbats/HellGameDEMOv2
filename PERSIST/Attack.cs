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
    public abstract class Attack
    {
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch _spriteBatch);
        public abstract void DebugDraw(SpriteBatch _spriteBatch, Texture2D blue);
    }

    public class Slash : Attack
    {
        private Player player;
        private char type;
        private Texture2D img;
        private Rectangle pos;
        private Rectangle frame = new Rectangle(0, 0, 32, 32);
        private float animateTimer = 0;
        private Level level;
        private bool pogoed = false;
        private float damage = 1f;

        private List<Enemy> enemies_struck = new List<Enemy>();
        private List<Breakable> specials = new List<Breakable>();

        public Slash(Player player, char type, Texture2D img, bool slash_dir, Level level, float damage)
        {
            this.player = player;
            this.type = type;
            this.img = img;
            this.level = level;
            this.damage = damage;
            pos = player.DrawBox;
            pos.X = player.HitBox.X + 16;
            pos.Y = player.HitBox.Y;
            if (type == 'l')
                frame.Y = 32;
            else if (type == 'u')
                frame.Y = 64;
            else if (type == 'd')
                frame.Y = 96;
            if (!slash_dir)
                frame.Y += 128;
        }

        public Rectangle HitBox
        {
            get
            {
                if (type == 'u')
                    return new Rectangle(pos.X + 2, pos.Y, 12, 20);
                else if (type == 'd')
                    return new Rectangle(pos.X + 2, pos.Y + 2, 12, 18);
                else if (type == 'l')
                    return new Rectangle(pos.X + 11, pos.Y + 1, 22, 14);
                else
                    return new Rectangle(pos.X - 1, pos.Y + 1, 22, 14);
            }
        }

        public override void Update(GameTime gameTime)
        {
            animateTimer += 50 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            frame.X = 32 * ((int)animateTimer % 9);
            // repositioning
            if (type == 'l')
            {
                pos.X = player.HitBox.X - 20 - player.HitBox.Width;
                pos.Y = player.HitBox.Y;
            }
            else if (type == 'u')
            {
                pos.X = player.HitBox.X - 1;
                pos.Y = player.HitBox.Y - 22;
            }
            else if (type == 'd')
            {
                pos.X = player.HitBox.X - 1;
                pos.Y = player.HitBox.Y + 14;
            }
            else
            {
                pos.X = player.HitBox.X + 16;
                pos.Y = player.HitBox.Y;
            }

            if (frame.X >= 256)
            {
                player.FinishAttack(this);
            }

            if (type == 'd')
            {
                List<Enemy> temp = level.CheckEnemyCollision(HitBox);
                if (temp.Count() != 0)
                {
                    if (!pogoed)
                    {
                        foreach (Enemy enemy in temp)
                            if (enemy.pogoable)
                            {
                                pogoed = true;
                                player.SetPogoed(temp[0].GetHitBox(HitBox).Y, true);
                                break;
                            }
                        
                    }
                    foreach (Enemy enemy in temp)
                    {
                        if (!enemies_struck.Contains(enemy))
                        {
                            enemies_struck.Add(enemy);
                            enemy.Damage(damage);
                        }
                    }
                }
            }
            else
            {
                List<Enemy> temp = level.CheckEnemyCollision(HitBox);
                foreach (Enemy enemy in temp)
                {
                    if (enemy != null && !enemies_struck.Contains(enemy))
                    {
                        enemies_struck.Add(enemy);
                        enemy.Damage(damage);
                    }
                }
            }

            List<Wall> temp_specials = level.ListCheckCollision(HitBox);

            foreach (Wall wall in temp_specials)
            {
                if (wall.GetType() == typeof(Breakable) && !specials.Contains(wall))
                {
                    specials.Add((Breakable)wall);
                    wall.Damage();

                    if (!pogoed && type == 'd')
                    {
                        pogoed = true;
                        player.SetPogoed(temp_specials[0].bounds.Y, true, true);
                    }
                }
            }
        }

        public override void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(img, pos, frame, Color.White);
        }

        public override void DebugDraw(SpriteBatch _spriteBatch, Texture2D blue)
        {
            _spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
        }
    }

    public class Ranged : Attack
    {
        private Player player;
        private Texture2D img;
        private int dir;
        private bool up;
        private Level level;
        private Rectangle frame = new Rectangle(0, 0, 16, 16);
        private Rectangle pos = new Rectangle(0, 0, 16, 16);
        private float up_X;
        private float timer = 0f;
        private float hspeed = 460f;
        private float up_hspeed = 30f;
        private float vspeed = -480f;
        private float grav = 20f;
        private float spin_timer = 0;
        private bool fx_created = false;
        private float damage = 1f;

        public Ranged(Player player, Texture2D img, int dir, bool up, Level level, float damage)
        {
            this.player = player;
            this.img = img;
            this.dir = dir;
            this.up = up;
            this.level = level;
            this.damage = damage;

            pos.X = player.DrawBox.X + 6;

            if (up)
                pos.X += 4 * dir;

            up_X = pos.X;
            pos.Y = player.DrawBox.Y + 16;

            if (dir == -1)
                frame.Y = 16;
            if (up)
                frame.X = 16;
        }

        public Rectangle HitBox
        { 
            get 
            {
                if (!up)
                    return new Rectangle(pos.X, pos.Y + 5, 12, 6);
                else
                    return new Rectangle(pos.X + 4, pos.Y + 4, 9, 9);
            } 
        }

        private void Finish()
        {
            RangedFX particle = new RangedFX(new Vector2(HitBox.X, HitBox.Y - 4), level.particle_img, level, !up);
            if (!fx_created)
            {
                level.AddFX(particle);
                fx_created = true;
            }
            
            player.FinishAttack(this);
        }

        public override void Update(GameTime gameTime)
        {
            if (!up)
            {
                float hsp = hspeed * dir * (float)gameTime.ElapsedGameTime.TotalSeconds;
                pos.X += (int)hsp;
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timer > 0.22f)
                    Finish();
                    

                Wall hcheck = level.SimpleCheckCollision(HitBox);
                if (hcheck != null)
                {
                    if (dir == -1)
                        pos.X = hcheck.bounds.Right - 7;
                    else
                        pos.X = hcheck.bounds.Left - 8;

                    Finish();
                }
            }

            else
            {
                float vsp = vspeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                float hsp = up_hspeed * dir * (float)gameTime.ElapsedGameTime.TotalSeconds;
                up_X += hsp;

                Wall vcheck = level.SimpleCheckCollision(new Rectangle(HitBox.X, HitBox.Y + (int)vsp, HitBox.Width, HitBox.Height));
                if (vcheck != null)
                {
                    if (vsp < 0)
                        pos.Y = vcheck.bounds.Bottom - 8;
                    else
                        pos.Y = vcheck.bounds.Top - 9;
                    Finish();
                }


                Wall hcheck = level.SimpleCheckCollision(new Rectangle((int)up_X, HitBox.Y, HitBox.Width, HitBox.Height));
                if (hcheck != null)
                {
                    if (hsp < 0)
                        pos.X = hcheck.bounds.Right - 7;
                    else
                        pos.X = hcheck.bounds.Left + 9;

                    hcheck.Damage();

                    Finish();
                }

                pos.Y += (int)vsp;
                pos.X = (int)up_X;

                vspeed += grav;

                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                spin_timer += 24 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                frame.X = 16 * ((int)spin_timer % 8);

                if (timer >= 1f)
                    Finish();
                    

            }

            List<Enemy> temp = level.CheckEnemyCollision(HitBox);
            if (temp.Count() != 0)
            {
                bool destroy_this = false;

                foreach (Enemy enemy in temp)
                {
                    enemy.Damage(damage);
                    if (enemy.destroy_projectile)
                        destroy_this = true;

                }

                if (destroy_this)
                    Finish();
            }


            List<Wall> temp_specials = level.ListCheckCollision(HitBox);

            foreach (Wall wall in temp_specials)
                if (wall.GetType() == typeof(Breakable))
                    wall.Damage();

            if (temp_specials.Count() != 0)
                Finish();

            Obstacle o = level.ObstacleCheckCollision(HitBox);
            if (o != null)
                Finish();
        }

        public override void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(img, pos, frame, Color.White);
        }

        public override void DebugDraw(SpriteBatch _spriteBatch, Texture2D blue)
        {
            _spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
        }

    }
}
