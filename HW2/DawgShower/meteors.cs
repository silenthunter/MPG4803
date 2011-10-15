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
    public class meteors : Microsoft.Xna.Framework.DrawableGameComponent
    {
        protected Texture2D texture;
        protected Rectangle spriteRectangle;
        protected Vector2 position;
        protected int Yspeed;
        protected int Xspeed;
        protected Random random;

        protected const int METEORWIDTH = 58; //45;
        protected const int METEORHEIGHT = 66; //45;

        public meteors(Game game, ref Texture2D theTexture)
            : base(game)
        {
            texture = theTexture;
            position = new Vector2();

            // TODO: Construct any child components here
            //spriteRectangle = new Rectangle(20, 16, METEORWIDTH, METEORHEIGHT);
            spriteRectangle = new Rectangle(0, 0, METEORWIDTH, METEORHEIGHT);
            random = new Random(this.GetHashCode());
            PutinStartPosition();
        }

        protected void PutinStartPosition()
        {
            position.X = random.Next(Game.Window.ClientBounds.Width - METEORWIDTH);
            position.Y = 0;
            Yspeed = 1 + random.Next(5);
            Xspeed = random.Next(3) - 1;
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

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            if ((position.Y >= Game.Window.ClientBounds.Width) || (position.X <= 0))
            {
                PutinStartPosition();
            }

            position.Y += Yspeed;
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
            Rectangle spriterect = new Rectangle((int)position.X, (int)position.Y, METEORWIDTH, METEORHEIGHT);
            return spriterect.Intersects(rect);
        }
    }
}