#region File Description
//-----------------------------------------------------------------------------
// SpacewarGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System.Threading;
#endregion

namespace Spacewar
{
    struct GameBuffer
    {
        public Screen screen;
        public GameState state;
        public Player[] players;

        public GameBuffer Copy()
        {
            GameBuffer retn = new GameBuffer();
            retn.screen = this.screen.Copy();
            retn.state = this.state;
            if (this.players != null)
            {
                retn.players = new Player[2];
                this.players.CopyTo(retn.players, 0);
            }

            return retn;
        }
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    partial class SpacewarGame : Microsoft.Xna.Framework.Game
    {
        // these are the size of the offscreen drawing surface
        // in general, no one wants to change these as there
        // are all kinds of UI calculations and positions based
        // on these dimensions.
        const int FixedDrawingWidth = 1280;
        const int FixedDrawingHeight = 720;

        // these are the size of the output window, ignored
        // on Xbox 360
        private int preferredWindowWidth = 1280;
        private int preferredWindowHeight = 720;

        private static ContentManager contentManager;

        /// <summary>
        /// The game settings from settings.xml
        /// </summary>
        private static Settings settings = new Settings();

        private static Camera camera;

        /// <summary>
        /// Information about the players such as score, health etc
        /// </summary>
        //private static Player[] players;

        /// <summary>
        /// The current game state
        /// </summary>
        //private static GameState gameState = GameState.Started;

        /// <summary>
        /// Which game board are we playing on
        /// </summary>
        private static int gameLevel;

        /// <summary>
        /// Stores game paused state
        /// </summary>
        private bool paused;

        private GraphicsDeviceManager graphics;

        private bool enableDrawScaling;
        private RenderTarget2D drawBuffer;
        private SpriteBatch spriteBatch;

        //private static Screen currentScreen;

        private static PlatformID currentPlatform;

        private static KeyboardState keyState;
        private bool justWentFullScreen;

        private static GameBuffer UpdateBuff, DrawBuff;
        private Thread updateThread;
        private GameTime gameTime;
        private ManualResetEvent updateDone;
        private ManualResetEvent renderBlock;
        private bool updateFirstRun = false;
        private bool isDone = false;
        private bool swapBuffer = true;

        #region Properties
        public static GameState GameState
        {
            get
            {
                return UpdateBuff.state;
            }
        }

        public static int GameLevel
        {
            get
            {
                return gameLevel;
            }
            set
            {
                gameLevel = value;
            }
        }

        public static Camera Camera
        {
            get
            {
                return camera;
            }
        }

        public static Settings Settings
        {
            get
            {
                return settings;
            }
        }

        public static Player[] Players
        {
            get
            {
                return UpdateBuff.players;
            }
        }

        public static ContentManager ContentManager
        {
            get
            {
                return contentManager;
            }
        }

        public static PlatformID CurrentPlatform
        {
            get
            {
                return currentPlatform;
            }
        }

        public static KeyboardState KeyState
        {
            get
            {
                return keyState;
            }
        }
        #endregion

        public SpacewarGame()
        {
#if XBOX360
            // we might as well use the xbox in all its glory
            preferredWindowWidth = FixedDrawingWidth;
            preferredWindowHeight = FixedDrawingHeight;
            enableDrawScaling = false;
#else
            enableDrawScaling = true;
#endif

            this.graphics = new Microsoft.Xna.Framework.GraphicsDeviceManager(this);
            this.graphics.PreferredBackBufferWidth = preferredWindowWidth;
            this.graphics.PreferredBackBufferHeight = preferredWindowHeight;            

            // Game should run as fast as possible.
            IsFixedTimeStep = false;

            updateThread = new Thread(new ThreadStart(UpdateThread));
            updateThread.Start();
            updateDone = new ManualResetEvent(true);
            renderBlock = new ManualResetEvent(true);
            UpdateBuff = new GameBuffer();
            UpdateBuff.state = GameState.Started;
        }        

        protected override void Initialize()
        {
            // game initialization code here

            //Uncomment this line to force a save of the default settings file. Useful when you had added things to settings.cs
            //NOTE in VS this will go in DEBUG or RELEASE - need to copy up to main project
            //Settings.Save("settings.xml");

            settings = Settings.Load("settings.xml");

            currentPlatform = System.Environment.OSVersion.Platform;

            //Initialise the sound
            Sound.Initialize();

            Window.Title = Settings.WindowTitle;

            base.Initialize();
        }

        protected override void BeginRun()
        {
            Sound.PlayCue(Sounds.TitleMusic);

            //Kick off the game by loading the logo splash screen
            ChangeState(GameState.LogoSplash);
            DrawBuff = UpdateBuff.Copy();

            float fieldOfView = (float)Math.PI / 4;
            float aspectRatio = (float)FixedDrawingWidth / (float)FixedDrawingHeight;
            float nearPlane = 10f;
            float farPlane = 700f;

            camera = new Camera(fieldOfView, aspectRatio, nearPlane, farPlane);
            camera.ViewPosition = new Vector3(0, 0, 500);

            base.BeginRun();
        }

        protected override void Update(GameTime gameTime)
        {
            updateDone.WaitOne();
            updateDone.Reset();

            this.gameTime =  gameTime;

            updateFirstRun = true;
            keyState = Keyboard.GetState();

            base.Update(gameTime);

            updateDone.Set();
        }

        //src: http://forums.create.msdn.com/forums/p/83031/501003.aspx
        protected void UpdateThread()
        {
            int timeManager = 0;
            TimeSpan lastTotal = new TimeSpan();

            while (!updateFirstRun) Thread.Sleep(100);
            while(!isDone)
            {
                updateDone.WaitOne();
                updateDone.Reset();
                //TimeSpan elapsedTime = gameTime.ElapsedGameTime;
                TimeSpan time = gameTime.TotalGameTime;
                TimeSpan elapsedTime = time - lastTotal;
                lastTotal = time;
                timeManager += elapsedTime.Milliseconds;
                if (timeManager < 1f / 60 * 1000)
                {
                    updateDone.Set();
                    continue;
                }
                timeManager = 0;

                // The time since Update was called last
                float elapsed = (float)elapsedTime.TotalSeconds;

                GameState changeState = GameState.None;

                XInputHelper.Update(this, keyState);

                if ((keyState.IsKeyDown(Keys.RightAlt) || keyState.IsKeyDown(Keys.LeftAlt)) && keyState.IsKeyDown(Keys.Enter) && !justWentFullScreen)
                {
                    ToggleFullScreen();
                    justWentFullScreen = true;
                }

                if (keyState.IsKeyUp(Keys.Enter))
                {
                    justWentFullScreen = false;
                }

                if (XInputHelper.GamePads[PlayerIndex.One].BackPressed ||
                    XInputHelper.GamePads[PlayerIndex.Two].BackPressed)
                {
                    if (UpdateBuff.state == GameState.PlayEvolved || UpdateBuff.state == GameState.PlayRetro)
                    {
                        paused = !paused;
                    }

                    if (UpdateBuff.state == GameState.LogoSplash)
                    {
                        this.Exit();
                    }
                }

                //Reload settings file?
                if (XInputHelper.GamePads[PlayerIndex.One].YPressed)
                {
                    //settings = Settings.Load("settings.xml");
                    //GC.Collect();
                }

                if (!paused)
                {
                    //Update everything
                    renderBlock.WaitOne();
                    renderBlock.Reset();
                    changeState = UpdateBuff.screen.Update(time, elapsedTime);
                    renderBlock.Set();

                    // Update the AudioEngine - MUST call this every frame!!
                    Sound.Update();

                    //If either player presses start then reset the game
                    if (XInputHelper.GamePads[PlayerIndex.One].StartPressed ||
                        XInputHelper.GamePads[PlayerIndex.Two].StartPressed)
                    {
                        changeState = GameState.LogoSplash;
                    }

                    if (changeState != GameState.None)
                    {
                        ChangeState(changeState);
                    }
                }
                updateDone.Set();

                if (swapBuffer)
                {
                    renderBlock.WaitOne();
                    renderBlock.Reset();

                    DrawBuff = UpdateBuff.Copy();
                    swapBuffer = false;
                    renderBlock.Set();
                }
            }

        }

        protected override bool BeginDraw()
        {
            if (!base.BeginDraw())
                return false;

            BeginDrawScaling();

            return true;
        }

        protected override void Draw(GameTime gameTime)
        {
            renderBlock.WaitOne();
            renderBlock.Reset();
            graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, 
                Color.CornflowerBlue, 1.0f, 0);            

            base.Draw(gameTime);

            DrawBuff.screen.Render();

            swapBuffer = true;
            renderBlock.Set();
        }

        protected override void EndDraw()
        {
            EndDrawScaling();

            base.EndDraw();
        }

        internal void ChangeState(GameState NextState)
        {
            Screen currentScreen = UpdateBuff.screen;
            //Logo spash can come from ANY state since its the place you go when you restart
            if (NextState == GameState.LogoSplash)
            {
                if (currentScreen != null)
                    currentScreen.Shutdown();

                currentScreen = new TitleScreen(this);
                UpdateBuff.state = GameState.LogoSplash;
            }
            else if (UpdateBuff.state == GameState.LogoSplash && NextState == GameState.ShipSelection)
            {
                Sound.PlayCue(Sounds.MenuAdvance);

                //This is really where the game starts so setup the player information
                UpdateBuff.players = new Player[2] { new Player(), new Player() };

                //Start at level 1
                gameLevel = 1;

                currentScreen.Shutdown();
                currentScreen = new SelectionScreen(this);
                UpdateBuff.state = GameState.ShipSelection;
            }
            else if (UpdateBuff.state == GameState.PlayEvolved && NextState == GameState.ShipUpgrade)
            {
                currentScreen.Shutdown();
                currentScreen = new ShipUpgradeScreen(this);
                UpdateBuff.state = GameState.ShipUpgrade;
            }
            else if ((UpdateBuff.state == GameState.ShipSelection || GameState == GameState.ShipUpgrade) && NextState == GameState.PlayEvolved)
            {
                Sound.PlayCue(Sounds.MenuAdvance);

                currentScreen.Shutdown();
                currentScreen = new EvolvedScreen(this);
                UpdateBuff.state = GameState.PlayEvolved;
            }
            else if (UpdateBuff.state == GameState.LogoSplash && NextState == GameState.PlayRetro)
            {
                //Game starts here for retro
                UpdateBuff.players = new Player[2] { new Player(), new Player() };

                currentScreen.Shutdown();
                currentScreen = new RetroScreen(this);
                UpdateBuff.state = GameState.PlayRetro;
            }
            else if (UpdateBuff.state == GameState.PlayEvolved && NextState == GameState.Victory)
            {
                currentScreen.Shutdown();
                currentScreen = new VictoryScreen(this);
                UpdateBuff.state = GameState.Victory;
            }
            else
            {
                //This is a BAD thing and should never happen
                // What does this map to on XBox 360?
                //Debug.Assert(false, String.Format("Invalid State transition {0} to {1}", gameState.ToString(), NextState.ToString()));
            }

            UpdateBuff.screen = currentScreen;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            contentManager = new ContentManager(Services);

            if (UpdateBuff.screen != null)
                UpdateBuff.screen.OnCreateDevice();

            Font.Init(this);

            if (enableDrawScaling)
            {
                PresentationParameters pp = graphics.GraphicsDevice.PresentationParameters;

                drawBuffer = new RenderTarget2D(graphics.GraphicsDevice,
                                                FixedDrawingWidth, FixedDrawingHeight,
                                                true, SurfaceFormat.Color,
                                                DepthFormat.Depth24Stencil8, pp.MultiSampleCount,
                                                RenderTargetUsage.DiscardContents);

                spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            }
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            if (drawBuffer != null)
            {
                drawBuffer.Dispose();
                drawBuffer = null;
            }

            if (spriteBatch != null)
            {
                spriteBatch.Dispose();
                spriteBatch = null;
            }

            Font.Dispose();

            if (contentManager != null)
            {
                contentManager.Dispose();
                contentManager = null;
            }
        }

        private void ToggleFullScreen()
        {
            PresentationParameters presentation = graphics.GraphicsDevice.PresentationParameters;

            if (presentation.IsFullScreen)
            {   // going windowed
                graphics.PreferredBackBufferWidth = preferredWindowWidth;
                graphics.PreferredBackBufferHeight = preferredWindowHeight;
            }
            else
            {
                // going fullscreen, use desktop resolution to minimize display mode changes
                // this also has the nice effect of working around some displays that lie about 
                // supporting 1280x720
                GraphicsAdapter adapter = graphics.GraphicsDevice.Adapter;
                graphics.PreferredBackBufferWidth = adapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = adapter.CurrentDisplayMode.Height;
            }

            graphics.ToggleFullScreen();
        }

        private void BeginDrawScaling()
        {
            if (enableDrawScaling && drawBuffer != null)
            {
                graphics.GraphicsDevice.SetRenderTarget(drawBuffer);
            }
        }

        private void EndDrawScaling()
        {
            // copy our offscreen surface to the backbuffer with appropriate
            // letterbox bars

            if (!enableDrawScaling || drawBuffer == null)
                return;

            graphics.GraphicsDevice.SetRenderTarget(null);

            PresentationParameters presentation = graphics.GraphicsDevice.PresentationParameters;

            float outputAspect = (float)presentation.BackBufferWidth / (float)presentation.BackBufferHeight;
            float preferredAspect = (float)FixedDrawingWidth / (float)FixedDrawingHeight;

            Rectangle dst;

            if (outputAspect <= preferredAspect)
            {
                // output is taller than it is wider, bars on top/bottom

                int presentHeight = (int)((presentation.BackBufferWidth / preferredAspect) + 0.5f);
                int barHeight = (presentation.BackBufferHeight - presentHeight) / 2;

                dst = new Rectangle(0, barHeight, presentation.BackBufferWidth, presentHeight);
            }
            else
            {
                // output is wider than it is tall, bars left/right

                int presentWidth = (int)((presentation.BackBufferHeight * preferredAspect) + 0.5f);
                int barWidth = (presentation.BackBufferWidth - presentWidth) / 2;

                dst = new Rectangle(barWidth, 0, presentWidth, presentation.BackBufferHeight);
            }

            // clear to get black bars
            graphics.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

            // draw a quad to get the draw buffer to the back buffer
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(drawBuffer, dst, Color.White);
            spriteBatch.End();
        }
    }
}