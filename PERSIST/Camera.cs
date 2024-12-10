using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace PERSIST
{
    public class Camera
    {
        private float current_x = 0;
        private float current_y = 0;

        private Persist root;

        public bool stable
        { get; set; }

        public Camera(Persist root)
        {
            this.root = root;
            stable = true;
        }

        public Matrix Transform { get; private set; }

        public void Follow(Vector2 target)
        {
            current_x = target.X;
            current_y = target.Y;

            var position = Matrix.CreateTranslation(
              -(int)target.X,
              -(int)target.Y,
              0);
            var offset = Matrix.CreateTranslation(
                0,
                0,
                0);
            Transform = position * offset;
        }

        public void TargetFollow(Vector2 target)
        {
            if (current_x != target.X)
                current_x += (target.X - current_x) / 5;
            if (current_y != target.Y)
                current_y += (target.Y - current_y) / 5;

            if (Math.Abs(current_x - target.X) <= 0.3)
                current_x = target.X;
            if (Math.Abs(current_y - target.Y) <= 0.3)
                current_y = target.Y;

            if (Math.Abs(current_x - target.X) <= 0.3 && Math.Abs(current_y - target.Y) <= 0.3)
                stable = true;
            else
                stable = false;

            Follow(new Vector2(current_x, current_y));
        }

        public void SetPos(Vector2 pos)
        {
            current_x = pos.X;
            current_y = pos.Y;
        }

        public void SmartSetPos(Vector2 pos)
        {
            Rectangle current_room = root.the_level.GetRoom(pos);

            if (current_room == new Rectangle(0, 0, 0, 0))
                return;

            int tempX = (int)pos.X - 160;
            int tempY = (int)pos.Y - 120;
            tempX = Math.Clamp(tempX, current_room.X, current_room.X + current_room.Width - 320);
            tempY = Math.Clamp(tempY, current_room.Y, Math.Max(current_room.Y + current_room.Height - 240, current_room.Y));
            SetPos(new Vector2(tempX, tempY));
        }

        public Vector2 GetPos()
        {
            return new Vector2(current_x, current_y);
        }
    }
}