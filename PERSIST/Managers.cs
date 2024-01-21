using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
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

        public Dictionary<string, Keys> key_map;
        public Dictionary<string, Keys> key_defaults;

        private bool multiple_atk_buttons = true;
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

        public bool ESC_PRESSED
        { get { return esc_pressed; } }
        public bool ESC_RELEASED
        { get { return esc_released; } }

        public ControllerManager()
        {
            key_map = new Dictionary<string, Keys>
            {
                {"up", Keys.W },
                {"down", Keys.S },
                {"left", Keys.A },
                {"right", Keys.D },
                {"jump", Keys.None },
                {"attack", Keys.None }
            };

            key_defaults = new Dictionary<string, Keys>
            {
                {"up", Keys.Up },
                {"down", Keys.Down },
                {"left", Keys.Left },
                {"right", Keys.Right },
                {"jump", Keys.Space },
                {"attack", Keys.Enter }
            };
        }

        public void GetInputs(KeyboardState key)
        {
            GamePadCapabilities capabilities = GamePad.GetCapabilities(PlayerIndex.One);

            up = key.IsKeyDown(Keys.Up) || key.IsKeyDown(key_map["up"]);
            down = key.IsKeyDown(Keys.Down) || key.IsKeyDown(key_map["down"]);
            left = key.IsKeyDown(Keys.Left) || key.IsKeyDown(key_map["left"]);
            right = key.IsKeyDown(Keys.Right) || key.IsKeyDown(key_map["right"]);
            new_space = key.IsKeyDown(Keys.Space) || key.IsKeyDown(key_map["jump"]);
            new_enter = key.IsKeyDown(Keys.Enter) || key.IsKeyDown(key_map["attack"]);
            new_esc = key.IsKeyDown(Keys.Escape);

            if (capabilities.IsConnected)
            {
                GamePadState state = GamePad.GetState(PlayerIndex.One);

                if (capabilities.HasLeftXThumbStick)
                {
                    left = left || state.ThumbSticks.Left.X < -0.4f;
                    right = right || state.ThumbSticks.Left.X > 0.4f;
                }

                if (capabilities.HasLeftYThumbStick)
                {
                    up = up || state.ThumbSticks.Left.Y > 0.4f;
                    down = down || state.ThumbSticks.Left.Y < -0.4f;
                }

                if (capabilities.GamePadType == GamePadType.GamePad)
                {
                    new_space = new_space || state.IsButtonDown(Buttons.A);
                    new_enter = new_enter || state.IsButtonDown(Buttons.X);
                    new_esc = new_esc || state.IsButtonDown(Buttons.Start);

                    up = up || state.IsButtonDown(Buttons.DPadUp);
                    down = down || state.IsButtonDown(Buttons.DPadDown);
                    left = left || state.IsButtonDown(Buttons.DPadLeft);
                    right = right || state.IsButtonDown(Buttons.DPadRight);

                    if (multiple_atk_buttons)
                        new_enter = new_enter || state.IsButtonDown(Buttons.RightTrigger);

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

            old_space = new_space;
            old_enter = new_enter;
            old_up = up;
            old_down = down;
            old_esc = new_esc;
        }
    }

    public class ProgressionManager
    {
        Checkpoint active_checkpoint;

        public bool knife
        { get; private set; }
        public bool ranged
        { get; private set; }
        public bool mask
        { get; private set; }
        
        // bosses and mini-bosses
        public bool slime_started
        { get; private set; }
        public bool slime_dead
        { get; private set; }

        public ProgressionManager()
        {
            knife = false;
            ranged = false;
            slime_dead = false;
            slime_started = false;
            mask = false;
        }

        public void SetActiveCheckpoint(Checkpoint newActiveCheckpoint)
        {
            active_checkpoint = newActiveCheckpoint;
        }

        public Checkpoint GetActiveCheckpoint()
        {
            return active_checkpoint;
        }

        public void GetKnife()
        {
            knife = true;
        }

        public void EncounterSlime()
        {
            slime_started = true;
        }
        
        public void DefeatSlime()
        {
            slime_dead = true;
            ranged = true;
        }
    }

    public class FPSCounter
    {
        int frames = 0;
        float time_elapsed = 0;

        public FPSCounter()
        {

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
            }
        }
    }

    public class ProgressionPickup
    {

    }
}
