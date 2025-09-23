using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PERSIST
{
    public class ControllerManager
    {
        private bool up;
        private bool down;
        private bool left;
        private bool right;
        private bool old_space;
        private bool new_space;
        private bool space_released;
        private bool space_pressed;
        private bool old_enter;
        private bool new_enter;
        private bool enter_pressed;
        private bool enter_released;
        private bool old_up;
        private bool up_pressed;
        private bool up_released;
        private bool old_down;
        private bool down_released;
        private bool down_pressed;
        private bool new_esc;
        private bool old_esc;
        private bool esc_pressed;
        private bool esc_released;
        private bool old_shift;
        private bool new_shift;
        private bool shift_pressed;
        private bool shift_released;

        private bool has_joystick;

        public Dictionary<string, Keys> key_map;
        public Dictionary<string, Keys> key_defaults;
        public Dictionary<string, Buttons> gp_map;
        public Dictionary<string, Buttons> gp_extras;
        private Dictionary<string, int> key_nums;

        private float joystick_threshold = 0.3f;

        private Keys[] list_defaults =
        {
            Keys.Up,
            Keys.Down,
            Keys.Left,
            Keys.Right,
            Keys.Z,
            Keys.X,
            Keys.C
        };

        private Keys[] list_customs =
        {
            Keys.W,
            Keys.S,
            Keys.A,
            Keys.D,
            Keys.Space,
            Keys.Enter,
            Keys.LeftShift
        };

        private bool multiple_down_buttons = true;

        public bool UP
        { get { return up; } }
        public bool DOWN
        { get { return down; } }
        public bool LEFT
        { get { return left; } }
        public bool RIGHT
        { get { return right; } }
        public bool SPACE
        { get { return new_space; } }
        public bool ENTER
        { get { return new_enter; } }
        public bool SHIFT
        { get { return new_shift; } }

        public bool UP_PRESSED
        { get { return up_pressed; } }
        public bool UP_RELEASED
        { get { return up_released; } }

        public bool DOWN_PRESSED
        { get { return down_pressed; } }
        public bool DOWN_RELEASED
        { get { return down_released; } }

        public bool SPACE_RELEASED
        { get { return space_released; } }
        public bool SPACE_PRESSED
        { get { return space_pressed; } }

        public bool ENTER_PRESSED
        { get { return enter_pressed; } }
        public bool ENTER_RELEASED
        { get { return enter_released; } }

        public bool SHIFT_PRESSED
        { get { return shift_pressed; } }
        public bool SHIFT_RELEASED
        { get { return shift_released; } }

        public bool ESC_PRESSED
        { get { return esc_pressed; } }
        public bool ESC_RELEASED
        { get { return esc_released; } }

        public bool HAS_JOYSTICK
        { get { return has_joystick; } }

        public ControllerManager()
        {
            key_map = new Dictionary<string, Keys>
            {
                {"up", Keys.W },
                {"down", Keys.S },
                {"left", Keys.A },
                {"right", Keys.D },
                {"jump", Keys.Space },
                {"attack", Keys.Enter },
                {"dash", Keys.RightShift }
            };

            key_defaults = new Dictionary<string, Keys>
            {
                {"up", Keys.Up },
                {"down", Keys.Down },
                {"left", Keys.Left },
                {"right", Keys.Right },
                {"jump", Keys.Z },
                {"attack", Keys.X },
                {"dash", Keys.C }
            };

            gp_map = new Dictionary<string, Buttons>
            {
                {"up", Buttons.DPadUp },
                {"down", Buttons.DPadDown },
                {"left", Buttons.DPadLeft },
                {"right", Buttons.DPadRight },
                {"jump", Buttons.A },
                {"attack", Buttons.X },
                {"dash", Buttons.LeftShoulder }
            };

            gp_extras = new Dictionary<string, Buttons>
            {
                {"up", Buttons.None },
                {"down", Buttons.LeftTrigger },
                {"left", Buttons.None },
                {"right", Buttons.None },
                {"jump", Buttons.None },
                {"attack", Buttons.RightTrigger },
                {"dash", Buttons.RightShoulder }
            };

            key_nums = new Dictionary<string, int>
            {
                {"up", 0 },
                {"down", 1 },
                {"left", 2 },
                {"right", 3 },
                {"jump", 4 },
                {"attack", 5 },
                {"dash", 6 }
            };
        }

        public void GetInputs(KeyboardState key)
        {
            up = key.IsKeyDown(key_defaults["up"]) || key.IsKeyDown(key_map["up"]);
            down = key.IsKeyDown(key_defaults["down"]) || key.IsKeyDown(key_map["down"]);
            left = key.IsKeyDown(key_defaults["left"]) || key.IsKeyDown(key_map["left"]);
            right = key.IsKeyDown(key_defaults["right"]) || key.IsKeyDown(key_map["right"]);
            new_space = key.IsKeyDown(key_defaults["jump"]) || key.IsKeyDown(key_map["jump"]);
            new_enter = key.IsKeyDown(key_defaults["attack"]) || key.IsKeyDown(key_map["attack"]);
            new_shift = key.IsKeyDown(key_defaults["dash"]) || key.IsKeyDown(key_map["dash"]);
            new_esc = key.IsKeyDown(Keys.Escape);

            GamePadCapabilities capabilities = GamePad.GetCapabilities(PlayerIndex.One);

            if (capabilities.IsConnected)
            {
                GamePadState state = GamePad.GetState(PlayerIndex.One);

                if (capabilities.HasLeftXThumbStick)
                {
                    left = left || state.ThumbSticks.Left.X < -joystick_threshold;
                    right = right || state.ThumbSticks.Left.X > joystick_threshold;
                }

                if (capabilities.HasLeftYThumbStick)
                {
                    up = up || state.ThumbSticks.Left.Y > joystick_threshold;
                    down = down || state.ThumbSticks.Left.Y < -joystick_threshold;
                }

                has_joystick = capabilities.HasLeftXThumbStick && capabilities.HasLeftYThumbStick;

                if (capabilities.GamePadType == GamePadType.GamePad)
                {
                    new_space = new_space || state.IsButtonDown(gp_map["jump"]) || state.IsButtonDown(gp_extras["jump"]);
                    new_enter = new_enter || state.IsButtonDown(gp_map["attack"]) || state.IsButtonDown(gp_extras["attack"]);
                    new_esc = new_esc || state.IsButtonDown(Buttons.Start);
                    new_shift = new_shift || state.IsButtonDown(gp_map["dash"]) || state.IsButtonDown(gp_extras["dash"]);

                    up = up || state.IsButtonDown(gp_map["up"]);
                    down = down || state.IsButtonDown(gp_map["down"]) || state.IsButtonDown(gp_extras["down"]);
                    left = left || state.IsButtonDown(gp_map["left"]);
                    right = right || state.IsButtonDown(gp_map["right"]);

                    if (multiple_down_buttons)
                        down = down || state.IsButtonDown(Buttons.LeftTrigger);
                }
            }


            up_released = !up && old_up;
            up_pressed = up && !old_up;
            down_released = !down && old_down;
            down_pressed = down && !old_down;
            space_released = !new_space && old_space;
            space_pressed = new_space && !old_space;
            enter_released = !new_enter && old_enter;
            enter_pressed = new_enter && !old_enter;
            esc_released = !new_esc && old_esc;
            esc_pressed = new_esc && !old_esc;
            shift_pressed = new_shift && !old_shift;
            shift_released = !new_shift && old_shift;

            old_space = new_space;
            old_enter = new_enter;
            old_up = up;
            old_down = down;
            old_esc = new_esc;
            old_shift = new_shift;
        }

        public void Rebind(string key, Keys new_key)
        {
            key_map[key] = new_key;
            list_customs[key_nums[key]] = new_key;
        }

        public Keys GetCurrentlyPressedKey(int selection)
        {
            if (Keyboard.GetState().GetPressedKeys().Length != 0)
                foreach (Keys key in Keyboard.GetState().GetPressedKeys())
                {
                    if (!list_defaults.Contains(key) && !list_customs.Contains(key))
                        return key;

                    // extra case to handle rebinding the same key over and over again
                    else if (list_customs[selection] == key && selection < 4)
                        return key;

                    // extra extra case to handle rebinding the attack/jump key over and over again
                    else if (list_customs[selection] == key && (enter_pressed || space_pressed))
                        return key;
                }
                    

            return Keys.None;
        }
    }

    public enum FLAGS
    {
        // player flags
        knife,
        ranged,
        mask,
        dash,

        // mechanic flags
        locks,
        jump_blocks,

        // boss flags
        slime_dead,
        slime_started,
        kanna_started,
        kanna_defeated,
        mushroom_started,
        mushroom_defeated,
        famine_started,
        famine_defeated,

        // story flags
        hideout_entered,
        map_obtained,
        stay_out_of_way,
        ask_hideout,
        ask_leaving,

        // secret flags
        journal_secret,
        charons_blessing,
        charon_door,
    }

    public class ProgressionManager
    {
        private Checkpoint active_checkpoint;

        private readonly Dictionary<FLAGS, bool> flag_map;

        public ProgressionManager()
        {
            flag_map = new Dictionary<FLAGS, bool>()
            {
                // player flags
                {FLAGS.knife                , true  },
                {FLAGS.ranged               , true  },
                {FLAGS.mask                 , true  },
                {FLAGS.dash                 , true  },

                // mechanic flags
                {FLAGS.jump_blocks          , false },
                {FLAGS.locks                , false },

                // boss flags
                {FLAGS.slime_started        , false },
                {FLAGS.slime_dead           , false },
                {FLAGS.kanna_started        , false },
                {FLAGS.kanna_defeated       , false },
                {FLAGS.mushroom_started     , false },
                {FLAGS.mushroom_defeated    , false },
                {FLAGS.famine_started       , false },
                {FLAGS.famine_defeated      , false },

                // story flags
                {FLAGS.hideout_entered      , false },
                {FLAGS.map_obtained         , false },
                {FLAGS.stay_out_of_way      , false },
                {FLAGS.ask_hideout          , false },
                {FLAGS.ask_leaving          , false },

                // secret flags
                {FLAGS.journal_secret       , false },
                {FLAGS.charons_blessing     , false },
                {FLAGS.charon_door          , false },
            };
        }

        public bool GetFlag(FLAGS flag)
        {
            return flag_map[flag];
        }

        public void SetFlag(FLAGS flag)
        {
            flag_map[flag] = true;
        }

        public void SetActiveCheckpoint(Checkpoint newActiveCheckpoint)
        {
            active_checkpoint = newActiveCheckpoint;
        }

        public Checkpoint GetActiveCheckpoint()
        {
            return active_checkpoint;
        }

        public void TakeOffMask()
        {
            flag_map[FLAGS.mask] = false;
        }
    }

    public class AudioManager
    {
        private readonly HellGame root;

        private Dictionary<string, SoundEffect> sfx = new Dictionary<string, SoundEffect>();

        public AudioManager(HellGame root)
        {
            this.root = root;
        }

        public void Load()
        {
            sfx.Add("atk", root.Content.Load<SoundEffect>("audio/snd_atk"));
            sfx.Add("hit", root.Content.Load<SoundEffect>("audio/snd_hit"));
            sfx.Add("tick", root.Content.Load<SoundEffect>("audio/snd_tick"));
            sfx.Add("tock", root.Content.Load<SoundEffect>("audio/snd_tock"));
            sfx.Add("woosh2", root.Content.Load<SoundEffect>("audio/snd_woosh2"));
        }

        public void PlaySound(string snd, float pitch=1f, float volume=1f)
        {
            SoundEffectInstance snd_tmp = sfx[snd].CreateInstance();
            snd_tmp.Pitch = pitch;
            snd_tmp.Volume = volume;

            snd_tmp.Play();
        }
    }

    public class FPSCounter
    {
        int frames = 0;
        float time_elapsed = 0;
        Player p;

        public FPSCounter(Player p)
        {
            this.p = p;
        }

        public void Update(GameTime gameTime)
        {
            frames++;
            time_elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (time_elapsed >= 5)
            {
                float f = (frames / time_elapsed);
                Debug.WriteLine(f);
                frames = 0;
                time_elapsed = 0;

                //float x = p.GetPos().X;
                //float v = p.GetPos().Y;

                //Debug.WriteLine(x);
                //Debug.WriteLine(v);
            }
        }
    }

    public class JumpSwitch
    {
        public Vector2 pos
        { get; private set; }
        public bool two
        { get; set; } = true;

        public JumpSwitch(Vector2 pos)
        {
            this.pos = pos;
        }
    }
    
}
