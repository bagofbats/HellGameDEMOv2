using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using TiledCS;

namespace PERSIST
{
    public class HellGame : Game
    {
        private bool debug = true;
        public bool opaque
        { get; private set; } = false;

        public bool pause
        { get; private set; }

        private bool options = false;
        private int pause_selection = 0;
        private string[] pause_options =
        {
            "Resume",
            "Options",
            "Quit"
        };

        private int options_selection = 0;
        private string[] options_options =
        {
            "", "", "", "", "", "", "", "Done"
        };

        private bool rebind = false;
        private float rebind_timer = 1f;
        private bool rebound_recently = false;
        private int rebind_buffer = 0;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Rectangle _screenRectangle;
        private RenderTarget2D _nativeRenderTarget;
        private int scale = 1;

        private ControllerManager contManager = new ControllerManager();
        private ProgressionManager progManager = new ProgressionManager();
        private FPSCounter fpsCounter;

        public AudioManager audioManager;

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

        private LevelStruct tutorial_one = new LevelStruct("\\rm_tutorial1.tmx", "\\tst_tutorial.tsx", "tutorial");
        private LevelStruct tutorial_two = new LevelStruct("\\rm_tutorial2.tmx", "\\tst_tutorial.tsx", "tutorial");
        private LevelStruct tutorial_thr = new LevelStruct("\\rm_tutorial3.tmx", "\\tst_tutorial.tsx", "tutorial");

        private LevelStruct styx_zero = new LevelStruct("\\rm_styx0.tmx", "\\tst_styx.tsx", "styx");
        private LevelStruct styx_one = new LevelStruct("\\rm_styx1.tmx", "\\tst_styx.tsx", "styx");

        public HellGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _graphics.GraphicsProfile = GraphicsProfile.HiDef; // <---- look up what this does cuz idfk
            IsMouseVisible = true;
            bbuffer_color = Color.DarkSalmon;

            // >>>>>>> change frame rate >>>>>>>>>>
            //this.IsFixedTimeStep = true;//false;
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d); //60);

            styx_one.map[1] = "\\rm_styx2.tmx";
            styx_one.anchors[1] = new Vector2(2096, 432 + (8 * 60));
            styx_one.num_files = 2;

            audioManager = new AudioManager(this);

            level_map = new Dictionary<(string, string), LevelStruct>()
            {
                {("rm_tutorial2", "blue"), tutorial_two },
                {("rm_tutorial1", "blue"), tutorial_one },
                {("rm_tutorial3", "red"), tutorial_thr },
                {("rm_tutorial2", "red"), tutorial_two },
                {("rm_tutorial3", "green"), tutorial_thr },
                {("rm_styx0", "purple"), styx_zero },
                {("rm_styx1", "purple"), styx_one },
            };


            player = new Player(this, new Vector2(100, 100), contManager, progManager);

            fpsCounter = new FPSCounter(player);

            // tutorial level template
            //TiledMap one_map = new TiledMap(Content.RootDirectory + "\\rm_tutorial1.tmx");
            //TiledTileset one_tst = new TiledTileset(Content.RootDirectory + "\\tst_tutorial.tsx");
            //TiledData one = new TiledData(new Rectangle(0, 0, 320, 240), one_map, one_tst);

            //List<TiledData> tld = new List<TiledData>{one};

            // styx level template

            TiledMap one_map = new TiledMap(Content.RootDirectory + "\\rm_styx0.tmx");
            //TiledMap two_map = new TiledMap(Content.RootDirectory + "\\rm_styx2.tmx");
            TiledTileset one_tst = new TiledTileset(Content.RootDirectory + "\\tst_styx.tsx");

            List<Rectangle> bounds = new List<Rectangle>
            {
                new Rectangle(0, 0, one_map.Width * one_map.TileWidth, one_map.Height * one_map.TileHeight),
                //    new Rectangle(2096, 432 + (8 * 60), two_map.Width * two_map.TileWidth, two_map.Height * two_map.TileHeight)
            };

            TiledData one = new TiledData(bounds[0], one_map, one_tst);
            //TiledData two = new TiledData(bounds[1], two_map, one_tst);

            //List<TiledData> tld = new List<TiledData>{one, two};
            List<TiledData> tld = new List<TiledData> {one};

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
            //the_level = new TutorialLevel(this, SmallestRectangle(bounds), player, tld, cam, progManager, audioManager, debug, "rm_tutorial1");

            the_level = new StyxLevel(this, SmallestRectangle(bounds), player, tld, cam, progManager, audioManager, debug, "rm_styx0");

            Window.Title = "Hell Escape [DEMO]";
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
            audioManager.Load();
            the_level.Load(spr_ui);
        }

        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here

            contManager.GetInputs(Keyboard.GetState());

            if (contManager.ESC_PRESSED)
            {
                pause = !pause;
                pause_selection = 0;
                options = false;
                options_selection = 0;
                rebind = false;
                rebind_timer = 1f;
            }


            if (!pause)
                the_level.Update(gameTime);

            else
                HandlePause(gameTime);

            //fpsCounter.Update(gameTime);

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
                DrawPause(_spriteBatch);

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


            // dont think this needs to be a special case but i dont want to mess with it xxd
            if (dst_info.num_files == 1)
            {
                TiledMap map = new TiledMap(Content.RootDirectory + dst_info.map[0]);
                TiledTileset tst = new TiledTileset(Content.RootDirectory + dst_info.tileset);
                TiledData data = new TiledData(new Rectangle(0, 0, 320, 240), map, tst);

                List<TiledData> tld = new List<TiledData> { data };

                Camera cam = new Camera(this);

                string type = dst_info.type;

                if (type == "tutorial")
                    the_level = new TutorialLevel(this, new Rectangle(0, 0, map.Width * map.TileWidth, map.Height * map.TileHeight), player, tld, cam, progManager, audioManager, debug, destination);

                else if (type == "styx")
                    the_level = new StyxLevel(this, new Rectangle(0, 0, map.Width * map.TileWidth, map.Height * map.TileHeight), player, tld, cam, progManager, audioManager, debug, destination);

                the_level.Load(spr_ui, code);
            }

            else
            {
                List<TiledData> tld = new List<TiledData>();
                List<Rectangle> bounds = new List<Rectangle>();

                for (int i = 0; i < dst_info.num_files; i++)
                {
                    TiledMap map = new TiledMap(Content.RootDirectory + dst_info.map[i]);
                    TiledTileset tst = new TiledTileset(Content.RootDirectory + dst_info.tileset);

                    Rectangle b = new Rectangle((int)dst_info.anchors[i].X, (int)dst_info.anchors[i].Y, map.Width * map.TileWidth, map.Height * map.TileHeight);

                    TiledData data = new TiledData(b, map, tst);

                    tld.Add(data);
                    bounds.Add(b);
                }

                Camera cam = new Camera(this);

                string type = dst_info.type;

                if (type == "tutorial")
                    the_level = new TutorialLevel(this, SmallestRectangle(bounds), player, tld, cam, progManager, audioManager, debug, destination);

                else if (type == "styx")
                    the_level = new StyxLevel(this, SmallestRectangle(bounds), player, tld, cam, progManager, audioManager, debug, destination);

                the_level.Load(spr_ui, code);
            }

            

            if (cutscene != "")
                the_level.HandleCutscene(cutscene, null, true);
        }

        public void SimpleGoToLevel(Level destination)
        {
            the_level = destination;
        }

        private void HandlePause(GameTime gameTime)
        {
            if (!options)
            {
                if (contManager.DOWN_PRESSED)
                    pause_selection = (pause_selection + 1) % pause_options.Length;

                if (contManager.UP_PRESSED)
                {
                    pause_selection -= 1;
                    if (pause_selection < 0)
                        pause_selection = pause_options.Length - 1;
                }

                if (contManager.ENTER_PRESSED || contManager.SPACE_PRESSED)
                {
                    if (pause_options[pause_selection] == "Resume")
                        pause = !pause;

                    if (pause_options[pause_selection] == "Options")
                        options = true;

                    if (pause_options[pause_selection] == "Quit")
                        Exit();
                }

            }

            // options menu
            else
            {
                string[] keys =
                {
                    "UP", "DOWN", "LEFT", "RIGHT", "JUMP", "ATTACK", "DASH"
                };

                if (!rebind)
                {
                    rebind_timer = 1f;

                    if (contManager.DOWN_PRESSED && !rebound_recently)
                        options_selection = (options_selection + 1) % options_options.Length;

                    if (contManager.UP_PRESSED && !rebound_recently)
                    {
                        options_selection -= 1;
                        if (options_selection < 0)
                            options_selection = options_options.Length - 1;
                    }

                    if ((contManager.ENTER_PRESSED || contManager.SPACE_PRESSED) && !rebound_recently)
                    {
                        if (options_options[options_selection] == "Done")
                            options = false;

                        else
                            rebind = true;
                    }
                }
                else
                {
                    rebind_timer += (float)gameTime.ElapsedGameTime.TotalSeconds * 4;

                    Keys new_key = contManager.GetCurrentlyPressedKey(options_selection);

                    if (new_key != Keys.None)
                    {
                        string selection = keys[options_selection].ToLower();
                        contManager.Rebind(selection, new_key);
                        rebind = false;
                        rebound_recently = true;
                    }
                }

                

                for (int i = 0; i < contManager.key_map.Keys.Count; i++)
                {
                    string pause_msg = keys[i];

                    if (pause_msg == "DOWN")
                        pause_msg += "/INTERACT";

                    pause_msg += " | " + contManager.key_defaults[keys[i].ToLower()].ToString();

                    string key = contManager.key_map[keys[i].ToLower()].ToString();
                    if (rebind && i == options_selection)
                    {
                        pause_msg += " or";
                        if ((int)rebind_timer % 2 == 1)
                            pause_msg += " ???";
                    }
                    else if (key != "None")
                        pause_msg += " or " + contManager.key_map[keys[i].ToLower()].ToString();

                    options_options[i] = pause_msg;
                }

                if (rebound_recently)
                {
                    rebind_buffer++;
                    if (rebind_buffer == 2)
                    {
                        rebind_buffer = 0;
                        rebound_recently = false;
                    }
                }
            }
        }

        private void DrawPause(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(_nativeRenderTarget, _screenRectangle, Color.Black * 0.5f);

            if (!options)
            {
                string pause_msg = "[ PAUSED ]";
                Vector2 textMiddlePoint = bm_font.MeasureString(pause_msg) / 2;
                textMiddlePoint.X = (int)textMiddlePoint.X;
                textMiddlePoint.Y = (int)textMiddlePoint.Y;

                _spriteBatch.DrawString(bm_font, pause_msg,
                                        new Vector2(_screenRectangle.X + (_screenRectangle.Width / 2), _screenRectangle.Y + (_screenRectangle.Height / 3.4f)),
                                        Color.Gray * 0.7f, 0, textMiddlePoint, scale * 2f, SpriteEffects.None, 0f
                                        );

                for (int i = 0; i < pause_options.Length; i++)
                {
                    pause_msg = pause_options[i];
                    Color text_color = Color.Gray;

                    int x_draw_offset = 0;

                    if (i == pause_selection)
                    {
                        text_color = Color.White;
                        pause_msg = "> " + pause_msg;
                        x_draw_offset = (int)((Vector2)bm_font.MeasureString(" ")).X + 1;
                    }

                    _spriteBatch.DrawString(bm_font, pause_msg,
                                            new Vector2(_screenRectangle.X + (_screenRectangle.Width / 2.36f), _screenRectangle.Y + (_screenRectangle.Height / 3) + (24 * (i + 1) * scale)),
                                            text_color, 0, new Vector2(0 + x_draw_offset, 0), scale, SpriteEffects.None, 0f
                                            );
                }
            }

            else
            {
                string pause_msg = "[ OPTIONS ]";
                Vector2 textMiddlePoint = bm_font.MeasureString(pause_msg) / 2;
                textMiddlePoint.X = (int)textMiddlePoint.X;
                textMiddlePoint.Y = (int)textMiddlePoint.Y;

                _spriteBatch.DrawString(bm_font, pause_msg,
                                        new Vector2(_screenRectangle.X + (_screenRectangle.Width / 2), _screenRectangle.Y + (_screenRectangle.Height / 4f)),
                                        Color.Gray * 0.7f, 0, textMiddlePoint, scale * 2f, SpriteEffects.None, 0f
                                        );

                

                for (int i = 0; i < options_options.Length; i++)
                {
                    Color text_color = Color.Gray;
                    if (i == options_selection)
                    {
                        text_color = Color.White;
                    }

                    if (options_options[i] != "Done")
                    {
                        string first_half = options_options[i].Split('|')[0];
                        int xoffset = (int)((Vector2)bm_font.MeasureString(first_half)).X;

                        string options_txt_draw_idk = options_options[i];

                        _spriteBatch.DrawString(bm_font, options_txt_draw_idk,
                                                new Vector2(_screenRectangle.X + (_screenRectangle.Width / 2f) - (xoffset * scale), _screenRectangle.Y + (_screenRectangle.Height / 3) + (12 * i * scale)),
                                                text_color, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0f
                                                );
                    }
                    
                    else
                    {
                        textMiddlePoint = bm_font.MeasureString(options_options[i]) / 2;
                        textMiddlePoint.X = (int)textMiddlePoint.X;
                        textMiddlePoint.Y = (int)textMiddlePoint.Y;

                        _spriteBatch.DrawString(bm_font, options_options[i],
                                                new Vector2(_screenRectangle.X + (_screenRectangle.Width / 2f), _screenRectangle.Y + (_screenRectangle.Height / 3) + (14 * i * scale)),
                                                text_color, 0, textMiddlePoint, scale, SpriteEffects.None, 0f
                                                );
                    }
                }
            }
        }

        private Rectangle SmallestRectangle(List<Rectangle> bounds)
        {
            int smallest_X = int.MaxValue;
            int smallest_Y = int.MaxValue;
            int largest_X = 0;
            int largest_Y = 0;

            foreach (Rectangle r in bounds)
            {
                if (r.X < smallest_X)
                    smallest_X = r.X;
                if (r.Y < smallest_Y)
                    smallest_Y = r.Y;
                if (r.X + r.Width > largest_X)
                    largest_X = r.X + r.Width;
                if (r.Y + r.Height > largest_Y)
                    largest_Y = r.Y + r.Height;
            }

            Rectangle ret = new Rectangle(
                smallest_X,
                smallest_Y,
                largest_X - smallest_X,
                largest_Y - smallest_Y
                );

            return ret;
        }
    }
}