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
        protected Model asteroidModel;
        protected Rectangle spriteRectangle;
        protected Vector2 position;
        protected int Yspeed;
        protected int Xspeed;
        protected Random random;
        protected Vector2 rotationSpeeds;
        protected float rotX, rotY;

        protected const int METEORWIDTH = 58; //45;
        protected const int METEORHEIGHT = 66; //45;

        public meteors(Game game, ref Model theModel)
            : base(game)
        {
            asteroidModel = theModel;
            position = new Vector2();

            // TODO: Construct any child components here
            //spriteRectangle = new Rectangle(20, 16, METEORWIDTH, METEORHEIGHT);
            spriteRectangle = new Rectangle(0, 0, METEORWIDTH, METEORHEIGHT);
            random = new Random(this.GetHashCode());
            PutinStartPosition();

            rotationSpeeds = new Vector2(random.Next(10), random.Next(10));
            rotationSpeeds.Normalize();
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

            rotX += rotationSpeeds.X;
            rotY += rotationSpeeds.Y;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Vector3 Pos = new Vector3(position.X, -position.Y, -20);
            //Draw asteroid
            foreach (ModelMesh mesh in asteroidModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = Matrix.CreateScale(2f) *  Matrix.CreateRotationX(MathHelper.ToRadians(rotX)) *
                         Matrix.CreateRotationY(MathHelper.ToRadians(rotY)) *
                        Matrix.CreateRotationZ(MathHelper.ToRadians(180f)) * Matrix.CreateTranslation(Pos);
                    effect.View = Matrix.CreateLookAt(new Vector3(0, 0, 10),
                        Vector3.Zero,
                        Vector3.Up);
                    effect.Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width,
                        -GraphicsDevice.Viewport.Height, 0, 1f, 200f);
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }

        public bool CheckCollision(Rectangle rect)
        {
            return false;
            Rectangle spriterect = new Rectangle((int)position.X, (int)position.Y, METEORWIDTH, METEORHEIGHT);
            return spriterect.Intersects(rect);
        }
    }
}