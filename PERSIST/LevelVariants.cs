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

        private List<EyeSwitch> switches = new List<EyeSwitch>();
        private List<Rectangle> switch_blocks_one = new List<Rectangle>();
        private List<Rectangle> switch_blocks_two = new List<Rectangle>();

        private Dictionary<Type, Texture2D> enemy_assets = new Dictionary<Type, Texture2D>();

        private DeadGuy dead_guy;

        private int slime_counter = 4;

        DialogueStruct[] dialogue_ck = {
            new DialogueStruct("The torch lights up at your presence.", 'd', Color.White, 'c'),
            new DialogueStruct("It soothes you.", 'd', Color.White, 'c', true)
        };

        DialogueStruct[] dialogue_slime = { 
            new DialogueStruct("-- Defeated Mama Slime! --", 'd', Color.White, 'c', true), 
        };

        DialogueStruct[] dialogue_deadguy = {
            new DialogueStruct("There is a knife stuck in the corpse's head.", 'd', Color.White, 'c'),
            new DialogueStruct("Leave it.\nPull it out.", 'o', Color.White, 'l', false, "exit 0|pull"),
            new DialogueStruct("Obtained the Silver Blade.", 'd', Color.White, 'c', true),
            new DialogueStruct("Like you, the corpse is wearing a cloak and\na wooden mask.", 'd', Color.White, 'c'),
            new DialogueStruct("There is a strange liquid leaking out of its\nskull.", 'd', Color.White, 'c', true),
            new DialogueStruct("Best not to dwell on it.", 'd', Color.Red, 'c', true)
        };

        public TutorialLevel(Persist root, Rectangle bounds, Player player, List<TiledData> tld, Camera cam, ProgressionManager prog_manager, bool debug, string name) : base(root, bounds, player, tld, cam, prog_manager, debug, name) 
        {
            dialogue_checkpoint = dialogue_ck;
            door_trans_color = new Color(36, 0, 0);

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
                                                             (int)l.objects[i].height), l.objects[i].name));

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
                                player.SetPos(new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y));

                            if (l.objects[i].name == "checkpoint")
                                AddCheckpoint(new Rectangle((int)l.objects[i].x + t.location.X - 8, (int)l.objects[i].y + t.location.Y - 16, 16, 32));

                            if (l.objects[i].name == "fake_checkpoint")
                            {
                                AddCheckpoint(new Rectangle((int)l.objects[i].x + t.location.X - 8, (int)l.objects[i].y + t.location.Y - 16, 16, 32));
                                checkpoints[checkpoints.Count - 1].visible = false;
                            }
                                

                            if (l.objects[i].name == "door")
                                doors.Add(new Door(new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height), l.objects[i].properties[1].value, l.objects[i].properties[0].value));

                            if (l.objects[i].name == "slime")
                            {
                                var temp = new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y);
                                AddEnemy(new Slime(temp, this));
                                enemy_locations.Add(temp);
                                enemy_types.Add("slime");
                            }

                            if (l.objects[i].name == "big_slime")
                            {
                                var temp = new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y);
                                AddEnemy(new BigSlime(temp, player, this));
                                enemy_locations.Add(temp);
                                enemy_types.Add("big_slime");
                            }

                            if (l.objects[i].name == "breakable")
                            {
                                int h_bound = (int)l.objects[i].x + (int)l.objects[i].width + t.location.X;
                                int v_bound = (int)l.objects[i].y + (int)l.objects[i].height + t.location.Y;

                                for (int h = (int)l.objects[i].x + t.location.X; h < h_bound; h += 8)
                                    for (int v = (int)l.objects[i].y + t.location.Y; v < v_bound; v += 8)
                                    {
                                        AddSpecialWall(new Breakable(new Rectangle(h, v, 8, 8), this));
                                        special_walls_bounds.Add(new Rectangle(h, v, 8, 8));
                                        special_walls_types.Add("breakable");
                                    }
                            }

                            if (l.objects[i].name == "switch_one")
                            {
                                int h_bound = (int)l.objects[i].x + (int)l.objects[i].width + t.location.X;
                                int v_bound = (int)l.objects[i].y + (int)l.objects[i].height + t.location.Y;
                                switch_blocks_one.Add(new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height));

                                for (int h = (int)l.objects[i].x + t.location.X; h < h_bound; h += 16)
                                    for (int v = (int)l.objects[i].y + t.location.Y; v < v_bound; v += 16)
                                    {
                                        AddSpecialWall(new SwitchBlock(new Rectangle(h, v, 16, 16), this));
                                        special_walls_bounds.Add(new Rectangle(h, v, 16, 16));
                                        special_walls_types.Add("switch");
                                    }
                            }

                            if (l.objects[i].name == "switch_two")
                                switch_blocks_two.Add(new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height));

                            if (l.objects[i].name == "switch")
                            {
                                var temp = new EyeSwitch(new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, 12, 12), player, this);
                                AddEnemy(temp);
                                switches.Add(temp);
                                enemy_locations.Add(new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y));
                                enemy_types.Add("switch");
                            }

                            if (l.objects[i].name == "knife_getter_guy")
                            {
                                var temp = new DeadGuy(
                                    new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, 32, 32), 
                                    dialogue_deadguy, prog_manager, this);
                                AddEnemy(temp);
                                dead_guy = temp;
                                enemy_locations.Add(new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y));
                                enemy_types.Add("deadguy");
                            }
                        }

                }
        }

        public override void Load(Texture2D spr_ui, string code="")
        {
            if (code != "")
            {
                for (int i = 0; i < doors.Count; i++)
                    if (doors[i].code == code)
                    {
                        Door dst = doors[i];
                        player.SetPos(new Vector2(dst.location.X, dst.location.Y));
                        break;
                    }
            }

            cam.SmartSetPos(new Vector2(player.DrawBox.X - 16, player.DrawBox.Y - 16));

            base.Load(spr_ui);

            Texture2D checkpoint = root.Content.Load<Texture2D>("sprites/spr_checkpoint");
            particle_img = root.Content.Load<Texture2D>("sprites/spr_particlefx");
            tst_tutorial = root.Content.Load<Texture2D>("tilesets/tst_tutorial");
            spr_slime = root.Content.Load<Texture2D>("sprites/spr_slime");
            bg_brick = root.Content.Load<Texture2D>("bgs/bg_brick2");
            // Texture2D spr_breakable = root.Content.Load<Texture2D>("spr_breakable");

            enemy_assets.Add(typeof(Slime), spr_slime);
            enemy_assets.Add(typeof(EyeSwitch), black);
            enemy_assets.Add(typeof(BigSlime), spr_slime);
            enemy_assets.Add(typeof(DeadGuy), spr_slime);

            foreach (Enemy enemy in enemies)
                enemy.LoadAssets(enemy_assets[enemy.GetType()]);

            foreach (Wall wall in special_walls)
            {
                var temp = wall.GetType();

                if (temp == typeof(Breakable) || temp == typeof(SwitchBlock))
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

            for (int i = 0; i < special_walls_bounds.Count; i++)
            {
                if (special_walls_types[i] == "breakable")
                    AddSpecialWall(new Breakable(special_walls_bounds[i], this));
                else if (special_walls_types[i] == "switch")
                    AddSpecialWall(new SwitchBlock(special_walls_bounds[i], this));
            }
                

            foreach (Wall wall in special_walls)
            {
                var temp = wall.GetType();
                if (temp == typeof(Breakable) || temp == typeof(SwitchBlock))
                    wall.Load(tst_tutorial);
            }
                

            // respawn enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
                RemoveEnemy(enemies[i]);

            for (int i = switches.Count - 1; i >= 0; i--)
                switches.Remove(switches[i]);

            for (int i = 0; i < enemy_locations.Count; i++)
            {
                if (enemy_types[i] == "slime")
                    AddEnemy(new Slime(enemy_locations[i], this));

                if (enemy_types[i] == "big_slime" && !prog_manager.slime_dead)
                    AddEnemy(new BigSlime(enemy_locations[i], player, this));

                if (enemy_types[i] == "switch")
                {
                    var temp = new EyeSwitch(new Rectangle((int)enemy_locations[i].X, (int)enemy_locations[i].Y, 12, 12), player, this);
                    AddEnemy(temp);
                    switches.Add(temp);
                }

                if (enemy_types[i] == "deadguy")
                {
                    var temp = new DeadGuy(
                                    new Rectangle((int)enemy_locations[i].X, (int)enemy_locations[i].Y, 32, 32),
                                    dialogue_deadguy, prog_manager, this);
                    AddEnemy(temp);
                    dead_guy = temp;
                }
                    
            }
                

            foreach (Enemy enemy in enemies)
                enemy.LoadAssets(enemy_assets[enemy.GetType()]);
                
        }

        public override void Switch(Room r, bool two)
        {
            for (int i = special_walls.Count - 1; i >= 0; i--)
                if (special_walls[i].GetType() == typeof(SwitchBlock))
                    if (special_walls[i].bounds.Intersects(r.bounds))
                        special_walls[i].FlashDestroy();

            if (two)
            {
                foreach (Rectangle rect in switch_blocks_two)
                    if (rect.Intersects(r.bounds))
                        for (int h = rect.X; h < rect.X + rect.Width; h += 16)
                            for (int v = rect.Y; v < rect.Y + rect.Height; v += 16)
                            {
                                var temp = new SwitchBlock(new Rectangle(h, v, 16, 16), this);
                                AddSpecialWall(temp);
                                temp.Load(tst_tutorial);
                            }
                                
            }
                
            else
                foreach (Rectangle rect in switch_blocks_one)
                    if (rect.Intersects(r.bounds))
                        for (int h = rect.X; h < rect.X + rect.Width; h += 16)
                            for (int v = rect.Y; v < rect.Y + rect.Height; v += 16)
                            {
                                var temp = new SwitchBlock(new Rectangle(h, v, 16, 16), this);
                                AddSpecialWall(temp);
                                temp.Load(tst_tutorial);
                            }

            foreach (EyeSwitch s in switches)
                if (s.GetHitBox(new Rectangle(0,0,0,0)).Intersects(r.bounds))
                    s.two = !two;
        }

        public override void HandleDialogueOption(string opt_code, int choice)
        {
            if (opt_code == "")
                return;

            string[] opts = opt_code.Split('|');

            if (choice >= opts.Length)
                return;

            string[] code = opts[choice].Split(' ');

            if (code[0] == "exit")
            {
                player.LeaveDialogue();
                dialogue = false;
                dialogue_letter = 0f;
                dialogue_num = 0;
                return;
            }

            else if (code[0] == "pull")
            {
                dead_guy.GetKnife();
                dialogue_num++;
                dialogue_letter = 0f;
                opts_highlighted = 0;
            }
        }

        public override void HandleCutscene(string code, bool start)
        {
            if (start)
            {
                cutscene_code = code.Split('|');

                player.EnterCutscene();
                cutscene = true;
                cutscene_timer = 0f;

                if (cutscene_code[0] == "finish_door")
                {
                    root.bbuffer_color = door_trans_color;
                    //root.blackout = true;
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

        public void WakeUpSlime(BigSlime slime)
        {
            slime.sleep = false;

            BossBlock temp1 = new BossBlock(new Rectangle(888, 960, 16, 16), this);
            BossBlock temp2 = new BossBlock(new Rectangle(888, 976, 16, 16), this);
            BossBlock temp3 = new BossBlock(new Rectangle(888 - 16, 960, 16, 16), this);
            BossBlock temp4 = new BossBlock(new Rectangle(888 - 16, 976, 16, 16), this);

            temp1.Load(tst_tutorial);
            temp2.Load(tst_tutorial);
            temp3.Load(tst_tutorial);
            temp4.Load(tst_tutorial);

            AddSpecialWall(temp1);
            AddSpecialWall(temp2);
            AddSpecialWall(temp3);
            AddSpecialWall(temp4);
        }

        public void DefeatSime()
        {
            slime_counter--;

            if (slime_counter != 0)
                return;

            prog_manager.DefeatSlime();

            Room temp = RealGetRoom(new Vector2(player.HitBox.X, player.HitBox.Y));

            Rectangle r = temp.bounds;
            Rectangle newbounds = new Rectangle(r.X, r.Y - 240, 320, 480);
            temp.Resize(newbounds);

            for (int i = rooms.Count - 1; i >= 0; i--)
                if (rooms[i].name == "Fundamentals")
                    rooms.Remove(rooms[i]);

            StartDialogue(dialogue_slime, 0, 'c', 10f, false, false);
        }

        public void SplitSlime(BigSlime slime)
        {
            Random rnd = new Random();

            RemoveEnemy(slime);

            slime_counter = 4;

            // create four baby slimes with different speeds and bounce intervals
            var bbslime = new BabySlime(new Vector2(slime.Pos.X + rnd.Next(0, 3) - 8, slime.Pos.Y - 28), this, this);
            bbslime.LoadAssets(spr_slime);
            bbslime.SetSpeed(0.6f + (float)rnd.NextDouble()/3);
            bbslime.SetTimer(2f + (0.7f * (float)rnd.NextDouble()));
            AddEnemy(bbslime);

            bbslime = new BabySlime(new Vector2(slime.Pos.X + 13 + rnd.Next(0, 6) - 8, slime.Pos.Y - 22), this, this);
            bbslime.LoadAssets(spr_slime);
            bbslime.SetSpeed(0.6f + (float)rnd.NextDouble()/3);
            bbslime.SetTimer(2f + (0.7f * (float)rnd.NextDouble()));
            AddEnemy(bbslime);

            bbslime = new BabySlime(new Vector2(slime.Pos.X - 17 - rnd.Next(0, 6) - 8, slime.Pos.Y - 16), this, this);
            bbslime.LoadAssets(spr_slime);
            bbslime.SetSpeed(0.6f + (float)rnd.NextDouble()/3);
            bbslime.SetTimer(2f + (0.7f * (float)rnd.NextDouble()));
            AddEnemy(bbslime);

            bbslime = new BabySlime(new Vector2(slime.Pos.X - 8 - rnd.Next(0, 6) - 8, slime.Pos.Y - 4), this, this);
            bbslime.LoadAssets(spr_slime);
            bbslime.SetSpeed(0.6f + (float)rnd.NextDouble()/3);
            bbslime.SetTimer(2f + (0.7f * (float)rnd.NextDouble()));
            AddEnemy(bbslime);


            // also make some vfx
            SlimeFX particle = new SlimeFX(new Vector2(slime.Pos.X, slime.Pos.Y), particle_img, this);
            AddFX(particle);
            particle = new SlimeFX(new Vector2(slime.Pos.X - 8 - rnd.Next(0, 6), slime.Pos.Y - 4), particle_img, this);
            AddFX(particle);
            particle = new SlimeFX(new Vector2(slime.Pos.X + 7 + rnd.Next(0, 6), slime.Pos.Y - 8), particle_img, this);
            AddFX(particle);
            particle = new SlimeFX(new Vector2(slime.Pos.X + 15 + rnd.Next(0, 3), slime.Pos.Y - 2), particle_img, this);
            AddFX(particle);
            particle = new SlimeFX(new Vector2(slime.Pos.X - 16 - rnd.Next(0, 4), slime.Pos.Y - 3), particle_img, this);
            AddFX(particle);
        }
    }
}