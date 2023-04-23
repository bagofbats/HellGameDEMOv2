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
        private ControllerManager contManager;
        private Level current_level;

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
        private int last_hdir;
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

        // animation fields
        private float width = 32; // scale factor for image
        private Rectangle frame = new Rectangle(0, 576, 32, 32);
        private Texture2D sheet;
        private float walk_timer = 0;
        private bool attacking = false;
        private bool atk_dir = true;
        private Texture2D spr_atk;

        public Player(Persist root, Vector2 pos, ControllerManager contManager)
        {
            this.root = root;
            this.pos = pos;
            this.contManager = contManager;
        }

        private float height { get { return frame.Height * width / frame.Width; } }
        public Rectangle DrawBox
        { get { return new Rectangle((int)pos.X, (int)pos.Y, (int)width, (int)height); } }
        public Rectangle HitBox
        { get { return new Rectangle((int)pos.X + 9, (int)pos.Y + 16, 14, 16); } }


        // core functions
        public void Load()
        {
            sheet = root.Content.Load<Texture2D>("spr_trigo_fullspritesheet");
            spr_atk = root.Content.Load<Texture2D>("spr_atk");
        }

        public void Update(GameTime gameTime)
        {
            GetInput();
            HandleMovementAndCollisions(gameTime);
            HandleAttacks();
            AnimateNormal(gameTime);

            for (int i = attacks.Count - 1; i >= 0; i--)
                attacks[i].Update(gameTime);
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(sheet, DrawBox, frame, Color.White);

            for (int i = attacks.Count - 1; i >= 0; i--)
                attacks[i].Draw(_spriteBatch);
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

        private void Die()
        {
            pos.X = current_level.active_checkpoint.box.X - 8;
            pos.Y = current_level.active_checkpoint.box.Y;
        }

        private void HandleMovementAndCollisions(GameTime gameTime)
        {
            (Wall left, Wall right, Wall up, Wall down) = current_level.FullCheckCollision(HitBox);

            wall_left = left != null;
            wall_right = right != null;
            wall_up = up != null;
            wall_down = down != null;

            wallslide = !wall_down && !wall_up && (wall_left || wall_right);


            // --------- death ---------
            Obstacle o = current_level.ObstacleCheckCollision(HitBox);

            if (o != null)
                Die();
            // --------- end death ---------


            // --------- wall jumping ---------
            if (wallslide && vsp > 0)
            {
                grav = 0.11f;
                grav_max = 2.3f;
                if (!old_wallslide)
                    vsp /= 4;
                if (vsp > grav_max)
                    vsp = grav_max;
            }
            else
            {
                grav = grav_default;
                grav_max = grav_max_default;
            }

            if (space_pressed && wallslide)
            {
                vsp = -4.2f;

                if (wall_right)
                    hoset = -3.0f;
                else
                    hoset = 3.0f;
            }
            // --------- end wall jumping ---------




            //  --------- vertical movement ---------
            if (vsp < grav_max)
                vsp += grav;

            if (space_pressed && wall_down)
                vsp = -4.2f;

            if (space_released && !wall_down)
                vsp /= 2;

            float vsp_col_check = vsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            if (vsp_col_check > 0)
                vsp_col_check += 1;
            else
                vsp_col_check -= 1;

            Wall vcheck = current_level.SimpleCheckCollision(new Rectangle(HitBox.X, (int)(HitBox.Y + vsp_col_check), HitBox.Width, HitBox.Height));

            if (vcheck != null)
            {
                if (vsp < 0)
                    pos.Y = vcheck.bounds.Bottom - 16;
                else if (vsp > 0)
                    pos.Y = vcheck.bounds.Top - 32;
                vsp = 0;
            }

            pos.Y += vsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            //  --------- end vertical movement ---------




            // --------- horizontal movement ---------
            hsp = (hsp_max * hdir) + (hoset * Math.Abs(Math.Sign(hoset) - hdir));

            if (wallslide && hoset == 0)
                hsp = 0;

            float hsp_col_check = hsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            if (hsp_col_check > 0)
                hsp_col_check += 1;
            else
                hsp_col_check -= 1;

            Wall hcheck = current_level.SimpleCheckCollision(new Rectangle((int)(HitBox.X + hsp_col_check), HitBox.Y, HitBox.Width, HitBox.Height));

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

        private void HandleAttacks()
        {
            if (enter_pressed && !attacking)
                StartAttack();
        }

        private void StartAttack()
        {
            attacking = true;
            Attack temp;
            char atk_type;

            if (up)
                atk_type = 'u';
            else if (down && !wall_down)
                atk_type = 'd';
            else if (last_hdir == -1)
                atk_type = 'l';
            else
                atk_type = 'r';

            temp = new Slash(this, atk_type, spr_atk, atk_dir, current_level);
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

        private void AnimateNormal(GameTime gameTime)
        {
            walk_timer += 14 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            int ydir = (int)(Convert.ToSingle(down) - Convert.ToSingle(up));

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

        public void SetCurrentLevel(Level newLevel)
        {
            current_level = newLevel;
        }
    }
}
