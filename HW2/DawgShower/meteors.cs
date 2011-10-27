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
        protected BoundingSphere bounding;
        protected Boolean exploding = false;
        protected float scale = 2f;

        private BasicEffect effect;
        private VertexPositionColor[] vertex;
        private VertexBuffer VBuffer;

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

            bounding = asteroidModel.Meshes[0].BoundingSphere;

            rotationSpeeds = new Vector2(random.Next(10), random.Next(10));
            rotationSpeeds.Normalize();
        }

        private void TriangleOne()
        {
            vertex = new VertexPositionColor[3];

            vertex[0] = new VertexPositionColor(new Vector3(-0.9f, -0.5f, 0.5f), Color.Red);
            vertex[1] = new VertexPositionColor(new Vector3(0.0f, 0.5f, 0.5f), Color.Orange);
            vertex[2] = new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.5f), Color.White);

            VBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 3, BufferUsage.WriteOnly);

            VBuffer.SetData(vertex);

        }

        private void setupEffect()
        {
            effect = new BasicEffect(GraphicsDevice);
            effect.Alpha = 1.0f;
            effect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            effect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            effect.SpecularPower = 5.0f;
            effect.AmbientLightColor = new Vector3(1.0f, 1.0f, 1.0f);

            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.DiffuseColor = Vector3.One;
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(1.0f, -1.0f, -1.0f));
            effect.DirectionalLight0.SpecularColor = Vector3.One;

            effect.DirectionalLight1.Enabled = true;
            effect.DirectionalLight1.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
            effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(-1.0f, -1.0f, 1.0f));
            effect.DirectionalLight1.SpecularColor = new Vector3(0.5f, 0.5f, 0.5f);

            effect.LightingEnabled = false;
            effect.VertexColorEnabled = true;
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
            TriangleOne();
            setupEffect();
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
            }
            else
                scale += 1f;

            rotX += rotationSpeeds.X;
            rotY += rotationSpeeds.Y;

            if (scale >= 40f) Game.Components.Remove(this);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Vector3 Pos = new Vector3(position.X, -position.Y, -20);
            if (!exploding)
            {
                //Draw asteroid
                foreach (ModelMesh mesh in asteroidModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.World = Matrix.CreateScale(scale) * Matrix.CreateRotationX(MathHelper.ToRadians(rotX)) *
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
            }
                //Draw the exploding triangles
            else
            {
                GraphicsDevice.SetVertexBuffer(VBuffer);
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    effect.World = Matrix.CreateScale(scale) * Matrix.CreateRotationZ(MathHelper.ToRadians(rotX)) * Matrix.CreateTranslation(Pos);
                    effect.View = Matrix.CreateLookAt(new Vector3(0, 0, 10),
                        Vector3.Zero,
                        Vector3.Up);
                    effect.Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width,
                        -GraphicsDevice.Viewport.Height, 0, 1f, 200f);
                    pass.Apply();

                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
                }
            }

            base.Draw(gameTime);
        }

        public bool CheckCollision(BoundingSphere sphere)
        {
            if (exploding) return false;//don't collide if already hit
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