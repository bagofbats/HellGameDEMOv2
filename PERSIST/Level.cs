using System;
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

        private Rectangle bounds;
        private List<Chunk> chunks = new List<Chunk>();
        private List<JSON> rooms = new List<JSON>();

        public Level(Persist root, Rectangle bounds, Player player)
        {
            this.root = root;
            this.player = player;
            this.bounds = bounds;

            for (int i = 0; i < bounds.Width; i += 320)
            {
                for (int j = 0; j < bounds.Height; j += 240)
                {
                    chunks.Add(new Chunk(new Rectangle(i - 32, j - 32, 320 + 64, 240 + 64)));
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
            for (int i = 0; i < chunks.Count(); i++)
                chunks[i].Load(root);
        }

        public void Update(GameTime gameTime)
        {
            player.Update(gameTime);
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise); // transformMatrix: camera.Transform);

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

        private List<Wall> walls = new List<Wall>();

        public Chunk(Rectangle bounds)
        {
            this.bounds = bounds;
        }

        public void AddWall(Wall newWall)
        {
            walls.Add(newWall);
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
        }

        public void Load(Persist root)
        {
            black = root.Content.Load<Texture2D>("black");
        }
    }


    public class JSON
    {
        private Rectangle location;
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
