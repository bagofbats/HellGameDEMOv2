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

        private DeadGuy dead_guy;

        

        private BigSlime slimeboss;
        private Lukas_Tutorial lukasboss = null;

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

        DialogueStruct[] dialogue_lukas = {
            new DialogueStruct(". . .", 'd', Color.White, 'r', false, "", 180, 45),
            new DialogueStruct("You should not be here, invader.", 'd', Color.White, 'r', true, "", 180, 135, 10f),
        };

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

                                if (!prog_manager.GetFlag(FLAGS.slime_dead))
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

                            if (l.objects[i].name == "ranger_pickup" && !prog_manager.GetFlag(FLAGS.ranged))
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

            asset_map.Add(typeof(Slime), spr_slime);
            asset_map.Add(typeof(EyeSwitch), black);
            asset_map.Add(typeof(BigSlime), spr_slime);
            asset_map.Add(typeof(DeadGuy), spr_slime);
            asset_map.Add(typeof(Lukas_Tutorial), spr_lukas);

            foreach (Enemy enemy in enemies)
                enemy.LoadAssets(asset_map[enemy.GetType()]);

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

                if (enemy_types[i] == "big_slime" && !prog_manager.GetFlag(FLAGS.slime_dead))
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

                    if (!prog_manager.GetFlag(FLAGS.slime_dead))
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
                enemy.LoadAssets(asset_map[enemy.GetType()]);
                
        }

        public override void Switch(Room r, bool two)
        {
            bool switched = false;

            for (int i = special_walls.Count - 1; i >= 0; i--)
                if (special_walls[i].GetType() == typeof(SwitchBlock))
                    if (special_walls[i].bounds.Intersects(r.bounds))
                    {
                        special_walls[i].FlashDestroy();
                        switched = true;
                    }
                        

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

                                switched = true;
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

                                switched = true;
                            }

            foreach (EyeSwitch s in switches)
                if (s.GetHitBox(new Rectangle(0,0,0,0)).Intersects(r.bounds))
                    s.two = !two;

            float pitch = 0.0f;
            if (!two)
                pitch = -0.4f;

            if (switched)
                audio_manager.DelaySound(0.05f, "snap", pitch, root.audioManager.switch_volume);
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
                prog_manager.SetFlag(FLAGS.journal_secret);
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

                        // should probably get rid of magic numbers here
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
                    prog_manager.SetFlag(FLAGS.slime_started);
                }
            }

            if (cutscene_code[0] == "fightlukas")
            {
                if (cutscene_timer > 0f)
                {
                    player.SetNoInput();
                    player.DoMovement(gameTime);
                    player.DoAnimate(gameTime);

                    lukasboss.Sleep(gameTime);
                }

                if (cutscene_timer > 1.4f)
                {
                    if (cutscene_code[1] != "-")
                    {
                        cutscene_code[1] = "-";
                        StartDialogue(dialogue_lukas, 0, 'c', 10f, false);
                    }
                }

                if (cutscene_timer > 1.43f)
                {
                    if (cutscene_code[2] != "-")
                    {
                        cutscene_code[2] = "-";
                        int blocks_x = 2392;
                        int blocks_y = 368;

                        for (int i = blocks_x; i < blocks_x + 32; i += 16)
                            for (int j = blocks_y; j < blocks_y + 64; j += 16)
                            {
                                BossBlock temp1 = new BossBlock(new Rectangle(i, j, 16, 16), this, new Rectangle(48, 112, 16, 16));
                                temp1.Load(tst_tutorial);
                                AddSpecialWall(temp1);
                            }
                    }

                    else
                    {
                        for (int i = special_walls.Count - 1; i >= 0; i--)
                            special_walls[i].Update(gameTime);
                    }
                }

                if (cutscene_timer > 2f)
                {
                    lukasboss.sleep = false;
                    lukasboss = null;

                    player.ExitCutscene();
                    cutscene = false;
                    prog_manager.SetFlag(FLAGS.lukas_started);
                }
            }
        }

        public override void DialogueActions(GameTime gameTime)
        {
            if (lukasboss != null)
                lukasboss.Sleep(gameTime);
        }

        public void WakeUpSlime(BigSlime slime, GameTime gameTime)
        {
            if (!prog_manager.GetFlag(FLAGS.slime_started))
            {
                HandleCutscene("wakeslime|", gameTime, true);
                slimeboss = slime;
            }
            else
            {
                slime.sleep = false;

                // should probably get rid of magic numbers here
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
            if (!prog_manager.GetFlag(FLAGS.lukas_started))
            {
                HandleCutscene("fightlukas|1|2|3|4|5|6|7", gameTime, true);
                lukasboss = lukas;
            }
            else
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
        }

        public void DefeatSime()
        {
            slime_counter--;

            if (slime_counter != 0)
                return;

            prog_manager.SetFlag(FLAGS.slime_dead);

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

            // should probably get rid of magic numbers in the vector here
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
        private GameTime saved_gameTime;

        private Texture2D tst_styx;
        private Texture2D bg_dark;
        private Texture2D bg_rocks;
        private Texture2D spr_mushroom;
        private Texture2D spr_kanna;
        private Texture2D spr_lukas;
        private Texture2D spr_guys;

        private List<JumpSwitch> switches = new List<JumpSwitch>();
        private List<Rectangle> switch_blocks_one = new List<Rectangle>();
        private List<Rectangle> switch_blocks_two = new List<Rectangle>();
        private List<bool> switch_blocks_one_danger = new List<bool>();
        private List<bool> switch_blocks_two_danger = new List<bool>();

        private List<Rectangle> rivers = new List<Rectangle>();
        private Rectangle river_frame_top = new Rectangle(160 + 64, 192, 64, 16);
        private Rectangle river_frame = new Rectangle(160 + 64, 208, 64, 16);
        private float river_timer = 0f;
        private int river_frame_oset = 0;
        private Rectangle switch_block_frame = new Rectangle(112, 128, 16, 16);
        private Rectangle ghost_block_frame = new Rectangle(112 + 32, 128, 16, 16);
        private Rectangle lock_block_frame = new Rectangle(224, 112, 16, 16);
        private Rectangle key_frame = new Rectangle(256, 120, 16, 8);
        private Rectangle charon_frame = new Rectangle(288, 184, 16, 40);
        private String dialogue_exit_code = "";

        private List<int> mouth_locs = new List<int>();
        private List<int> key_inits = new List<int>();

        public Rectangle kanna_trigger
        { get; private set; } = new Rectangle(0, 0, 0, 0);

        public Rectangle kanna_zone
        { get; private set; } = new Rectangle(0, 0, 0, 0);

        public Rectangle mushroom_trigger
        { get; private set; } = new Rectangle(0, 0, 0, 0);

        public Rectangle mushroom_zone
        { get; private set; } = new Rectangle(0, 0, 0, 0);

        public Rectangle hideout_trigger
        { get; private set; } = new Rectangle(0, 0, 0, 0);

        private Rectangle kanna_boss_blocks = new Rectangle(0, 0, 0, 0);

        public Rectangle charon_door_trigger
        { get; private set; } = new();

        public CharonBlock charondoor
        { get; set; } = null;

        private Kanna_Boss kanna_boss;
        private Lukas_Cutscene lukas_cutscene = null;
        private ShadePickup shade_pickup = null;
        private Kanna_Cutscene kanna_cutscene = null;

        private float saved_camera_x = 0f;
        private float saved_camera_y = 0f;

        private bool saved_choice = false;

        DialogueStruct[] dialogue_ck = {
            new DialogueStruct("The flame burns bright in the dark.", 'd', Color.White, 'c'),
            // new DialogueStruct("It energizes you.", 'd', Color.White, 'c', true),
            new DialogueStruct("You feel encouraged.", 'd', Color.White, 'c', true),
            new DialogueStruct("( Oh, so there are torches here too. )", 'd', Color.DodgerBlue, 'p', false, "", 135, 0),
            new DialogueStruct("( So many torches . . .\n  I wonder what they're for? )", 'd', Color.DodgerBlue, 'p', true, "",135, 45),
            //new DialogueStruct("( Who makes all these torches, anyway?\n  Are they getting paid? )", 'd', Color.DodgerBlue, 'p', false, "", 90, 0),
            //new DialogueStruct("( Maybe I should learn how to make torches.\n  Seems like a lucrative business )", 'd', Color.DodgerBlue, 'p', true, "", 90, 0),
        };

        DialogueStruct[] dialogue_kanna_fight = {
            new DialogueStruct(". . .", 'd', Color.White, 'r', false, "", 270, 0),
            new DialogueStruct(". . .", 'd', Color.White, 'p', false, "", 135, 90),
            new DialogueStruct(". . . uh . . . \nHello!\nHow's it going?", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct(". . .", 'd', Color.White, 'r', true, "", 270, 0),
        };

        DialogueStruct[] dialogue_kanna_fight_done = {
            new DialogueStruct("Stop.", 'd', Color.White, 'r', true, "", 270, 0, 10),
            new DialogueStruct(". . . \"How's it going\" ??", 'd', Color.White, 'r', false, "", 270, 45),      // one
            new DialogueStruct(". . . ", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct(". . . y-yeah?", 'd', Color.White, 'p', false, "", 135, 180),
            new DialogueStruct("You're not actually a demon, are you.", 'd', Color.White, 'r', false, "", 270, 90),
            new DialogueStruct("Yeah, you got me.\nNo, I'm a demon . . .\nYour MOM's not a demon!", 'o', Color.White, 'l', true, "exit 0|exit 1|exit 2"),
            new DialogueStruct("Yeah, figures.", 'd', Color.White, 'r', true, "", 225, 0),                  // six
            new DialogueStruct("Really, man?", 'd', Color.White, 'r', true, "", 225, 135),                  // seven
            new DialogueStruct("Oh . . .\nAre you okay?", 'd', Color.White, 'p', false, "", 90, 135),       // eight
            new DialogueStruct("My name's Trigo.\nSorry about that!\nI wasn't expecting to meet another one down here.", 'd', Color.White, 'p', false, "", 90, 225),
            new DialogueStruct("Kanna.\nAnd I get it.", 'd', Color.White, 'r', false, "", 225, 0),
            new DialogueStruct("Nice to meet you, Kanna!", 'd', Color.White, 'p', true, "", 45, 45),
            new DialogueStruct("I mean . . .\nI'm not wrong, am I?", 'd', Color.White, 'p', false, "", 45, 45),     // twelve
            new DialogueStruct("I guess not . . .", 'd', Color.White, 'r', false, "", 225, 45),
            new DialogueStruct("Name's Kanna.\nSorry for attacking you\nDidn't think I would meet another one down here.", 'd', Color.White, 'r', false, "", 225, 0),
            new DialogueStruct("Don't worry about it.\nI'm Trigo.", 'd', Color.White, 'p', false, "", 45, 0),
            new DialogueStruct("Nice to meet you, Kanna!", 'd', Color.White, 'p', true, "", 45, 45),
            new DialogueStruct("Just try to stay out of my way, okay?\nI don't need more enemies down here . . .", 'd', Color.White, 'r', true, "", 270, 0),        // seventeen
            new DialogueStruct("( A simple \"Nice to meet you, too\" goes a long way,\n  you know . . . )", 'd', Color.DodgerBlue, 'p', true, "", 45, 90),          // eighteen
        };

        DialogueStruct[] dialogue_lukas_pickup = {
            new DialogueStruct("I wouldn't bother picking that thing up if I were you.", 'd', Color.White, 'l', true), // <--- change this to true later
            new DialogueStruct("It's practically useless in its current form.\nYou're better off leaving it there.", 'd', Color.White, 'p', false, "", 180, 180),
            new DialogueStruct("Oh, okay . . .", 'd', Color.White, 'r', false, "", 315, 90),
            new DialogueStruct("Hey, hold on a sec --\nDidn't you try to kill me earlier?", 'd', Color.White, 'r', false, "", 315, 0),
            new DialogueStruct("Sure did.", 'p', Color.White, 'p', false, "", 180, 180),
            new DialogueStruct("And now you're trying to give me advice?", 'd', Color.White, 'r', false, "", 315, 45),
            new DialogueStruct("Sure am.", 'd', Color.White, 'p', false, "", 180, 180),
            new DialogueStruct(". . .", 'd', Color.White, 'r', false, "", 315, 225),
            new DialogueStruct("Thanks for not attacking me again.\nWhy should I trust you?\nYou're a freak.", 'o', Color.White, 'l', false, "lukas1 0|lukas1 1|lukas1 2"),
            new DialogueStruct("What happened earlier wasn't really up to me.\nNow that I'm a shade, I'm part of this place's\ndefenses.", 'd', Color.White, 'p', false, "", 180, 90),
            new DialogueStruct("If I see someone who doesn't belong, I have to\nattack.\nRight now, I'm lucky you have that disguise on.", 'd', Color.White, 'p', false, "", 180, 180),
            new DialogueStruct("Oh . . . I see.", 'd', Color.White, 'r', false, "", 315, 0),
            new DialogueStruct("I'm sorry.\n\"Now\" that you're a shade? What were you before?", 'o', Color.White, 'l', false, "lukas2 0|lukas2 1"),
            new DialogueStruct(". . .", 'd', Color.White, 'p', true, "", 180, 90),                         // thirteen
            new DialogueStruct("You can go ahead and pick up that medallion now.", 'd', Color.White, 'p', false, "", 180, 0),
            new DialogueStruct("It'll give you access to secret passages.\nMaybe you can make use of it.", 'd', Color.White, 'p', false, "", 180, 225),
            new DialogueStruct(". . . \nThanks.", 'o', Color.White, 'l', false, "lukas3 0|lukas3 1"),
            new DialogueStruct("Yeah, sure.", 'd', Color.White, 'p', false, "", 180, 0),
            new DialogueStruct("Good luck, hero.", 'd', Color.White, 'p', true, "", 180, 225),
            new DialogueStruct("You got that right.", 'd', Color.White, 'p', false, "", 180, 180),             // nineteen
            new DialogueStruct("A mindless sentry, forced to attack intruders.\nIt's a freakish existence.", 'd', Color.White, 'p', false, "", 180, 90),
            new DialogueStruct("\"Forced\", huh?", 'd', Color.White, 'r', false, "", 315, 45),
            new DialogueStruct("You mean you didn't want to attack me?", 'd', Color.White, 'r', false, "", 315, 0),
            new DialogueStruct("That's right.\nNow that I'm a shade, I'm part of this place's\ndefenses.", 'd', Color.White, 'p', false, "", 180, 90),
            new DialogueStruct("If I see someone who doesn't belong, I have to\nattack.\nRight now, I'm lucky you have that disguise on.", 'd', Color.White, 'p', false, "", 180, 180),
            new DialogueStruct("Oh . . . I see.", 'd', Color.White, 'r', false, "", 315, 0),
            new DialogueStruct("I'm sorry.\n\"Now\" that you're a shade? What were you before?", 'o', Color.White, 'l', false, "lukas2 0|lukas2 1"),
            new DialogueStruct("Thanks, but it's not really up to me.\nNow that I'm a shade, I'm part of this place's\ndefenses.", 'd', Color.White, 'p', false, "", 180, 90),
            new DialogueStruct("If I see someone who doesn't belong, I have to\nattack.\nRight now, I'm lucky you have that disguise on.", 'd', Color.White, 'p', false, "", 180, 180),
            new DialogueStruct("Oh . . . I see.", 'd', Color.White, 'r', false, "", 315, 0),
            new DialogueStruct("I'm sorry.\n\"Now\" that you're a shade? What were you before?", 'o', Color.White, 'l', false, "lukas2 0|lukas2 1"),
        };

        DialogueStruct[] dialogue_pickup = {
            new DialogueStruct("Obtained the Key Medallion", 'd', Color.White, 'c', true),
            new DialogueStruct("Obtained the Spectral Medallion", 'd', Color.White, 'c', true),
        };

        DialogueStruct[] dialogue_deadguy = {
            new DialogueStruct("Another corpse.", 'd', Color.White, 'c'),
            new DialogueStruct("There is a crumpled up piece of paper in her pocket.", 'd', Color.White, 'c'),
            new DialogueStruct("Read it.\nDo not.", 'o', Color.White, 'l', false, "corpse 0|corpse 1"),
            new DialogueStruct("IN CASE I DIE HERE:\nMY NAME IS ALICE VIMES\nI AM 17 YEARS OLD.", 'd', Color.White, 'l', false, "", 0, 0, 9999999),
            new DialogueStruct("I HAVE A BROTHER NAMED LUKAS VIMES\nWE WOKE UP HERE TOGETHER.", 'd', Color.White, 'l', false, "", 0, 0, 9999999),
            new DialogueStruct("TAKE WHATEVER YOU NEED FROM ME.\nCARRY MY MEMORY WITH YOU.", 'd', Color.White, 'l', false, "", 0, 0, 9999999),
            new DialogueStruct("GODSPEED TRAVELER.\nMAY YOU SUCCEED WHERE I DID NOT.", 'd', Color.White, 'l', false, "", 0, 0, 9999999),
            new DialogueStruct(". . .", 'd', Color.White, 'c'),
            new DialogueStruct("On the back, there is a map of The Underworld.", 'd', Color.White, 'c'),
            new DialogueStruct("In the center, there is a great tower.\nUnderneath, a place labeled \"The Heart\".", 'd', Color.White, 'c'),
            new DialogueStruct("You got the Underworld Map.", 'd', Color.White, 'c', true),
            new DialogueStruct("( Kanna might want to see this. )", 'd', Color.DodgerBlue, 'p', true, "", 135, 0),
        };

        DialogueStruct[] dialogue_hideout =
        {
            new DialogueStruct(". . .", 'd', Color.White, 'r', false, "", 270, 0),
            new DialogueStruct(". . .", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("I'll leave.", 'd', Color.White, 'p', false, "", 135, 180),
            new DialogueStruct(". . . No, it's alright.\nYou can stay if you want to.", 'd', Color.White, 'r', false, "", 270, 135),
            new DialogueStruct("This is my hideout.\nMake yourself at home, I guess.", 'd', Color.White, 'r', true, "", 270, 0),

            // index 5
            new DialogueStruct("I thought you wanted me to \"stay out of your way\"?\n\"Hideout\", huh?\nNevermind.", 'o', Color.White, 'l', false, "hideout0 0|hideout0 1|hideout0 2"),

            // index 6
            new DialogueStruct("Yeah.\nDon't push your luck.", 'd', Color.White, 'p', false, "", 405, 0),
            new DialogueStruct("I'm not against helping each other out.\nBut I'm not taking back what I said.", 'd', Color.White, 'p', true, "", 405, 0),

            // index 8
            new DialogueStruct("Yep. My hideout.", 'd', Color.White, 'p', false, "", 405, 0),
            new DialogueStruct("What do you think?\nTakes a lot of energy pretending all the time.\nNice to have a space where I won't be seen.", 'd', Color.White, 'p', false, "", 405, 0),

            // index 10
            new DialogueStruct("It's cool!\nIt sucks.\nIt's somewhere between 'cool' and 'sucks'.", 'o', Color.White, 'l', false, "hideout1 0|hideout1 1|hideout1 2"),
            new DialogueStruct("Is it? I dunno.\nI'd still rather be anywhere else, honestly.", 'd', Color.White, 'p', false, "", 405, 0),
            new DialogueStruct("But it is better than nothing.\nSo I take what I can get.", 'd', Color.White, 'p', true, "", 405, 0),

            // index 13
            new DialogueStruct("rude.", 'd', Color.White, 'p', false, "", 405, 0),
            new DialogueStruct("But also pretty true, this does kinda suck.\nI'd rather be anywhere else, honestly.", 'd', Color.White, 'p', false, "", 405, 0),
            new DialogueStruct("But a sucky hideout is still better than nothing.\nSo I take what I can get.", 'd', Color.White, 'p', true, "", 405, 0),

            // index 16
            new DialogueStruct(". . .", 'd', Color.White, 'p', false, "", 405, 90),
            new DialogueStruct("You know, I was thinking the same thing about\nyou.", 'd', Color.White, 'p', true, "", 405, 90),
            
            // index 18
            new DialogueStruct("\"Hideout\", huh?\nNevermind.", 'o', Color.White, 'l', false, "hideout0 1|hideout0 2"),

            // index 19
            new DialogueStruct("I thought you wanted me to \"stay out of your way\"?\nNevermind.", 'o', Color.White, 'l', false, "hideout0 0|hideout0 2"),

            // index 20
            new DialogueStruct("I'm getting out of here. Are you?\nNevermind.", 'o', Color.White, 'l', false, "hideout2 0|hideout0 2"),

            // index 21
            new DialogueStruct("You're . . . what?", 'd', Color.White, 'r', false, "", 270, 0),
            new DialogueStruct("Yeah, sure man. Whatever you say.", 'd', Color.White, 'r', false, "", 270, 180),
            new DialogueStruct("You don't think I can do it?", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("I don't \"think\" you can't, I know you can't.", 'd', Color.White, 'r', false, "", 270, 90),
            new DialogueStruct("There's a way out.\nIn the Heart of the Underworld.", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("Charon told me so.\nWe can leave!", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("Charon told you, huh?\nDid he tell you where it was?", 'd', Color.White, 'r', false, "", 270, 90),
            new DialogueStruct("Huh?", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("It's, uh . . .", 'd', Color.White, 'p', false, "", 135, 90),
            new DialogueStruct("I don't know.\nI haven't gotten that far yet.\nIt's a work in progress, okay??", 'o', Color.White, 'l', false, "hideout3 0|hideout3 0|hideout3 0"),
            new DialogueStruct("That's what I thought.", 'd', Color.White, 'r', false, "", 270, 90),
            new DialogueStruct("Charon told me the same thing.\nAbout that heart, I mean.", 'd', Color.White, 'r', false, "", 270, 180),
            new DialogueStruct("And you don't even want to try to look for it?", 'd', Color.White, 'p', false, "", 135, 45),
            new DialogueStruct("This place is dangerous, Trigo.\nOne wrong turn and we're demon food.", 'd', Color.White, 'r', false, "", 270, 135),
            new DialogueStruct("No thanks. I'm good.", 'd', Color.White, 'r', true, "", 270, 45),

            // index 36
            new DialogueStruct("Nevermind.", 'o', Color.White, 'l', false, "hideout0 2"),

            // index 37
            new DialogueStruct("I'm getting out of here. Are you?\nNevermind.", 'o', Color.White, 'l', false, "hideout4 0|hideout0 2"),

            // index 38
            new DialogueStruct("You're . . . what?", 'd', Color.White, 'r', false, "", 270, 0),
            new DialogueStruct("Yeah, sure man. Whatever you say.", 'd', Color.White, 'r', false, "", 270, 180),
            new DialogueStruct("You don't think I can do it?", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("I don't \"think\" you can't, I know you can't.", 'd', Color.White, 'r', false, "", 270, 90),
            new DialogueStruct("There's a way out.\nIn the Heart of the Underworld.", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("Charon told me so.\nWe can leave!", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("Charon told you, huh?\nDid he tell you where it was?", 'd', Color.White, 'r', false, "", 270, 90),
            new DialogueStruct("Huh?", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("Well, no, but . . .", 'd', Color.White, 'p', false, "", 135, 90),
            new DialogueStruct("I have a map to the Heart of the Underworld.", 'o', Color.White, 'l', false, "hideout5 0"),

            // index 48
            new DialogueStruct("I have a map to the Heart of the Underworld.\nNevermind.", 'o', Color.White, 'l', false, "hideout5 0|hideout0 2"),

            // index 49
            new DialogueStruct("What??\nLet me see that . . .\nWhere'd you get THIS from?", 'd', Color.White, 'r', false, "", 270, 0),
            new DialogueStruct("Not important.\nWhat matters is we have a way out.\nAnd we know how to get there now.", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("Guess so.\nLooks like the way forward is just over that\nmoat.", 'd', Color.White, 'r', false, "", 270, 0),
            new DialogueStruct("Moat?", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("Yeah, the moat.\nThe one all the way down and to the right?", 'd', Color.White, 'r', false, "", 270, 0),
            new DialogueStruct("Oh.", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("Yeah, I uh . . .\nCan't get accross that.", 'd', Color.White, 'p', false, "", 135, 180),
            new DialogueStruct("What do you mean?\nJust dash accross.", 'd', Color.White, 'r', false, "", 270, 45),
            new DialogueStruct(". . . Dash?", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("Trigo I did that against you when we fought.\nDon't tell me you don't know how to dash.", 'd', Color.White, 'r', false, "", 270, 180),
            new DialogueStruct("I, uh . . .", 'd', Color.White, 'p', false, "", 135, 90),

            // index 60
            new DialogueStruct("Yeah, I don't know how to dash.\nObviously I know how! Who doesn't?", 'o', Color.White, 'l', false, "hideout6 0|hideout6 1"),

            // index 61
            new DialogueStruct(". . .", 'd', Color.White, 'r', false, "", 270, 90),
            new DialogueStruct("Well, that's a relief.\nGuess you don't need me to teach you, then?", 'd', Color.White, 'r', false, "", 270, 0),
            new DialogueStruct("Hey, hey hey!\nI was kidding!!", 'd', Color.White, 'p', false, "", 135, 0),

            // index 64
            new DialogueStruct("Alright, then.\nI'll show you how.", 'd', Color.White, 'r', true, "", 270, 0),

            // index 65
            new DialogueStruct("You learned how to dash!", 'd', Color.White, 'c', true),

            // index 66
            new DialogueStruct("Nevermind.", 'o', Color.White, 'l', false, "hideout0 2"),
        };

        DialogueStruct[] dialogue_defeat =
        {
            new DialogueStruct("-- Defeated King Mush! --", 'd', Color.White, 'c', true, "", 0, 0, 10f),
        };

        DialogueStruct[] dialogue_fisher =
        {
            new DialogueStruct("You see that torch up there?\nMost of them are upright, but that one's sideways.", 'd', Color.White, 'p', false, "", 0, 45),
            new DialogueStruct("They say that sideways torches are bad omens.\nA sign that the area ahead will be especially\ndangerous.", 'd', Color.White, 'p', false, "", 0, 45),
            new DialogueStruct("So I wouldn't go any farther than here.\nUnless you fancy yourself a challenge, of course.", 'd', Color.White, 'p', true, "", 0, 45),
            new DialogueStruct("Hmm?\nYou want to know what I'm doing?", 'd', Color.White, 'p', false, "", 0, 45),
            new DialogueStruct("I'm fishing!", 'd', Color.White, 'p', false, "", 0, 45),
            new DialogueStruct("They say there aren't any fish in the Styx.\nBut you never know, right?", 'd', Color.White, 'p', false, "", 0, 45),
            new DialogueStruct("Maybe some day, one will bite.\nSo I'll keep fishing, just in case.", 'd', Color.White, 'p', true, "", 0, 45),
            new DialogueStruct("Beware the sideways torch, friend.", 'd', Color.White, 'p', true, "", 0, 45),
            new DialogueStruct("You see that torch up there?\nMost of them are upright, but that one's sideways.", 'd', Color.White, 'p', false, "", 0, 45),
            new DialogueStruct("They say that sideways torches are bad omens.\nA sign that the area ahead will be especially\ndangerous.", 'd', Color.White, 'p', false, "", 0, 45),
            new DialogueStruct("But you knew that already, didn't you?\nYou've seen these torches before.", 'd', Color.White, 'p', false, "", 0, 45),
            new DialogueStruct("He-heh . . .\nYou're a real go-getter, aren't you?", 'd', Color.White, 'p', true, "", 0, 45),
            new DialogueStruct("Challenging yourself is good, but it'll wear you\ndown if you aren't careful.", 'd', Color.White, 'p', false, "", 0, 45),
            new DialogueStruct("When that happens, don't forget to rest.\nMaybe do some fishing!\nThat's what I always do, anyway.", 'd', Color.White, 'p', true, "", 0, 45),
            new DialogueStruct("Good luck, friend.", 'd', Color.White, 'p', true, "", 0, 45),
        };
        readonly int[] fisher_bps = { 0, 3, 7 };
        readonly int[] fisher_bps_secret = { 8, 12, 14 };

        DialogueStruct[] dialogue_charon_gossip =
        {
            new DialogueStruct("*Crrrkkk* (One of Charon's, eh?)", 'd', Color.White, 'p', false, "", 0, 90),
            new DialogueStruct("*Crrroouuukk* (Heard Charon's pretty chill.)\n*Craaauukkkk* (Unlike The General.)", 'd', Color.White, 'p', false, "", 0, 90),
            new DialogueStruct("*Creerk* (Little guy over here's been telling me\nstories.)\n*Cookie* (Sounds like a real nutcase.)", 'd', Color.White, 'p', true, "", 0, 90),
            new DialogueStruct("*Coke* (Take it easy, new guy.)", 'd', Color.White, 'p', true, "", 0, 90),
            new DialogueStruct("Oh, that's Charon's mask, isn't it?\nYou work for him?", 'd', Color.White, 'p', false, "", 0, 135),
            new DialogueStruct("Wonder what Charon wanted you for.\nGuy doesn't usually ask for help.", 'd', Color.White, 'p', false, "", 0, 135),
            new DialogueStruct("Now Doc, she's always asking for more people.\nBut not Charon.\nSeems happy just keeping to himself, honestly.", 'd', Color.White, 'p', true, "", 0, 135),
            new DialogueStruct("Charon's not that bad once you get to know him.\nYou'll do fine.", 'd', Color.White, 'p', true, "", 0, 135),
        };
        readonly int[] frog_bps = { 0, 3 };
        readonly int[] gossiper_bps = { 4, 7 };

        DialogueStruct[] dialogue_look_down =
        {
            new DialogueStruct("Reaper can be really cryptic sometimes, you know?", 'd', Color.White, 'p', false, "", 0, 180),
            new DialogueStruct("Like the other day she was talking about how\nusing the controller's analog stick to move and\nlook down at the same time is really difficult.", 'd', Color.White, 'p', false, "", 0, 180),
            new DialogueStruct("And about how there's an option in the menu to\nbind looking down to a button instead of using\nthe stick for that.", 'd', Color.White, 'p', false, "", 0, 180),
            new DialogueStruct("Oh well.\nMaybe I'm not supposed to get it.\nMaybe she was talking to someone else . . .", 'd', Color.White, 'p', true, "", 0, 180),
            new DialogueStruct("Between this and the Evil Fungus Pit I'm getting\nreally tired of Reaper just saying stuff without\nelaborating.", 'd', Color.White, 'p', true, "", 0, 180),
        };
        readonly int[] look_down_bps = { 0, 4 };

        DialogueStruct[] dialogue_chair = {
            new DialogueStruct("It's a chair.", 'd', Color.White, 'c'),
            new DialogueStruct("It would be rude to sit in someone else's chair.\nSo, you don't.", 'd', Color.White, 'c', true),
            new DialogueStruct("Do you actually sit in this thing?", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct(". . . Yes?", 'd', Color.White, 'r', false, "", 405, 135),
            new DialogueStruct("Is it comfortable?", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("No.", 'd', Color.White, 'r', true, "", 405, 90),
        };
        int[] chair_bps = { 0, 2 };

        DialogueStruct[] dialogue_crate = {
            new DialogueStruct("It's a crate.", 'd', Color.White, 'c'),
            new DialogueStruct("Kanna is probably storing stuff in these.", 'd', Color.White, 'c', true),
            new DialogueStruct("Whatcha got in here?", 'd', Color.White, 'p', false, "", 135, 90),
            new DialogueStruct("Boring stuff.", 'd', Color.White, 'r', false, "", 405, 180),
            new DialogueStruct("( Probably could have guessed . . . )", 'd', Color.DodgerBlue, 'p', true, "", 135, 180)
        };
        int[] crate_bps = { 0, 2 };

        DialogueStruct[] dialogue_bed = {
            new DialogueStruct("It's a bed.", 'd', Color.White, 'c'),
            new DialogueStruct("Kanna probably sleeps in this.", 'd', Color.White, 'c', true),
            new DialogueStruct("Search the bed.\nDo not.", 'o', Color.White, 'l', false, "bed 0|exit"),
            new DialogueStruct("You start searching the bed.", 'd', Color.White, 'c'),
            new DialogueStruct(". . . Oh?", 'd', Color.White, 'c'),
            new DialogueStruct("There's something under the bed frame.", 'd', Color.White, 'c'),
            new DialogueStruct("Trigo, what are you doing?", 'd', Color.White, 'r', false, "", 405, 180),
            new DialogueStruct("Nothing!", 'd', Color.White, 'p', false, "", 315, 90),
            new DialogueStruct("( Should probably check this out when Kanna isn't\n  looking . . . )", 'd', Color.DodgerBlue, 'p', true, "", 315, 135),

            // index 9
            new DialogueStruct("( Think the coast is clear . . . )", 'd', Color.DodgerBlue, 'p', false, "", 315, 180),
            new DialogueStruct("( Oh? )", 'd', Color.DodgerBlue, 'p', false, "", 315, 0),
            new DialogueStruct("( There's a key under the bed . . . )", 'd', Color.DodgerBlue, 'p', false, "", 315, 0),
            new DialogueStruct("Grab the key.\nDo not.", 'o', Color.White, 'l', false, "bed 1|exit"),

            // index 13
            new DialogueStruct("It's a bed.", 'd', Color.White, 'c'),
            new DialogueStruct("Kanna probably sleeps in this.", 'd', Color.White, 'c', true),
        };
        int[] bed_bps = { 0, 2, 8, 9, 13 };

        DialogueStruct[] dialogue_table = {
            new DialogueStruct("It's a crate.", 'd', Color.White, 'c'),
            new DialogueStruct("It's positioned so that Kanna\ncan use it as a reading table.", 'd', Color.White, 'c', true),
            new DialogueStruct("What kind of books do you read?", 'd', Color.White, 'p', false, "", 135, 90),
            new DialogueStruct("Me?", 'd', Color.White, 'r', false, "", 405, 135),
            new DialogueStruct("I, uh.\nDon't really read that much, actually.", 'd', Color.White, 'r', false, "", 405, 90),
            new DialogueStruct("Oh, okay.", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("( Guess it's not a reading table then . . . ? )", 'd', Color.DodgerBlue, 'p', true, "", 135, 180)
        };
        int[] table_bps = { 0, 2 };

        DialogueStruct[] dialogue_shelf = {
            new DialogueStruct("It's a bookshelf.\nThe bottom two shelves are full of books.\nAnd the top appears to have knick-knacks.", 'd', Color.White, 'l'),
            new DialogueStruct("Look through the bottom two shelves.\nLook through the top shelf.\nDo not look.", 'o', Color.White, 'l', false, "bookshelf 0|bookshelf 1|exit"),
            new DialogueStruct("You thumb through the books.", 'd', Color.White, 'c'),
            new DialogueStruct("Most of these are text books written by someone named\n\"Doctor Arapacia\".\nSeems boring.", 'd', Color.White, 'l'),
            new DialogueStruct("On the second shelf is a book called \"Spott Killman III:\nthe Endless Masquerade\".", 'd', Color.White, 'l'),
            new DialogueStruct("Looks like a graphic novel.\nThere's a badass wolf man on the cover with big muscles.\nHe has grey fur with a patch of white over one eye.", 'd', Color.White, 'l'),
            //new DialogueStruct("( Spot Killman was never really my thing, honestly.\n  I'm more of an Azure Files kind of guy. )", 'd', Color.DodgerBlue, 'p', false, "", 90, 0),
            new DialogueStruct("( How did this end up down here? )", 'd', Color.DodgerBlue, 'p', false, "", 135, 0),
            new DialogueStruct("( More importantly, has Kanna read the Lavender\n  Sunrise series yet?\n  It's WAY better than Spott Killman . . . )", 'd', Color.DodgerBlue, 'p', true, "", 135, 90),

            // index 8
            new DialogueStruct("On the top shelf is a singular plush toy of a red demon.", 'd', Color.White, 'c'),
            new DialogueStruct("Woah, Kanna is this yours?", 'd', Color.White, 'p', false, "", 135, 90),
            new DialogueStruct("What?\nN-no . . .", 'd', Color.White, 'r', false, "", 270, 225),
            new DialogueStruct("Oh.\nCan I have it, then?", 'd', Color.White, 'p', false, "", 135, 0),
            new DialogueStruct("NO!", 'd', Color.White, 'r', false, "", 270, 0),
            new DialogueStruct("I mean, no.\nYou can't take it.", 'd', Color.White, 'r', false, "", 270, 225),
            new DialogueStruct("( Sheesh.\n  Kanna seems really embarrassed to admit she \n  likes this thing . . . )", 'd', Color.DodgerBlue, 'p', true, "", 135, 180),

            // index 15
            new DialogueStruct("It's a bookshelf.\nThe bottom two shelves are full of books.\nAnd the top appears to have knick-knacks.", 'd', Color.White, 'l', true),
        };
        int[] shelf_bps = { 0, 15 };

        DialogueStruct[] dialogue_dummy = {
            new DialogueStruct("It's a training dummy.", 'd', Color.White, 'c'),
            new DialogueStruct("Kanna probably uses this for target practice.\nMaybe someday, you'll be able to hit it too!", 'd', Color.White, 'c', true),
        };
        int[] dummy_bps = { 0 };

        DialogueStruct[] dialogue_plush = {
            new DialogueStruct("It's a plush toy of a demon.", 'd', Color.White, 'c', true),
            new DialogueStruct("( Someone REALLY likes plushies . . . )", 'd', Color.DodgerBlue, 'p', true, "", 135, 180),
        };
        int[] plush_bps = { 0, 1 };

        DialogueStruct[] dialogue_slime_plush = {
            new DialogueStruct("It's a large plush toy of a slime.", 'd', Color.White, 'c'),
            new DialogueStruct("Looks comfortable.", 'd', Color.White, 'c', true),
            new DialogueStruct("( I bet this would make for an AWESOME bean bag chair. )", 'd', Color.DodgerBlue, 'p', true, "", 135, 180)
        };
        int[] slime_plush_bps = { 0, 2 };

        DialogueStruct[] dialogue_small_plush = {
            new DialogueStruct("It's a small plush toy of a slime.", 'd', Color.White, 'c'),
            new DialogueStruct("It's small enough that you could take it with you,\nif you wanted to.", 'd', Color.White, 'c'),
            new DialogueStruct("Take the plush.\nDo not.", 'o', Color.White, 'l', false, "plush|exit"),
            new DialogueStruct("You got the Slime Plush.", 'd', Color.White, 'c', true),
        };

        // dialogue_key is inside the key object -- i know, confusing...

        private readonly Dictionary<Room, int> keys_in_room = new Dictionary<Room, int>();

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
                                var crumb = new Crumble(temp, this);
                                string s = "crumble";

                                if (l.objects[i].properties.Length != 0)
                                {
                                    crumb.behind_styx = true;
                                    s = "crumble_b";
                                }

                                AddSpecialWall(crumb);
                                special_walls_bounds.Add(temp);
                                special_walls_types.Add(s);
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


                                var mouth = 0;
                                if (l.objects[i].properties.Length != 0)
                                    mouth = int.Parse(l.objects[i].properties[0].value);

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

                                int key_init = 0;
                                bool kannas = false;

                                if (l.objects[i].properties.Count() != 0)
                                    key_init = int.Parse(l.objects[i].properties[0].value);

                                if (l.objects[i].properties.Count() > 1)
                                {
                                    kannas = true;
                                    if (prog_manager.GetFlag(FLAGS.kanna_bed))
                                        continue;
                                }
                                    

                                for (int j = 0; j < temp.Width; j += 16)
                                    for (int k = 0; k < temp.Height; k += 16)
                                    {
                                        Rectangle temp2 = new Rectangle(temp.X + j, temp.Y + k, 16, 16);
                                        var new_lock = new Lock(temp2, this, lock_block_frame);

                                        AddSpecialWall(new_lock);
                                        special_walls_bounds.Add(temp2);

                                        if (kannas)
                                            special_walls_types.Add("lock_k");
                                        else
                                            special_walls_types.Add("lock");

                                        if (key_init != 0)
                                        {
                                            new_lock.SetKeys(key_init);
                                            new_lock.keys_set = true;
                                            key_inits.Add(key_init);
                                        }
                                        else
                                            key_inits.Add(0);

                                    }
                            }

                            if (l.objects[i].name == "key")
                            {
                                var temp = new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y);
                                AddKey(new Key(temp, this, key_frame));
                                key_locations.Add(temp);
                            }

                            if (l.objects[i].name == "kanna" && !prog_manager.GetFlag(FLAGS.kanna_defeated))
                            {
                                var temp = new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y);
                                AddEnemy(new Kanna_Boss(temp, player, this));
                                enemy_locations.Add(temp);
                                enemy_types.Add("kanna");
                            }

                            if (l.objects[i].name == "mushroom_boss" && !prog_manager.GetFlag(FLAGS.mushroom_defeated))
                            {
                                var temp = new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y);
                                AddEnemy(new Mushroom_Boss(temp, player, this));
                                enemy_locations.Add(temp);
                                enemy_types.Add("mushroom_boss");
                            }

                            if (l.objects[i].name == "famine" && !prog_manager.GetFlag(FLAGS.famine_defeated))
                            {
                                var temp = new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y);
                                AddEnemy(new Famine(temp, player, this));
                                enemy_locations.Add(temp);
                                enemy_types.Add("famine");
                            }

                            if (l.objects[i].name == "kanna_trigger")
                                kanna_trigger = new Rectangle((int)l.objects[i].x + t.location.X,
                                                                (int)l.objects[i].y + t.location.Y,
                                                                (int)l.objects[i].width,
                                                                (int)l.objects[i].height);

                            if (l.objects[i].name == "mushroom_trigger")
                                mushroom_trigger = new Rectangle((int)l.objects[i].x + t.location.X,
                                                                (int)l.objects[i].y + t.location.Y,
                                                                (int)l.objects[i].width,
                                                                (int)l.objects[i].height);

                            if (l.objects[i].name == "kanna_zone")
                                kanna_zone = new Rectangle((int)l.objects[i].x + t.location.X,
                                                                (int)l.objects[i].y + t.location.Y,
                                                                (int)l.objects[i].width,
                                                                (int)l.objects[i].height);

                            if (l.objects[i].name == "mushroom_zone")
                                mushroom_zone = new Rectangle((int)l.objects[i].x + t.location.X,
                                                                (int)l.objects[i].y + t.location.Y,
                                                                (int)l.objects[i].width,
                                                                (int)l.objects[i].height);

                            if (l.objects[i].name == "kanna_boss_blocks")
                                kanna_boss_blocks = new Rectangle((int)l.objects[i].x + t.location.X,
                                                                  (int)l.objects[i].y + t.location.Y,
                                                                  (int)l.objects[i].width,
                                                                  (int)l.objects[i].height);

                            if (l.objects[i].name == "hideout_trigger")
                                hideout_trigger = new Rectangle((int)l.objects[i].x + t.location.X,
                                                                  (int)l.objects[i].y + t.location.Y,
                                                                  (int)l.objects[i].width,
                                                                  (int)l.objects[i].height);

                            if (l.objects[i].name == "charon_door_trigger")
                                charon_door_trigger = new Rectangle((int)l.objects[i].x + t.location.X,
                                                                  (int)l.objects[i].y + t.location.Y,
                                                                  (int)l.objects[i].width,
                                                                  (int)l.objects[i].height);

                            if (l.objects[i].name == "key_pickup" && !prog_manager.GetFlag(FLAGS.locks))
                            {
                                var temp = new KeyPickup(new Vector2((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y),
                                                            this,
                                                            prog_manager,
                                                            dialogue_pickup,
                                                            0
                                                            );
                                AddInteractable(temp);
                            }

                            if (l.objects[i].name == "shade_pickup" && !prog_manager.GetFlag(FLAGS.jump_blocks))
                            {
                                var temp = new ShadePickup(new Vector2((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y),
                                                            this,
                                                            prog_manager,
                                                            dialogue_pickup,
                                                            1
                                                            );
                                AddInteractable(temp);
                            }

                            if (l.objects[i].name == "deadguy")
                            {
                                var temp = new DeadGuyTwo(
                                    new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, 32, 32),
                                    dialogue_deadguy, prog_manager, this);
                                AddEnemy(temp);
                                enemy_locations.Add(new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y));
                                enemy_types.Add("deadguy");
                            }

                            if (l.objects[i].name == "lukas_cutscene_pickup" && !prog_manager.GetFlag(FLAGS.locks))
                            {
                                var temp = new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y);
                                AddEnemy(new Lukas_Cutscene(temp, this, "pickup"));
                                enemy_locations.Add(temp);
                                enemy_types.Add("lukas_cutscene_pickup");
                            }

                            if (l.objects[i].name == "kanna_cutscene_hideout" && !prog_manager.GetFlag(FLAGS.dash))
                            {
                                var temp = new Vector2(l.objects[i].x + t.location.X, l.objects[i].y + t.location.Y);
                                var kanna = new Kanna_Cutscene(temp, this, player, "hideout", dialogue_hideout);
                                AddEnemy(kanna);
                                kanna_cutscene = kanna;
                                enemy_locations.Add(temp);
                                enemy_types.Add("kanna_cutscene_hideout");
                            }

                            if (l.objects[i].name == "guy" && l.objects[i].properties.Length != 0)
                            {
                                var temp_loc = new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height);
                                var guy = new InteractableGuy(temp_loc, this);
                                string value = l.objects[i].properties[0].value;

                                if (value == "fisher")
                                {
                                    if (!prog_manager.GetFlag(FLAGS.charons_blessing))
                                        guy.SetType(dialogue_fisher, fisher_bps);
                                    else
                                        guy.SetType(dialogue_fisher, fisher_bps_secret);

                                    guy.SetGuyInfo(
                                        temp_loc,
                                        new Rectangle(0, 0, 32, 96),
                                        true,
                                        6,
                                        4
                                        );
                                }

                                if (value == "frog")
                                {
                                    guy.SetType(dialogue_charon_gossip, frog_bps);

                                    guy.SetGuyInfo(
                                        temp_loc,
                                        new Rectangle(128, 0, 32, 32),
                                        true,
                                        1f,
                                        2
                                        );
                                }

                                if (value == "emo_imp")
                                {
                                    guy.SetType(dialogue_charon_gossip, gossiper_bps);

                                    guy.SetGuyInfo(
                                        temp_loc,
                                        new Rectangle(128, 32, 16, 32),
                                        false
                                        );
                                }

                                if (value == "downer")
                                {
                                    guy.SetType(dialogue_look_down, look_down_bps);

                                    guy.SetGuyInfo(
                                        temp_loc,
                                        new Rectangle(0, 96, 16, 32),
                                        true,
                                        6,
                                        4
                                        );
                                }


                                AddInteractable(guy);
                            }

                            if (l.objects[i].name == "charonblock" && !prog_manager.GetFlag(FLAGS.charon_door))
                            {
                                var temp = new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height);
                                AddSpecialWall(new CharonBlock(temp, this, charon_frame));

                                special_walls_bounds.Add(temp);
                                special_walls_types.Add("charonblock");
                            }

                            if (l.objects[i].name == "small_plush" && !prog_manager.GetFlag(FLAGS.kanna_plushie))
                            {
                                var temp = new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, 8, 8);
                                AddEnemy(new SmallPlush(temp, dialogue_small_plush, prog_manager, this));

                                enemy_locations.Add(new Vector2((int)l.objects[i].x + t.location.X, (int)l.objects[i].y));
                                enemy_types.Add("small_plush");
                            }

                            if (l.objects[i].name == "furniture")
                            {
                                var temp = new Furniture(new Rectangle((int)l.objects[i].x + t.location.X, (int)l.objects[i].y + t.location.Y, (int)l.objects[i].width, (int)l.objects[i].height), this);
                                string value = l.objects[i].properties[0].value;

                                temp.kannas = true;

                                if (value == "chair")
                                    temp.SetType(dialogue_chair, chair_bps);

                                if (value == "crate")
                                    temp.SetType(dialogue_crate, crate_bps);

                                if (value == "bed")
                                {
                                    temp.SetType(dialogue_bed, bed_bps);
                                    temp.kannas_bed = true;
                                }
                                    

                                if (value == "dummy")
                                    temp.SetType(dialogue_dummy, dummy_bps);

                                if (value == "shelf")
                                {
                                    temp.SetType(dialogue_shelf, shelf_bps);
                                    temp.kannas_shelf = true;
                                }

                                if (value == "plush")
                                    temp.SetType(dialogue_plush, plush_bps);

                                if (value == "slime_plush")
                                    temp.SetType(dialogue_slime_plush, slime_plush_bps);


                                if (value == "reading_table")
                                    temp.SetType(dialogue_table, table_bps);

                                AddInteractable(temp);
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
            spr_kanna = root.Content.Load<Texture2D>("sprites/spr_kanna");
            spr_lukas = root.Content.Load<Texture2D>("sprites/spr_lukas");
            spr_guys = root.Content.Load<Texture2D>("sprites/spr_guys");

            asset_map.Add(typeof(Walker), spr_mushroom);
            asset_map.Add(typeof(Trampoline), spr_mushroom);
            asset_map.Add(typeof(GhostBlock), tst_styx);
            asset_map.Add(typeof(Kanna_Boss), spr_kanna);
            asset_map.Add(typeof(DeadGuyTwo), tst_styx);
            asset_map.Add(typeof(Mushroom_Boss), spr_mushroom);
            asset_map.Add(typeof(Mushroom_Body), spr_mushroom);
            asset_map.Add(typeof(Mushroom_Hand), spr_mushroom);
            asset_map.Add(typeof(Lukas_Cutscene), spr_lukas);
            asset_map.Add(typeof(Kanna_Cutscene), spr_kanna);
            asset_map.Add(typeof(Famine), spr_lukas);
            asset_map.Add(typeof(Famine_Head), spr_lukas);
            asset_map.Add(typeof(SmallPlush), tst_styx);

            asset_map.Add(typeof(ShadePickup), tst_styx);
            asset_map.Add(typeof(KeyPickup), tst_styx);
            asset_map.Add(typeof(InteractableGuy), spr_guys);
            asset_map.Add(typeof(Furniture), tst_styx);

            foreach (Enemy enemy in enemies)
                enemy.LoadAssets(asset_map[enemy.GetType()]);

            foreach (Wall wall in special_walls)
            {
                var temp = wall.GetType();

                if (temp == typeof(Stem))
                    wall.Load(spr_mushroom);

                else
                    wall.Load(tst_styx);
            }

            foreach (Key key in keys)
            {
                key.Load(tst_styx);

                Room r = RealGetRoom(new Vector2(key.bounds.X, key.bounds.Y));

                if (keys_in_room.ContainsKey(r))
                    keys_in_room[r]++;
                else
                    keys_in_room.Add(r, 1);
            }

            for (int i = 0; i < interactables.Count; i++)
                interactables[i].LoadAssets(asset_map[interactables[i].GetType()]);
                

            for (int i = 0; i < chunks.Count; i++)
                chunks[i].Load(black);

            for (int i = 0; i < checkpoints.Count; i++)
                checkpoints[i].Load(checkpoint);

            foreach (Checkpoint c in checkpoints)
                if (c.sideways)
                    c.GetSidewaysWall();

            foreach (Wall w in special_walls)
                if (w.GetType() == typeof(Lock) && !w.keys_set)
                {
                    Room r = RealGetRoom(new Vector2(w.bounds.X, w.bounds.Y));

                    if (keys_in_room.ContainsKey(r))
                        w.SetKeys(keys_in_room[r]);
                    else
                        w.SetKeys(0);
                }
        }

        public override void Update(GameTime gameTime)
        {
            saved_gameTime = gameTime;

            base.Update(gameTime);

            if (name == "rm_styx1")
                prog_manager.SetFlag(FLAGS.mask);

            if (lukas_cutscene != null && dialogue)
                lukas_cutscene.Update(gameTime);

            river_timer += (float)gameTime.ElapsedGameTime.TotalSeconds * 10;

            river_frame_oset = (int)river_timer;

            if (river_frame_oset >= 64)
            {
                river_frame_oset = 0;
                river_timer = 0;

                river_timer += (float)gameTime.ElapsedGameTime.TotalSeconds * 10;
                river_frame_oset = (int)river_timer;
            }

            if (prog_manager.GetFlag(FLAGS.locks) && !(player_dead && !finish_player_dead))
            {
                List<Key> killed_keys = KeyCheckCollision(player.HitBox);

                for (int i = killed_keys.Count() - 1; i >= 0; i--)
                {
                    Room r = RealGetRoom(new Vector2(killed_keys[i].bounds.X, killed_keys[i].bounds.Y));

                    for (int j = special_walls.Count() - 1; j >= 0; j--)
                        if (special_walls[j].GetType() == typeof(Lock) && special_walls[j].bounds.Intersects(r.bounds))
                            special_walls[j].DecrementKeys();

                    killed_keys[i].Die();
                }
            }
            

            for (int i = keys.Count() - 1; i >= 0; i--)
                keys[i].Update(gameTime);
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
            kanna_cutscene = null;

            // reset breakable walls
            for (int i = special_walls.Count - 1; i >= 0; i--)
                RemoveSpecialWall(special_walls[i]);

            // remove enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
                RemoveEnemy(enemies[i]);

            for (int i = keys.Count - 1; i >= 0; i--)
                RemoveKey(keys[i]);

            int mouth_counter = 0;
            int key_init_counter = 0;

            // replace special walls
            for (int i = 0; i < special_walls_bounds.Count; i++)
            {
                if (special_walls_types[i] == "crumble")
                    AddSpecialWall(new Crumble(special_walls_bounds[i], this));

                if (special_walls_types[i] == "crumble_b")
                {
                    var crumb = new Crumble(special_walls_bounds[i], this);
                    crumb.behind_styx = true;
                    AddSpecialWall(crumb);
                }
                    

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

                else if (special_walls_types[i] == "lock" || (special_walls_types[i] == "lock_k" && !prog_manager.GetFlag(FLAGS.kanna_bed)))
                {
                    var new_lock_temp = new Lock(special_walls_bounds[i], this, lock_block_frame);
                    AddSpecialWall(new_lock_temp);

                    if (key_inits[key_init_counter] != 0)
                    {
                        new_lock_temp.SetKeys(key_inits[key_init_counter]);
                        new_lock_temp.keys_set = true;
                    }

                    key_init_counter++;
                }

                else if (special_walls_types[i] == "lock_k" && prog_manager.GetFlag(FLAGS.kanna_bed))
                {
                    key_init_counter++;
                }
                    

                if (special_walls_types[i] == "oneway")
                    AddSpecialWall(new OneWay(special_walls_bounds[i], new Rectangle(8, 184, 8, 8), this));

                if (special_walls_types[i] == "charonblock" && !prog_manager.GetFlag(FLAGS.charon_door))
                    AddSpecialWall(new CharonBlock(special_walls_bounds[i], this, charon_frame));
            }


            foreach (Wall wall in special_walls)
            {
                var temp = wall.GetType();
                

                if (temp == typeof(Stem))
                    wall.Load(spr_mushroom);

                else
                    wall.Load(tst_styx);
            }


            
            // replace enemies
            for (int i = 0; i < enemy_locations.Count; i++)
            {

                // re-add enemies

                if (enemy_types[i] == "walker")
                    AddEnemy(new Walker(enemy_locations[i], this));

                if (enemy_types[i] == "kanna" && !prog_manager.GetFlag(FLAGS.kanna_defeated))
                    AddEnemy(new Kanna_Boss(enemy_locations[i], player, this));

                if (enemy_types[i] == "mushroom_boss" && !prog_manager.GetFlag(FLAGS.mushroom_defeated))
                    AddEnemy(new Mushroom_Boss(enemy_locations[i], player, this));

                if (enemy_types[i] == "famine" && !prog_manager.GetFlag(FLAGS.famine_defeated))
                    AddEnemy(new Famine(enemy_locations[i], player, this));

                //if (enemy_types[i] == "trampoline")
                //    AddEnemy(new Trampoline(enemy_locations[i], this));

                if (enemy_types[i] == "deadguy")
                {
                    var temp = new DeadGuyTwo(
                                    new Rectangle((int)enemy_locations[i].X, (int)enemy_locations[i].Y, 32, 32),
                                    dialogue_deadguy, prog_manager, this);
                    AddEnemy(temp);
                }

                if (enemy_types[i] == "small_plush" && !prog_manager.GetFlag(FLAGS.kanna_plushie))
                {
                    var temp = new SmallPlush(
                                    new Rectangle((int)enemy_locations[i].X, (int)enemy_locations[i].Y, 8, 8),
                                    dialogue_small_plush, prog_manager, this);
                    AddEnemy(temp);
                }

                if (enemy_types[i] == "lukas_cutscene_pickup" && !prog_manager.GetFlag(FLAGS.locks))
                    AddEnemy(new Lukas_Cutscene(enemy_locations[i], this, "pickup"));

                if (enemy_types[i] == "kanna_cutscene_hideout" && !prog_manager.GetFlag(FLAGS.dash))
                {
                    var temp = new Kanna_Cutscene(enemy_locations[i], this, player, "hideout", dialogue_hideout);
                    AddEnemy(temp);
                    kanna_cutscene = temp;
                }
                    
            }

            // replace keys
            for (int i = 0; i < key_locations.Count; i++)
            {
                var temp = new Key(key_locations[i], this, key_frame);
                temp.Load(tst_styx);
                AddKey(temp);
            }

            foreach (Wall w in special_walls)
                if (w.GetType() == typeof(Lock) && !w.keys_set)
                {
                    Room r = RealGetRoom(new Vector2(w.bounds.X, w.bounds.Y));

                    if (keys_in_room.ContainsKey(r))
                        w.SetKeys(keys_in_room[r]);
                    else
                        w.SetKeys(0);
                }


            foreach (Enemy enemy in enemies)
                enemy.LoadAssets(asset_map[enemy.GetType()]);

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


                //audio_manager.PlaySound("tick");
                    

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

                //audio_manager.PlaySound("tock");
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
                if (code.Length > 1)
                    dialogue_exit_code = code[1];

                player.LeaveDialogue();
                dialogue = false;
                dialogue_letter = 0f;
                dialogue_num = 0;
                return;
            }

            if (code[0] == "lukas1" && code.Length > 1)
            {
                if (code[1] == "1")
                {
                    dialogue_num++;
                    dialogue_letter = 0f;
                    return;
                }

                else if (code[1] == "0")
                {
                    dialogue_num = 27;
                    dialogue_letter = 0f;
                    return;
                }

                else if (code[1] == "2")
                {
                    dialogue_num = 19;
                    dialogue_letter = 0f;
                    return;
                }
            }

            if (code[0] == "lukas2" && code.Length > 1)
            {
                dialogue_num = 13;
                dialogue_letter = 0f;
                return;
            }

            if (code[0] == "lukas3" && code.Length > 1)
            {
                if (code[1] == "1")
                {
                    dialogue_num++;
                    dialogue_letter = 0f;
                    return;
                }

                else
                {
                    player.LeaveDialogue();
                    dialogue = false;
                    dialogue_letter = 0f;
                    dialogue_num = 0;
                    return;
                }
            }

            if (code[0] == "hideout0" && code.Length > 1)
            {

                if (code[1] == "0")
                {
                    dialogue_num = 6;
                    dialogue_letter = 0f;
                    prog_manager.SetFlag(FLAGS.stay_out_of_way);
                    return;
                }

                if (code[1] == "1")
                {
                    dialogue_num = 8;
                    dialogue_letter = 0f;
                    prog_manager.SetFlag(FLAGS.ask_hideout);
                    return;
                }

                if (code[1] == "2")
                {
                    player.LeaveDialogue();
                    dialogue = false;
                    dialogue_letter = 0f;
                    dialogue_num = 0;
                    return;
                }
            }

            if (code[0] == "hideout1" && code.Length > 1)
            {

                if (code[1] == "0")
                {
                    dialogue_num++;
                    dialogue_letter = 0f;
                    return;
                }

                if (code[1] == "1")
                {
                    dialogue_num = 13;
                    dialogue_letter = 0f;
                    return;
                }

                if (code[1] == "2")
                {
                    dialogue_num = 16;
                    dialogue_letter = 0f;
                    return;
                }
            }

            if (code[0] == "hideout2" || code[0] == "hideout3" || code[0] == "hideout4")
            {
                dialogue_num++;
                dialogue_letter = 0f;

                if (code[0] == "hideout2")
                    prog_manager.SetFlag(FLAGS.ask_leaving);


                return;
            }

            if (code[0] == "hideout5")
            {
                dialogue_num = 49;
                dialogue_letter = 0f;

                return;
            }

            if (code[0] == "hideout6")
            {
                // start a cutscene here
                player.LeaveDialogue();
                dialogue = false;
                dialogue_letter = 0f;
                dialogue_num = 0;

                saved_choice = code[1] == "0";

                HandleCutscene("learndash|empty|empty|empty|empty|empty", saved_gameTime, true);

                return;
            }

            if (code[0] == "bookshelf")
            {
                if (code[1] == "0")
                {
                    dialogue_num++;
                    dialogue_letter = 0f;

                    prog_manager.SetFlag(FLAGS.spott_killman);
                }
                else
                {
                    dialogue_num = 8;
                    dialogue_letter = 0f;
                }
            }

            if (code[0] == "corpse")
            {
                if (code[1] == "1")
                {
                    player.LeaveDialogue();
                    dialogue = false;
                    dialogue_letter = 0f;
                    dialogue_num = 0;
                    return;
                }

                if (code[1] == "0")
                {
                    dialogue_num++;
                    dialogue_letter = 0f;
                    prog_manager.SetFlag(FLAGS.map_obtained);
                    return;
                }
            }

            if (code[0] == "bed")
            {
                if (code[1] == "0")
                {
                    // mark that you did this
                    prog_manager.SetFlag(FLAGS.investigated_bed);

                    dialogue_num++;
                    dialogue_letter = 0f;
                }

                if (code[1] == "1")
                {
                    // remove the locks
                    Room r = RealGetRoom(player.GetPos());

                    for (int j = special_walls.Count() - 1; j >= 0; j--)
                        if (special_walls[j].GetType() == typeof(Lock) && special_walls[j].bounds.Intersects(r.bounds))
                            special_walls[j].DecrementKeys();

                    prog_manager.SetFlag(FLAGS.kanna_bed);

                    player.LeaveDialogue();
                    dialogue = false;
                    dialogue_letter = 0f;
                    dialogue_num = 0;
                    return;
                }
            }

            if (code[0] == "plush")
            {
                prog_manager.SetFlag(FLAGS.kanna_plushie);

                dialogue_num++;
                dialogue_letter = 0f;

                for (int i = enemies.Count - 1; i >= 0; i--)
                    if (enemies[i].GetType() == typeof(SmallPlush))
                    {
                        RemoveEnemy(enemies[i]);
                        break;
                    }
            }
        }

        public override void HandleCutscene(string code, GameTime gameTime, bool start)
        {
            base.HandleCutscene(code, gameTime, start);

            if (cutscene_code[0] == "fightkanna")
            {
                if (cutscene_timer > 0f)
                {
                    player.SetNoInput();
                    player.DoMovement(gameTime);
                    player.DoAnimate(gameTime);

                }

                if (cutscene_timer > 0.9f)
                    player.SetLastHdir(1);

                if (cutscene_timer > 1.7f)
                {
                    if (cutscene_code[1] == "empty")
                    {
                        cutscene_code[1] = "-";
                        StartDialogue(dialogue_kanna_fight, 0, 'c', 25f, true);
                    }
                    
                }

                if (cutscene_timer > 2.6f)
                {
                    if (cutscene_code[2] == "empty")
                    {
                        cutscene_code[2] = "-";

                        BossBlock temp1 = new BossBlock(new Rectangle(kanna_boss_blocks.X, kanna_boss_blocks.Y, 16, 16), this, new Rectangle(32, 144, 16, 16));
                        BossBlock temp2 = new BossBlock(new Rectangle(kanna_boss_blocks.X, kanna_boss_blocks.Y + 16, 16, 16), this, new Rectangle(32, 144, 16, 16));

                        temp1.Load(tst_styx);
                        temp2.Load(tst_styx);

                        AddSpecialWall(temp1);
                        AddSpecialWall(temp2);
                    }

                    else
                    {
                        for (int i = special_walls.Count - 1; i >= 0; i--)
                            special_walls[i].Update(gameTime);
                    }
                }

                if (cutscene_timer > 3.2f)
                {
                    player.ExitCutscene();
                    cutscene = false;
                    door_trans = false;
                    kanna_boss.Trigger();
                    kanna_boss = null;
                    prog_manager.SetFlag(FLAGS.kanna_started);
                }
            }

            if (cutscene_code[0] == "fightkanna_short")
            {
                if (cutscene_timer > 0f)
                {
                    player.SetNoInput();
                    player.DoMovement(gameTime);
                    player.DoAnimate(gameTime);
                }

                if (cutscene_timer > 0.4f)
                {
                    if (cutscene_code[1] == "empty")
                    {
                        cutscene_code[1] = "-";

                        BossBlock temp1 = new BossBlock(new Rectangle(kanna_boss_blocks.X, kanna_boss_blocks.Y, 16, 16), this, new Rectangle(32, 144, 16, 16));
                        BossBlock temp2 = new BossBlock(new Rectangle(kanna_boss_blocks.X, kanna_boss_blocks.Y + 16, 16, 16), this, new Rectangle(32, 144, 16, 16));

                        temp1.Load(tst_styx);
                        temp2.Load(tst_styx);

                        AddSpecialWall(temp1);
                        AddSpecialWall(temp2);
                    }

                    else
                    {
                        for (int i = special_walls.Count - 1; i >= 0; i--)
                            special_walls[i].Update(gameTime);
                    }
                }

                if (cutscene_timer > 0.8f)
                {
                    player.ExitCutscene();
                    cutscene = false;
                    door_trans = false;
                    kanna_boss.Trigger();
                    kanna_boss = null;
                }
            }

            if (cutscene_code[0] == "defeatkanna")
            {
                if (cutscene_timer > 0f)
                {
                    player.SetNoInput();
                    player.DoMovement(gameTime);
                    player.DoAnimate(gameTime);

                    kanna_boss.HandleFlash(gameTime);
                    kanna_boss.DoPhysics(gameTime);

                    if (cutscene_code[1] != "-")
                    {
                        cutscene_code[1] = "-";
                        StartDialogue(dialogue_kanna_fight_done, 0, 'c', 25f, false);
                    }
                }

                if (cutscene_timer > 1f)
                {
                    if (cutscene_code[2] != "-")
                    {
                        cutscene_code[2] = "-";
                        StartDialogue(dialogue_kanna_fight_done, 1, 'c', 25f, true);
                    }
                }

                if (cutscene_timer > 1.6f && cutscene_timer <= 4.95f)
                {
                    kanna_boss.mask = false;
                }

                if (cutscene_timer > 3f)
                {
                    if (cutscene_code[3] != "-")
                    {
                        cutscene_code[3] = "-";

                        if (dialogue_exit_code == "0")
                            StartDialogue(dialogue_kanna_fight_done, 6, 'c', 25f, true);

                        else
                            StartDialogue(dialogue_kanna_fight_done, 7, 'c', 25f, true);
                    }
                }

                if (cutscene_timer > 3.4f)
                {
                    prog_manager.TakeOffMask();
                }

                if (cutscene_timer > 4f)
                {
                    if (cutscene_code[4] != "-")
                    {
                        cutscene_code[4] = "-";

                        if (dialogue_exit_code == "2")
                            StartDialogue(dialogue_kanna_fight_done, 12, 'c', 25f, true);

                        else
                            StartDialogue(dialogue_kanna_fight_done, 8, 'c', 25f, true);
                    }
                }

                if (cutscene_timer > 4.3f)
                {
                    if (cutscene_code[5] != "-")
                    {
                        cutscene_code[5] = "-";

                        for (int i = special_walls.Count - 1; i >= 0; i--)
                            if (special_walls[i].GetType() == typeof(BossBlock))
                                special_walls[i].DestroySelf();
                    }

                    else
                    {
                        for (int i = special_walls.Count - 1; i >= 0; i--)
                            special_walls[i].Update(gameTime);
                    }
                }

                if (cutscene_timer > 4.8f)
                {

                    if (kanna_boss_blocks.X > kanna_boss.Pos.X + 16)
                    {
                        cutscene_timer = 4.81f;
                        kanna_boss.DoWalk(gameTime);
                    }
                }

                if (cutscene_timer > 5.1f)
                {
                    kanna_boss.mask = true;
                }

                if (cutscene_timer > 5.3f)
                {

                    if (cutscene_code[6] != "-")
                    {
                        cutscene_code[6] = "-";

                        StartDialogue(dialogue_kanna_fight_done, 17, 'c', 25f, true);
                    }
                }

                if (cutscene_timer > 5.6f)
                {

                    if (kanna_boss_blocks.X + 64 > kanna_boss.Pos.X - 16)
                    {
                        cutscene_timer = 5.61f;
                        kanna_boss.DoWalk(gameTime);
                    }
                }

                if (cutscene_timer > 6.0f)
                {
                    player.ExitCutscene();
                    cutscene = false;
                    door_trans = false;
                    RemoveEnemy(kanna_boss);
                    kanna_boss = null;
                    prog_manager.SetFlag(FLAGS.kanna_defeated);

                    StartDialogue(dialogue_kanna_fight_done, 18, 'c', 25f, true);
                }

                
            }

            if (cutscene_code[0] == "lukaspickup")
            {

                // note that this doesn't cause lukas to update during dialogue
                // so there's also a special clause in this level's update function now
                // is that dumb? probably lol whatever
                if (lukas_cutscene != null)
                    lukas_cutscene.Update(gameTime);


                if (cutscene_timer > 0f && cutscene_code[1] == "empty")
                {
                    foreach (Enemy e in enemies)
                        if (e.GetType() == typeof(Lukas_Cutscene))
                            lukas_cutscene = (Lukas_Cutscene)e;

                    foreach (Interactable i in interactables)
                        if (i.GetType() == typeof(ShadePickup))
                            shade_pickup = (ShadePickup)i;

                    if (lukas_cutscene == null || shade_pickup == null)
                    {
                        cutscene = false;
                        player.ExitCutscene();
                        return;
                    }

                    lukas_cutscene.looking = true;

                    player.SetNoInput();
                    player.DoMovement(gameTime);
                    player.DoAnimate(gameTime);
                    StartDialogue(dialogue_lukas_pickup, 0, 'c', 25f, true);
                    cutscene_code[1] = "-";

                    saved_camera_x = cam.GetPos().X;
                    saved_camera_y = cam.GetPos().Y;
                }

                if (cutscene_timer > 0.03f)
                {
                    cutscene_cam = true;
                    cutscene_cam_speed = 10f;
                    cutscene_cam_pos = new Vector2(saved_camera_x - 96, saved_camera_y);
                }

                if (cutscene_timer > 1.3f && cutscene_code[2] == "empty")
                {
                    StartDialogue(dialogue_lukas_pickup, 1, 'c', 25f, true);
                    cutscene_code[2] = "-";
                }

                if (cutscene_timer > 1.8f)
                {
                    lukas_cutscene.magic = true;
                    shade_pickup.floating = true;
                }

                if (cutscene_timer > 3.5f && cutscene_code[3] == "empty")
                {
                    shade_pickup.Transform();
                    cutscene_code[3] = "-";
                }

                if (cutscene_timer > 4.5f)
                {
                    shade_pickup.floating = false;
                    lukas_cutscene.magic = false;
                }

                if (cutscene_timer > 5.8f && cutscene_code[4] == "empty")
                {
                    StartDialogue(dialogue_lukas_pickup, 14, 'c', 25f, true);
                    cutscene_code[4] = "-";
                }

                if (cutscene_timer > 5.83f)
                {
                    cutscene_cam_pos = new Vector2(saved_camera_x, saved_camera_y);
                }

                if (cutscene_timer > 7.0f)
                {
                    lukas_cutscene.looking = false;
                    lukas_cutscene = null;
                    cutscene_cam = false;
                    player.ExitCutscene();
                    cutscene = false;
                }
            }

            if (cutscene_code[0] == "enterhideout")
            {
                player.SetNoInput();
                player.DoMovement(gameTime);
                player.DoAnimate(gameTime);

                if (cutscene_timer > 0.8f)
                {
                    player.SetLastHdir(1);
                }

                if (cutscene_timer > 1.5f && cutscene_code[1] == "empty")
                {
                    StartDialogue(dialogue_hideout, 0, 'c', 25f, true);
                    cutscene_code[1] = "-";
                }


                if (cutscene_timer > 1.53f)
                {
                    Room r = RealGetRoom(player.GetPos());

                    if (null != r)
                        r.name = "Hideout";

                    player.ExitCutscene();
                    cutscene = false;
                    prog_manager.SetFlag(FLAGS.hideout_entered);
                }
                    
            }

            if (cutscene_code[0] == "fightmushroom")
            {
                if (cutscene_timer > 0f)
                {
                    player.SetNoInput();
                    player.DoMovement(gameTime);
                    player.DoAnimate(gameTime);


                    if (cutscene_code[1] == "empty")
                    {
                        saved_camera_x = cam.GetPos().X;
                        saved_camera_y = cam.GetPos().Y;
                        cutscene_code[1] = "-";
                    }
                }

                if (cutscene_timer > 0.9f)
                {
                    cutscene_cam = true;
                    cutscene_cam_speed = 10f;
                    cutscene_cam_pos = new Vector2(saved_camera_x - 128, saved_camera_y);
                }

                if (cutscene_timer > 2f && cutscene_timer < 3f)
                {
                    for (int i = enemies.Count - 1; i >= 0; i--)
                        enemies[i].Update(gameTime);
                }

                if (cutscene_timer > 3f)
                {
                    for (int i = enemies.Count - 1; i >= 0; i--)
                        if (enemies[i].GetType() == typeof(ArcProjectile))
                            enemies[i].Update(gameTime);
                }

                if (cutscene_timer > 4f && cutscene_timer < 4.8f)
                {
                    for (int i = enemies.Count - 1; i >= 0; i--)
                        if (enemies[i].GetType() == typeof(Mushroom_Boss))
                            enemies[i].Update(gameTime);
                }

                if (cutscene_timer > 5.4f)
                {
                    cutscene_cam_pos = new Vector2(saved_camera_x, saved_camera_y);
                }

                if (cutscene_timer > 6.7f)
                {
                    if (cutscene_code[2] == "empty")
                    {
                        for (int i = mushroom_trigger.X; i < mushroom_trigger.X + 32; i += 16)
                            for (int j = mushroom_trigger.Y; j < mushroom_trigger.Y + 48; j += 16)
                            {
                                BossBlock temp1 = new BossBlock(new Rectangle(i, j, 16, 16), this, new Rectangle(32, 144, 16, 16));
                                temp1.Load(tst_styx);
                                AddSpecialWall(temp1);
                            }

                        cutscene_code[2] = "-";
                    }

                    else
                    {
                        for (int i = special_walls.Count - 1; i >= 0; i--)
                            special_walls[i].Update(gameTime);
                    }
                }

                if (cutscene_timer > 7.5f)
                {
                    cutscene_cam = false;
                    player.ExitCutscene();
                    cutscene = false;

                    prog_manager.SetFlag(FLAGS.mushroom_started);
                }
            }

            if (cutscene_code[0] == "charondoor")
            {

                // nearly 10 second cutscene of a door opening
                // sorry speedrunners!!!

                if (cutscene_timer > 1.6f && cutscene_code[1] == "empty")
                {
                    if (charondoor != null)
                    {
                        var temp = charon_frame;
                        temp.X += 16;
                        charondoor.SetFrame(temp);

                        MaskFX fx = new MaskFX(new Vector2(charondoor.bounds.X, charondoor.bounds.Y + 11), 
                            tst_styx, 
                            this, 
                            new Rectangle(320, 168, 16, 16)
                            );

                        AddFX(fx);
                    }

                    cutscene_code[1] = "-";
                }

                if (cutscene_timer > 4f && charondoor != null)
                {
                    charondoor.MoveUp(gameTime, 0.11f);
                }

                if (cutscene_timer > 11.2f)
                {
                    player.ExitCutscene();
                    cutscene = false;

                    if (charondoor != null)
                    {
                        RemoveSpecialWall(charondoor);
                        charondoor = null;
                    }

                    prog_manager.SetFlag(FLAGS.charon_door);
                }
            }

            if (cutscene_code[0] == "learndash")
            {
                int wipe_width = door_trans_rect_1.Width;

                if (cutscene_timer > 0f && cutscene_code[1] == "empty")
                {
                    player.SetNoInput();
                    player.DoMovement(gameTime);
                    player.DoAnimate(gameTime);

                    int index = 61;
                    if (saved_choice)
                        index = 64;

                    StartDialogue(dialogue_hideout, index, 'c', 25f, true);
                    cutscene_code[1] = "-";
                }

                if (cutscene_timer > 0.3f && cutscene_timer < 1.7f)
                {
                    door_trans = true;
                    force_draw_player = true;

                    float mult = cutscene_timer - 0.3f;

                    door_trans_rect_1.X = (int)(cam.GetPos().X - wipe_width + (258 * mult));
                    door_trans_rect_2.X = (int)(cam.GetPos().X + 320 - (258 * mult));

                    door_trans_rect_1.Y = (int)cam.GetPos().Y;
                    door_trans_rect_2.Y = (int)cam.GetPos().Y;
                }


                if (cutscene_timer > 2f && cutscene_code[2] == "empty")
                {
                    StartDialogue(dialogue_hideout, 65, 'c', 25f, true);
                    cutscene_code[2] = "-";

                    prog_manager.SetFlag(FLAGS.dash);

                    for (int i = enemies.Count - 1; i > 0; i--)
                        if (enemies[i].GetType() == typeof(Kanna_Cutscene))
                        {
                            RemoveEnemy(enemies[i]);
                            break;
                        }
                }

                if (cutscene_timer > 2.3f && cutscene_timer < 3.7f)
                {
                    door_trans = true;

                    float mult = cutscene_timer - 2.3f;

                    door_trans_rect_1.X = (int)(cam.GetPos().X - 128 - (258 * mult));
                    door_trans_rect_2.X = (int)(cam.GetPos().X + 48 + (258 * mult));

                    door_trans_rect_1.Y = (int)cam.GetPos().Y;
                    door_trans_rect_2.Y = (int)cam.GetPos().Y;
                }

                if (cutscene_timer > 4f)
                {
                    door_trans = false;
                    force_draw_player = false;
                    player.ExitCutscene();
                    cutscene = false;
                }
            }
        }


        // --------- end mandatory overrides ------------


        // optional override
        public override void DrawTiles(SpriteBatch _spriteBatch, Texture2D tileset, Texture2D background)
        {
            // overwritten for special case drawing river styx

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
                if (enemies[i].GetType() != typeof(Mushroom_Hand))  // special case for mushroom boss
                    enemies[i].Draw(_spriteBatch);

            for (int i = interactables.Count - 1; i >= 0; i--)
                interactables[i].Draw(_spriteBatch);

            if ((!player_dead && !door_trans) || force_draw_player)
                player.Draw(_spriteBatch);

            int cam_x = (int)cam.GetPos().X;
            int cam_y = (int)cam.GetPos().Y;
            var camera_rect = new Rectangle(cam_x - 8, cam_y - 8, 320 + 16, 240 + 16);

            for (int i = keys.Count() - 1; i >= 0; i--)
                keys[i].Draw(_spriteBatch);

            for (int i = special_walls.Count - 1; i >= 0; i--)
                if (special_walls[i].behind_styx)
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

            for (int i = special_walls.Count - 1; i >= 0; i--)
                if (!special_walls[i].behind_styx)
                    special_walls[i].Draw(_spriteBatch);

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

                    for (int i = 0; i < interactables.Count; i++)
                        interactables[i].DebugDraw(_spriteBatch, black);
                }

            }

            // UI stuff
            if (overlay)
            {
                //int cam_x = (int)cam.GetPos().X;
                //int cam_y = (int)cam.GetPos().Y;


                var overlay_rect = new Rectangle(cam_x, cam_y, 320, 12);

                float opacity = 0.3f;

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
            if (!prog_manager.GetFlag(FLAGS.jump_blocks))
                return;

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



        // boss functions

        public void FightKanna(Kanna_Boss kanna, GameTime gameTime)
        {
            if (!prog_manager.GetFlag(FLAGS.kanna_started))
            {
                HandleCutscene("fightkanna|empty|empty", gameTime, true);
                kanna_boss = kanna;
            }
            else
            {
                HandleCutscene("fightkanna_short|empty", gameTime, true);
                kanna_boss = kanna;
            }
        }

        public void DefeatKanna(Kanna_Boss kanna, GameTime gameTime)
        {
            kanna_boss = kanna;
            HandleCutscene("defeatkanna|one|two|three|four|five|six", gameTime, true);
        }

        public void RemoveArrows()
        {
            for (int i = enemies.Count - 1; i > 0; i--)
                if (enemies[i].GetType() == typeof(Kanna_Projectile))
                    RemoveEnemy(enemies[i]);
        }

        public void EnterHideout(GameTime gameTime)
        {
            if (!prog_manager.GetFlag(FLAGS.hideout_entered))
            {
                HandleCutscene("enterhideout|empty", gameTime, true);
            }
        }

        public void FightMushroom(GameTime gameTime)
        {
            if (!prog_manager.GetFlag(FLAGS.mushroom_started))
            {
                HandleCutscene("fightmushroom|empty|empty", gameTime, true);
            }
            else
            {
                for (int i = mushroom_trigger.X; i < mushroom_trigger.X + 32; i += 16)
                    for (int j = mushroom_trigger.Y; j < mushroom_trigger.Y + 48; j += 16)
                    {
                        BossBlock temp1 = new BossBlock(new Rectangle(i, j, 16, 16), this, new Rectangle(32, 144, 16, 16));
                        temp1.Load(tst_styx);
                        AddSpecialWall(temp1);
                    }
            }
        }

        public void DefeatMushroom()
        {
            KeyPickup kpickup = new KeyPickup(
                new Vector2(mushroom_zone.X + (mushroom_zone.Width / 2) - 16, mushroom_zone.Y + mushroom_zone.Height - 16),
                this,
                prog_manager,
                dialogue_pickup,
                0
                );

            kpickup.LoadAssets(tst_styx);
            AddInteractable(kpickup);

            StartDialogue(dialogue_defeat, 0, 'c', 10f, false, false);

            prog_manager.SetFlag(FLAGS.mushroom_defeated);

            player.SetPogoed(0, false);
        }

        // end boss functions





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

        public void RemoveLukas()
        {
            foreach (Enemy e in enemies)
                if (e.GetType() == typeof(Lukas_Cutscene))
                {
                    RemoveEnemy(e);
                    break;
                }
        }
    }



    public class CharonBlock : BossBlock
    {
        new readonly private StyxLevel root;
        private bool triggered = false;

        private float saved_y;

        public CharonBlock(Rectangle bounds, StyxLevel root, Rectangle frame) : base(bounds, root, frame)
        {
            this.root = root;
            white = false;

            saved_y = bounds.Y;

            behind_styx = true;
        }

        public override void Update(GameTime gameTime)
        {
            Vector2 ppos = root.player.GetPos() + new Vector2(16, 16);

            if (!triggered && root.prog_manager.GetFlag(FLAGS.charons_blessing) && root.charon_door_trigger.Contains(ppos))
            {
                triggered = true;
                root.HandleCutscene("charondoor|empty|empty|empty|empty", gameTime, true);
                root.charondoor = this;
            }
        }

        public void SetFrame(Rectangle frame)
        {
            this.frame = frame;
        }

        public void SetBounds(Rectangle bounds)
        {
            this.bounds = bounds;
            draw_rectangle = bounds;
        }

        public void MoveUp(GameTime gameTime, float diff)
        {
            saved_y -= diff * CONSTANTS.frame_rate * (float)gameTime.ElapsedGameTime.TotalSeconds;

            bounds = new Rectangle(bounds.X, (int)saved_y, bounds.Width, bounds.Height);
            draw_rectangle.Y = (int)saved_y;
        }
    }
}