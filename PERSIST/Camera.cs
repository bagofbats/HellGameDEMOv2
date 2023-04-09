using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PERSIST
{
    public class Camera
    {
        int w;
        int h;
        float current_x = 0;
        float current_y = 0;

        public Camera(int w, int h)
        {
            this.w = w;
            this.h = h;
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

            Follow(new Vector2(current_x, current_y));
        }

        public void SetPos(Vector2 pos)
        {
            current_x = pos.X;
            current_y = pos.Y;
        }

        public Vector2 GetPos()
        {
            return new Vector2(current_x, current_y);
        }
    }
}