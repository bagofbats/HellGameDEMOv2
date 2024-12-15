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
        private Texture2D spr_lukas;

        private List<EyeSwitch> switches = new List<EyeSwitch>();
        private List<Rectangle> switch_blocks_one = new List<Rectangle>();
        private List<Rectangle> switch_blocks_two = new List<Rectangle>();

        private Dictionary<Type, Texture2D> enemy_assets = new Dictionary<Type, Texture2D>();

        private DeadGuy dead_guy;

        

        private BigSlime slimeboss;

        private int slime_counter = 4;

        DialogueStruct[] dialogue_ck = {
            new DialogueStruct("The torch lights up at your presence.", 'd', Color.White, 'c'),
            new DialogueStruct("You feel soothed.", 'd', Color.White, 'c', true),
            new DialogueStruct("( So many torches . . . )", 'd', Color.DodgerBlue, 'p', false, "", 45, 0),
            new DialogueStruct("( Seems like whoever runs this place isn't a fan\n  of light bulbs. )", 'd', Color.DodgerBlue, 'p', true, "", 90, 0)
        };

        DialogueStruct[] dialogue_slime = { 
            new DialogueStruct("-- Defeated Mama Slime! --", 'd', Color.White, 'c', true, "", 0, 0, 10f), 
        };

        DialogueStruct[] dialogue_deadguy = {
            new DialogueStruct("There is a knife stuck in the corpse's head.", 'd', Color.White, 'c'),
            new DialogueStruct("Leave it alone.\nPull it out.", 'o', Color.White, 'l', false, "exit 0|pull"),
            new DialogueStruct("Obtained the Silver Blade.", 'd', Color.White, 'c', true),
            new DialogueStruct("Like you, the corpse is wearing a cloak and\na wooden mask.", 'd', Color.White, 'c'),
            new DialogueStruct("There is a strange liquid leaking out of its\nskull.", 'd', Color.White, 'c', true),
            new DialogueStruct("( . . . )", 'd', Color.DodgerBlue, 'p', false, "", 45, 135),
            new DialogueStruct("( . . . Best not to dwell on it. )", 'd', Color.DodgerBlue, 'p', true, "", 90, 135)
        };

        DialogueStruct[] dialogue_ranged = {
            new DialogueStruct("Obtained the Ranger's Medallion", 'd', Color.White, 'c'),
            new DialogueStruct("Hold down the attack button to do a ranged attack!", 'd', Color.White, 'c', true)
        };

        DialogueStruct[] dialogue_chair = {
            new DialogueStruct("It's a chair.", 'd', Color.White, 'c'),
            new DialogueStruct("You don't feel like sitting down, though.", 'd', Color.White, 'c', true),
            new DialogueStruct("( My back hurts just looking at it . . . )", 'd', Color.DodgerBlue, 'p', true, "", 90, 0)
        };
        int[] chair_bps = { 0, 2 };

        DialogueStruct[] dialogue_crate = {
            new DialogueStruct("It's a crate.", 'd', Color.White, 'c'),
            new DialogueStruct("Someone is probably storing stuff in these.", 'd', Color.White, 'c', true),
            new DialogueStruct("You try to pry it open with your knife.", 'd', Color.White, 'c'),
            new DialogueStruct("It's bolted too tightly for that to work.", 'd', Color.White, 'c', true),
            new DialogueStruct("( Whatever.\n  probably just BORING stuff in there\n  anyway . . . )", 'd', Color.DodgerBlue, 'p', true, "", 90, 0)
        };
        int[] crate_bps = { 0, 2, 4 };

        DialogueStruct[] dialogue_desk = {
            new DialogueStruct("It's a desk.", 'd', Color.White, 'c'),
            new DialogueStruct("There's a paper and a pen on top of it.", 'd', Color.White, 'c'),
            new DialogueStruct("Leave it alone.\nRead the paper.", 'o', Color.White, 'l', false, "exit 0|read"),
            new DialogueStruct("Lukas, if you're reading this, don't go anywhere.\nI've gone out to look for you, but I'll be back.\nSo you should stay put for the time being.", 'd', Color.White, 'l', false, "", 0, 0, 99999999999999),
            new DialogueStruct("I'm still not sure where we are but this seems like a safe\nroom we can chill out in.\nAt least, there's no nasty red sludge in here.", 'd', Color.White, 'l', false, "", 0, 0, 99999999999999),
            new DialogueStruct("I'm really sorry we got separated like that.\nIt won't happen again, okay?\nPromise.", 'd', Color.White, 'l', false, "", 0, 0, 99999999999999),
            new DialogueStruct("Be back soon.\nLove, big sister Alice", 'd', Color.White, 'l', false, "", 0, 0, 99999999999999),
            new DialogueStruct("P.S. I found this coin lying around.\nPretty neat, huh? Maybe you could tell me where it's from.\nYou always know more about this kind of thing than I do.", 'd', Color.White, 'l', false, "", 0, 0, 99999999999999),
            new DialogueStruct("That's the end of the letter.", 'd', Color.White, 'c'),
            new DialogueStruct("The coin Alice mentioned is still there.", 'd', Color.White, 'c'),
            new DialogueStruct("You got the Strange Coin.", 'd', Color.White, 'c', true),
            new DialogueStruct("( Alice . . . Lukas . . .\n  Are they okay? )", 'd', Color.DodgerBlue, 'p', false, "", 90, 135),
            new DialogueStruct("( I want to think they are\n  but the coin is still here\n  and that corpse I pulled the knife from . . . )", 'd', Color.DodgerBlue, 'p', false, "", 45, 135),
            new DialogueStruct("( You know what, let's not think about it.\n  I'm sure they're fine. )", 'd', Color.DodgerBlue, 'p', true, "", 90, 180),
            new DialogueStruct("( Don't think about it.\n  Everything will be fine. )", 'd', Color.DodgerBlue, 'p', true, "", 90, 180)
        };
        int[] desk_bps = { 0, 11, 14 };



        public TutorialLevel(HellGame root, Rectangle bounds, Player player, List<TiledData> tld, Camera cam, ProgressionManager prog_manager, AudioManager audio_manager, bool debug, string name) : base(root, bounds, player, tld, cam, prog_manager, audio_manager, debug, name) 
        {
            dialogue_checkpoint = dialogue_ck;
            dialogue_second_index = 2;
            door_trans_color = Color.Black; // new Color(36, 0, 0);

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
                            {
                                var temp = AddCheckpoint(new Rectangle((int)l.objects[i].x + t.location.X - 8, (int)l.objects[i].y + t.location.Y - 16, 16, 32));

                                if (l.objects[i].properties.Count() != 0)
                                    temp.SetSideways(true, l.objects[i].properties[0].value);
                            }

                            if (l.objects[i].name == "fake_checkpoint")
                            {
                                AddCheckpoint(new Rectangle((int)l.objects[i].x + t.location.X - 8, (int)l.objects[i].y + t.location.Y - 16, 16, 32));
                                checkpoints[checkpoints.Count - 1].visible = false;
                            }
                                
                            if (l.objects[i].name == "door")
                            {
                                var temp = new Door(new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height), l.objects[i].properties[1].value, l.objects[i].properties[0].value);
                                if (l.objects[i].properties.Count() > 2)
                                    temp.SetOneWay(l.objects[i].properties[2].value);
                                doors.Add(temp);
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
                                        AddSpecialWall(new SwitchBlock(new Rectangle(h, v, 16, 16), this, new Rectangle(48, 32, 16, 16)));
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

                            if (l.objects[i].name == "switch_boss")
                            {
                                var temp = new EyeSwitch(new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, 12, 12), player, this);
                                AddEnemy(temp);
                                switches.Add(temp);
                                enemy_locations.Add(new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y));
                                enemy_types.Add("switch_boss");

                                if (!prog_manager.slime_dead)
                                    temp.SetDisabled(true);
                            }

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

                            if (l.objects[i].name == "lukas_boss")
                            {
                                var temp = new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y);
                                AddEnemy(new Lukas_Tutorial(temp, player, this));
                                enemy_locations.Add(temp);
                                enemy_types.Add("lukas_boss");
                            }

                            if (l.objects[i].name == "ranger_pickup" && !prog_manager.ranged)
                            {
                                AddInteractable(new RangerPickup(new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y), this, prog_manager, dialogue_ranged));
                            }

                            if (l.objects[i].name == "furniture")
                            {
                                var temp = new Furniture(new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height), this);
                                string value = l.objects[i].properties[0].value;

                                if (value == "desk")
                                {
                                    temp = new SecretDesk(new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height), this, prog_manager);
                                    temp.SetType(dialogue_desk, desk_bps);
                                }

                                if (value == "chair")
                                    temp.SetType(dialogue_chair, chair_bps);

                                if (value == "crate")
                                    temp.SetType(dialogue_crate, crate_bps);

                                AddInteractable(temp);
                            }

                            if (l.objects[i].name == "oneway")
                            {
                                var temp = new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, 2);
                                AddSpecialWall(new OneWay(temp, new Rectangle(32, 136, 8, 8), this));
                                special_walls_bounds.Add(temp);
                                special_walls_types.Add("oneway");
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
                        player.SetPos(new Vector2(dst.location.X + 6, dst.location.Y - 8));
                        break;
                    }
            }

            cam.SmartSetPos(new Vector2(player.DrawBox.X - 16, player.DrawBox.Y - 16));

            base.Load(spr_ui);

            Texture2D checkpoint = root.Content.Load<Texture2D>("sprites/spr_checkpoint");
            particle_img = root.Content.Load<Texture2D>("sprites/spr_particlefx");
            tst_tutorial = root.Content.Load<Texture2D>("tilesets/tst_tutorial");
            spr_slime = root.Content.Load<Texture2D>("sprites/spr_slime");
            spr_lukas = root.Content.Load<Texture2D>("sprites/spr_lukas");
            bg_brick = root.Content.Load<Texture2D>("bgs/bg_brick2");
            // Texture2D spr_breakable = root.Content.Load<Texture2D>("spr_breakable");

            enemy_assets.Add(typeof(Slime), spr_slime);
            enemy_assets.Add(typeof(EyeSwitch), black);
            enemy_assets.Add(typeof(BigSlime), spr_slime);
            enemy_assets.Add(typeof(DeadGuy), spr_slime);
            enemy_assets.Add(typeof(Lukas_Tutorial), spr_lukas);

            foreach (Enemy enemy in enemies)
                enemy.LoadAssets(enemy_assets[enemy.GetType()]);

            foreach (Interactable i in interactables)
                i.LoadAssets(spr_slime);

            foreach (Wall wall in special_walls)
            {
                var temp = wall.GetType();

                if (temp == typeof(Breakable) || temp == typeof(SwitchBlock) || temp == typeof(OneWay))
                    wall.Load(tst_tutorial);
            }

            for (int i = 0; i < chunks.Count(); i++)
                chunks[i].Load(black);

            for (int i = 0; i < checkpoints.Count(); i++)
                checkpoints[i].Load(checkpoint);

            foreach (Checkpoint c in checkpoints)
                if (c.sideways)
                    c.GetSidewaysWall();
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
                    AddSpecialWall(new SwitchBlock(special_walls_bounds[i], this, new Rectangle(48, 32, 16, 16)));
                else if (special_walls_types[i] == "oneway")
                    AddSpecialWall(new OneWay(special_walls_bounds[i], new Rectangle(32, 136, 8, 8), this));
            }
                

            foreach (Wall wall in special_walls)
            {
                var temp = wall.GetType();
                if (temp == typeof(Breakable) || temp == typeof(SwitchBlock) || temp == typeof(OneWay))
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

                if (enemy_types[i] == "switch_boss")
                {
                    var temp = new EyeSwitch(new Rectangle((int)enemy_locations[i].X, (int)enemy_locations[i].Y, 12, 12), player, this);
                    AddEnemy(temp);
                    switches.Add(temp);

                    if (!prog_manager.slime_dead)
                        temp.SetDisabled(true);
                }

                if (enemy_types[i] == "deadguy")
                {
                    var temp = new DeadGuy(
                                    new Rectangle((int)enemy_locations[i].X, (int)enemy_locations[i].Y, 32, 32),
                                    dialogue_deadguy, prog_manager, this);
                    AddEnemy(temp);
                    dead_guy = temp;
                }

                if (enemy_types[i] == "lukas_boss")
                    AddEnemy(new Lukas_Tutorial(enemy_locations[i], player, this));

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
                                var temp = new SwitchBlock(new Rectangle(h, v, 16, 16), this, new Rectangle(48, 32, 16, 16));
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
                                var temp = new SwitchBlock(new Rectangle(h, v, 16, 16), this, new Rectangle(48, 32, 16, 16));
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

            else if (code[0] == "read")
            {
                prog_manager.ReadJournal();
                dialogue_num++;
                dialogue_letter = 0f;
                opts_highlighted = 0;
            }
        }

        public override void HandleCutscene(string code, GameTime gameTime, bool start)
        {
            base.HandleCutscene(code, gameTime, start);

            if (cutscene_code[0] == "wakeslime")
            {
                if (cutscene_timer > 0f)
                    slimeboss.UpdateSleepFX(gameTime);

                if (cutscene_timer > 1.7f)
                {
                    slimeboss.sleep = false;
                    slimeboss.shake = true;
                }
                
                if (cutscene_timer > 2f)
                {
                    slimeboss.shake = false;
                }
                    

                if (cutscene_timer > 2.4f && !slimeboss.up)
                {
                    slimeboss.Update(gameTime);
                    slimeboss.up = true;
                }

                if (cutscene_timer > 3.7f)
                {
                    if (cutscene_code[1] == "")
                    {
                        BossBlock temp1 = new BossBlock(new Rectangle(888, 960, 16, 16), this, new Rectangle(48, 112, 16, 16));
                        BossBlock temp2 = new BossBlock(new Rectangle(888, 976, 16, 16), this, new Rectangle(48, 112, 16, 16));
                        BossBlock temp3 = new BossBlock(new Rectangle(888 - 16, 960, 16, 16), this, new Rectangle(48, 112, 16, 16));
                        BossBlock temp4 = new BossBlock(new Rectangle(888 - 16, 976, 16, 16), this, new Rectangle(48, 112, 16, 16));

                        temp1.Load(tst_tutorial);
                        temp2.Load(tst_tutorial);
                        temp3.Load(tst_tutorial);
                        temp4.Load(tst_tutorial);

                        AddSpecialWall(temp1);
                        AddSpecialWall(temp2);
                        AddSpecialWall(temp3);
                        AddSpecialWall(temp4);

                        cutscene_code[1] = "_";
                    }
                    else
                    {
                        for (int i = special_walls.Count - 1; i >= 0; i--)
                            special_walls[i].Update(gameTime);
                    }
                }

                if (cutscene_timer > 4.2f)
                {
                    player.ExitCutscene();
                    cutscene = false;
                    door_trans = false;
                    slimeboss = null;
                    prog_manager.EncounterSlime();
                }
            }
        }

        public void WakeUpSlime(BigSlime slime, GameTime gameTime)
        {
            if (!prog_manager.slime_started)
            {
                HandleCutscene("wakeslime|", gameTime, true);
                slimeboss = slime;
            }
            else
            {
                slime.sleep = false;

                BossBlock temp1 = new BossBlock(new Rectangle(888, 960, 16, 16), this, new Rectangle(48, 112, 16, 16));
                BossBlock temp2 = new BossBlock(new Rectangle(888, 976, 16, 16), this, new Rectangle(48, 112, 16, 16));
                BossBlock temp3 = new BossBlock(new Rectangle(888 - 16, 960, 16, 16), this, new Rectangle(48, 112, 16, 16));
                BossBlock temp4 = new BossBlock(new Rectangle(888 - 16, 976, 16, 16), this, new Rectangle(48, 112, 16, 16));

                temp1.Load(tst_tutorial);
                temp2.Load(tst_tutorial);
                temp3.Load(tst_tutorial);
                temp4.Load(tst_tutorial);

                AddSpecialWall(temp1);
                AddSpecialWall(temp2);
                AddSpecialWall(temp3);
                AddSpecialWall(temp4);
            }
            
        }

        public void FightLukas(Lukas_Tutorial lukas, GameTime gameTime)
        {
            int blocks_x = 2392;
            int blocks_y = 368;

            lukas.sleep = false;

            for (int i = blocks_x; i < blocks_x + 32; i += 16)
                for (int j = blocks_y; j < blocks_y + 64; j += 16)
                {
                    BossBlock temp1 = new BossBlock(new Rectangle(i, j, 16, 16), this, new Rectangle(48, 112, 16, 16));
                    temp1.Load(tst_tutorial);
                    AddSpecialWall(temp1);
                }
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

            foreach (EyeSwitch s in switches)
                if (s.disabled)
                    s.SetDisabled(false);

            RangerPickup rpickup = new RangerPickup(new Vector2(984, 976), this, prog_manager, dialogue_ranged);
            rpickup.LoadAssets(spr_slime);
            AddInteractable(rpickup);

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

    public class StyxLevel : Level
    {
        private Texture2D tst_styx;
        private Texture2D bg_dark;
        private Texture2D bg_rocks;
        private Texture2D spr_mushroom;

        private List<JumpSwitch> switches = new List<JumpSwitch>();
        private List<Rectangle> switch_blocks_one = new List<Rectangle>();
        private List<Rectangle> switch_blocks_two = new List<Rectangle>();
        private List<bool> switch_blocks_one_danger = new List<bool>();
        private List<bool> switch_blocks_two_danger = new List<bool>();

        private Dictionary<Type, Texture2D> enemy_assets = new Dictionary<Type, Texture2D>();

        private List<Rectangle> rivers = new List<Rectangle>();
        private Rectangle river_frame_top = new Rectangle(160 + 64, 192, 64, 16);
        private Rectangle river_frame = new Rectangle(160 + 64, 208, 64, 16);
        private float river_timer = 0f;
        private int river_frame_oset = 0;
        private Rectangle switch_block_frame = new Rectangle(112, 128, 16, 16);
        private Rectangle ghost_block_frame = new Rectangle(112 + 32, 128, 16, 16);
        private Rectangle lock_block_frame = new Rectangle(224, 112, 16, 16);

        private List<int> mouth_locs = new List<int>();

        DialogueStruct[] dialogue_ck = {
            new DialogueStruct("The flame burns bright in the dark.", 'd', Color.White, 'c'),
            // new DialogueStruct("It energizes you.", 'd', Color.White, 'c', true),
            new DialogueStruct("You feel encouraged.", 'd', Color.White, 'c', true),
            new DialogueStruct("( Oh, so there are torches here too. )", 'd', Color.DodgerBlue, 'p', false, "", 135, 0),
            new DialogueStruct("( So many torches . . .\n  I wonder what they're for? )", 'd', Color.DodgerBlue, 'p', true, "",135, 45),
            //new DialogueStruct("( Who makes all these torches, anyway?\n  Are they getting paid? )", 'd', Color.DodgerBlue, 'p', false, "", 90, 0),
            //new DialogueStruct("( Maybe I should learn how to make torches.\n  Seems like a lucrative business )", 'd', Color.DodgerBlue, 'p', true, "", 90, 0),
        };

        public StyxLevel(HellGame root, Rectangle bounds, Player player, List<TiledData> tld, Camera cam, ProgressionManager prog_manager, AudioManager audio_manager, bool debug, string name) : base(root, bounds, player, tld, cam, prog_manager, audio_manager, debug, name)
        {
            dialogue_checkpoint = dialogue_ck;
            dialogue_second_index = 2;
            door_trans_color = Color.Black; // new Color(36, 0, 0);

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
                        {
                            var temp = new Rectangle((int)l.objects[i].x + t.location.X,
                                                  (int)l.objects[i].y + t.location.Y,
                                                  (int)l.objects[i].width,
                                                  (int)l.objects[i].height);

                            AddObstacle(temp);

                            if (l.objects[i].name == "river")
                                rivers.Add(temp);
                        }
                            

                    if (l.name == "entities")
                        for (int i = 0; i < l.objects.Count(); i++)
                        {
                            if (l.objects[i].name == "player")
                                player.SetPos(new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y));

                            if (l.objects[i].name == "checkpoint")
                            {
                                var temp = AddCheckpoint(new Rectangle((int)l.objects[i].x + t.location.X - 8, (int)l.objects[i].y + t.location.Y - 16, 16, 32));

                                if (l.objects[i].properties.Count() != 0)
                                    temp.SetSideways(true, l.objects[i].properties[0].value);
                            }

                            if (l.objects[i].name == "fake_checkpoint")
                            {
                                AddCheckpoint(new Rectangle((int)l.objects[i].x + t.location.X - 8, (int)l.objects[i].y + t.location.Y - 16, 16, 32));
                                checkpoints[checkpoints.Count - 1].visible = false;
                            }

                            if (l.objects[i].name == "door")
                            {
                                var temp = new Door(new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height), l.objects[i].properties[1].value, l.objects[i].properties[0].value);
                                if (l.objects[i].properties.Count() > 2)
                                    temp.SetOneWay(l.objects[i].properties[2].value);
                                doors.Add(temp);
                            }

                            if (l.objects[i].name == "crumble")
                            {
                                var temp = new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height);
                                AddSpecialWall(new Crumble(temp, this));

                                special_walls_bounds.Add(temp);
                                special_walls_types.Add("crumble");
                            }

                            if (l.objects[i].name == "walker")
                            {
                                var temp = new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y);
                                AddEnemy(new Walker(temp, this));
                                enemy_locations.Add(temp);
                                enemy_types.Add("walker");
                            }

                            if (l.objects[i].name == "trampoline")
                            {
                                var temp = new Vector2(l.objects[i].x - 8 + t.location.X, l.objects[i].y - 32 + t.location.Y);
                                var tempoline = new Trampoline(temp, this);
                                AddEnemy(tempoline);
                                enemy_locations.Add(temp);
                                enemy_types.Add("trampoline");

                                var temp2 = new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height);
                                var mouth = Int32.Parse(l.objects[i].properties[0].value);
                                mouth_locs.Add(mouth);
                                AddSpecialWall(new Stem(temp2, this, tempoline, mouth));
                                special_walls_bounds.Add(temp2);
                                special_walls_types.Add("stem");


                            }

                            if (l.objects[i].name == "switch_one")
                            {
                                int h_bound = (int)l.objects[i].x + (int)l.objects[i].width + t.location.X;
                                int v_bound = (int)l.objects[i].y + (int)l.objects[i].height + t.location.Y;
                                switch_blocks_one.Add(new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height));

                                bool danger = l.objects[i].properties.Count() != 0;

                                switch_blocks_one_danger.Add(danger);

                                if (!danger)
                                {
                                    for (int h = (int)l.objects[i].x + t.location.X; h < h_bound; h += 16)
                                        for (int v = (int)l.objects[i].y + t.location.Y; v < v_bound; v += 16)
                                        {
                                            AddSpecialWall(new SwitchBlock(new Rectangle(h, v, 16, 16), this, switch_block_frame));
                                            special_walls_bounds.Add(new Rectangle(h, v, 16, 16));
                                            special_walls_types.Add("switch");
                                        }
                                }

                                else
                                {
                                    for (int h = (int)l.objects[i].x + t.location.X; h < h_bound; h += 16)
                                        for (int v = (int)l.objects[i].y + t.location.Y; v < v_bound; v += 16)
                                        {
                                            AddEnemy(new GhostBlock(new Vector2(h, v), this, ghost_block_frame));
                                            special_walls_bounds.Add(new Rectangle(h, v, 16, 16));
                                            special_walls_types.Add("badswitch");
                                        }
                                }
                                
                            }

                            if (l.objects[i].name == "switch_two")
                            {
                                switch_blocks_two.Add(new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height));

                                bool danger = l.objects[i].properties.Count() != 0;

                                switch_blocks_two_danger.Add(danger);
                            }
                                

                            if (l.objects[i].name == "switch")
                                switches.Add(new JumpSwitch(new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y)));

                            if (l.objects[i].name == "oneway")
                            {
                                var temp = new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, 2);
                                AddSpecialWall(new OneWay(temp, new Rectangle(8, 184, 8, 8), this));
                                special_walls_bounds.Add(temp);
                                special_walls_types.Add("oneway");
                            }

                            if (l.objects[i].name == "lock")
                            {
                                var temp = new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height);

                                for (int j = 0; j < temp.Width; j += 16)
                                    for (int k = 0; k < temp.Height; k += 16)
                                    {
                                        Rectangle temp2 = new Rectangle(temp.X + j, temp.Y + k, 16, 16);
                                        AddSpecialWall(new Lock(temp2, this, lock_block_frame));
                                        special_walls_bounds.Add(temp2);
                                        special_walls_types.Add("lock");
                                    }
                            }

                        }

                }
        }



        // ------- mandatory overrides ---------
        public override void Load(Texture2D spr_ui, string code = "")
        {
            if (code != "")
            {
                for (int i = 0; i < doors.Count; i++)
                    if (doors[i].code == code)
                    {
                        Door dst = doors[i];
                        player.SetPos(new Vector2(dst.location.X + 6, dst.location.Y - 8));
                        break;
                    }
            }

            cam.SmartSetPos(new Vector2(player.DrawBox.X - 16, player.DrawBox.Y - 16));

            base.Load(spr_ui);

            Texture2D checkpoint = root.Content.Load<Texture2D>("sprites/spr_checkpoint");
            particle_img = root.Content.Load<Texture2D>("sprites/spr_particlefx");
            tst_styx = root.Content.Load<Texture2D>("tilesets/tst_styx");
            bg_dark = root.Content.Load<Texture2D>("bgs/bg_styx");
            bg_rocks = root.Content.Load<Texture2D>("bgs/bg_styx_rocks");
            spr_mushroom = root.Content.Load<Texture2D>("sprites/spr_mushroom");
            // Texture2D spr_breakable = root.Content.Load<Texture2D>("spr_breakable");

            enemy_assets.Add(typeof(Walker), spr_mushroom);
            enemy_assets.Add(typeof(Trampoline), spr_mushroom);
            enemy_assets.Add(typeof(GhostBlock), tst_styx);

            foreach (Enemy enemy in enemies)
                enemy.LoadAssets(enemy_assets[enemy.GetType()]);

            foreach (Wall wall in special_walls)
            {
                var temp = wall.GetType();

                if (temp == typeof(Crumble) || temp == typeof(SwitchBlock) || temp == typeof(OneWay) || temp == typeof(Lock))
                    wall.Load(tst_styx);

                if (temp == typeof(Stem))
                    wall.Load(spr_mushroom);
            }

            for (int i = 0; i < chunks.Count(); i++)
                chunks[i].Load(black);

            for (int i = 0; i < checkpoints.Count(); i++)
                checkpoints[i].Load(checkpoint);

            foreach (Checkpoint c in checkpoints)
                if (c.sideways)
                    c.GetSidewaysWall();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            river_timer += (float)gameTime.ElapsedGameTime.TotalSeconds * 10;

            river_frame_oset = (int)river_timer;

            if (river_frame_oset >= 64)
            {
                river_frame_oset = 0;
                river_timer = 0;

                river_timer += (float)gameTime.ElapsedGameTime.TotalSeconds * 10;
                river_frame_oset = (int)river_timer;
            }
        }

        public override void Draw(SpriteBatch _spriteBatch)
        {
            if (name == "rm_styx0")
                DrawTiles(_spriteBatch, tst_styx, bg_dark);
            else
                DrawTiles(_spriteBatch, tst_styx, bg_rocks);
        }

        public override void ResetUponDeath()
        {
            // reset breakable walls
            for (int i = special_walls.Count - 1; i >= 0; i--)
                RemoveSpecialWall(special_walls[i]);

            // remove enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
                RemoveEnemy(enemies[i]);

            int mouth_counter = 0;

            // replace special walls
            for (int i = 0; i < special_walls_bounds.Count; i++)
            {
                if (special_walls_types[i] == "crumble")
                    AddSpecialWall(new Crumble(special_walls_bounds[i], this));

                if (special_walls_types[i] == "stem")
                {
                    var temp = new Vector2(special_walls_bounds[i].X - 8, special_walls_bounds[i].Y - 32);
                    var tempoline = new Trampoline(temp, this);
                    AddEnemy(tempoline);

                    AddSpecialWall(new Stem(special_walls_bounds[i], this, tempoline, mouth_locs[mouth_counter]));
                    mouth_counter++;
                }

                else if (special_walls_types[i] == "switch")
                    AddSpecialWall(new SwitchBlock(special_walls_bounds[i], this, switch_block_frame));

                else if (special_walls_types[i] == "badswitch")
                    AddEnemy(new GhostBlock(new Vector2(special_walls_bounds[i].X, special_walls_bounds[i].Y), this, ghost_block_frame));

                else if (special_walls_types[i] == "lock")
                    AddSpecialWall(new Lock(special_walls_bounds[i], this, lock_block_frame));

                if (special_walls_types[i] == "oneway")
                    AddSpecialWall(new OneWay(special_walls_bounds[i], new Rectangle(8, 184, 8, 8), this));
            }


            foreach (Wall wall in special_walls)
            {
                var temp = wall.GetType();
                if (temp == typeof(Crumble) || temp == typeof(SwitchBlock) || temp == typeof(OneWay) || temp == typeof(Lock))
                    wall.Load(tst_styx);

                if (temp == typeof(Stem))
                    wall.Load(spr_mushroom);
            }


            
            // replace enemies
            for (int i = 0; i < enemy_locations.Count; i++)
            {

                // re-add enemies

                if (enemy_types[i] == "walker")
                    AddEnemy(new Walker(enemy_locations[i], this));

                //if (enemy_types[i] == "trampoline")
                //    AddEnemy(new Trampoline(enemy_locations[i], this));

            }


            foreach (Enemy enemy in enemies)
                enemy.LoadAssets(enemy_assets[enemy.GetType()]);

            foreach (JumpSwitch s in switches)
                s.two = true;

        }

        public override void Switch(Room r, bool two)
        {
            for (int i = special_walls.Count - 1; i >= 0; i--)
                if (special_walls[i].GetType() == typeof(SwitchBlock))
                    if (special_walls[i].bounds.Intersects(r.bounds))
                        special_walls[i].FlashDestroy();

            for (int i = enemies.Count - 1; i >= 0; i--)
                if (enemies[i].GetType() == typeof(GhostBlock))
                    if (enemies[i].GetHitBox(r.bounds).Intersects(r.bounds))
                        enemies[i].FlashDestroy();

            if (two)
            {
                for (int i = 0; i < switch_blocks_two.Count; i++)
                {
                    Rectangle rect = switch_blocks_two[i];
                    if (rect.Intersects(r.bounds))
                    {
                        if (!switch_blocks_two_danger[i])
                            for (int h = rect.X; h < rect.X + rect.Width; h += 16)
                                for (int v = rect.Y; v < rect.Y + rect.Height; v += 16)
                                {
                                    var temp = new SwitchBlock(new Rectangle(h, v, 16, 16), this, switch_block_frame);
                                    AddSpecialWall(temp);
                                    temp.Load(tst_styx);
                                }
                        
                        else
                            for (int h = rect.X; h < rect.X + rect.Width; h += 16)
                                for (int v = rect.Y; v < rect.Y + rect.Height; v += 16)
                                {
                                    var temp = new GhostBlock(new Vector2(h, v), this, ghost_block_frame);
                                    AddEnemy(temp);
                                    temp.LoadAssets(tst_styx);
                                }
                    }
                }
                    

            }

            else
            {
                for (int i = 0; i < switch_blocks_one.Count; i++)
                {
                    Rectangle rect = switch_blocks_one[i];
                    if (rect.Intersects(r.bounds))
                    {
                        if (!switch_blocks_one_danger[i])
                            for (int h = rect.X; h < rect.X + rect.Width; h += 16)
                                for (int v = rect.Y; v < rect.Y + rect.Height; v += 16)
                                {
                                    var temp = new SwitchBlock(new Rectangle(h, v, 16, 16), this, switch_block_frame);
                                    AddSpecialWall(temp);
                                    temp.Load(tst_styx);
                                }

                        else
                            for (int h = rect.X; h < rect.X + rect.Width; h += 16)
                                for (int v = rect.Y; v < rect.Y + rect.Height; v += 16)
                                {
                                    var temp = new GhostBlock(new Vector2(h, v), this, ghost_block_frame);
                                    AddEnemy(temp);
                                    temp.LoadAssets(tst_styx);
                                }
                    }
                }
            }

            foreach (JumpSwitch s in switches)
                if (r.bounds.Contains(s.pos))
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
        }

        public override void HandleCutscene(string code, GameTime gameTime, bool start)
        {
            base.HandleCutscene(code, gameTime, start);
        }


        // --------- end mandatory overrides ------------


        // optional override
        public override void DrawTiles(SpriteBatch _spriteBatch, Texture2D tileset, Texture2D background)
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

            for (int i = interactables.Count - 1; i >= 0; i--)
                interactables[i].Draw(_spriteBatch);

            if (!player_dead && !door_trans)
                player.Draw(_spriteBatch);

            int cam_x = (int)cam.GetPos().X;
            int cam_y = (int)cam.GetPos().Y;
            var camera_rect = new Rectangle(cam_x - 8, cam_y - 8, 320 + 16, 240 + 16);

            for (int i = special_walls.Count - 1; i >= 0; i--)
                special_walls[i].Draw(_spriteBatch);

            for (int i = 0; i < rivers.Count(); i++)
            {
                if (!rivers[i].Intersects(camera_rect))
                    continue;

                DrawRiver(_spriteBatch, rivers[i]);
            }

            foreach (TiledData t in tld)
                foreach (TiledLayer l in t.map.Layers)
                    if (l.name == "tiles_lower")
                        DrawLayerOnScreen(_spriteBatch, l, t, tileset, cam);

            foreach (TiledData t in tld)
                foreach (TiledLayer l in t.map.Layers)
                    if (l.name == "tiles")
                        DrawLayerOnScreen(_spriteBatch, l, t, tileset, cam);

            for (int i = particles.Count - 1; i >= 0; i--)
            {
                bool draw = true;

                for (int j = 0; j < rivers.Count(); j++)
                    if (particles[i].GetType() == typeof(RangedFX))
                        if (particles[i].pos.Intersects(rivers[j]))
                        {
                            draw = false;
                            break;
                        }

                if (draw)
                    particles[i].Draw(_spriteBatch);
            }


            if (debug)
            {
                foreach (Chunk c in chunks)
                    if (c.bounds.Contains(player.HitBox.X, player.HitBox.Y))
                        c.Draw(_spriteBatch, root.opaque);

                if (!root.opaque)
                {
                    player.DebugDraw(_spriteBatch, black);

                    for (int i = enemies.Count - 1; i >= 0; i--)
                        enemies[i].DebugDraw(_spriteBatch, black);

                    for (int i = 0; i < doors.Count; i++)
                        _spriteBatch.Draw(black, doors[i].location, Color.Blue * 0.2f);
                }

            }

            // UI stuff
            if (overlay)
            {
                //int cam_x = (int)cam.GetPos().X;
                //int cam_y = (int)cam.GetPos().Y;


                var overlay_rect = new Rectangle(cam_x, cam_y, 320, 12);

                float opacity = 1f;

                if (overlay_rect.Intersects(player.HitBox))
                    opacity = 0.3f;

                _spriteBatch.Draw(black, overlay_rect, Color.Black * opacity);

                if (dialogue)
                {
                    var dialogue_rect = new Rectangle(cam_x, cam_y, 320, 49);
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

        public override (Wall, Wall, Wall, Wall, Wall) FullCheckCollision(Rectangle input)
        {
            Wall left = null;
            Wall right = null;
            Wall up = null;
            Wall down = null;
            Wall inside = null;

            (left, right, up, down, inside) = base.FullCheckCollision(input);

            if (IsCrumble(left))
                left.Trigger();
            if (IsCrumble(right))
                right.Trigger();
            if (IsCrumble(down))
                down.Trigger();
            if (IsCrumble(inside))
                inside.Trigger();

            return (left, right, up, down, inside);
        }

        public override void JumpAction()
        {
            Vector2 player_pos = player.GetPos();

            player_pos.X += 16;
            player_pos.Y += 16;

            Room r = RealGetRoom(player_pos);

            if (r == null) return;

            bool two = false;
            bool found = false;

            foreach (JumpSwitch s in switches)
                if (r.bounds.Contains(s.pos))
                {
                    two = s.two;
                    found = true;
                    break;
                }

            if (!found) return;

            Switch(r, two);
        }

        // end optional override





        // --------- helper functions ---------

        private void DrawRiver(SpriteBatch _spriteBatch, Rectangle river)
        {
            for (int i = 0; i < river.Height + 8; i+= 16)
                for (int j = 0; j < river.Width; j += 64)
                {
                    var dstRectangle = new Rectangle(river.X + j, river.Y + i - 8, 64, 16);

                    var real_frame = river_frame;

                    if (i == 0)
                        real_frame = river_frame_top;

                    real_frame.X -= river_frame_oset;

                    _spriteBatch.Draw(tst_styx, dstRectangle, real_frame, Color.White);
                }
        }

        private bool IsCrumble(Wall w)
        {
            if (w != null)
                return w.GetType() == typeof(Crumble);

            return false;
        }
    }
}