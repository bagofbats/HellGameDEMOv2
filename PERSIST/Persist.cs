using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.IO;
using TiledCS;

namespace PERSIST
{
    public class Persist : Game
    {
        private bool debug = false;
        private bool pause = false;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Rectangle _screenRectangle;
        private RenderTarget2D _nativeRenderTarget;
        private int scale = 1;

        private ControllerManager contManager = new ControllerManager();
        private ProgressionManager progManager = new ProgressionManager();
        private FPSCounter fpsCounter = new FPSCounter();

        private Texture2D spr_ui;
        private BitmapFont bm_font;

        public bool blackout
        { get; set; }
        public Color bbuffer_color
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
            bbuffer_color = Color.DarkSalmon;


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
            bm_font = Content.Load<BitmapFont>("fonts/pixellocale_bmp");

            player.Load();
            the_level.Load(spr_ui);
        }

        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here

            contManager.GetInputs(Keyboard.GetState());

            if (contManager.ESC_PRESSED)
                pause = !pause;

            if (!pause)
                the_level.Update(gameTime);

            // fpsCounter.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_nativeRenderTarget);
            GraphicsDevice.Clear(bbuffer_color);

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

            if (pause)
            {
                string pause_msg = "[ PAUSED ]";
                Vector2 textMiddlePoint = bm_font.MeasureString(pause_msg) / 2;
                textMiddlePoint.X = (int)textMiddlePoint.X;
                textMiddlePoint.Y = (int)textMiddlePoint.Y;

                _spriteBatch.Draw(_nativeRenderTarget, _screenRectangle, Color.Black * 0.4f);
                _spriteBatch.DrawString(bm_font,
                                        pause_msg, 
                                        new Vector2(_screenRectangle.X + (_screenRectangle.Width / 2), _screenRectangle.Y + (_screenRectangle.Height / 2) - (12 * scale)), 
                                        Color.White, 0, textMiddlePoint, scale * 2f, SpriteEffects.None, 0f
                                        );
            }

            _spriteBatch.End();

            base.Draw(gameTime);

            bbuffer_color = Color.DarkSalmon;
        }

        public void GoToLevel(string destination, string code, string cutscene="")
        {
            if (progManager.GetActiveCheckpoint().root.name == destination)
            {
                SimpleGoToLevel(progManager.GetActiveCheckpoint().root);
                progManager.GetActiveCheckpoint().root.PlayerGotoDoor(code);
                if (cutscene != "")
                    the_level.HandleCutscene(cutscene, null, true);
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

            if (cutscene != "")
                the_level.HandleCutscene(cutscene, null, true);
        }

        public void SimpleGoToLevel(Level destination)
        {
            the_level = destination;
        }
    }
}