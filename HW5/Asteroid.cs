#region File Description
//-----------------------------------------------------------------------------
// Asteroid.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
#endregion

namespace Spacewar
{
    /// <summary>
    /// The types of asteroid
    /// </summary>
    public enum AsteroidType
    {
        /// <summary>
        /// Large asteroid
        /// </summary>
        Large,

        /// <summary>
        /// Small asteroid
        /// </summary>
        Small,
    }
    class Asteroid : SpacewarSceneItem
    {
        private float roll;
        private float pitch;
        private float yaw;

        private bool destroyed;

        private float rollIncrement;
        private float pitchIncrement;
        private float yawIncrement;

        private static Random random = new Random();

        #region Properties
        public bool Destroyed
        {
            get
            {
                return destroyed;
            }
            set
            {
                destroyed = value;
            }
        }
        #endregion

        public Asteroid(Game game, AsteroidType asteroidType, Vector3 position)
            : base(game, new BasicEffectShape(game, BasicEffectShapes.Asteroid, (int)asteroidType, LightingType.InGame), position)
        {
            //Random spin increments on all 3 axis
            rollIncrement = (float)random.NextDouble() - .5f;
            pitchIncrement = (float)random.NextDouble() - .5f;
            yawIncrement = (float)random.NextDouble() - .5f;

            if (asteroidType == AsteroidType.Large)
                Radius = 15;

            if (asteroidType == AsteroidType.Small)
                Radius = 6;
        }

        public override void Update(TimeSpan time, TimeSpan elapsedTime)
        {
            //Random rotation
            roll += rollIncrement * (float)elapsedTime.TotalSeconds;
            yaw += yawIncrement * (float)elapsedTime.TotalSeconds;
            pitch += pitchIncrement * (float)elapsedTime.TotalSeconds;

            rotation = new Vector3(roll, pitch, yaw);
            base.Update(time, elapsedTime);
        }

        public new Asteroid Copy()
        {
            Asteroid retn = new Asteroid(this.GameInstance, AsteroidType.Large, 
                new Vector3(this.position.X, this.position.Y, this.position.Z));
            retn.radius = this.radius;
            retn.roll = this.roll;
            retn.rollIncrement = this.rollIncrement;
            retn.pitch = this.pitch;
            retn.pitchIncrement = this.pitchIncrement;
            retn.yaw = this.yaw;
            retn.yawIncrement = this.yawIncrement;
            retn.destroyed = this.destroyed;
            retn.shape = this.shape.Copy();
            retn.delete = this.delete;

            retn.acceleration = new Vector3(this.acceleration.X, this.acceleration.Y, this.acceleration.Z);
            retn.center = new Vector3(this.center.X, this.center.Y, this.center.Z);
            retn.radius = this.radius;
            retn.rotation = new Vector3(this.rotation.X, this.rotation.Y, this.rotation.Z);
            retn.scale = new Vector3(this.scale.X, this.scale.Y, this.scale.Z);
            retn.position = new Vector3(this.position.X, this.position.Y, this.position.Z);

            return retn;
        }
    }
}
