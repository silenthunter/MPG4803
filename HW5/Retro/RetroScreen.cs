#region File Description
//-----------------------------------------------------------------------------
// RetroScreen.cs
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
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Spacewar
{
    public class RetroScreen : SpacewarScreen
    {
        /// <summary>
        /// Creates a new SpacewarScreen
        /// </summary>
        public RetroScreen(Game game)
            : base(game)
        {
            //Retro
            backdrop = new SceneItem(game, new RetroStarfield(game));
            scene.Add(backdrop);

            bullets = new RetroProjectiles(game);
            ship1 = new Ship(game, PlayerIndex.One, new Vector3(-250, 0, 0), bullets);
            ship1.Radius = 10f;
            scene.Add(ship1);

            ship2 = new Ship(game, PlayerIndex.Two, new Vector3(250, 0, 0), bullets);
            ship2.Radius = 10f;
            scene.Add(ship2);

            sun = new Sun(game, new RetroSun(game), new Vector3(SpacewarGame.Settings.SunPosition, 0.0f));
            scene.Add(sun);

            scene.Add(bullets);

            paused = false;
        }

        public override void Render()
        {
            Texture2D background = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"textures\retro_backdrop");
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));
            GraphicsDevice device = graphicsService.GraphicsDevice;

            //Backdrop
            SpriteBatch.Begin();
            SpriteBatch.Draw(background, new Vector2(0, 0), null, Color.White);
            SpriteBatch.End();

            //Always at the back so no need for zbuffer.
            device.DepthStencilState = DepthStencilState.None;            

            base.Render();

            Font.Begin();
            Font.Draw(FontStyle.WeaponLarge, 300, 15, player1Score);
            Font.Draw(FontStyle.WeaponLarge, 940, 15, player2Score);
            Font.End();
        }

        public override GameState Update(TimeSpan time, TimeSpan elapsedTime)
        {
            handleCollisions(time);

            return base.Update(time, elapsedTime);
        }

        public override void OnCreateDevice()
        {
            base.OnCreateDevice();

            bullets.OnCreateDevice();
        }

        private void ReplaceScene(SceneItem sun, Ship one, Ship two,
            Particles part, Projectiles proj)
        {
            scene.Remove(this.sun);
            scene.Remove(ship1);
            scene.Remove(ship2);
            this.sun = sun;
            this.ship1 = one;
            this.ship2 = two;

            scene.Add(ship1);
            scene.Add(ship2);
            scene.Add(sun);

            this.particles = part;
            if(particles != null)
                scene.Add(particles);

            this.bullets = proj;
            scene.Add(bullets);
        }

        public override Screen Copy()
        {
            int tempHealth1 = SpacewarGame.Players[0].Health;
            int tempHealth2 = SpacewarGame.Players[1].Health;

            RetroScreen retn = new RetroScreen(this.game);

            SpacewarGame.Players[0].Health = tempHealth1;
            SpacewarGame.Players[1].Health = tempHealth2;

            retn.bullets = this.bullets;
            retn.particles = this.particles;

            retn.paused = this.paused;
            retn.player1Score = this.player1Score;
            retn.player2Score = this.player2Score;
            retn.backdrop = this.backdrop;//Doesn't change so I won't deep copy
            retn.ReplaceScene(this.sun.Copy(), this.ship1.Copy(), this.ship2.Copy(), this.particles, this.bullets);

            return retn;
        }
    }
}
