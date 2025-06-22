using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
        private HellGame root;
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
        private bool shift;
        private bool space_released;
        private bool space_pressed;
        private bool enter_pressed;
        private bool enter_released;
        private bool shift_pressed;
        private bool shift_released;
        private bool down_pressed;
        private bool down_released;

        // state fields
        private bool dialogue = false;
        private bool cutscene = false;
        private bool pogoed = false;
        private bool attacking = false;
        private bool just_entered_dialogue = false;

        // gameplay fields
        private Vector2 pos;
        private float hsp = 0f;
        private float vsp = 0f;
        private int hdir;
        private int last_hdir = 1;
        private float hsp_max = 2.27f;
        private float hsp_max_default = 2.27f;
        private float hsp_ratio = 3 / 8f;
        private float hsp_ratio_default = 3 / 8f;
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
        private bool wall_inside = false;
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
        private float pogo_float_timer = 0.05f;
        private float pogo_float = 1.1f;
        private float coyote_time = 0.08f;
        private float coyote_timer = 0f;
        private int hp = 1;
        private int max_hp = 1;
        private int damage = 1;
        private float damage_multiplier = 1f;
        private bool just_pogoed = false;
        private bool one_way_inside = false;
        private bool one_way_down = false;
        private bool breakable_down = false;
        private bool dash_ready = false;
        private bool dashing = false;
        private float dash_timer = 0f;
        private float dash_limit = 0.14f;
        private float dash_multiplier = 2.4f;
        private float dash_multiplier_default = 2.4f;
        private float dash_decay = 0.87f;
        private int dash_dir = 0;
        private float dash_cooldown = 0.22f;
        private float dash_cooldown_over = 0.3f;

        // animation fields
        private float width = 32; // scale factor for image
        private Rectangle frame = new Rectangle(0, 576, 32, 32);
        private Texture2D sheet;
        private Texture2D spr_atk;
        private Texture2D spr_ranged;
        private float walk_timer = 0;
        private bool atk_dir = true;
        private char atk_type = 'r';
        private bool thrown = false;
        private float thrown_timer = 0f;
        private float thrown_time = 0.2f;
        private float death_hsp = 0f;
        private GameTime game_time;
        private Vector2 freeze_pos = new Vector2(0, 0);
        private bool dialogue_recent = false;

        public Player(HellGame root, Vector2 pos, ControllerManager contManager, ProgressionManager progManager)
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
        { get { return new Rectangle((int)pos.X + 11, (int)pos.Y + 16, 10, 15); } }


        // core functions
        public void Load()
        {
            sheet = root.Content.Load<Texture2D>("sprites/spr_trigo_fullspritesheet");
            spr_atk = root.Content.Load<Texture2D>("sprites/spr_atk");
            spr_ranged = root.Content.Load<Texture2D>("sprites/spr_atk_ranged");
        }

        public void Update(GameTime gameTime)
        {
            if (progManager.charons_blessing)
                damage_multiplier = 0.6f;
            else
                damage_multiplier = 1f;

            GetInput();

            game_time = gameTime;

            if (!dialogue && !cutscene)
            {
                HandleMovementAndCollisions(gameTime);

                if (!root.the_level.player_dead)
                    AtkTree(gameTime);

                AnimateTree(gameTime);
            }

            else if (dialogue)
            {
                if (contManager.ENTER_PRESSED || contManager.SPACE_PRESSED)
                    root.the_level.AdvanceDialogue();

                dialogue_recent = true;
            }

            if (dialogue_recent && contManager.ENTER_RELEASED)
                dialogue_recent = false;

            for (int i = attacks.Count - 1; i >= 0; i--)
                attacks[i].Update(gameTime);
        }

        public void UpdateDead(GameTime gameTime)
        {
            pos.X += death_hsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            death_hsp /= 1.06f;
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            for (int i = attacks.Count - 1; i >= 0; i--)
                attacks[i].Draw(_spriteBatch);

            _spriteBatch.Draw(sheet, DrawBox, frame, Color.White);
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

            if (timer < 0.2)
            {
                if (progManager.mask)
                    frame.X += 256;

                _spriteBatch.Draw(sheet, DrawBox, frame, Color.White);
                frame.X = 32;
                _spriteBatch.Draw(sheet, DrawBox, frame, Color.White * (30 * timer * timer));
            }
            else
            {
                int frame_xoset = 0;
                if (progManager.mask)
                    frame_xoset = 256;

                frame.X = 32 + (32 * (int)((timer - 0.1) * 16)) + frame_xoset;
                if (frame.X > 224 + frame_xoset)
                    frame.X = 224 + frame_xoset;
                _spriteBatch.Draw(sheet, DrawBox, frame, Color.White);
            }

        }

        public void EnterDialogue(bool lookforward=true)
        {
            dialogue = true;
            if (lookforward)
            {
                down = false;
                AnimateTree(game_time);
            }

            just_entered_dialogue = true;

        }

        public void LeaveDialogue()
        {
            dialogue = false;
        }



        // wrapper functions for cutscenes
        public void EnterCutscene()
        {
            cutscene = true;
        }

        public void ExitCutscene()
        {
            cutscene = false;
        }

        public void SetNoInput()
        {
            // for use in cutscenes
            up = false;
            down = false;
            left = false;
            right = false;
            space = false;
            enter = false;
            shift = false;


            space_pressed = false;
            space_released = false;
            enter_pressed = false;
            enter_released = false;
            shift_pressed = false;
            shift_released = false;
        }

        public void DoMovement(GameTime gameTime)
        {
            HandleMovementAndCollisions(gameTime);
        }

        public void DoAnimate(GameTime gameTime)
        {
            thrown = false;
            thrown_timer = 0;
            AnimateNormal(gameTime);
        }

        public void SetLastHdir(int last_hdir)
        {
            this.last_hdir = last_hdir;
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
            shift = contManager.SHIFT;


            space_pressed = contManager.SPACE_PRESSED;
            space_released = contManager.SPACE_RELEASED;
            enter_pressed = contManager.ENTER_PRESSED;
            enter_released = contManager.ENTER_RELEASED;
            shift_pressed = contManager.SHIFT_PRESSED;
            shift_released = contManager.SHIFT_RELEASED;
            down_pressed = contManager.DOWN_PRESSED;
            down_released = contManager.DOWN_RELEASED;
        }

        

        private void Die(GameTime gameTime)
        {
            root.the_level.HandleDeath(gameTime);

            freeze_pos = pos;

            death_hsp = -2.3f * last_hdir;
            if (last_hdir == 0)
                death_hsp = 2.3f;
            //pos.X = current_level.active_checkpoint.box.X - 8;
            //pos.Y = current_level.active_checkpoint.box.Y;

            for (int i = attacks.Count - 1; i >= 0; i--)
                FinishAttack(attacks[i]);

            hsp = 0;
            vsp = 0;
            hoset = 0;
            ranged_ready = false;
            ranged_timer = 0;
            SetPogoed(0, false);

            dashing = false;
            dash_timer = 0f;
        }

        private void HandleMovementAndCollisions(GameTime gameTime)
        {
            hdir = (int)(Convert.ToSingle(this.right) - Convert.ToSingle(this.left));
            if (hdir != 0)
                last_hdir = hdir;


            (Wall left, Wall right, Wall up, Wall down, Wall inside) = root.the_level.FullCheckCollision(HitBox);

            wall_left = left != null;
            wall_right = right != null;
            wall_up = up != null;
            wall_down = down != null;
            wall_inside = inside != null;

            wallslide = !wall_down && !wall_up && (wall_left || wall_right);


            // set flags for animation and attacking purposes
            if (wall_inside)
                one_way_inside = inside.one_way;
            else
                one_way_inside = false;

            if (wall_down)
                one_way_down = down.one_way;
            else
                one_way_down = false;

            if (wall_down)
                breakable_down = down.GetType() == typeof(Breakable);
            else
                breakable_down = false;

            if (wall_down || wallslide)
                dash_ready = true;


            // --------- death ---------
            Obstacle o = root.the_level.ObstacleCheckCollision(HitBox); // <---- changed to use hitbox instead of hurtbox
            List<Enemy> e = root.the_level.CheckEnemyCollision(HurtBox);

            if (o != null)
            {
                Die(gameTime);
                return;
            }
                
            else if (e.Count > 0)
                foreach (Enemy temp in e)
                    if (temp.hurtful && !just_pogoed)
                    {
                        temp.OnDamage();
                        Die(gameTime);
                        return;
                    }

            if (wall_inside)
                if (inside.GetType() == typeof(SwitchBlock))
                {
                    Die(gameTime);
                    return;
                }

            just_pogoed = false;
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
                    hoset = -2.9f;
                else
                    hoset = 2.9f;

                hsp_ratio = 1f;

                // for switch blocks in area 2
                root.the_level.JumpAction();
            }
            // --------- end wall jumping ---------



            // --------- dashing ---------

            if (progManager.dash && shift_pressed && dash_ready && dash_cooldown >= dash_cooldown_over)
            {
                dashing = true;
                dash_dir = last_hdir;

                if (wallslide && wall_left)
                    dash_dir = 1;
                else if (wallslide && wall_right)
                    dash_dir = -1;

                dash_ready = false;

                dash_multiplier = dash_multiplier_default;
            }


            if (dashing)
            {
                dash_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (dash_timer > dash_limit)
                {
                    // stop dashing
                    dashing = false;
                    dash_timer = 0f;

                    hoset = 2.1f * dash_dir;
                    hsp_ratio = 1f;
                }

                wallslide = false;

                dash_cooldown = 0;

                dash_multiplier *= dash_decay;
            }

            else if (dash_cooldown <= dash_cooldown_over)
                dash_cooldown += (float)gameTime.ElapsedGameTime.TotalSeconds;


            // --------- end ---------



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
                    vsp += grav * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            }
            
            if (space_pressed && (wall_down || coyote_timer <= coyote_time))
            {
                vsp = -4.2f;
                coyote_timer = coyote_time + 1;

                // for switch blocks in area 2
                root.the_level.JumpAction();
            }

            // ******** death (again) ********

            // this is for the switch blocks in area 2 so things don't get weird when u die inside them

            Wall temp_check = root.the_level.SimpleCheckCollision(HitBox);
            if (temp_check != null)
                if (temp_check.GetType() == typeof(SwitchBlock))
                {
                    Die(gameTime);
                    return;
                }
            // ******** end death (again) ********

            if (!wall_down && coyote_timer <= coyote_time + 1)
                coyote_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            float vsp_col_check = vsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            if (vsp_col_check > 0)
                vsp_col_check += 1;
            else
                vsp_col_check -= 1;

            Wall vcheck = root.the_level.SimpleCheckCollision(new Rectangle(HitBox.X, (int)(HitBox.Y + vsp_col_check), HitBox.Width, HitBox.Height), false);

            if (vcheck != null)
            {
                if (!vcheck.one_way)
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

                else
                {
                    if (inside == null)
                    {
                        if (vsp > 0)
                        {
                            pos.Y = vcheck.bounds.Top - 32;
                            vsp = 0;
                        }
                    }
                    else if (inside != vcheck && vsp > 0)
                    {
                        pos.Y = vcheck.bounds.Top - 32;
                        vsp = 0;
                    }
                }
                
            }

            // ******* special case for dashing ********
            if (dashing)
                vsp = 0;
            // ******* end special case for dashing *******

            pos.Y += vsp * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            //  --------- end vertical movement ---------

            


            // --------- horizontal movement ---------
            if (ranged_ready)
                hsp_max = hsp_max_default * 0.48f;
            else
                hsp_max = hsp_max_default;

            float hsp_abs = (hsp_max * hdir);
            hsp += hsp_abs * hsp_ratio * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;

            // reset hsp_ratio if it was changed for whatever reason
            // (right now only wall jumping has the power to do that)
            hsp_ratio = hsp_ratio_default;

            if (Math.Abs(hsp) > Math.Abs(hsp_abs))
                hsp = hsp_abs;

            float hsp_final = hsp + (hoset * Math.Abs(Math.Sign(hoset) - hdir));


            // ******** special case for dashing **********
            if (dashing)
                hsp_final = hsp_max * dash_dir * dash_multiplier;
            // ******** end special case for dashing ********

            if (wallslide && hoset == 0)
                hsp_final = 0;

            float hsp_col_check = hsp_final * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            if (hsp_col_check > 0)
                hsp_col_check += 1;
            else
                hsp_col_check -= 1;

            Wall hcheck = root.the_level.SimpleCheckCollision(new Rectangle((int)(HitBox.X + hsp_col_check), HitBox.Y, HitBox.Width, HitBox.Height));

            if (hcheck != null)
            {
                if (hsp_final > 0)
                    pos.X = hcheck.bounds.Left - 23;
                else if (hsp_final < 0)
                    pos.X = hcheck.bounds.Right - 9;
                hsp_final = 0;
                hsp = 0;
            }

            pos.X += hsp_final * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            // --------- end horizontal movement ---------




            // --------- interactable dialogue ---------

            if (e.Count > 0 && contManager.DOWN_PRESSED && wall_down && !contManager.SPACE_PRESSED)
                foreach (Enemy temp in e)
                    temp.Interact();

            List<Interactable> i = root.the_level.CheckInteractableCollision(HitBox);

            if (i.Count > 0 && contManager.DOWN_PRESSED && wall_down && !contManager.SPACE_PRESSED)
                i[0].Interact();

            Checkpoint C = root.the_level.CheckpointCheckCollision(HitBox);
            if (C != null && contManager.DOWN_PRESSED && wall_down && !contManager.SPACE_PRESSED && !contManager.LEFT && !contManager.RIGHT)
                C.Interact();

            if (!progManager.locks)
            {
                List<Key> keys = root.the_level.KeyCheckCollision(HitBox);
                if (keys.Count > 0 && contManager.DOWN_PRESSED && wall_down && !contManager.SPACE_PRESSED)
                    keys[0].Interact();
            }


            // --------- end interactable dialogue ---------


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

                Attack temp = new Ranged(this, spr_ranged, ranged_dir, up, root.the_level, damage * damage_multiplier);
                attacks.Add(temp);
                ranged_timer = 0;
                ranged_ready = false;
                thrown = true;
            }
            else if (enter_released && ! attacking && !dialogue_recent)
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
            //root.audioManager.PlaySound("atk");

            // restrict attacks while on walls
            if (!wall_down && wall_right && !up && !down)
                atk_type = 'l';
            else if (!wall_down && wall_left && !up && !down)
                atk_type = 'r';
            else
            {
                if (up)
                    atk_type = 'u';
                else if (down && (!wall_down || one_way_down || breakable_down))
                    atk_type = 'd';
                else if (last_hdir == -1)
                    atk_type = 'l';
                else
                    atk_type = 'r';
            }

            temp = new Slash(this, atk_type, spr_atk, atk_dir, root.the_level, damage * damage_multiplier);
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

        public void SetPogoed(int victim_y, bool value, bool halved=false, bool super_pogo = false)
        {
            // helper function to handle transitions between pogo/not pogoing states
            pogoed = value;
            if (value)
            {
                pogo_target = Math.Max(victim_y - pogo_height, pos.Y - pogo_height + 16);

                if (super_pogo)
                    pogo_target = Math.Max(victim_y - (pogo_height * 1.2f), pos.Y - (pogo_height * 1.2f) + 16);

                pogo_timer = 0;
                if (halved)
                {
                    var temp = Math.Abs(pogo_target - pos.Y) / 3;
                    pogo_target += temp;
                }

                just_pogoed = true;
                dash_ready = true;
                    
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

        public Vector2 GetPos(bool player_dead=false)
        {
            if (player_dead)
                return freeze_pos;

            return pos;
        }

        public (int, int) GetHP()
        {
            return (hp, max_hp);
        }

        public int HandleDialogueOptions(int option, int max_options)
        {
            // clause for when you interact with someone
            // and the first dialogue box is interactable
            // this prevents the down to interact from also doing a down to change dialogue option
            if (just_entered_dialogue)
            {
                just_entered_dialogue = false;
                return option;
            }

            if (contManager.DOWN_PRESSED)
            {
                option += 1;
                option = option % max_options;
            }
            else if (contManager.UP_PRESSED)
            {
                option -= 1;
                if (option < 0)
                    option = max_options - 1;
            }

            return option;
        }

        private void AtkTree(GameTime gameTime)
        {
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
        }

        private void AnimateTree(GameTime gameTime)
        {
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

        private void AnimateNormal(GameTime gameTime)
        {
            walk_timer += 14 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            int ydir = (int)(Convert.ToSingle(down) - Convert.ToSingle(up));



            if (dashing)
            {
                frame.X = 192;
                frame.Y = 1152;

                if (dash_dir == -1)
                    frame.Y += 32;

                if (progManager.mask)
                    frame.X += 256;

                return;
            }

            // special case for animating ranged attacks
            if (ranged_ready)
            {
                if (!wall_down)
                {
                    if (wall_left)
                    {
                        frame.X = 128;
                        frame.Y = 832;

                        if (up)
                        {
                            frame.X = 128;
                            frame.Y = 1152;
                        }
                    }
                    else if (wall_right)
                    {
                        frame.X = 128;
                        frame.Y = 864;

                        if (up)
                        {
                            frame.X = 128;
                            frame.Y = 1184;
                        }
                    }
                    else
                    {
                        if (!up)
                        {
                            frame.X = 128;
                            if (hdir == -1 || last_hdir == -1)
                                frame.Y = 800;
                            else
                                frame.Y = 768;
                        }
                        else
                        {
                            frame.X = 0;
                            if (hdir == -1 || last_hdir == -1)
                                frame.Y = 1184;
                            else
                                frame.Y = 1152;
                        }
                    }
                    
                }
                else
                {
                    if (!up)
                    {
                        frame.X = 192;
                        if (hdir == -1 || last_hdir == -1)
                            frame.Y = 736;
                        else
                            frame.Y = 704;
                    }
                    else
                    {
                        frame.X = 64;
                        if (hdir == -1 || last_hdir == -1)
                            frame.Y = 1184;
                        else
                            frame.Y = 1152;
                    }
                    
                }

                if (progManager.mask)
                    frame.X += 256;

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

                        if (up)
                        {
                            frame.X = 160;
                            frame.Y = 1152;
                        }
                    }
                    else if (wall_right)
                    {
                        frame.X = 160;
                        frame.Y = 864;

                        if (up)
                        {
                            frame.X = 160;
                            frame.Y = 1184;
                        }
                    }
                    else if (!up)
                    {
                        frame.X = 160;
                        if (hdir == -1 || last_hdir == -1)
                            frame.Y = 800;
                        else
                            frame.Y = 768;
                    }
                    else
                    {
                        frame.X = 32;
                        if (hdir == -1 || last_hdir == -1)
                            frame.Y = 1184;
                        else
                            frame.Y = 1152;
                    }
                    
                }
                else
                {
                    if (!up)
                    {
                        frame.X = 224;
                        if (hdir == -1 || last_hdir == -1)
                            frame.Y = 736;
                        else
                            frame.Y = 704;
                    }
                    else
                    {
                        frame.X = 96;
                        if (hdir == -1 || last_hdir == -1)
                            frame.Y = 1184;
                        else
                            frame.Y = 1152;
                    }
                    
                }

                if (progManager.mask)
                    frame.X += 256;

                return;
            }

            // on wall
            if (wall_right && (!wall_down || (one_way_down && vsp != 0)))
            {
                frame.X = 64;
                frame.Y = 608;

                if (ydir == -1)
                {
                    frame.X = 64;
                    frame.Y = 1248;
                }

                if (ydir == 1)
                {
                    frame.X = 96;
                    frame.Y = 1248;
                }
            }
            else if (wall_left && (!wall_down || (one_way_down && vsp != 0)))
            {
                frame.X = 64;
                frame.Y = 576;

                if (ydir == -1)
                {
                    frame.X = 64;
                    frame.Y = 1216;
                }

                if (ydir == 1)
                {
                    frame.X = 96;
                    frame.Y = 1216;
                }
            }

            // in air
            else if (!wall_down || (one_way_down && vsp != 0))
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

            // mask
            if (progManager.mask)
                frame.X += 256;
        }

        private void AnimateAtk(GameTime gameTime)
        {
            walk_timer += 14 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // on wall
            if (wall_right && (!wall_down || (one_way_down && vsp != 0)))
            {
                frame.Y = 864;
                if (atk_dir)
                    frame.X = 192;
                else
                    frame.X = 224;

                if (atk_type == 'u')
                {
                    frame.Y = 1248;
                    if (atk_dir)
                        frame.X = 128;
                    else
                        frame.X = 160;
                }

                else if (atk_type == 'd')
                {
                    frame.Y = 1248;
                    if (atk_dir)
                        frame.X = 192;
                    else
                        frame.X = 224;
                }

            }
            else if (wall_left && (!wall_down || (one_way_down && vsp != 0)))
            {
                frame.Y = 832;
                if (atk_dir)
                    frame.X = 192;
                else
                    frame.X = 224;

                if (atk_type == 'u')
                {
                    frame.Y = 1216;
                    if (atk_dir)
                        frame.X = 128;
                    else
                        frame.X = 160;
                }

                else if (atk_type == 'd')
                {
                    frame.Y = 1216;
                    if (atk_dir)
                        frame.X = 192;
                    else
                        frame.X = 224;
                }
            }

            // in air
            else if (!wall_down || (one_way_down && vsp != 0))
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

            // mask
            if (progManager.mask)
                frame.X += 256;
        }

        private void AnimateNoKnife(GameTime gameTime)
        {
            walk_timer += 14 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            int ydir = (int)(Convert.ToSingle(down) - Convert.ToSingle(up));

            // on wall
            if (wall_left && (!wall_down || (one_way_down && vsp != 0)))
            { 
                frame.X = 32; 
                frame.Y = 768; 

                if (ydir == -1)
                {
                    frame.X = 0;
                    frame.Y = 1216;
                }

                if (ydir == 1)
                {
                    frame.X = 32;
                    frame.Y = 1216;
                }
            }
            else if (wall_right && (!wall_down || (one_way_down && vsp != 0)))
            { 
                frame.X = 32; 
                frame.Y = 800;

                if (ydir == -1)
                {
                    frame.X = 0;
                    frame.Y = 1248;
                }

                if (ydir == 1)
                {
                    frame.X = 32;
                    frame.Y = 1248;
                }
            }

            // in air
            else if (!wall_down || (one_way_down && vsp != 0))
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
