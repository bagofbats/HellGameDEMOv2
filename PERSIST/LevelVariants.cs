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
    public class TutorialLevel : Level
    {
        private Texture2D tst_tutorial;
        private Texture2D spr_slime;
        private Texture2D bg_brick;

        public TutorialLevel(Persist root, Rectangle bounds, Player player, List<TiledData> tld, Camera cam, ProgressionManager prog_manager, bool debug) : base(root, bounds, player, tld, cam, prog_manager, debug) 
        {
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
                            {
                                var temp = new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y);
                                AddEnemy(new Slime(temp, this));
                                enemy_locations.Add(temp);
                                enemy_types.Add("slime");
                            }


                            if (l.objects[i].name == "breakable")
                            {
                                int h_bound = (int)l.objects[i].x + (int)l.objects[i].width;
                                int v_bound = (int)l.objects[i].y + (int)l.objects[i].height;

                                for (int h = (int)l.objects[i].x; h < h_bound; h += 8)
                                    for (int v = (int)l.objects[i].y; v < v_bound; v += 8)
                                    {
                                        AddSpecialWall(new Breakable(new Rectangle(h, v, 8, 8), this));
                                        special_walls_bounds.Add(new Rectangle(h, v, 8, 8));
                                    }

                            }
                        }

                }
        }

        public override void Load()
        {
            cam.SmartSetPos(new Vector2(player.DrawBox.X - 16, player.DrawBox.Y - 16));

            black = root.Content.Load<Texture2D>("black");
            Texture2D checkpoint = root.Content.Load<Texture2D>("spr_checkpoint");
            particle_img = root.Content.Load<Texture2D>("spr_particlefx");
            tst_tutorial = root.Content.Load<Texture2D>("tst_tutorial");
            spr_slime = root.Content.Load<Texture2D>("spr_slime");
            spr_screenwipe = root.Content.Load<Texture2D>("spr_screenwipe");
            bg_brick = root.Content.Load<Texture2D>("bg_brick2");
            // Texture2D spr_breakable = root.Content.Load<Texture2D>("spr_breakable");

            foreach (Enemy enemy in enemies)
            {
                if (enemy.GetType() == typeof(Slime))
                    enemy.LoadAssets(spr_slime);
            }

            foreach (Wall wall in special_walls)
            {
                if (wall.GetType() == typeof(Breakable))
                    wall.Load(tst_tutorial);
            }

            for (int i = 0; i < chunks.Count(); i++)
                chunks[i].Load(black);

            for (int i = 0; i < checkpoints.Count(); i++)
                checkpoints[i].Load(checkpoint);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch _spriteBatch)
        {
            DrawTiles(_spriteBatch, tst_tutorial, bg_brick);
        }

        public override void ResetUponDeath()
        {
            // reset breakable walls
            for (int i = special_walls.Count - 1; i >= 0; i--)
                RemoveSpecialWall(special_walls[i]);

            foreach (Rectangle bounds in special_walls_bounds)
                AddSpecialWall(new Breakable(bounds, this));

            foreach (Wall wall in special_walls)
                if (wall.GetType() == typeof(Breakable))
                    wall.Load(tst_tutorial);

            // respawn enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
                RemoveEnemy(enemies[i]);

            for (int i = 0; i < enemy_locations.Count; i++)
                if (enemy_types[i] == "slime")
                    AddEnemy(new Slime(enemy_locations[i], this));

            foreach (Enemy enemy in enemies)
                if (enemy.GetType() == typeof(Slime))
                    enemy.LoadAssets(spr_slime);
        }
    }
}