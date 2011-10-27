using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace DawgShower
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch = null;
        private Texture2D backgroundTexture;
        private Texture2D[] backgroundTextureOptions;
        private Texture2D gameoverTexture;
        private Texture2D leeTexture;
        private Texture2D pauseTexture;
        private Texture2D pause2Texture;
        private Model shipModel;
        private Model asteroidModel;
        private Model missileModel;
        private Model triangle;
        private int pauseCtr;

        private ship player;
        private Cue backmusic;

        private const int STARTMETEORCOUNT = 20;
        private const int ADDMETEORTIME = 500;
        private const int PENALTY = 10;
        private const int MIN_ADVANCE = 20;
        private const int DEAD_COUNT = 3;
        private const int MAXBACKGROUND = 4; // 3 background pictures

        private int lastTickCount;
        private int rockCount;
        private int highest_rockCount;
        private int right_margin;
        private int score;
        private int highestscore;
        private int dead;
        private int lives;
        private int target_hit;
        private int last_hit;
        private int current_level;
        protected int missileWing = 1;
        private bool pause, gameover, pauseAlt;

        private AudioComponent audioComponent;
        private SpriteFont gamefont;

        public Game1()
        {

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if FS
            // for running at Full Screen mode
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.IsFullScreen = true;
#else
            graphics.PreferredBackBufferWidth = 980;
            graphics.PreferredBackBufferHeight = 650;
            graphics.IsFullScreen = false;
#endif
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            audioComponent = new AudioComponent(this);
            Components.Add(audioComponent);

            highest_rockCount = STARTMETEORCOUNT;
            right_margin = 500; // graphics.GraphicsDevice.PresentationParameters.BackBufferWidth - 420;
            score = 0;
            dead = 0;
            target_hit = 0;
            last_hit = 0;
            highestscore = 0;
            pauseCtr = 0;
            pause = false;
            gameover = false;
            current_level = 0;

            base.Initialize(); // if this is done last, then PlayCue won't play because initialization is not done yet

            backmusic = audioComponent.GetCue("backmusic");
            backmusic.Play();

            
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);

            backgroundTextureOptions = new Texture2D[4];
            // Prepare four background textures
            backgroundTextureOptions[0] = Content.Load<Texture2D>("earth1");
            backgroundTextureOptions[1] = Content.Load<Texture2D>("e2");
            backgroundTextureOptions[2] = Content.Load<Texture2D>("startbackground");
            backgroundTextureOptions[3] = Content.Load<Texture2D>("Spacebackground");

            //Load ship
            shipModel = Content.Load<Model>("Ship");

            //Load asteroid
            asteroidModel = Content.Load<Model>("asteroid");

            //Load Missile
            missileModel = Content.Load<Model>("missile");

            // Earth to be used as the default
            backgroundTexture = backgroundTextureOptions[0];

            // Set up textures for game pause
            pauseTexture = Content.Load<Texture2D>("pause"); 
            pause2Texture = Content.Load<Texture2D>("pauseR");

#if GT
            leeTexture = Content.Load<Texture2D>("buzz");
#else
            leeTexture = Content.Load<Texture2D>("leeporN3");
            missileTexture = Content.Load<Texture2D>("leePhantom");
#endif
            gameoverTexture = Content.Load<Texture2D>("gameover");
            gamefont = Content.Load<SpriteFont>("font");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>

        private void Start()
        {
            if (player == null)
            {
                player = new ship(this, ref shipModel);
                Components.Add(player);
                lastTickCount = System.Environment.TickCount;
            }

            rockCount = STARTMETEORCOUNT;
            player.PutinStartPosition();

            for (int i = 0; i < STARTMETEORCOUNT; i++)
            {
                Components.Add(new meteors(this, ref asteroidModel));
            }
        }

        private void CheckMissileFired()
        {
            // add component is SPACE bar was hit
            if (player.shootMissile())
            {
                audioComponent.PlayCue("shoot");
                Components.Add(new missile(this, ref missileModel, player.GetPosition() + new Vector2(20, -10) * missileWing));
                missileWing *= -1;
                player.resetShoot();    
            }

            //Fire missile barrage
            if (player.barrageMissile())
            {
                audioComponent.PlayCue("shoot");
                Vector2 velocity = new Vector2(-10, 0);
                Matrix rotate = Matrix.CreateRotationZ(MathHelper.ToRadians(-18f));
                for (int i = 0; i < 10; i++)
                {
                    Components.Add(new missile(this, ref missileModel, player.GetPosition(), velocity));
                    velocity = Vector2.Transform(velocity, rotate);
                }
                player.resetShoot();
            }
    
            // The following code remove missile component when it is moved out of the screen
            for (int i=0; i<Components.Count; i++)
            {
                if (Components[i] is missile)
                {
                    Vector2 current_pos;

                    current_pos = ((missile)Components[i]).getPosition();

                    // remove the component if missile is out of the screen
                    if (current_pos.Y < 0)
                    {
                        Components.RemoveAt(i);
                    }
                }
            }
        }

        private void CheckMissileHit()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] is missile)
                {
                    for (int j = 0; j < Components.Count; j++)
                    {
                        if (Components[j] is meteors)
                        {
                            bool hasCollision = false;
                            hasCollision = ((meteors)Components[j]).CheckCollision(((missile)Components[i]).GetBounds());
                            if (hasCollision)
                            {
                                audioComponent.PlayCue("explosion");
                                // remove collided meteros (BadDawgs)
                                score++; 
                                Components.RemoveAt(i);
                                
                                i --; // remove 1 components
                                target_hit++;
                          
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void CheckforNewMeteor()
        {
            if ((System.Environment.TickCount - lastTickCount) > ADDMETEORTIME)
            {
                lastTickCount = System.Environment.TickCount;
                Components.Add(new meteors(this, ref asteroidModel));
                rockCount++;

                audioComponent.PlayCue("newmeteor");

            }
        }

        private void AdvanceLevel()
        {
            if ((target_hit - last_hit) > MIN_ADVANCE)
            {
                last_hit = target_hit;
                highest_rockCount = rockCount;

                current_level = ((current_level + 1) % MAXBACKGROUND);
                backgroundTexture = backgroundTextureOptions[current_level];
            }
                
        }

        private void RemoveGame()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                    Components.RemoveAt(i);
                    i--;
            }
        }

        private void GameOver()
        {
            if (dead >= DEAD_COUNT)
            {
                if (highestscore < score)
                    highestscore = score;

                RemoveGame();
                backgroundTexture = gameoverTexture;
                gameover = true;
                player = null;
                backmusic.Pause();
                  
            }
        }

        private void DoGameLogic()
        {
            bool hasCollision = false;
            BoundingSphere shipRectangle = player.GetBounds();

            foreach (GameComponent gc in Components)
            {
                if (gc is meteors)
                {
                    hasCollision = ((meteors)gc).CheckCollision(shipRectangle);
                    if (hasCollision)
                    {
                        audioComponent.PlayCue("missile");
                        score -= PENALTY;
                        dead++;
                        RemoveAllMeteors();
                        Start();
                        break;
                    }
                }
            }
           
            CheckforNewMeteor();
            CheckMissileFired();
            CheckMissileHit();
            AdvanceLevel();
            
        }

        private void RemoveAllMeteors()
        {
            for (int i=0; i< Components.Count; i++)
            {
                if (Components[i] is meteors)
                {
                    Components.RemoveAt(i);
                    i--;
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();
               
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (keyboard.IsKeyDown(Keys.Escape))
                this.Exit();


            if (gameover && (keyboard.IsKeyDown(Keys.Enter)))
            {
                gameover = false;
                backgroundTexture = backgroundTextureOptions[0];
                //gameovermusic.Stop(AudioStopOptions.Immediate);
                //backmusic = audioComponent.GetCue("backmusic");
                backmusic.Resume();
                score = 0;
                dead = 0;
                target_hit = 0;
                last_hit = 0;
                current_level = 0;

            }    
            
            if (player == null && !gameover)
            {
                Start();
            }
            
            // Pause     a game

            if (!pause && keyboard.IsKeyDown(Keys.P))
            {
                pause = true;
            }
            else if (pause && keyboard.IsKeyDown(Keys.Tab)) 
            {
                pause = false;
            }

            // Check if Game is overs
            GameOver();

            if (pause == false && !gameover)
            {
                DoGameLogic();
                base.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0,
                graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                graphics.GraphicsDevice.PresentationParameters.BackBufferHeight),
                null, // Scales original texture to fill the full drawn rectangle
                Color.LightGray);

            if (pause)
            {
                if (pauseAlt)
                {
                    spriteBatch.Draw(pauseTexture, 
                        new Rectangle(graphics.GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - 110,
                                      graphics.GraphicsDevice.PresentationParameters.BackBufferHeight / 2 - 38,
                                      220, 75), Color.White);
                }
                else
                {
                    spriteBatch.Draw(pause2Texture,
                        new Rectangle(graphics.GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - 110,
                                      graphics.GraphicsDevice.PresentationParameters.BackBufferHeight / 2 - 38,
                                      220, 75), Color.White);
                }
                if (pauseCtr++ > 30)
                {
                    pauseAlt = !pauseAlt;
                    pauseCtr = 0;
                }
            }

            spriteBatch.DrawString(gamefont, "Bad Dawgs Annihilated: " + target_hit.ToString(), new Vector2(16, 16), Color.Red);
            spriteBatch.DrawString(gamefont, "Bad Dawgs Annihilated: " + target_hit.ToString(), new Vector2(17, 17), Color.Cyan);
            spriteBatch.DrawString(gamefont, "Bad Dawgs Annihilated: " + target_hit.ToString(), new Vector2(18, 18), Color.Cyan);

            lives = DEAD_COUNT - dead;
            spriteBatch.DrawString(gamefont, "Lives: " + lives.ToString(), new Vector2(right_margin, 16), Color.Red);
            spriteBatch.DrawString(gamefont, "Lives: " + lives.ToString(), new Vector2(right_margin+1, 17), Color.Yellow);
            spriteBatch.DrawString(gamefont, "Lives: " + lives.ToString(), new Vector2(right_margin+2, 18), Color.Yellow);

            spriteBatch.DrawString(gamefont, "Score: " + score.ToString(), new Vector2(350, 16), Color.Red);
            spriteBatch.DrawString(gamefont, "Score: " + score.ToString(), new Vector2(350, 17), Color.White);
            spriteBatch.DrawString(gamefont, "Score: " + score.ToString(), new Vector2(350, 18), Color.White);

            if (gameover)
            {
                spriteBatch.DrawString(gamefont, "Highest Score: " + highestscore.ToString(), new Vector2(400, 500), Color.Red);
                spriteBatch.DrawString(gamefont, "Highest Score: " + highestscore.ToString(), new Vector2(400, 501), Color.White);
                spriteBatch.DrawString(gamefont, "Highest Score: " + highestscore.ToString(), new Vector2(400, 502), Color.White);
            }

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.BackToFront,BlendState.AlphaBlend);
            base.Draw(gameTime);
            spriteBatch.End();

        }
    }
}
