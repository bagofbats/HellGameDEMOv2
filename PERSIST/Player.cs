using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PERSIST
{
    public class Player
    {
        private Persist root;
        public ControllerManager contManager
        { get; private set; }
        private ProgressionManager progManager;
        // public Level current_level;

        // input fields
        private bool up;
        private bool down;
        private bool left;
        private bool right;
        private bool space;
        private bool enter;
        private bool space_released;
        private bool space_pressed;
        private bool enter_pressed;
        private bool enter_released;

        // gameplay fields
        private Vector2 pos;
        private float hsp = 0f;
        private float vsp = 0f;
        private int hdir;
        private int last_hdir = 1;
        private float hsp_max = 2.4f;
        private float hsp_max_default = 2.4f;
        private float grav = 0.211f;
        private float grav_default = 0.211f;
        private float grav_max = 5f;
        private float grav_max_default = 5f;
        private float hoset = 0f;
        private float hoset_decay = 0.2f;
        private bool wall_left = false;
        private bool wall_right = false;
        private bool wall_up = false;
        private bool wall_down = false;
        private bool wallslide = false;
        private bool old_wallslide = false;
        private List<Attack> attacks = new List<Attack>();
        private float ranged_timer = 0f;
        private float ranged_time = 0.36f;
        private bool ranged_ready = false;
        private float pogo_height = 70f;
        private float pogo_target = 0f;
        private float pogo_timer = 0f;
        private float pogo_time = 0.4f;
        private bool pogoed = false;
        private float pogo_float_timer = 0.05f;
        private float pogo_float = 1.1f;
        private float coyote_time = 0.08f;
        private float coyote_timer = 0f;
        private bool dialogue = false;

        // animation fields
        private float width = 32; // scale factor for image
        private Rectangle frame = new Rectangle(0, 576, 32, 32);
        private Texture2D sheet;
        private Texture2D spr_atk;
        private Texture2D spr_ranged;
        private float walk_timer = 0;
        private bool attacking = false;
        private bool atk_dir = true;
        private char atk_type = 'r';
        private bool thrown = false;
        private float thrown_timer = 0f;
        private float thrown_time = 0.2f;
        private float death_hsp = 0f;

        public Player(Persist root, Vector2 pos, ControllerManager contManager, ProgressionManager progManager)
        {
            this.root = root;
            this.pos = pos;
            this.contManager = contManager;
            this.progManager = progManager;
        }

        private float height { get { return frame.Height * width / frame.Width; } }
        public Rectangle DrawBox
        { get { return new Rectangle((int)pos.X, (int)pos.Y, (int)width, (int)height); } }
        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X + 9, (int)pos.Y + 16, 14, 16); } }
        public Rectangle HurtBox
        { get { return new Rectangle((int)pos.X + 12, (int)pos.Y + 16, 8, 14); } }


        // core functions
        public void Load()
        {
            sheet = root.Content.Load<Texture2D>("spr_trigo_fullspritesheet");
            spr_atk = root.Content.Load<Texture2D>("spr_atk");
            spr_ranged = root.Content.Load<Texture2D>("spr_atk_ranged");
        }

        public void Update(GameTime gameTime)
        {
            GetInput();

            if (!dialogue)
            {
                HandleMovementAndCollisions(gameTime);

                if (progManager.knife)
                {
                    if (progManager.ranged)
                    {
                        HandleAttacks(gameTime);
                        HandleThrown(gameTime);
                    }
                    else
                        HandleAttacksNoRanged(gameTime);
                }

                if (progManager.knife)
                {
                    if (attacking)
                        AnimateAtk(gameTime);
                    else
                        AnimateNormal(gameTime);
                }
                else
                    AnimateNoKnife(gameTime);
            }

            else
            {
                if (contManager.ENTER_PRESSED)
                    root.the_level.AdvanceDialogue();
            }

            for (int i = attacks.Count - 1; i >= 0; i--)
                attacks[i].Update(gameTime);
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(sheet, DrawBox, frame, Color.White);

            for (int i = attacks.Count - 1; i >= 0; i--)
                attacks[i].Draw(_spriteBatch);
        }

        public void DebugDraw(SpriteBatch _spriteBatch, Texture2D blue)
        {
            _spriteBatch.Draw(blue, HitBox, Color.Blue * 0.3f);
            _spriteBatch.Draw(blue, HurtBox, Color.Red * 0.5f);

            for (int i = attacks.Count - 1; i >= 0; i--)
                attacks[i].DebugDraw(_spriteBatch, blue);
        }

        public void DrawDead(SpriteBatch _spriteBatch, float timer)
        {
            frame.X = 0;

            if (last_hdir == 1)
                frame.Y = 1088;
            else
                frame.Y = 1120;

            pos.X += death_hsp;
            death_hsp /= 1.06f;

            if (timer < 0.2)
            {
                _spriteBatch.Draw(sheet, DrawBox, frame, Color.White);
                frame.X = 32;
                _spriteBatch.Draw(sheet, DrawBox, frame, Color.White * (30 * timer * timer));
            }
            else
            {
                frame.X = 32 + (32 * (int)((timer - 0.1) * 16));
                if (frame.X > 224)
                    frame.X = 224;
                _spriteBatch.Draw(sheet, DrawBox, frame, Color.White);
            }

        }

        public void EnterDialogue()
        {
            dialogue = true;
        }

        public void LeaveDialogue()
        {
            dialogue = false;
        }



        // helper functions
        private void GetInput()
        {
            up = contManager.UP;
            down = contManager.DOWN;
            left = contManager.LEFT;
            right = contManager.RIGHT;
            space = contManager.SPACE;
            enter = contManager.ENTER;

            space_pressed = contManager.SPACE_PRESSED;
            space_released = contManager.SPACE_RELEASED;
            enter_pressed = contManager.ENTER_PRESSED;
            enter_released = contManager.ENTER_RELEASED;

            hdir = (int)(Convert.ToSingle(right) - Convert.ToSingle(left));
            if (hdir != 0)
                last_hdir = hdir;
        }

        private void Die(GameTime gameTime)
        {
            root.the_level.HandleDeath(gameTime);
            death_hsp = -2.3f * last_hdir;
            if (last_hdir == 0)
                death_hsp = 2.3f;
            //pos.X = current_level.active_checkpoint.box.X - 8;
            //pos.Y = current_level.active_checkpoint.box.Y;

            for (int i = attacks.Count - 1; i >= 0; i--)
                FinishAttack(attacks[i]);

            hsp = 0;
            vsp = 0;
            SetPogoed(0, false);
        }

        private void HandleMovementAndCollisions(GameTime gameTime)
        {
            (Wall left, Wall right, Wall up, Wall down) = root.the_level.FullCheckCollision(HitBox);

            wall_left = left != null;
            wall_right = right != null;
            wall_up = up != null;
            wall_down = down != null;

            wallslide = !wall_down && !wall_up && (wall_left || wall_right);


            // --------- death ---------
            Obstacle o = root.the_level.ObstacleCheckCollision(HurtBox);
            List<Enemy> e = root.the_level.CheckEnemyCollision(HurtBox);

            if (o != null)
                Die(gameTime);
            else if (e.Count > 0)
                foreach (Enemy temp in e)
                    if (temp.hurtful)
                    {
                        Die(gameTime);
                        break;
                    }
            // --------- end death ---------


            // gravity stuff
            if (wallslide && vsp > 0)
            {
                grav = 0.11f;
                grav_max = 2.3f;
                if (!old_wallslide)
                    vsp /= 4;
                if (vsp > grav_max)
                    vsp = grav_max;
            }
            else if (pogo_float < pogo_float_timer)
            {
                pogo_float += (float)gameTime.ElapsedGameTime.TotalSeconds;
                grav = 0.11f;
            }
            else
            {
                grav = grav_default;
                grav_max = grav_max_default;
            }
            // end gravity stuff


            // --------- wall jumping ---------
            if (space_pressed && wallslide)
            {
                vsp = -4.2f;
                coyote_timer = coyote_time + 1;

                if (wall_right)
                    hoset = -3.0f;
                else
                    hoset = 3.0f;
            }
            // --------- end wall jumping ---------




            //  --------- vertical movement ---------
            if (wall_down)
                coyote_timer = 0;

            if (pogoed)
            {
                vsp = (pogo_target - pos.Y) / 8;
                if (vsp > -1)
                    SetPogoed(0, false);

                pogo_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (pogo_timer >= pogo_time)
                    SetPogoed(0, false);
            }
            else
            {
                if (space_released && !wall_down && vsp < 0)
                    vsp /= 2;

                if (vsp < grav_max)
                    vsp += grav;
            }
            
            if (space_pressed && (wall_down || coyote_timer <= coyote_time))
            {
                vsp = -4.2f;
                coyote_timer = coyote_time + 1;
            }

            if (!wall_down && coyote_timer <= coyote_time + 1)
                coyote_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            float vsp_col_check = vsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            if (vsp_col_check > 0)
                vsp_col_check += 1;
            else
                vsp_col_check -= 1;

            Wall vcheck = root.the_level.SimpleCheckCollision(new Rectangle(HitBox.X, (int)(HitBox.Y + vsp_col_check), HitBox.Width, HitBox.Height));

            if (vcheck != null)
            {
                if (vsp < 0)
                {
                    pos.Y = vcheck.bounds.Bottom - 16;
                    SetPogoed(0, false);
                }
                else if (vsp > 0)
                    pos.Y = vcheck.bounds.Top - 32;
                vsp = 0;
            }

            pos.Y += vsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            //  --------- end vertical movement ---------




            // --------- horizontal movement ---------
            if (ranged_ready)
                hsp_max = hsp_max_default * 0.48f;
            else
                hsp_max = hsp_max_default;

            hsp = (hsp_max * hdir) + (hoset * Math.Abs(Math.Sign(hoset) - hdir));

            if (wallslide && hoset == 0)
                hsp = 0;

            float hsp_col_check = hsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            if (hsp_col_check > 0)
                hsp_col_check += 1;
            else
                hsp_col_check -= 1;

            Wall hcheck = root.the_level.SimpleCheckCollision(new Rectangle((int)(HitBox.X + hsp_col_check), HitBox.Y, HitBox.Width, HitBox.Height));

            if (hcheck != null)
            {
                if (hsp > 0)
                    pos.X = hcheck.bounds.Left - 23;
                else if (hsp < 0)
                    pos.X = hcheck.bounds.Right - 9;
                hsp = 0;
            }

            pos.X += hsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            // --------- end horizontal movement ---------




            // prep for next cycle
            old_wallslide = wallslide;
            if (hoset < -1)
                hoset += hoset_decay;
            else if (hoset > 1)
                hoset -= hoset_decay;
            else
                hoset = 0f;
        }

        private void HandleAttacks(GameTime gameTime)
        {
            //if (enter_pressed && !attacking)
            //    StartAttack();

            if (enter)
                ranged_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (ranged_timer >= ranged_time)
                ranged_ready = true;

            if (enter_released && ranged_ready)
            {
                int ranged_dir = last_hdir;
                if (!wall_down && wall_left)
                    ranged_dir = 1;
                if (!wall_down && wall_right)
                    ranged_dir = -1;

                Attack temp = new Ranged(this, spr_ranged, ranged_dir, up, root.the_level);
                attacks.Add(temp);
                ranged_timer = 0;
                ranged_ready = false;
                thrown = true;
            }
            else if (enter_released && ! attacking)
            {
                StartAttack();
                ranged_timer = 0;
                ranged_ready = false;
            }
        }

        private void HandleAttacksNoRanged(GameTime gameTime)
        {
            if (enter_pressed && !attacking)
            {
                StartAttack();
            }
        }

        private void StartAttack()
        {
            attacking = true;
            Attack temp;

            // restrict attacks while on walls
            if (!wall_down && wall_right)
                atk_type = 'l';
            else if (!wall_down && wall_left)
                atk_type = 'r';
            else
            {
                if (up)
                    atk_type = 'u';
                else if (down && !wall_down)
                    atk_type = 'd';
                else if (last_hdir == -1)
                    atk_type = 'l';
                else
                    atk_type = 'r';
            }

            temp = new Slash(this, atk_type, spr_atk, atk_dir, root.the_level);
            attacks.Add(temp);
            atk_dir = !atk_dir;
        }

        public void FinishAttack(Attack atk)
        {
            if (atk.GetType() == typeof(Slash))
                attacking = false;
            attacks.Remove(atk);
            atk = null;
        }

        private void HandleThrown(GameTime gameTime)
        {
            if (thrown)
            {
                thrown_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (thrown_timer > thrown_time)
                {
                    thrown = false;
                    thrown_timer = 0;
                }
            }
        }

        public void SetPogoed(int victim_y, bool value, bool halved=false)
        {
            // helper function to handle transitions between pogo/not pogoing states
            pogoed = value;
            if (value)
            {
                pogo_target = Math.Max(victim_y - pogo_height, pos.Y - pogo_height + 16);
                pogo_timer = 0;
                if (halved)
                {
                    var temp = Math.Abs(pogo_target - pos.Y) / 3;
                    pogo_target += temp;
                }
                    
            }
            else
            {
                vsp = vsp / 2;
                pogo_float = 0f;
            }
        }

        public void SetPos(Vector2 new_pos)
        {
            pos = new Vector2(new_pos.X - 16, new_pos.Y);
        }

        private void AnimateNormal(GameTime gameTime)
        {
            walk_timer += 14 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            int ydir = (int)(Convert.ToSingle(down) - Convert.ToSingle(up));

            // special case for animating ranged attacks
            if (ranged_ready)
            {
                if (!wall_down)
                {
                    if (wall_left)
                    {
                        frame.X = 128;
                        frame.Y = 832;
                    }
                    else if (wall_right)
                    {
                        frame.X = 128;
                        frame.Y = 864;
                    }
                    else
                    {
                        frame.X = 128;
                        if (hdir == -1 || last_hdir == -1)
                            frame.Y = 800;
                        else
                            frame.Y = 768;
                    }
                    
                }
                else
                {
                    frame.X = 192;
                    if (hdir == -1 || last_hdir == -1)
                        frame.Y = 736;
                    else
                        frame.Y = 704;
                }
                return;
            }
            // ranged attacks cont.
            if (thrown)
            {
                if (!wall_down)
                {
                    if (wall_left)
                    {
                        frame.X = 160;
                        frame.Y = 832;
                    }
                    if (wall_right)
                    {
                        frame.X = 160;
                        frame.Y = 864;
                    }
                    else
                    {
                        frame.X = 160;
                        if (hdir == -1 || last_hdir == -1)
                            frame.Y = 800;
                        else
                            frame.Y = 768;
                    }
                    
                }
                else
                {
                    frame.X = 224;
                    if (hdir == -1 || last_hdir == -1)
                        frame.Y = 736;
                    else
                        frame.Y = 704;
                }
                return;
            }

            // on wall
            if (!wall_down && wall_right)
            {
                frame.X = 64;
                frame.Y = 608;
            }
            else if (!wall_down && wall_left)
            {
                frame.X = 64;
                frame.Y = 576;
            }

            // in air
            else if (!wall_down)
            {
                if (ydir == -1)
                    frame.X = 192;
                else if (ydir == 1)
                    frame.X = 224;
                else
                    frame.X = 96;

                if (hdir == -1)
                    frame.Y = 608;
                else if (hdir == 1)
                    frame.Y = 576;
                else if (last_hdir == -1)
                    frame.Y = 608;
                else
                    frame.Y = 576;
            }

            // walking
            else if (hsp != 0)
            {
                // last_hdir = hdir;
                frame.X = 32 * ((int)walk_timer % 8);
                if (hdir == -1)
                {
                    if (ydir == -1) { frame.Y = 320; }
                    else if (ydir == 1) { frame.Y = 352; }
                    else { frame.Y = 288; }
                }
                else
                {
                    if (ydir == -1) { frame.Y = 32; }
                    else if (ydir == 1) { frame.Y = 64; }
                    else { frame.Y = 0; }
                }
            }

            // standing
            else
            {
                // looking up/down
                if (ydir == 1) { frame.X = 160; }
                else if (ydir == -1) { frame.X = 128; }
                else { frame.X = 0; }

                // facing left/right
                if (last_hdir == -1) { frame.Y = 608; }
                else { frame.Y = 576; }
            }
        }

        private void AnimateAtk(GameTime gameTime)
        {
            walk_timer += 14 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // on wall
            if (!wall_down && wall_right)
            {
                frame.Y = 864;
                if (atk_dir)
                    frame.X = 192;
                else
                    frame.X = 224;
                
            }
            else if (!wall_down && wall_left)
            {
                frame.Y = 832;
                if (atk_dir)
                    frame.X = 192;
                else
                    frame.X = 224;
            }

            // in air
            else if (!wall_down)
            {
                // facing left
                if (atk_type == 'l' || last_hdir == -1)
                    frame.Y = 672;

                // facing right
                else
                    frame.Y = 640;

                if (atk_type == 'u')
                {
                    if ((atk_dir && last_hdir == -1) || (!atk_dir && last_hdir == 1))
                        frame.X = 160;
                    else
                        frame.X = 128;
                }
                else if (atk_type == 'd')
                {
                    if ((atk_dir && last_hdir == -1) || (!atk_dir && last_hdir == 1))
                        frame.X = 224;
                    else
                        frame.X = 192;
                }
                else if (atk_dir)
                {
                    if (down)
                        frame.X = 224;
                    else
                        frame.X = 96;
                }
                else
                {
                    if (down)
                        frame.X = 192;
                    else
                        frame.X = 64;
                }
            }

            // walking
            else if (hsp != 0)
            {
                frame.X = 32 * ((int)walk_timer % 8);
                if (atk_type == 'l')
                {
                    if (down)
                    {
                        if (atk_dir)
                            frame.Y = 544;
                        else
                            frame.Y = 512;
                    }
                    else
                    {
                        if (atk_dir)
                            frame.Y = 416;
                        else
                            frame.Y = 384;
                    }
                }
                else if (atk_type == 'r')
                {
                    if (down)
                    {
                        if (atk_dir)
                            frame.Y = 256;
                        else
                            frame.Y = 224;
                    }
                    else
                    {
                        if (atk_dir)
                            frame.Y = 128;
                        else
                            frame.Y = 96;
                    }
                }
                else if (atk_type == 'u')
                {
                    if (hdir == -1)
                    {
                        if (atk_dir)
                            frame.Y = 480;
                        else
                            frame.Y = 448;
                    }
                    else
                    {
                        if (atk_dir)
                            frame.Y = 160;
                        else
                            frame.Y = 192;
                    }
                }
                else if (atk_type == 'd')
                {
                    if (hdir == -1)
                    {
                        if (atk_dir)
                            frame.Y = 544;
                        else
                            frame.Y = 512;
                    }
                    else
                    {
                        if (atk_dir)
                            frame.Y = 256;
                        else
                            frame.Y = 224;
                    }
                }
            }

            // standing still
            else
            {
                if (atk_type == 'l')
                {
                    if (down)
                    {
                        frame.Y = 736;
                        if (atk_dir)
                            frame.X = 128;
                        else
                            frame.X = 160;
                    }
                    else
                    {
                        frame.Y = 672;
                        if (atk_dir)
                            frame.X = 32;
                        else
                            frame.X = 0;
                    }
                }
                else if (atk_type == 'r')
                {
                    if (down)
                    {
                        frame.Y = 704;
                        if (atk_dir)
                            frame.X = 128;
                        else
                            frame.X = 160;
                    }
                    else
                    {
                        frame.Y = 640;
                        if (atk_dir)
                            frame.X = 32;
                        else
                            frame.X = 0;
                    }

                }
                else if (atk_type == 'u')
                {
                    if (last_hdir == -1)
                        frame.Y = 736;
                    else
                        frame.Y = 704;

                    if (atk_dir)
                        frame.X = 32;
                    else
                        frame.X = 0;
                }
                else if (atk_type == 'd')
                {
                    if (last_hdir == -1)
                    {
                        frame.Y = 736;
                        if (atk_dir)
                            frame.X = 128;
                        else
                            frame.X = 160;
                    }
                    else
                    {
                        frame.Y = 704;
                        if (atk_dir)
                            frame.X = 128;
                        else
                            frame.X = 160;
                    }
                }
            }
        }

        private void AnimateNoKnife(GameTime gameTime)
        {
            walk_timer += 14 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            int ydir = (int)(Convert.ToSingle(down) - Convert.ToSingle(up));

            // on wall
            if (!wall_down && wall_left)
            { frame.X = 32; frame.Y = 768; }
            else if (!wall_down && wall_right )
            { frame.X = 32; frame.Y = 800; }

            // in air
            else if (!wall_down)
            {
                if (ydir == 1 && hdir == 1)
                { frame.X = 32; frame.Y = 832; }
                else if (ydir == -1 && hdir == 1)
                { frame.X = 0; frame.Y = 832; }
                else if (ydir == 1 && hdir == -1)
                { frame.X = 32; frame.Y = 864; }
                else if (ydir == -1 && hdir == -1)
                { frame.X = 0; frame.Y = 864; }
                else if (hdir == 1)
                { frame.X = 96; frame.Y = 768; }
                else if (hdir == -1)
                { frame.X = 96; frame.Y = 800; }
                else if (last_hdir == 1)
                {
                    if (ydir == 1) { frame.X = 32; frame.Y = 832; }
                    else if (ydir == -1) { frame.X = 0; frame.Y = 832; }
                    else { frame.X = 96; frame.Y = 768; }
                }
                else if (last_hdir == -1)
                {
                    if (ydir == 1) { frame.X = 32; frame.Y = 864; }
                    else if (ydir == -1) { frame.X = 0; frame.Y = 864; }
                    else { frame.X = 96; frame.Y = 800; }
                }
            }

            // walking
            else if (hsp != 0)
            {
                frame.X = 32 * ((int)walk_timer % 8);
                if (hdir == -1)
                {
                    if (ydir == -1) { frame.Y = 992; }
                    else if (ydir == 1) { frame.Y = 1056; }
                    else { frame.Y = 928; }
                }
                else
                {
                    if (ydir == -1) { frame.Y = 960; }
                    else if (ydir == 1) { frame.Y = 1024; }
                    else { frame.Y = 896; }
                }
            }

            // standing
            else
            {
                // looking up/down
                if (ydir == 1) { frame.X = 224; }
                else if (ydir == -1) { frame.X = 192; }
                else { frame.X = 0; }

                // facing left/right
                if (last_hdir == -1) { frame.Y = 800; }
                else { frame.Y = 768; }
            }
        }
    }
}
