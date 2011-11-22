﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
// using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace Dungeon
{
    public class Enemy : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private Model mech;
        public Effect mechEffect;
        public Matrix worldMatrix;
        public Matrix WVP;

        public Vector3 init_position;
        public Vector3 rotation;
        public Vector3 offset;
        public Vector3 velocity = new Vector3(0 , 0, 5);
        public Vector4 a_material;
        public Vector4 d_material;
        public Vector4 s_material;
        public Vector3 scaling;
        public Vector3 playerPos;

        public Enemy(Game game, Effect effect) : base(game)
        {
            mechEffect = effect;
            mech = game.Content.Load<Model>("mech8");
            foreach(ModelMesh mesh in mech.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = mechEffect.Clone();
            //mechEffect = new BasicEffect(game.GraphicsDevice);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            Vector3 toPlayer3 = init_position - playerPos;
            Vector2 toPlayer = new Vector2(toPlayer3.X, toPlayer3.Z);
            toPlayer.Normalize();
            toPlayer3.Y = 0;
            toPlayer3.Normalize();
            Vector2 facing = new Vector2(velocity.X, velocity.Z);
            facing.Normalize();

            float dot = Vector2.Dot(toPlayer, facing);
            if (dot > 1) dot = 1;
            if (dot < -1) dot = -1;
            float acos = (float)Math.Acos(dot);

            rotation.Y = acos;
            if (toPlayer.X < facing.X) rotation.Y *= -1;

            rotation.Y += MathHelper.ToRadians(-90f);//Model is off by 90 degrees

            init_position += -toPlayer3 * 5 * gameTime.ElapsedGameTime.Milliseconds / 1000;

            worldMatrix = Matrix.CreateScale(scaling) * Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationZ(rotation.Z) * Matrix.CreateRotationY(rotation.Y)
            * Matrix.CreateTranslation(init_position);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            mechEffect.Parameters["WVP"].SetValue(WVP);
            //mechEffect.Parameters["World"].SetValue(worldMatrix);
            /*mechEffect.Parameters["material"].StructureMembers["a_material"].SetValue(a_material);
            mechEffect.Parameters["material"].StructureMembers["d_material"].SetValue(d_material);
            mechEffect.Parameters["material"].StructureMembers["s_material"].SetValue(s_material);*/
            //mechEffect.Parameters["map"].SetValue(texture_ducky); // will change inside the pass

            Matrix[] transforms = new Matrix[mech.Bones.Count];
            mech.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in mech.Meshes)
            {
                foreach (EffectPass pass in mechEffect.CurrentTechnique.Passes)
                {
                    mechEffect.Parameters["BoneTransform"].SetValue(transforms[mesh.ParentBone.Index]);
                    pass.Apply();
                }
                foreach (ModelMeshPart part in mesh.MeshParts)
                {

                    part.Effect = mechEffect;

                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}