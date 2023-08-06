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
            space_released = !new_space && old_space;
            space_pressed = new_space && !old_space;
            enter_released = !new_enter && old_enter;
            enter_pressed = new_enter && !old_enter;

            old_space = new_space;
            old_enter = new_enter;
        }
    }

    public class ProgressionManager
    {
        Checkpoint default_respawn = new FakeCheckpoint(new Rectangle(632, 416, 1, 1), null);
        Checkpoint active_checkpoint;

        public bool knife
        { get; private set; }
        public bool ranged
        { get; private set; }

        public ProgressionManager()
        {
            knife = true;
            ranged = true;
        }

        public void SetActiveCheckpoint(Checkpoint newActiveCheckpoint)
        {
            active_checkpoint = newActiveCheckpoint;
        }

        public Checkpoint GetActiveCheckpoint()
        {
            if (active_checkpoint == null)
                return default_respawn;

            return active_checkpoint;
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
