using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PERSIST
{
    public class Level
    {
        private Persist root;
        private Player player;
        private Camera cam;
        private Checkpoint active_checkpoint;

        private Rectangle bounds;
        private List<Chunk> chunks = new List<Chunk>();
        private List<JSON> JSONs = new List<JSON>();
        private List<Room> rooms = new List<Room>();
        private List<Checkpoint> checkpoints = new List<Checkpoint>();

        public Level(Persist root, Rectangle bounds, Player player, List<JSON> JSONs, Camera cam)
        {
            this.root = root;
            this.player = player;
            this.bounds = bounds;
            this.JSONs = JSONs;
            this.cam = cam;

            for (int i = 0; i < bounds.Width; i += 320)
                for (int j = 0; j < bounds.Height; j += 240)
                    chunks.Add(new Chunk(new Rectangle(i - 32, j - 32, 320 + 64, 240 + 64)));

            foreach (JSON json in JSONs)
                foreach (Layer layer in json.raw.layers)
                {
                    if (layer.name == "walls")
                        for (int i = 0; i < layer.data.Count(); i += 4)
                            AddWall(new Rectangle(layer.data[i] + json.location.X, layer.data[i + 1] + json.location.Y, layer.data[i + 2], layer.data[i + 3]));

                    else if (layer.name == "rooms")
                        for (int i = 0; i < layer.data.Count(); i += 4)
                            rooms.Add(new Room(new Rectangle(layer.data[i] + json.location.X, layer.data[i + 1] + json.location.Y, layer.data[i + 2], layer.data[i + 3])));

                    else if (layer.name == "entities")
                    {
                        foreach (Entity entity in layer.entities)
                        {
                            if (entity.name == "checkpoint")
                                checkpoints.Add(new Checkpoint(new Rectangle(entity.x + json.location.X, entity.y + json.location.Y - 16, 16, 32)));
                        }
                    }

                    else if (layer.name == "obstacles")
                        for (int i = 0; i < layer.data.Count(); i += 4)
                            AddObstacle(new Rectangle(layer.data[i] + json.location.X, layer.data[i + 1] + json.location.Y, layer.data[i + 2], layer.data[i + 3]));
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
            for (int i = 0; i < chunks.Count(); i++)
                if (bounds.Intersects(chunks[i].bounds))
                    chunks[i].AddObstacle(bounds);
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

        public void Load()
        {
            Texture2D black = root.Content.Load<Texture2D>("black");
            Texture2D red = root.Content.Load<Texture2D>("red");
            Texture2D checkpoint = root.Content.Load<Texture2D>("spr_checkpoint");

            for (int i = 0; i < chunks.Count(); i++)
                chunks[i].Load(black, red);

            for (int i = 0; i < checkpoints.Count(); i++)
                checkpoints[i].Load(checkpoint);
        }

        public void Update(GameTime gameTime)
        {
            player.Update(gameTime);

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
                tempY = Math.Clamp(tempY, current_room.Y, current_room.Y + current_room.Height - 240);

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

            for (int i = 0; i < checkpoints.Count(); i++)
                checkpoints[i].Draw(_spriteBatch);

            player.Draw(_spriteBatch);

            for (int i = 0; i < chunks.Count(); i++)
                chunks[i].Draw(_spriteBatch);

            _spriteBatch.End();
        }
    }


    public class Chunk
    {
        public Rectangle bounds
        { get; private set; }
        private Texture2D black;
        private Texture2D red;

        private List<Wall> walls = new List<Wall>();
        private List<Rectangle> obstacles = new List<Rectangle>();

        public Chunk(Rectangle bounds)
        {
            this.bounds = bounds;
        }

        public void AddWall(Wall newWall)
        {
            walls.Add(newWall);
        }

        public void AddObstacle(Rectangle bounds)
        {
            obstacles.Add(bounds);
        }

        public Wall SimpleCheckCollision(Rectangle input)
        {
            for (int i = 0; i < walls.Count(); i++)
                if (walls[i].bounds.Intersects(input))
                    return walls[i];

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
                    _spriteBatch.Draw(black, wall.bounds, Color.White);

            foreach (Rectangle obstacle in obstacles)
                _spriteBatch.Draw(red, obstacle, Color.White);
        }

        public void Load(Texture2D black, Texture2D red)
        {
            this.black = black;
            this.red = red;
        }
    }


    public class JSON
    {
        public Rectangle location
        { get; private set; }

        public RawJSON raw
        { get; private set; }

        public JSON(Rectangle location, RawJSON raw) 
        { 
            this.location = location;
            this.raw = raw;
        }
    }


    public class Room
    {
        public Rectangle bounds
        { get; private set; }

        public Room(Rectangle bounds)
        { 
            this.bounds = bounds; 
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
}
