using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace DawgShower
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class missile : Microsoft.Xna.Framework.DrawableGameComponent
    {
        protected Texture2D texture;
        protected Vector2 position;
        protected Rectangle spriteRectangle;
        protected Random random;
        protected int Yspeed;
        protected int Xspeed;
#if GT
        protected const int MISSILEWIDTH = 75;  // for GT helmet
        protected const int MISSILEHEIGHT = 56;
#else
        protected const int MISSILEWIDTH = 48; 
        protected const int MISSILEHEIGHT = 56;
#endif

        public missile(Game game, ref Texture2D theTexture, Vector2 playerPosition)
            : base(game)
        {
            texture = theTexture;
            position = new Vector2();

            // TODO: Construct any child components            here

            spriteRectangle = new Rectangle(0, 0, MISSILEWIDTH, MISSILEHEIGHT);
            random = new Random(this.GetHashCode());
            PutinStartPosition(playerPosition);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        
        protected void PutinStartPosition(Vector2 playerPosition)
        {
            position = playerPosition;
            Yspeed = 50;// +random.Next(7);
            Xspeed = random.Next(3) - 1;
        }
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        public Vector2 getPosition()
        {
            return position;
        }

        public Rectangle GetBounds()
        {
            return new Rectangle((int)position.X, (int)position.Y, MISSILEWIDTH, MISSILEHEIGHT);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            position.Y -= Yspeed;
            position.X += Xspeed;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch sBatch =
                (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

            sBatch.Draw(texture, position, spriteRectangle, Color.White);

            base.Draw(gameTime);
        }

        public bool CheckCollision(Rectangle rect)
        {
            Rectangle spriterect = new Rectangle((int)position.X, (int)position.Y, 
                                                MISSILEWIDTH, MISSILEHEIGHT);
            return spriterect.Intersects(rect);
        }
    }
}