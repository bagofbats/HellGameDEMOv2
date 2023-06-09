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

namespace PERSIST
{
    public class Level
    {
        private Persist root;
        public Player player
        { get; private set; }
        private Camera cam;
        private Texture2D black;
        public Texture2D particle_img
        { get; private set; }
        private Texture2D tst_tutorial;
        private Texture2D spr_slime;
        private Texture2D spr_screenwipe;
        private Texture2D bg_brick;
        private bool debug;
        public Checkpoint active_checkpoint
        { get; private set; }

        private Rectangle bounds;
        private List<Chunk> chunks = new List<Chunk>();
        private List<TiledData> tld = new List<TiledData>();
        private List<Room> rooms = new List<Room>();
        private List<Checkpoint> checkpoints = new List<Checkpoint>();
        private List<Enemy> enemies = new List<Enemy>();
        private List<ParticleFX> particles = new List<ParticleFX>();

        private bool player_dead = false;
        private bool finish_player_dead = false;
        private float dead_timer = 0;
        private Rectangle screenwipe_rect = new Rectangle(0, 0, 960, 240);

        public Level(Persist root, Rectangle bounds, Player player, List<TiledData> tld, Camera cam, bool debug)
        {
            this.root = root;
            this.player = player;
            this.bounds = bounds;
            this.tld = tld;
            this.cam = cam;
            this.debug = debug;

            for (int i = 0; i < bounds.Width; i += 320)
                for (int j = 0; j < bounds.Height; j += 240)
                    chunks.Add(new Chunk(new Rectangle(i - 32, j - 32, 320 + 64, 240 + 64)));

            foreach (TiledData t in tld)
                foreach (TiledLayer l in t.map.Layers)
                {
                    if (l.name == "walls")
                        for (int i = 0; i < l.objects.Count(); i++)
                            AddWall(new Rectangle((int)l.objects[i].x + t.location.X,
                                                  (int)l.objects[i].y + t.location.Y,
                                                  (int)l.objects[i].width,
                                                  (int)l.objects[i].height));

                    if (l.name == "rooms")
                        for (int i = 0; i < l.objects.Count(); i++)
                            rooms.Add(new Room(new Rectangle((int)l.objects[i].x + t.location.X,
                                                             (int)l.objects[i].y + t.location.Y,
                                                             (int)l.objects[i].width,
                                                             (int)l.objects[i].height)));

                    if (l.name == "obstacles")
                        for (int i = 0; i < l.objects.Count(); i++)
                            AddObstacle(new Rectangle((int)l.objects[i].x + t.location.X,
                                                  (int)l.objects[i].y + t.location.Y,
                                                  (int)l.objects[i].width,
                                                  (int)l.objects[i].height));

                    if (l.name == "entities")
                        for (int i = 0; i < l.objects.Count(); i++)
                        {
                            if (l.objects[i].name == "player")
                                player.SetPos(new Vector2(l.objects[i].x, l.objects[i].y));

                            if (l.objects[i].name == "checkpoint")
                                AddCheckpoint(new Rectangle((int)l.objects[i].x + t.location.X - 8, (int)l.objects[i].y + t.location.Y - 16, 16, 32));

                            if (l.objects[i].name == "slime")
                                AddEnemy(new Slime(new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y), this));
                        }
                            
                }
        }
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

        public void AddCheckpoint(Rectangle bounds)
        {
            Checkpoint temp = new Checkpoint(bounds);
            checkpoints.Add(temp);
            for (int i = 0; i < chunks.Count(); i++)
                if (bounds.Intersects(chunks[i].bounds))
                    chunks[i].AddCheckpoint(temp);
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

        public (Wall, Wall, Wall, Wall) FullCheckCollision(Rectangle input)
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

            for (int i = 0; i < chunks.Count(); i++)
            {
                if (col_checker.Intersects(chunks[i].bounds))
                {
                    (Wall ltemp, Wall rtemp, Wall utemp, Wall dtemp) = chunks[i].FullCheckCollision(in_left, in_right, in_up, in_down);

                    if (ltemp != null)
                        left = ltemp;
                    if (rtemp != null)
                        right = rtemp;
                    if (utemp != null)
                        up = utemp;
                    if (dtemp != null)
                        down = dtemp;

                    if (left != null && right != null && up != null && down != null)
                        break;
                }
            }

            return (left, right, up, down);
        }

        public List<Enemy> CheckEnemyCollision(Rectangle input)
        {
            List<Enemy> ret = new List<Enemy>();
            for (int i = 0; i < enemies.Count(); i++)
                if (enemies[i] != null && enemies[i].GetHitBox().Intersects(input))
                    ret.Add(enemies[i]);

            return ret;
        }

        public void Load()
        {
            black = root.Content.Load<Texture2D>("black");
            particle_img = root.Content.Load<Texture2D>("spr_particlefx");
            tst_tutorial = root.Content.Load<Texture2D>("tst_tutorial");
            spr_slime = root.Content.Load<Texture2D>("spr_slime");
            spr_screenwipe = root.Content.Load<Texture2D>("spr_screenwipe");
            bg_brick = root.Content.Load<Texture2D>("bg_brick2");

            foreach (Enemy enemy in enemies)
            {
                if (enemy.GetType() == typeof(Slime))
                    enemy.LoadAssets(spr_slime);
            }

            Texture2D checkpoint = root.Content.Load<Texture2D>("spr_checkpoint");

            for (int i = 0; i < chunks.Count(); i++)
                chunks[i].Load(black);

            for (int i = 0; i < checkpoints.Count(); i++)
                checkpoints[i].Load(checkpoint);
        }

        public void Update(GameTime gameTime)
        {
            if (!player_dead)
                player.Update(gameTime);
            if (player_dead || finish_player_dead)
                HandleDeath(gameTime);

            for (int i = 0; i < checkpoints.Count(); i++)
                checkpoints[i].DontAnimate(gameTime);

            Checkpoint temp = CheckpointCheckCollision(player.HitBox);

            for (int i = 0; i < enemies.Count(); i++)
                enemies[i].Update(gameTime);

            for (int i = particles.Count - 1; i >= 0; i--)
                particles[i].Update(gameTime);

            if (temp != null)
                active_checkpoint = temp;

            if (active_checkpoint != null)
                active_checkpoint.Animate(gameTime);

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
                    cam.Follow(new Vector2(tempX, tempY)); // panning within the room
            }
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

        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise, transformMatrix: cam.Transform);

            Rectangle source = bounds;
            _spriteBatch.Draw(bg_brick, bounds, source, Color.White);

            foreach (TiledData t in tld)
                foreach (TiledLayer l in t.map.Layers)
                    if (l.name == "background_lower")
                        DrawLayerOnScreen(_spriteBatch, l, t, tst_tutorial, cam);

            foreach (TiledData t in tld)
                foreach (TiledLayer l in t.map.Layers)
                    if (l.name == "background")
                        DrawLayerOnScreen(_spriteBatch, l, t, tst_tutorial, cam);

            for (int i = 0; i < checkpoints.Count(); i++)
                checkpoints[i].Draw(_spriteBatch);

            if (!player_dead)
                player.Draw(_spriteBatch);

            for (int i = enemies.Count - 1; i >= 0; i--)
                enemies[i].Draw(_spriteBatch);

            foreach (TiledData t in tld)
                foreach (TiledLayer l in t.map.Layers)
                    if (l.name == "tiles_lower")
                        DrawLayerOnScreen(_spriteBatch, l, t, tst_tutorial, cam);

            foreach (TiledData t in tld)
                foreach (TiledLayer l in t.map.Layers)
                    if (l.name == "tiles")
                        DrawLayerOnScreen(_spriteBatch, l, t, tst_tutorial, cam);

            for (int i = particles.Count - 1; i >= 0; i--)
                particles[i].Draw(_spriteBatch);

            if (debug)
                player.DebugDraw(_spriteBatch, black);

            if ((player_dead || finish_player_dead) && dead_timer > 0.36)
                _spriteBatch.Draw(spr_screenwipe, screenwipe_rect, Color.White);

            if (player_dead)
                player.DrawDead(_spriteBatch, dead_timer);
                

            _spriteBatch.End();
        }

        private void DrawLayer(SpriteBatch spriteBatch, TiledLayer layer, TiledData t, Texture2D tileset)
        {
            if (layer.data == null)
                return;

            int bounds_xoset = t.location.X;
            int bounds_yoset = t.location.Y;

            for (int i = 0; i < layer.data.Length; i++)
            {
                int gid = layer.data[i];


                if (gid == 0)
                    continue;

                int t_width = t.tst.TileWidth;
                int t_height = t.tst.TileHeight;

                int tileFrame = gid - 1;
                int column = tileFrame % t.tst.Columns;
                int row = (int)Math.Floor((double)tileFrame / (double)t.tst.Columns);

                int loc_x = (i % t.map.Width) * t.map.TileWidth;
                int loc_y = (int)Math.Floor(i / (double)t.map.Width) * t.map.TileHeight;

                loc_x += bounds_xoset;
                loc_y += bounds_yoset;

                Rectangle tile = new Rectangle(t_width * column, t_height * row, t_width, t_height);
                Rectangle loc = new Rectangle(loc_x, loc_y, t_width, t_height);

                spriteBatch.Draw(tileset, loc, tile, Color.White);
            }
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

            dead_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (dead_timer >= 0.7 && player_dead)
            {
                player.SetPos(new Vector2(active_checkpoint.box.X + 8, active_checkpoint.box.Y));
                Rectangle current_room = GetRoom(new Vector2(player.DrawBox.X + 16, player.DrawBox.Y + 16));
                int tempX = player.DrawBox.X + 16 - 160;
                int tempY = player.DrawBox.Y + 16 - 120;
                tempX = Math.Clamp(tempX, current_room.X, current_room.X + current_room.Width - 320);
                tempY = Math.Clamp(tempY, current_room.Y, Math.Max(current_room.Y + current_room.Height - 240, current_room.Y));
                cam.SetPos(new Vector2(tempX, tempY));
                player_dead = false;
                finish_player_dead = true;
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
                if (checkpoints[i].box.Intersects(input))
                    return checkpoints[i];

            return null;
        }

        public (Wall, Wall, Wall, Wall) FullCheckCollision(Rectangle in_left, Rectangle in_right, Rectangle in_up, Rectangle in_down)
        {
            Wall left = null;
            Wall right = null;
            Wall up = null;
            Wall down = null;

            for (int i = 0; i < walls.Count(); i++)
            {
                if (walls[i].bounds.Intersects(in_left)) left = walls[i];
                if (walls[i].bounds.Intersects(in_right)) right = walls[i];
                if (walls[i].bounds.Intersects(in_up)) up = walls[i];
                if (walls[i].bounds.Intersects(in_down)) down = walls[i];
            }

            return (left, right, up, down);
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            foreach (Wall wall in walls)
                if (wall != null)
                    _spriteBatch.Draw(black, wall.bounds, Color.Black);

            foreach (Obstacle obstacle in obstacles)
                if (obstacle != null)
                    _spriteBatch.Draw(black, obstacle.bounds, Color.Red);
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

        public Room(Rectangle bounds)
        { 
            this.bounds = bounds; 
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
    }


    public class Wall
    {
        public Rectangle bounds
        { get; private set; }

        public Wall(Rectangle bounds)
        {
            this.bounds = bounds;
        }
    }

    public class Obstacle
    {
        public Rectangle bounds
        { get; private set; }

        public Obstacle(Rectangle bounds)
        { this.bounds = bounds; }
    }

    public class Breakable : Wall
    {
        public Breakable(Rectangle bounds) : base(bounds) { }
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
}
