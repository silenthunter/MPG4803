/*
 * ECE4893
 * Multicore and GPU Programming for Video Games
 * 
 * Author: Hsien-Hsin Sean Lee
 * 
 * School of Electrical and Computer Engineering
 * Georgia Tech
 * 
 * leehs@gatech.edu
 * 
 * Updated to XNA 4.0 by Aaron Lanterman
 * 
 * */

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
// using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace Dungeon.Furniture
{

    public class Ducky : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private Model thisDucky;
        private Effect duckyEffect;
        private Matrix worldMatrix;
        private Matrix WVP;

        private Vector3 init_position;
        private Vector3 rotation;
        private Vector3 offset;
        public Vector4 a_material;
        public Vector4 d_material;
        public Vector4 s_material;
        public Vector3 scaling;

        private int wireFrame;

        public Texture2D texture_ducky;

        public Ducky(Game game)
            : base(game)
        {
            thisDucky = game.Content.Load<Model>("ducky_highres");

            texture_ducky = game.Content.Load<Texture2D>("ducky");
        }

        public Vector3 Init_position
        {
            get
            {
                return init_position;
            }
            set
            {
                init_position = value;
            }
        }

        public int WireFrame
        {
            get
            {
                return wireFrame;
            }
            set
            {
                wireFrame = value;
            }
        }

        public Vector3 Scaling
        {
            get
            {
                return scaling;
            }
            set
            {
                scaling = value;
            }
        }

        public override void Initialize()
        {
          
            rotation = new Vector3(0.0f, 0.0f, 0.0f);
            offset = new Vector3(0.02f, 0.018f, 0.033f);

            base.Initialize();
        }

        public Effect DuckyEffect
        {
            get
            {
                return duckyEffect;
            }
            set
            {
                duckyEffect = value;
            }
        }

        public Matrix WorldMatrix
        {
            get
            {
                return worldMatrix;
            }
            set
            {
                worldMatrix = value;
            }
        }

        public Matrix gWVP
        {
            get
            {
                return WVP;
            }
            set
            {
                WVP = value;
            }
        }

        public Vector4 Coefficient_A_Materials
        {
            get
            {
                return a_material;
            }
            set
            {
                a_material = value;
            }
        }

        public Vector4 Coefficient_D_Materials
        {
            get
            {
                return d_material;
            }
            set
            {
                d_material = value;
            }
        }


        public Vector4 Coefficient_S_Materials
        {
            get
            {
                return s_material;
            }
            set
            {
                s_material = value;
            }
        }


        public override void Update(GameTime gameTime)
        {
            rotation += offset;

            worldMatrix = Matrix.CreateScale(scaling) * Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationZ(rotation.Z) * Matrix.CreateRotationY(rotation.Y)
                * Matrix.CreateTranslation(init_position);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {

            /*if (wireFrame == 1)
            {
                duckyEffect.CurrentTechnique = duckyEffect.Techniques["myPlainWireTech"];
            }
            else
            {
                duckyEffect.CurrentTechnique = duckyEffect.Techniques["myTech"];
            }
            duckyEffect.Parameters["gWVP"].SetValue(WVP);
            duckyEffect.Parameters["gWorld"].SetValue(worldMatrix);
            duckyEffect.Parameters["material"].StructureMembers["a_material"].SetValue(a_material);
            duckyEffect.Parameters["material"].StructureMembers["d_material"].SetValue(d_material);
            duckyEffect.Parameters["material"].StructureMembers["s_material"].SetValue(s_material);
            duckyEffect.Parameters["map"].SetValue(texture_ducky); // will change inside the pass
        
   
            foreach (ModelMesh mesh in thisDucky.Meshes)
            {
                foreach (EffectPass pass in duckyEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }

                foreach (ModelMeshPart part in thisDucky.Meshes[0].MeshParts)
                {

                    part.Effect = duckyEffect;

                }
                mesh.Draw();
            }*/

            base.Draw(gameTime);
        }
    }
}