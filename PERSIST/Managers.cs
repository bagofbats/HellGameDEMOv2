using System;
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

        public void GetInputs(KeyboardState key)
        {
            up = key.IsKeyDown(Keys.W);
            down = key.IsKeyDown(Keys.S);
            left = key.IsKeyDown(Keys.A);
            right = key.IsKeyDown(Keys.D);
            new_space = key.IsKeyDown(Keys.Space);
            new_enter = key.IsKeyDown(Keys.Enter);
            up_released = !up && old_up;
            up_pressed = up && !old_up;
            down_released = !down && old_down;
            down_pressed = down && !old_down;
            space_released = !new_space && old_space;
            space_pressed = new_space && !old_space;
            enter_released = !new_enter && old_enter;
            enter_pressed = new_enter && !old_enter;

            old_space = new_space;
            old_enter = new_enter;
            old_up = up;
            old_down = down;
        }
    }

    public class ProgressionManager
    {
        Checkpoint active_checkpoint;

        public bool knife
        { get; private set; }
        public bool ranged
        { get; private set; }
        
        // bosses and mini-bosses
        public bool slime_started
        { get; private set; }
        public bool slime_dead
        { get; private set; }

        public ProgressionManager()
        {
            knife = true;
            ranged = false;
            slime_dead = false;
            slime_started = false;
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
