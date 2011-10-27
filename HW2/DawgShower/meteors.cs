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
        protected Model triangle;
        protected Rectangle spriteRectangle;
        protected Vector2 position;
        protected int Yspeed;
        protected int Xspeed;
        protected Random random;
        protected Vector2 rotationSpeeds;
        protected float rotX, rotY;
        protected BoundingSphere bounding;
        protected Boolean exploding = false;
        protected float scale = 2f;

        protected const int METEORWIDTH = 58; //45;
        protected const int METEORHEIGHT = 66; //45;

        public meteors(Game game, ref Model theModel, ref Model triangleRef)
            : base(game)
        {
            asteroidModel = theModel;
            triangle = triangleRef;
            position = new Vector2();

            // TODO: Construct any child components here
            //spriteRectangle = new Rectangle(20, 16, METEORWIDTH, METEORHEIGHT);
            spriteRectangle = new Rectangle(0, 0, METEORWIDTH, METEORHEIGHT);
            random = new Random(this.GetHashCode());
            PutinStartPosition();

            bounding = asteroidModel.Meshes[0].BoundingSphere;

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

            if (!exploding)
            {
                position.Y += Yspeed;
                position.X += Xspeed;
                rotX += rotationSpeeds.X;
                rotY += rotationSpeeds.Y;
            }
            else
                scale += 1f;

            if (scale >= 40f) Game.Components.Remove(this);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Vector3 Pos = new Vector3(position.X, -position.Y, -20);
            //Draw asteroid
            foreach (ModelMesh mesh in exploding ? triangle.Meshes : asteroidModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = Matrix.CreateScale(scale) *  Matrix.CreateRotationX(MathHelper.ToRadians(rotX)) *
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

        public bool CheckCollision(BoundingSphere sphere)
        {
            Vector3 Pos = new Vector3(position.X, -position.Y, -20);
            BoundingSphere retn;
            Matrix transform = Matrix.CreateScale(2f) *  Matrix.CreateRotationX(MathHelper.ToRadians(rotX)) *
                         Matrix.CreateRotationY(MathHelper.ToRadians(rotY)) *
                        Matrix.CreateRotationZ(MathHelper.ToRadians(180f)) * Matrix.CreateTranslation(Pos);

            bounding.Transform(ref transform, out retn);
            bool intersect = retn.Intersects(sphere);
            if (intersect)
            {
                exploding = true;
                scale = 5f;
                rotX = rotY = 0;
            }
            return intersect;
        }
    }
}