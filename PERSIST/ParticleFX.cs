using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HellGame
{
    public abstract class ParticleFX
    {
        protected Level root;

        public Rectangle pos
        { get; protected set; } = new Rectangle(0, 0, 0, 0);

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
    }

    public class RangedFX : ParticleFX
    {
        private Rectangle frame = new Rectangle(0, 0, 16, 16);
        private Texture2D img;
        //private Rectangle pos;
        private float animate_timer = 0;

        public RangedFX(Vector2 pos, Texture2D img, Level root, bool fourway)
        {
            this.pos = new Rectangle((int)pos.X, (int)pos.Y, 16, 16);
            this.root = root;
            this.img = img;

            if (fourway)
                frame.Y = 16;
        }

        public override void Update(GameTime gameTime)
        {
            animate_timer += 36 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            frame.X = 16 * ((int)animate_timer);

            if (frame.X >= 80)
                root.RemoveFX(this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color c = Color.White;

            if (root.prog_manager.GetFlag(FLAGS.charons_blessing))
                c = root.charon_atk_color;

            spriteBatch.Draw(img, pos, frame, c);
        }
    }

    public class SlimeFX : ParticleFX
    {
        private Rectangle frame = new Rectangle(0, 48, 32, 32);
        private Texture2D img;
        //private Rectangle pos;
        private float animate_timer = 0;

        public SlimeFX(Vector2 pos, Texture2D img, Level root)
        {
            this.pos = new Rectangle((int)pos.X, (int)pos.Y + 16, 32, 32);
            this.root = root;
            this.img = img;
        }

        public override void Update(GameTime gameTime)
        {
            animate_timer += 12 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            frame.X = 32 * ((int)animate_timer);

            if (frame.X >= 96)
                root.RemoveFX(this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(img, pos, frame, Color.White);
        }
    }

    public class SleepFX : ParticleFX
    {
        // private Rectangle frame = new Rectangle(0, 80, 16, 16);
        private Texture2D img;
        private Enemy host;
        public bool trail_off
        { get; set; }
        public bool trailed_off
        { get; private set; }

        private float max_diff = 72f;

        private Rectangle z_one = new Rectangle(0, 0, 16, 16);
        private Rectangle z_one_frame = new Rectangle(32, 80, 16, 16);
        private float z_one_diff = 72f - 32f;
        private float z_one_xdiff = 0;
        private float z_one_transparency = 0f;

        private Rectangle z_two = new Rectangle(0, 0, 16, 16);
        private Rectangle z_two_frame = new Rectangle(32, 80, 16, 16);
        private float z_two_diff = 72f - 16f;
        private float z_two_xdiff = 0;
        private float z_two_transparency = 0f;

        private Rectangle z_three = new Rectangle(0, 0, 16, 16);
        private Rectangle z_three_frame = new Rectangle(0, 80, 16, 16);
        private float z_three_diff = 72f - 24f;
        private float z_three_xdiff = 0;
        private float z_three_transparency = 0f;

        private float trail_off_transparency = 0f;

        

        public SleepFX(Texture2D img, Level root, Enemy host)
        {
            this.root = root;
            this.img = img;
            this.host = host;

            trail_off = false;
            trailed_off = false;
        }

        public override void Update(GameTime gameTime)
        {
            // z_one
            z_one.X = host.GetHitBox(new Rectangle(0, 0, 0, 0)).X + (int)z_one_xdiff;
            z_one.Y = host.GetHitBox(new Rectangle(0, 0, 0, 0)).Y - 12 + (int)z_one_diff;

            z_one_diff -= 0.5f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);
            z_one_diff = z_one_diff % max_diff;

            z_one_xdiff = 8 * (float)Math.Sin(z_one_diff / 4);

            z_one_transparency = 0.42f + (z_one_diff / 100);
            if (z_one_diff >= -5f)
                z_one_transparency = z_one_diff / -10;

            // z_two
            z_two.X = host.GetHitBox(new Rectangle(0, 0, 0, 0)).X + (int)z_two_xdiff;
            z_two.Y = host.GetHitBox(new Rectangle(0, 0, 0, 0)).Y - 12 + (int)z_two_diff;

            z_two_diff -= 0.5f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);
            z_two_diff = z_two_diff % max_diff;
            
            z_two_xdiff = 8 * (float)Math.Sin(z_two_diff / 4);

            z_two_transparency = 0.42f + (z_two_diff / 100);
            if (z_two_diff >= -5f)
                z_two_transparency = z_two_diff / -10;

            // z_three
            z_three.X = host.GetHitBox(new Rectangle(0, 0, 0, 0)).X + (int)z_three_xdiff;
            z_three.Y = host.GetHitBox(new Rectangle(0, 0, 0, 0)).Y - 12 + (int)z_three_diff;

            z_three_diff -= 0.5f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);
            z_three_diff = z_three_diff % max_diff;

            z_three_xdiff = 8 * (float)Math.Sin(z_three_diff / 4);

            z_three_transparency = 0.42f + (z_three_diff / 100);
            if (z_three_diff >= -5f)
                z_three_transparency = z_three_diff / -10;


            if (trail_off)
            {
                trail_off_transparency += 0.018f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);
                z_one_transparency -= trail_off_transparency;
                z_two_transparency -= trail_off_transparency;
                z_three_transparency -= trail_off_transparency;

                trailed_off = z_one_transparency < 0 && z_two_transparency < 0 && z_three_transparency < 0;
            }
            else
            {
                trail_off_transparency = 0;
                trailed_off = false;
            }
        }

        public void ResetZs()
        {
            z_one_diff = max_diff - 32f;
            z_two_diff = max_diff - 16f;
            z_three_diff = max_diff - 24f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(img, z_one, z_one_frame, Color.White * z_one_transparency);
            spriteBatch.Draw(img, z_two, z_two_frame, Color.White * z_two_transparency);
            spriteBatch.Draw(img, z_three, z_three_frame, Color.White * z_three_transparency);
        }
    }

    public class CrumbleFX : ParticleFX
    {
        private Rectangle frame = new Rectangle(32, 192, 32, 32);
        private Texture2D img;
        //private Rectangle pos;
        private float animate_timer = 0;

        public CrumbleFX(Vector2 pos, Texture2D img, Level root)
        {
            this.pos = new Rectangle((int)pos.X - 8, (int)pos.Y - 8, 32, 32);
            this.root = root;
            this.img = img;
        }

        public override void Update(GameTime gameTime)
        {
            animate_timer += 16 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            frame.X = 32 * ((int)animate_timer);

            if (frame.X >= 128)
                root.RemoveFX(this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(img, pos, frame, Color.White);
        }
    }

    public class MushroomFX : ParticleFX
    {
        private Rectangle frame = new Rectangle(128, 80, 16, 16);
        private Texture2D img;
        //private Rectangle pos;
        private float animate_timer = 0;
        private Vector2 traj;

        private Random rnd = new Random();

        public MushroomFX(Vector2 pos, Texture2D img, Level root, Vector2 traj)
        {
            this.pos = new Rectangle((int)pos.X - 8, (int)pos.Y - 8, 16, 16);
            this.root = root;
            this.img = img;
            this.traj = traj;

            this.traj.X += ((float)rnd.NextDouble() + Math.Sign(this.traj.X)) * 0.4f;
            this.traj.Y += ((float)rnd.NextDouble() - 1f) * 1f;

            //while(Math.Abs(this.traj.X) < 0.5)
            //    this.traj.X += ((float)rnd.NextDouble() - 0.5f) * 1f;

            //while (Math.Abs(this.traj.Y) < 0.5)
            //    this.traj.Y += ((float)rnd.NextDouble() - 1f) * 1f;
        }

        public override void Update(GameTime gameTime)
        {
            animate_timer += 14 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            frame.X = 128 + (16 * ((int)animate_timer));

            pos = new Rectangle(pos.X + (int)(2.2f * traj.X), pos.Y + (int)(2.2f * traj.Y), pos.Width, pos.Height);

            traj.X *= 0.89f;
            traj.Y *= 0.89f;


            if (frame.X >= 176)
                root.RemoveFX(this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(img, pos, frame, Color.White * Math.Abs(traj.Y) * 0.8f);
        }
    }

    public class KeyFX : ParticleFX
    {
        protected Rectangle frame = new Rectangle(304, 120, 16, 8);
        protected Texture2D img;

        protected float timer = 0f;

        protected float timer_max = 0.14f;
        protected float multiplier = 180;

        protected float y_ratio = 2;

        public KeyFX(Vector2 pos, Texture2D img, Level root)
        {
            this.pos = new Rectangle((int)pos.X, (int)pos.Y, 16, 8);
            this.root = root;
            this.img = img;
        }

        public override void Update(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timer > timer_max)
                root.RemoveFX(this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float oset = timer * multiplier;

            Rectangle draw_pos = new Rectangle(pos.X - (int)(oset / 2), pos.Y - (int)(oset / 2), pos.Width + (int)oset, pos.Height + (int)(oset / y_ratio));

            float opacity = 1 - ((timer + 0.01f) / timer_max);

            spriteBatch.Draw(img, draw_pos, frame, Color.White * opacity);
        }
    }

    public class MaskFX : KeyFX
    {
        public MaskFX(Vector2 pos, Texture2D img, Level root, Rectangle frame) : base(pos, img, root)
        {
            this.pos = new Rectangle((int)pos.X, (int)pos.Y, 16, 16);
            this.frame = frame;
            y_ratio = 1;
            timer_max = 0.4f;
            multiplier = 70;
        }
    }

    public class PlusFX : ParticleFX
    {
        private Color c;
        private float timer = 0f;
        private Texture2D img;
        private float trans = 1f;
        private float rate_one = 1000f;
        private float rate_two = 700f;
        private float threshhold_one = 1f;
        private float threshhold_two = 2.4f;

        private Rectangle up;
        private Rectangle down;
        private Rectangle left;
        private Rectangle right;
        private Rectangle all;

        public PlusFX(Vector2 pos, Color c, Level root)
        {
            this.pos = new Rectangle((int)pos.X, (int)pos.Y, 2, 2);
            this.root = root;
            this.c = c;

            img = root.black;

            up = this.pos;
            down = this.pos;
            left = this.pos;
            right = this.pos;

            all = new Rectangle(0, 0, 0, 0);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (timer < threshhold_two)
            {
                spriteBatch.Draw(img, pos, c * trans);
                spriteBatch.Draw(img, up, c * trans);
                spriteBatch.Draw(img, down, c * trans);
                spriteBatch.Draw(img, left, c * trans);
                spriteBatch.Draw(img, right, c * trans);
            }
            else
            {
                spriteBatch.Draw(img, all, c * trans);
            }
        }

        // expand the + outward
        // once it covers four screens in size, expand to fill out square
        // once it fills entire space, fade out and delete self
        public override void Update(GameTime gameTime)
        {
            float frame_factor = 60 / CONSTANTS.frame_rate;
            float tdiff = (float)gameTime.ElapsedGameTime.TotalSeconds * frame_factor;

            timer += tdiff;

            if (timer < threshhold_one)
            {
                // expand the + outward
                int expand = (int)(tdiff * rate_one);

                up.Y -= expand;
                up.Height += expand;

                down.Height += expand;

                left.X -= expand;
                left.Width += expand;

                right.Width += expand;
            }
            else if (timer < threshhold_two)
            {
                // expand to fill out square
                int expand = (int)(tdiff * rate_two);

                up.X -= expand;
                up.Width += expand * 2;

                down.X -= expand;
                down.Width += expand * 2;

                left.Y -= expand;
                left.Height += expand * 2;

                right.Y -= expand;
                right.Height += expand * 2;
            }
            else
            {
                // fade out and delete self
                trans -= tdiff;

                if (all.Width == 0)
                    all = Rectangle.Union(Rectangle.Union(up, down), Rectangle.Union(left, right));

                if (trans <= 0)
                    root.RemoveFX(this);
            }
        }
    }

    public class EvilMagicFX : ParticleFX
    {
        private Texture2D img;
        private Rectangle frame;
        private int num_types;
        private float frequency;
        private Vector2 target = new Vector2(0, 0);
        private bool target_set = false;
        private bool shutting_down = false;

        private Random rd = new Random();
        private List<Color> palette = new List<Color>();
        private List<Color> blues = new List<Color>
        {
            Color.SkyBlue,
            Color.DeepSkyBlue,
            Color.CadetBlue,
            Color.SkyBlue,
            Color.DodgerBlue,
            Color.DodgerBlue,
        };

        private struct CircleStruct
        {
            public Rectangle bounds;
            public Rectangle frame;
            public Color color;
            public float speed;
            public float different;
            public float simple_oset;

            public CircleStruct(Rectangle bounds, Rectangle frame, Color color, float speed, float different, float simple_oset)
            {
                this.bounds = bounds;
                this.frame = frame;
                this.color = color;
                this.speed = speed;
                this.different = different;
                this.simple_oset = simple_oset;
            }
        }

        private List<CircleStruct> circles = new List<CircleStruct>();
        private float timer = 99f;
        private float base_speed = 425f;
        private float speed_modifier = 75f;
        private float freq = 0.04f;
        private float diff_chance = 0.1f;

        public EvilMagicFX(Rectangle pos, Texture2D img, Rectangle frame, int num_types, float frequency, Level root, string colors="blues") 
        { 
            this.pos = pos;
            this.img = img;
            this.frame = frame;
            this.num_types = num_types;
            this.frequency = frequency;
            this.root = root;

            // add if clauses here if more palettes ever become desirable
            palette = blues;
        }

        public void SetTarget(Vector2 target)
        {
            this.target = target;
            target_set = true;
        }

        public void GracefulShutdown()
        {
            shutting_down = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = circles.Count - 1; i >= 0; i--) 
            {
                Rectangle tmp = circles[i].bounds;

                if (target_set)
                {
                    int start = pos.Y + pos.Height;

                    float amp = tmp.X - target.X;
                    float y = start - tmp.Y;
                    float target_y = start - target.Y;
                    float nintey = 90f * (float)CONSTANTS.DegToRad;

                    float xoset = (float)(amp * Math.Sin(freq * (y - target_y)));

                    if (freq * (y - target_y) > nintey)
                        xoset = amp;

                    if (circles[i].different < diff_chance)
                        xoset *= -1;

                    tmp.X += (int)(xoset + circles[i].simple_oset);
                }

                spriteBatch.Draw(img, tmp, circles[i].frame, circles[i].color);
            }
        }

        public override void Update(GameTime gameTime)
        {
            float tdiff = (float)gameTime.ElapsedGameTime.TotalSeconds * 60 / CONSTANTS.frame_rate;
            timer += tdiff;

            for (int i = circles.Count - 1; i >= 0; i--) 
            {
                CircleStruct c = circles[i];

                c.bounds.Y -= (int)(c.speed * tdiff);

                circles[i] = c;
                if (circles[i].bounds.Y <= pos.Y)
                    circles.Remove(circles[i]);
            }

            if (timer > frequency && !shutting_down)
            {
                for (int i = 0; i < 3; i++)
                {
                    int x_init = pos.X + (int)(pos.Width * rd.NextDouble());
                    Color c = palette[rd.Next(palette.Count)];
                    float spd = base_speed + (int)(speed_modifier * 2 * (float)(0.5f - rd.NextDouble()));
                    float trans = 0.6f + (0.2f * (float)(0.5f - rd.NextDouble()));
                    int frame_idx = rd.Next(num_types);

                    Rectangle frame_tmp = frame;
                    frame_tmp.X += frame_idx * frame.Width;

                    timer = 0f;
                    CircleStruct tmp = new CircleStruct(
                        new Rectangle(x_init, pos.Y + pos.Height, frame.Width, frame.Height),
                        frame_tmp,
                        c * trans,
                        spd,
                        (float)rd.NextDouble(),
                        24 * (float)(0.5f - rd.NextDouble())
                        );

                    circles.Add(tmp);
                }
            }

            if (shutting_down && circles.Count == 0)
                root.RemoveFX(this);
        }
    }
}