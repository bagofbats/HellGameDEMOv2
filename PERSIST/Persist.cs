using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;

namespace PERSIST
{
    public class Persist : Game
    {
        private bool debug = false;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Rectangle _screenRectangle;
        private RenderTarget2D _nativeRenderTarget;

        private ControllerManager contManager = new ControllerManager();
        private Player player;
        private Level the_level;

        public Persist()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _graphics.GraphicsProfile = GraphicsProfile.HiDef; // <---- look up what this does cuz idfk
            IsMouseVisible = true;

            player = new Player(this, new Vector2(320, 120), contManager);

            RawJSON one = JsonFileReader.Read<RawJSON>("rm_tutorial1.json");
            RawJSON two = JsonFileReader.Read<RawJSON>("rm_tutorial2.json");
            RawJSON three = JsonFileReader.Read<RawJSON>("sample.json");
            List<JSON> JSONs = new List<JSON>
            {
                new JSON(new Rectangle(0, 0 + 368 + 120 - 16 - 16, 320, 240), one),
                new JSON(new Rectangle(320, 40 + 368 + 120 - 16 - 16, 320, 240), two),
                new JSON(new Rectangle(640, 128, 960, 608), three)
            };

            // determine how much to scale the window up
            // given how big the monitor is
            int w = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int h = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            int target_w = 320;
            int target_h = 240;
            int scale = 1;

            while (target_w * (scale + 1) < w && target_h * (scale + 1) < h)
                scale++;

            target_w *= scale;
            target_h *= scale;

            _graphics.PreferredBackBufferWidth = target_w;  // set this value to the desired width of your window
            _graphics.PreferredBackBufferHeight = target_h;   // set this value to the desired height of your window

            _screenRectangle = new Rectangle(0, 0, target_w, target_h);

            _graphics.ApplyChanges();

            the_level = new Level(this, new Rectangle(0, 0, 1600, 960), player, JSONs, new Camera(target_w, target_h), debug);

            Window.Title = "Persist [DEMO]";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            _nativeRenderTarget = new RenderTarget2D(GraphicsDevice, 320, 240);

            player.SetCurrentLevel(the_level);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            player.Load();
            the_level.Load();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            contManager.GetInputs(Keyboard.GetState());
            the_level.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_nativeRenderTarget);
            GraphicsDevice.Clear(Color.DarkSalmon);

            the_level.Draw(_spriteBatch);

            GraphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_nativeRenderTarget, _screenRectangle, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}