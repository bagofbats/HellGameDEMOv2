using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TiledCS;
using MonoGame.Extended;
using static System.Net.Mime.MediaTypeNames;
using MonoGame.Extended.BitmapFonts;

namespace PERSIST
{
    public class Level
    {
        protected Persist root;
        public Player player
        { get; protected set; }
        protected Camera cam;
        public Texture2D black
        { get; protected set; }
        //protected SpriteFont font;
        protected BitmapFont bm_font;
        public Texture2D particle_img
        { get; protected set; }
        protected Texture2D spr_ui;
        protected Texture2D spr_screenwipe;
        protected Texture2D spr_doorwipe;
        protected bool debug;
        protected ProgressionManager prog_manager;
        protected bool overlay = true;
        public string name
        { get; protected set; }

        protected Rectangle bounds;
        protected List<Chunk> chunks = new List<Chunk>();
        protected List<TiledData> tld = new List<TiledData>();
        protected List<Room> rooms = new List<Room>();
        protected List<Checkpoint> checkpoints = new List<Checkpoint>();
        protected List<Enemy> enemies = new List<Enemy>();
        protected List<Door> doors = new List<Door>();
        protected List<ParticleFX> particles = new List<ParticleFX>();
        protected List<Wall> special_walls = new List<Wall>();
        protected List<Rectangle> special_walls_bounds = new List<Rectangle>();
        protected List<String> special_walls_types = new List<String>();
        protected List<Vector2> enemy_locations = new List<Vector2>();
        protected List<String> enemy_types = new List<String>();

        protected bool player_dead = false;
        protected bool finish_player_dead = false;
        protected float dead_timer = 0;

        protected bool dialogue = false;
        protected DialogueStruct[] dialogue_txt;
        protected char dialogue_loc = 'c';
        protected int dialogue_num = 0;
        protected float dialogue_letter = 0;
        protected float dialogue_speed = 5f;
        protected bool dialogue_skippable = true;
        protected int opts_highlighted = 0;

        protected int boss_hp = 0;
        protected int boss_max_hp = 0;

        protected bool cutscene = false;
        protected float cutscene_timer = 0f;
        protected string[] cutscene_code;

        protected bool door_trans = false;
        protected Color door_trans_color = Color.Black;

        protected Rectangle screenwipe_rect = new Rectangle(0, 0, 960, 240);
        protected Rectangle door_trans_rect_1 = new Rectangle(0, 0, 400, 240);
        protected Rectangle door_trans_rect_2 = new Rectangle(0, 0, 400, 240);

        public DialogueStruct[] dialogue_checkpoint = {
            new DialogueStruct("The torch lights up at your presence.", 'd', Color.White, 'c', true)};

        public Level(Persist root, Rectangle bounds, Player player, List<TiledData> tld, Camera cam, ProgressionManager prog_manager, bool debug, string name)
        {
            this.root = root;
            this.player = player;
            this.bounds = bounds;
            this.tld = tld;
            this.cam = cam;
            this.debug = debug;
            this.prog_manager = prog_manager;
            this.name = name;

            for (int i = 0; i < bounds.Width; i += 320)
                for (int j = 0; j < bounds.Height; j += 240)
                    chunks.Add(new Chunk(new Rectangle(i - 32, j - 32, 320 + 64, 240 + 64)));
            this.name = name;
        }

        // -----------------------------------------------

        // override these in child classes
        // (only Update, Load, and Cutscene Handler have any code in the parent class -- the rest are blank functions to be overwritten)

        public virtual void Load(Texture2D spr_ui, string code="")
        {
            //font = root.Content.Load<SpriteFont>("pixellocale");
            black = root.Content.Load<Texture2D>("black");
            bm_font = root.Content.Load<BitmapFont>("fonts/pixellocale_bmp");
            spr_screenwipe = root.Content.Load<Texture2D>("sprites/spr_screenwipe");
            spr_doorwipe = root.Content.Load<Texture2D>("sprites/spr_doorwipe");


            this.spr_ui = spr_ui;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!player_dead)
                player.Update(gameTime);
            else
                player.UpdateDead(gameTime);

            if (player_dead || finish_player_dead)
                HandleDeath(gameTime);
                



            // cutscene nonsense
            if (cutscene)
            {
                cutscene_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                HandleCutscene("", gameTime);
            }


            for (int i = 0; i < checkpoints.Count(); i++)
                checkpoints[i].DontAnimate(gameTime);

            Checkpoint temp = CheckpointCheckCollision(player.HitBox);

            if (temp != null && (temp.visible || prog_manager.GetActiveCheckpoint() == null))
                prog_manager.SetActiveCheckpoint(temp);

            if (!dialogue && !cutscene)
            {
                for (int i = 0; i < enemies.Count(); i++)
                    enemies[i].Update(gameTime);

                for (int i = special_walls.Count - 1; i >= 0; i--)
                    special_walls[i].Update(gameTime);
            }

            for (int i = particles.Count - 1; i >= 0; i--)
                particles[i].Update(gameTime);

            if (prog_manager.GetActiveCheckpoint() != null)
                prog_manager.GetActiveCheckpoint().Animate(gameTime);

            // door nonsense idk
            for (int i = 0; i < doors.Count(); i++)
                if (doors[i] != null)
                    if (doors[i].location.Intersects(player.HitBox) && player.contManager.DOWN_PRESSED && !cutscene)
                    {
                        //player.EnterCutscene();
                        //cutscene = true;
                        //root.GoToLevel(doors[i].destination, doors[i].code);
                        HandleCutscene("door|" + doors[i].destination + "|" + doors[i].code, gameTime, true);
                    }
                        

            // camera following
            Rectangle current_room = GetRoom(new Vector2(player.DrawBox.X + 16, player.DrawBox.Y + 16));
            Vector2 camera_pos = cam.GetPos();
            Rectangle camera_room = GetRoom(camera_pos);

            if (current_room.Width == 0 || current_room.Height == 0)
            {
                // default case that (hopefully) never happens
                //int tempX = (player.PositionRectangle.X + 16) / 320;
                //int tempY = (player.PositionRectangle.Y + 16) / 240;
                cam.Follow(new Vector2(player.DrawBox.X - 160 + 16, player.DrawBox.Y - 120 + 16));
            }

            else
            {
                int tempX = player.DrawBox.X + 16 - 160;
                int tempY = player.DrawBox.Y + 16 - 120;

                tempX = Math.Clamp(tempX, current_room.X, current_room.X + current_room.Width - 320);
                tempY = Math.Clamp(tempY, current_room.Y, Math.Max(current_room.Y + current_room.Height - 240, current_room.Y));

                if (current_room != camera_room
                    || camera_pos.X > current_room.X + current_room.Width - 320
                    || camera_pos.Y > current_room.Y + current_room.Height - 240)
                    cam.TargetFollow(new Vector2(tempX, tempY)); // transitions between rooms
                else
                {
                    cam.Follow(new Vector2(tempX, tempY)); // panning within the room
                    cam.stable = true;
                }
                    
            }

            screenwipe_rect.Y = (int)cam.GetPos().Y;

            // dialogue stuff
            if (dialogue)
            {
                dialogue_letter += (float)gameTime.ElapsedGameTime.TotalSeconds * dialogue_speed;
                dialogue_letter = Math.Min(dialogue_letter, dialogue_txt[dialogue_num].text.Length);
            }
        }

        public virtual void Draw(SpriteBatch _spriteBatch)
        {
            // nothing xd
        }

        public virtual void ResetUponDeath()
        {
            // nothing xd
        }

        public virtual void Switch(Room r, bool two)
        {
            // nothing
        }

        public virtual void HandleDialogueOption(string opt_code, int choice)
        {
            // nothing
        }


        // comes pre-loaded with code to handle room transitions
        // override everything else
        public virtual void HandleCutscene (string code, GameTime gameTime, bool start=false)
        {
            if (start)
            {
                cutscene_code = code.Split('|');

                player.EnterCutscene();
                cutscene = true;
                cutscene_timer = 0f;

                if (cutscene_code[0] == "finish_door")
                {
                    //root.bbuffer_color = door_trans_color;
                    root.blackout = true;
                }
            }


            // transitions between levels
            if (cutscene_code[0] == "door")
            {
                int wipe_width = door_trans_rect_1.Width;

                door_trans = true;

                if (cutscene_timer > 0.7f)
                {
                    root.GoToLevel(cutscene_code[1], cutscene_code[2], "finish_door");
                    //player.ExitCutscene();
                    cutscene = false;
                    door_trans = false;
                }
                door_trans_rect_1.X = (int)(cam.GetPos().X - wipe_width + (516 * cutscene_timer));
                door_trans_rect_2.X = (int)(cam.GetPos().X + 320 - (516 * cutscene_timer));

                door_trans_rect_1.Y = (int)cam.GetPos().Y;
                door_trans_rect_2.Y = (int)cam.GetPos().Y;
            }

            if (cutscene_code[0] == "finish_door")
            {
                door_trans = true;

                if (cutscene_timer > 0.7f)
                {
                    player.ExitCutscene();
                    cutscene = false;
                    door_trans = false;
                }
                door_trans_rect_1.X = (int)(cam.GetPos().X - 128 - (516 * cutscene_timer));
                door_trans_rect_2.X = (int)(cam.GetPos().X + 48 + (516 * cutscene_timer));

                door_trans_rect_1.Y = (int)cam.GetPos().Y;
                door_trans_rect_2.Y = (int)cam.GetPos().Y;
            }
        }

        // -----------------------------------------------



        public void AddWall(Rectangle bounds)
        {
            Wall temp = new Wall(bounds);
            for (int i = 0; i < chunks.Count(); i++)
                if (bounds.Intersects(chunks[i].bounds))
                    chunks[i].AddWall(temp);
        }

        public void AddObstacle(Rectangle bounds)
        {
            Obstacle temp = new Obstacle(bounds);
            for (int i = 0; i < chunks.Count(); i++)
                if (bounds.Intersects(chunks[i].bounds))
                    chunks[i].AddObstacle(temp);
        }

        public Checkpoint AddCheckpoint(Rectangle bounds)
        {
            Checkpoint temp = new Checkpoint(bounds, this);
            checkpoints.Add(temp);
            for (int i = 0; i < chunks.Count(); i++)
                if (bounds.Intersects(chunks[i].bounds))
                    chunks[i].AddCheckpoint(temp);

            return temp;
        }

        public void AddFX(ParticleFX particle)
        {
            particles.Add(particle);
        }

        public void RemoveFX(ParticleFX particle)
        {
            particles.Remove(particle);
        }

        public void AddEnemy(Enemy enemy) 
        {
            enemies.Add(enemy);
        }

        public void RemoveEnemy(Enemy enemy)
        {
            enemies.Remove(enemy);
        }

        public void AddSpecialWall(Wall wall)
        {
            special_walls.Add(wall);
            for (int i = 0; i < chunks.Count(); i++)
                if (wall.bounds.Intersects(chunks[i].bounds))
                    chunks[i].AddWall(wall);
        }

        public void RemoveSpecialWall(Wall wall)
        {
            special_walls.Remove(wall);

            for (int i = 0; i < chunks.Count(); i++)
                if (bounds.Intersects(chunks[i].bounds))
                    chunks[i].RemoveWall(wall);
        }

        public Wall SimpleCheckCollision(Rectangle input)
        {
            for (int i = 0; i < chunks.Count(); i++)
            {
                if (chunks[i].bounds.Intersects(input))
                {
                    Wall temp = chunks[i].SimpleCheckCollision(input);
                    if (temp != null)
                        return temp;
                }
            }
            return null;
        }
        
        public Obstacle ObstacleCheckCollision(Rectangle input)
        {
            for (int i = 0; i < chunks.Count(); i++)
            {
                if (chunks[i].bounds.Intersects(input))
                {
                    Obstacle temp = chunks[i].ObstacleCheckCollision(input);
                    if (temp != null)
                        return temp;
                }
            }
            return null;
        }

        public Checkpoint CheckpointCheckCollision(Rectangle input)
        {
            for (int i = 0; i < chunks.Count(); i++)
            {
                if (chunks[i].bounds.Intersects(input))
                {
                    Checkpoint temp = chunks[i].CheckpointCheckCollision(input);
                    if (temp != null)
                        return temp;
                }
            }
            return null;
        }

        public (Wall, Wall, Wall, Wall, Wall) FullCheckCollision(Rectangle input)
        {
            Rectangle in_left = input;
            in_left.X -= 1;
            Rectangle in_right = input;
            in_right.X += 1;
            Rectangle in_up = input;
            in_up.Y -= 1;
            Rectangle in_down = input;
            in_down.Y += 1;

            Rectangle col_checker = new Rectangle(input.X - 1, input.Y - 1, input.Width + 2, input.Height + 2);

            Wall left = null;
            Wall right = null;
            Wall up = null;
            Wall down = null;
            Wall inside = null;

            for (int i = 0; i < chunks.Count(); i++)
            {
                if (col_checker.Intersects(chunks[i].bounds))
                {
                    (Wall ltemp, Wall rtemp, Wall utemp, Wall dtemp, Wall itemp) = chunks[i].FullCheckCollision(in_left, in_right, in_up, in_down, input);

                    if (ltemp != null)
                        left = ltemp;
                    if (rtemp != null)
                        right = rtemp;
                    if (utemp != null)
                        up = utemp;
                    if (dtemp != null)
                        down = dtemp;
                    if (itemp != null)
                        inside = itemp;

                    if (left != null && right != null && up != null && down != null)
                        break;
                }
            }

            return (left, right, up, down, inside);
        }

        public List<Enemy> CheckEnemyCollision(Rectangle input)
        {
            List<Enemy> ret = new List<Enemy>();
            for (int i = 0; i < enemies.Count(); i++)
                if (enemies[i] != null)
                    if (enemies[i].CheckCollision(input))
                        ret.Add(enemies[i]);

            return ret;
        }

        public List<Wall> ListCheckCollision(Rectangle input)
        {
            List<Wall> ret = new List<Wall>();

            for (int i = 0; i < chunks.Count(); i++)
                if (chunks[i].bounds.Intersects(input))
                {
                    var temp = chunks[i].ListCheckCollision(input);
                    foreach (Wall wall in temp)
                        ret.Add(wall);
                }

            return ret;
        }

        public Rectangle GetRoom(Vector2 input)
        {
            Rectangle room = new Rectangle(0, 0, 0, 0);
            foreach (Room r in rooms)
            {
                if (r.bounds.Contains(input.X, input.Y))
                    return r.bounds;
            }
            return room;
        }

        public Room RealGetRoom(Vector2 input)
        {
            foreach (Room r in rooms)
            {
                if (r.bounds.Contains(input.X, input.Y))
                    return r;
            }

            return null;
        }

        public void DrawTiles(SpriteBatch _spriteBatch, Texture2D tileset, Texture2D background)
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise, transformMatrix: cam.Transform);

            Rectangle source = bounds;
            _spriteBatch.Draw(background, bounds, source, Color.White);

            foreach (TiledData t in tld)
                foreach (TiledLayer l in t.map.Layers)
                    if (l.name == "background_lower")
                        DrawLayerOnScreen(_spriteBatch, l, t, tileset, cam);

            foreach (TiledData t in tld)
                foreach (TiledLayer l in t.map.Layers)
                    if (l.name == "background")
                        DrawLayerOnScreen(_spriteBatch, l, t, tileset, cam);

            for (int i = 0; i < checkpoints.Count(); i++)
                checkpoints[i].Draw(_spriteBatch);

            for (int i = enemies.Count - 1; i >= 0; i--)
                enemies[i].Draw(_spriteBatch);

            if (!player_dead && !door_trans)
                player.Draw(_spriteBatch);

            foreach (TiledData t in tld)
                foreach (TiledLayer l in t.map.Layers)
                    if (l.name == "tiles_lower")
                        DrawLayerOnScreen(_spriteBatch, l, t, tileset, cam);

            for (int i = special_walls.Count - 1; i >= 0; i--)
                special_walls[i].Draw(_spriteBatch);

            foreach (TiledData t in tld)
                foreach (TiledLayer l in t.map.Layers)
                    if (l.name == "tiles")
                        DrawLayerOnScreen(_spriteBatch, l, t, tileset, cam);

            for (int i = particles.Count - 1; i >= 0; i--)
                particles[i].Draw(_spriteBatch);

            if (debug)
            {
                player.DebugDraw(_spriteBatch, black);
                foreach (Chunk c in chunks)
                    if (c.bounds.Contains(player.HitBox.X, player.HitBox.Y))
                        c.Draw(_spriteBatch);

                for (int i = enemies.Count - 1; i >= 0; i--)
                    enemies[i].DebugDraw(_spriteBatch, black);

                for (int i = 0; i < doors.Count; i++)
                    _spriteBatch.Draw(black, doors[i].location, Color.Blue * 0.2f);
            }

            if (overlay)
            {
                int cam_x = (int)cam.GetPos().X;
                int cam_y = (int)cam.GetPos().Y;


                var overlay_rect = new Rectangle(cam_x, cam_y, 320, 12);
                _spriteBatch.Draw(black, overlay_rect, Color.Black);
                
                if (dialogue)
                {
                    var dialogue_rect = new Rectangle(cam_x, cam_y, 320, 48);
                    _spriteBatch.Draw(black, dialogue_rect, Color.Black);
                }
                else if (!(player_dead || finish_player_dead) && !cutscene)
                {
                    // draw the player's hp bar

                    (int hp, int max_hp) = player.GetHP();
                    int pos = 2;

                    for (int i = 0; i < hp; i++)
                    {
                        Rectangle heart_loc = new Rectangle(cam_x + pos, cam_y + 2, 10, 8);
                        _spriteBatch.Draw(spr_ui, heart_loc, new Rectangle(0, 0, 10, 8), Color.White);
                        pos += 11;
                    }

                    for (int i = hp; i < max_hp; i++)
                    {
                        Rectangle heart_loc = new Rectangle(cam_x + pos, cam_y + 2, 10, 8);
                        _spriteBatch.Draw(spr_ui, heart_loc, new Rectangle(11, 0, 10, 8), Color.White);
                        pos += 11;
                    }

                }

                if (boss_max_hp != 0 && !cutscene)
                    DrawBossHP(_spriteBatch, boss_hp, boss_max_hp);
            }

            if ((player_dead || finish_player_dead) && dead_timer > 0.36)
                _spriteBatch.Draw(spr_screenwipe, screenwipe_rect, Color.Black);

            if (door_trans)
            {
                _spriteBatch.Draw(spr_doorwipe, door_trans_rect_1, new Rectangle(0, 0, 400, 240), door_trans_color);
                _spriteBatch.Draw(spr_doorwipe, door_trans_rect_2, new Rectangle(0, 240, 400, 240), door_trans_color);
            }
                

            if (player_dead)
                player.DrawDead(_spriteBatch, dead_timer);

            _spriteBatch.End();
        }

        public void DrawText(SpriteBatch _spriteBatch)
        {
            if (!overlay || (cutscene && !dialogue))
                return;

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied,
                SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, transformMatrix: cam.Transform);

            var current_room = RealGetRoom(cam.GetPos());

            if (current_room != null && cam.stable && !(player_dead || finish_player_dead))


                if (dialogue)
                {
                    dialogue_loc = dialogue_txt[dialogue_num].loc;
                    char dialogue_type = dialogue_txt[dialogue_num].type;

                    // default is to left-justify text, no portrait
                    Vector2 textMiddlePoint = new Vector2(0, 0);
                    Vector2 textDrawPoint = new Vector2(cam.GetPos().X + 12, cam.GetPos().Y + 2);

                    if (dialogue_loc == 'c')
                    {
                        // center justify
                        textMiddlePoint = bm_font.MeasureString(dialogue_txt[dialogue_num].text) / 2;
                        // Vector2 textMiddlePoint = bm_font.MeasureString(dialogue_txt[dialogue_num].Substring(0, (int)dialogue_letter)) / 2;
                        textDrawPoint = new Vector2(cam.GetPos().X + 159, cam.GetPos().Y + 21);
                    }

                    textMiddlePoint.X = (int)textMiddlePoint.X;
                    textMiddlePoint.Y = (int)textMiddlePoint.Y;

                    if (dialogue_type == 'o')
                    {
                        dialogue_letter = dialogue_txt[dialogue_num].text.Length;

                        string[] opts = dialogue_txt[dialogue_num].text.Split('\n');
                        int opts_num = opts.Length;

                        opts_highlighted = player.HandleDialogueOptions(opts_highlighted, opts_num);

                        for (int i = 0; i < opts_num; i++)
                        {
                            int x_draw_offset = (int)((Vector2)bm_font.MeasureString("  ")).X + 1;

                            textDrawPoint = new Vector2(cam.GetPos().X + 12, cam.GetPos().Y + 2 + (12 * i));
                            Vector2 offset_textDrawPoint = new Vector2(cam.GetPos().X + 12 + x_draw_offset, cam.GetPos().Y + 2 + (12 * i));

                            if (i == opts_highlighted)
                                _spriteBatch.DrawString(bm_font, " > " + opts[i], textDrawPoint, dialogue_txt[dialogue_num].color, 0, textMiddlePoint, 1f, SpriteEffects.None, 0f);
                            else
                                _spriteBatch.DrawString(bm_font, opts[i], offset_textDrawPoint, Color.Gray, 0, textMiddlePoint, 1f, SpriteEffects.None, 0f);
                        }
                    }

                    else
                    {
                        _spriteBatch.DrawString(bm_font, dialogue_txt[dialogue_num].text.Substring(0, (int)dialogue_letter), textDrawPoint, dialogue_txt[dialogue_num].color, 0, textMiddlePoint, 1f, SpriteEffects.None, 0f);
                    }
                }
                

                else if (current_room.name != null)
                {
                    Vector2 textMiddlePoint = bm_font.MeasureString(current_room.name) / 2;
                    textMiddlePoint.X = (int)textMiddlePoint.X;
                    textMiddlePoint.Y = (int)textMiddlePoint.Y;

                    Vector2 textDrawPoint = new Vector2(cam.GetPos().X + 159, cam.GetPos().Y + 3);
                    _spriteBatch.DrawString(bm_font, current_room.name, textDrawPoint, Color.White, 0, textMiddlePoint, 1f, SpriteEffects.None, 0f);
                }

            _spriteBatch.End();
        }

        public void DrawBossHP(SpriteBatch _spriteBatch, int HP, int maxHP)
        {
            Rectangle bar_pos = new Rectangle((int)cam.GetPos().X + 10, (int)cam.GetPos().Y + 220, 300, 8);
            Rectangle health_pos = new Rectangle((int)cam.GetPos().X + 12, (int)cam.GetPos().Y + 222, 296 * HP / maxHP, 4);

            _spriteBatch.Draw(black, bar_pos, Color.Black);
            _spriteBatch.Draw(black, health_pos, Color.Red);
        }

        private void DrawLayerOnScreen(SpriteBatch spriteBatch, TiledLayer layer, TiledData t, Texture2D tileset, Camera cam)
        {
            if (layer.data == null)
                return;

            int bounds_xoset = t.location.X;
            int bounds_yoset = t.location.Y;

            int cam_x = ((int)cam.GetPos().X - bounds_xoset) / t.map.TileWidth;
            int cam_y = ((int)cam.GetPos().Y - bounds_yoset) / t.map.TileHeight * t.map.Width;

            int t_width = t.tst.TileWidth;
            int t_height = t.tst.TileHeight;

            for (int j = 0; j < 248 / t.map.TileHeight; j++)
                for (int i = 0; i < 328 / t.map.TileWidth; i++)
                {
                    int index = cam_x + i + cam_y + (j * t.map.Width);

                    if (index >= layer.data.Length || index < 0)
                        return;

                    int gid = layer.data[index];

                    if (gid == 0)
                        continue;

                    int tileFrame = gid - 1;
                    int column = tileFrame % t.tst.Columns;
                    int row = (int)Math.Floor(tileFrame / (double)t.tst.Columns);

                    int loc_x = (index % t.map.Width) * t.map.TileWidth;
                    int loc_y = (int)Math.Floor(index / (double)t.map.Width) * t.map.TileHeight;

                    loc_x += bounds_xoset;
                    loc_y += bounds_yoset;

                    Rectangle tile = new Rectangle(t_width * column, t_height * row, t_width, t_height);
                    Rectangle loc = new Rectangle(loc_x, loc_y, t_width, t_height);

                    spriteBatch.Draw(tileset, loc, tile, Color.White);
                }
        }

        public void HandleDeath(GameTime gameTime)
        {
            if (dead_timer == 0)
                player_dead = true;

            ResetBossHP();

            dead_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (dead_timer >= 0.7 && player_dead)
            {
                var active_check = prog_manager.GetActiveCheckpoint();

                if (active_check.root != this)
                {
                    root.SimpleGoToLevel(active_check.root);
                    active_check.root.MiddleHandleDeath();
                    active_check.root.ResetUponDeath();
                    root.blackout = true;
                }
                    
                if (active_check.sideways && !active_check.sideways_right)
                    player.SetPos(new Vector2(active_check.side_wall.bounds.Left - 23 + 16, active_check.box.Y));
                else if (active_check.sideways && active_check.sideways_right)
                    player.SetPos(new Vector2(active_check.side_wall.bounds.Right - 9 + 16, active_check.box.Y));
                else
                    player.SetPos(new Vector2(active_check.box.X + 8, active_check.box.Y));
                cam.SmartSetPos(new Vector2(active_check.box.X + 8, active_check.box.Y));
                player_dead = false;
                finish_player_dead = true;

                ResetUponDeath();
            }

            if (dead_timer >= 1.04)
            {
                player_dead = false;
                finish_player_dead = false;
                dead_timer = 0;
            }
            
            screenwipe_rect.Y = (int)cam.GetPos().Y;

            if (dead_timer > 0.36)
                screenwipe_rect.X = (int)(cam.GetPos().X - 960 + (32 * (dead_timer - 0.36) * 60));

        }

        public void MiddleHandleDeath()
        {
            dead_timer = 0.7f;
            player_dead = true;
            screenwipe_rect.Y = (int)cam.GetPos().Y;
        }

        public void PlayerGotoDoor(string code)
        {
            if (code != "")
            {
                for (int i = 0; i < doors.Count; i++)
                    if (doors[i].code == code)
                    {
                        Door dst = doors[i];
                        player.SetPos(new Vector2(dst.location.X + 6, dst.location.Y - 8));
                        cam.SmartSetPos(new Vector2(dst.location.X, dst.location.Y));
                        break;
                    }
            }
        }

        public void StartDialogue(DialogueStruct[] array, int start_index, char justification, float speed, bool skippable=true, bool lookforward=true)
        {
            dialogue = true;
            dialogue_txt = array;
            dialogue_num = start_index;
            dialogue_loc = justification;
            dialogue_speed = speed;
            dialogue_skippable = skippable;
            player.EnterDialogue();
        }

        public void AdvanceDialogue()
        {
            if (dialogue_letter < dialogue_txt[dialogue_num].text.Length)
            {
                if (dialogue_skippable)
                    dialogue_letter = dialogue_txt[dialogue_num].text.Length;
                return;
            }

            char dialogue_type = dialogue_txt[dialogue_num].type;
            if (dialogue_type == 'o')
            {
                HandleDialogueOption(dialogue_txt[dialogue_num].opt_code, opts_highlighted);
                return;
            }

            dialogue_num++;
            dialogue_letter = 0f;
            opts_highlighted = 0;

            if (dialogue_num >= dialogue_txt.Length || dialogue_txt[dialogue_num - 1].end)
            {
                player.LeaveDialogue();
                dialogue = false;
                return;
            }
        }

        public void GetBossHP(int HP, int HP_max)
        {
            boss_hp = HP;
            boss_max_hp = HP_max;
        }

        public void ResetBossHP()
        {
            boss_hp = 0;
            boss_max_hp = 0;
        }
    }


    public class Chunk
    {
        public Rectangle bounds
        { get; private set; }
        private Texture2D black;

        private List<Wall> walls = new List<Wall>();
        private List<Obstacle> obstacles = new List<Obstacle>();
        private List<Checkpoint> checkpoints = new List<Checkpoint>();

        public Chunk(Rectangle bounds)
        {
            this.bounds = bounds;
        }

        public void AddWall(Wall newWall)
        {
            walls.Add(newWall);
        }

        public void RemoveWall(Wall wall)
        {
            walls.Remove(wall);
        }

        public void AddObstacle(Obstacle newObstacle)
        {
            obstacles.Add(newObstacle);
        }

        public void AddCheckpoint(Checkpoint checkpoint)
        {
            checkpoints.Add(checkpoint);
        }

        public Wall SimpleCheckCollision(Rectangle input)
        {
            for (int i = 0; i < walls.Count(); i++)
                if (walls[i].bounds.Intersects(input))
                    return walls[i];

            return null;
        }

        public List<Wall> ListCheckCollision(Rectangle input)
        {
            List<Wall> ret = new List<Wall>();

            for (int i = 0; i < walls.Count(); i++)
                if (walls[i].bounds.Intersects(input))
                    ret.Add(walls[i]);

            return ret;
        }

        public Obstacle ObstacleCheckCollision(Rectangle input)
        {
            for (int i = 0; i < obstacles.Count(); i++)
                if (obstacles[i].bounds.Intersects(input))
                    return obstacles[i];

            return null;
        }

        public Checkpoint CheckpointCheckCollision(Rectangle input)
        {
            for (int i = 0; i < checkpoints.Count(); i++)
                if (checkpoints[i].HitBox.Intersects(input))
                    return checkpoints[i];

            return null;
        }

        public (Wall, Wall, Wall, Wall, Wall) FullCheckCollision(Rectangle in_left, Rectangle in_right, Rectangle in_up, Rectangle in_down, Rectangle in_inside)
        {
            Wall left = null;
            Wall right = null;
            Wall up = null;
            Wall down = null;
            Wall inside = null;

            for (int i = 0; i < walls.Count(); i++)
            {
                if (walls[i].bounds.Intersects(in_left)) left = walls[i];
                if (walls[i].bounds.Intersects(in_right)) right = walls[i];
                if (walls[i].bounds.Intersects(in_up)) up = walls[i];
                if (walls[i].bounds.Intersects(in_down)) down = walls[i];
                if (walls[i].bounds.Intersects(in_inside)) inside = walls[i]; 
            }

            return (left, right, up, down, inside);
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            foreach (Wall wall in walls)
                if (wall != null)
                    _spriteBatch.Draw(black, wall.bounds, Color.Blue * 0.2f);

            foreach (Obstacle obstacle in obstacles)
                if (obstacle != null)
                    _spriteBatch.Draw(black, obstacle.bounds, Color.Red * 0.2f);

            foreach (Checkpoint c in checkpoints)
                if (c != null)
                    _spriteBatch.Draw(black, c.HitBox, Color.Blue * 0.2f);
        }

        public void Load(Texture2D black)
        {
            this.black = black;
        }
    }

    public class Room
    {
        public Rectangle bounds
        { get; private set; }
        private List<Enemy> enemies = new List<Enemy>();

        public String name
        { get; private set; }

        public Room(Rectangle bounds, String name)
        { 
            this.bounds = bounds;
            this.name = name;
        }

        public void UpdateEnemies(GameTime gameTime)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
                enemies[i].Update(gameTime);
        }

        public void DrawEnemies(SpriteBatch _spriteBatch)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
                enemies[i].Draw(_spriteBatch);
        }

        public void AddEnemy(Enemy enemy)
        {
            enemies.Add(enemy);
        }

        public void RemoveEnemy(Enemy enemy)
        {
            enemies.Remove(enemy);
        }

        public void Resize(Rectangle input)
        {
            bounds = input;
        }
    }

    public class Wall
    {
        public Rectangle bounds
        { get; private set; }

        public Wall(Rectangle bounds)
        {
            this.bounds = bounds;
        }

        public virtual void Draw(SpriteBatch spriteBatch) { }

        public virtual void Update(GameTime gameTime) { }

        public virtual void Load(Texture2D img) { }

        public virtual void Damage() { }

        public virtual void FlashDestroy() { }
    }

    public class Obstacle
    {
        public Rectangle bounds
        { get; private set; }

        public Obstacle(Rectangle bounds)
        { this.bounds = bounds; }
    }

    public class SwitchBlock : Wall
    {
        private Level root;
        public Texture2D img { get; set; }
        private Texture2D black;
        private Rectangle draw_rectangle;
        private Rectangle frame = new Rectangle(48, 32, 16, 16);
        private bool flash = true;
        private bool self_destruct = false;
        private float flash_timer = 0f;

        public SwitchBlock(Rectangle bounds, Level root) : base(bounds)
        {
            this.root = root;
            draw_rectangle = bounds;
        }

        public override void Load(Texture2D img)
        {
            this.img = img;
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
                        root.RemoveSpecialWall(this);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (flash)
                spriteBatch.Draw(black, draw_rectangle, frame, Color.White);
            else
                spriteBatch.Draw(img, draw_rectangle, frame, Color.White);
        }

        public override void FlashDestroy()
        {
            flash = true;
            self_destruct = true;
            flash_timer = 0f;
        }
    }

    public class Breakable : Wall
    {
        private Level root;
        public Texture2D img { get; set; }
        private Rectangle frame = new Rectangle(24, 128, 8, 8);
        private int hp = 0;
        private bool damaged;
        private Random rnd = new Random();
        private Rectangle draw_rectangle;
        private float damaged_timer = 0f;

        public Breakable(Rectangle bounds, Level root) : base(bounds) 
        {
            this.root = root;
            draw_rectangle = bounds;
        }

        public override void Load(Texture2D img)
        {
            this.img = img;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (damaged)
            {
                draw_rectangle.X = bounds.X + (int)((rnd.Next(0, 2) - 0.5f) * 2);
                draw_rectangle.Y = bounds.Y + (int)((rnd.Next(0, 2) - 0.5f) * 2);
            }
            else
            {
                draw_rectangle.X = bounds.X;
                draw_rectangle.Y = bounds.Y;
            }
            spriteBatch.Draw(img, draw_rectangle, frame, Color.White);
        }

        public override void Update(GameTime gameTime)
        {
            frame.X = (8 * hp) + 24;

            if (damaged)
            {
                damaged_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (damaged_timer >= 0.1)
                {
                    damaged = false;
                    damaged_timer = 0f;
                    hp += 1;
                }
            }

            if (hp >= 3 && !damaged)
                root.RemoveSpecialWall(this);
        }

        public override void Damage()
        {
            damaged = true;
        }
    }

    public class BossBlock : Wall
    {
        private Level root;
        private Rectangle draw_rectangle;
        private Rectangle frame = new Rectangle(48, 112, 16, 16);
        private Texture2D img;
        private Texture2D black;
        private float spawn_timer = 0;
        private bool white = true;

        public BossBlock(Rectangle bounds, Level root) : base(bounds)
        {
            this.root = root;
            draw_rectangle = bounds;
        }

        public override void Load(Texture2D img)
        {
            this.img = img;
            this.black = root.black;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (white)
                spriteBatch.Draw(black, draw_rectangle, frame, Color.White);
            else
                spriteBatch.Draw(img, draw_rectangle, frame, Color.White);
        }

        public override void Update(GameTime gameTime)
        {
            if (white)
                spawn_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (spawn_timer > 0.08f)
                white = false;
        }
    }

    public class TiledData
    {
        public Rectangle location
        { get; private set; }

        public TiledMap map
        { get; private set; }

        public TiledTileset tst
        { get; private set; }

        public TiledData(Rectangle location, TiledMap map, TiledTileset tst)
        {
            this.location = location;
            this.map = map;
            this.tst = tst;
        }
    }

    public class Door
    {
        public Rectangle location { get; private set; }
        public string code { get; private set; }
        public string destination { get; private set; }

        public Door(Rectangle location, string code, string destination) 
        {
            this.location = location;
            this.code = code;
            this.destination = destination;
        }
    }

    public struct LevelStruct
    {
        public LevelStruct(string map, string tileset)
        {
            this.map = map;
            this.tileset = tileset;
        }

        public string map;
        public string tileset;
    }

    public struct DialogueStruct
    {
        public DialogueStruct(string text, char type, Color color, char loc='l', bool end=false, string opt_code="", int portrait_x =0, int portrait_y=0)
        {
            this.text = text;
            this.type = type;
            this.loc = loc;
            this.color = color;
            this.end = end;
            portrait = new Rectangle(portrait_x, portrait_y, 45, 45);
            this.opt_code = opt_code;
        }

        public string text;
        public char type;
        public char loc;
        public Rectangle portrait;
        public Color color;
        public bool end;
        public string opt_code;
    }
}
