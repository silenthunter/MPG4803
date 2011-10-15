#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
#endregion

namespace DawgShower
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ship : Microsoft.Xna.Framework.DrawableGameComponent
    {
        protected Texture2D texture;
        protected Rectangle spriteRectangle;
        protected Vector2 position;
        protected bool shoot;
        
#if GT
        // for GT Buzz
        protected const int SHIPWIDTH = 100; 
        protected const int SHIPHEIGHT = 100; 
#else
        protected const int SHIPWIDTH = 58;
        protected const int SHIPHEIGHT = 68;
#endif

        // Screen Area
        protected Rectangle screenBounds;

        public ship(Game game, ref Texture2D theTexture)
            : base(game)
        {
            // TODO: Construct any child components here
            texture = theTexture;
            position = new Vector2();
            shoot = false;
           
            //spriteRectangle = new Rectangle(31, 83, SHIPWIDTH, SHIPHEIGHT);
            spriteRectangle = new Rectangle(0, 0, SHIPWIDTH, SHIPHEIGHT);

#if XBOX360

#else
            screenBounds = new Rectangle(0, 0, 
                                        Game.Window.ClientBounds.Width,
                                        Game.Window.ClientBounds.Height);
#endif
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

       
        public override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();
            
            // Move the ship with keyboard
            if (!shoot)
            {
                if (keyboard.IsKeyDown(Keys.Space))
                {
                      shoot = true;
                }
            }

            if (keyboard.IsKeyDown(Keys.Up))
            {
                position.Y -= 3;
            }
            if (keyboard.IsKeyDown(Keys.Down))
            {
                position.Y += 3;
            }
            if (keyboard.IsKeyDown(Keys.Left))
            {
                position.X -= 3;
            }
            if (keyboard.IsKeyDown(Keys.Right))
            {
                position.X += 3;
            }

            if (position.X < screenBounds.Left)
            {
                position.X = screenBounds.Left;
            }
            if (position.X > screenBounds.Width - SHIPWIDTH)
            {
                position.X = screenBounds.Width - SHIPWIDTH;
            }
            if (position.Y < screenBounds.Top)
            {
                position.Y = screenBounds.Top;
            }
            if (position.Y > screenBounds.Height - SHIPHEIGHT)
            {
                position.Y = screenBounds.Height - SHIPHEIGHT;
            }

            base.Update(gameTime);
        }


        public void PutinStartPosition()
        {
            position.X = screenBounds.Width / 2;
            position.Y = screenBounds.Height - SHIPHEIGHT;
        }

        public Vector2 GetPosition()
        {
            return position;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch sBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

            sBatch.Draw(texture, position, spriteRectangle, Color.White);

            base.Draw(gameTime);
        }

        public Rectangle GetBounds()
        {
            return new Rectangle((int)position.X, (int)position.Y, SHIPWIDTH, SHIPHEIGHT);
        }

        public void resetShoot()
        {
            shoot = false;
        }

        public bool shootMissile()
        {
            return shoot;
        }
    }
}