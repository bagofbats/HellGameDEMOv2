using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using TiledCS;

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
        private ProgressionManager progManager = new ProgressionManager();
        private FPSCounter fpsCounter = new FPSCounter();

        private Texture2D spr_ui;

        public bool blackout
        { get; set; }

        private Player player;
        public Level the_level
        { get; private set; }

        private Dictionary<(string, string), LevelStruct> level_map;

        private LevelStruct tutorial_one = new LevelStruct("\\rm_tutorial1.tmx", "\\tst_tutorial.tsx");
        private LevelStruct tutorial_two = new LevelStruct("\\rm_tutorial2.tmx", "\\tst_tutorial.tsx");

        public Persist()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _graphics.GraphicsProfile = GraphicsProfile.HiDef; // <---- look up what this does cuz idfk
            IsMouseVisible = true;


            level_map = new Dictionary<(string, string), LevelStruct>()
            {
                {("rm_tutorial2", "blue"), tutorial_two },
                {("rm_tutorial1", "blue"), tutorial_one }
            };


            player = new Player(this, new Vector2(100, 100), contManager, progManager);

            TiledMap one_map = new TiledMap(Content.RootDirectory + "\\rm_tutorial1.tmx");
            TiledTileset one_tst = new TiledTileset(Content.RootDirectory + "\\tst_tutorial.tsx");
            TiledData one = new TiledData(new Rectangle(0, 0, 320, 240), one_map, one_tst);

            List<TiledData> tld = new List<TiledData>
            {
                one
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

            Camera cam = new Camera(this);
            the_level = new TutorialLevel(this, new Rectangle(0, 0, one_map.Width * one_map.TileWidth, one_map.Height * one_map.TileHeight), player, tld, cam, progManager, debug, "rm_tutorial1");

            Window.Title = "HellGame [DEMO]";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            _nativeRenderTarget = new RenderTarget2D(GraphicsDevice, 320, 240); // <--- use this to change camera zoom
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            spr_ui = Content.Load<Texture2D>("sprites/spr_ui");

            player.Load();
            the_level.Load(spr_ui);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            contManager.GetInputs(Keyboard.GetState());
            the_level.Update(gameTime);

            // fpsCounter.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_nativeRenderTarget);
            GraphicsDevice.Clear(Color.DarkSalmon);

            the_level.Draw(_spriteBatch);

            the_level.DrawText(_spriteBatch);

            GraphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_nativeRenderTarget, _screenRectangle, Color.White);

            if (blackout)
            {
                _spriteBatch.Draw(_nativeRenderTarget, _screenRectangle, Color.Black);
                blackout = false;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public void GoToLevel(string destination, string code)
        {
            if (progManager.GetActiveCheckpoint().root.name == destination)
            {
                SimpleGoToLevel(progManager.GetActiveCheckpoint().root);
                progManager.GetActiveCheckpoint().root.PlayerGotoDoor(code);
                return;
            }


            LevelStruct dst_info = level_map[(destination, code)];

            TiledMap map = new TiledMap(Content.RootDirectory + dst_info.map);
            TiledTileset tst = new TiledTileset(Content.RootDirectory + dst_info.tileset);
            TiledData data = new TiledData(new Rectangle(0, 0, 320, 240), map, tst);

            List<TiledData> tld = new List<TiledData> { data };

            Camera cam = new Camera(this);
            the_level = new TutorialLevel(this, new Rectangle(0, 0, map.Width * map.TileWidth, map.Height * map.TileHeight), player, tld, cam, progManager, debug, destination);

            the_level.Load(spr_ui, code);
        }

        public void SimpleGoToLevel(Level destination)
        {
            the_level = destination;
        }
    }
}