using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using AuxLib.ScreenManagement.Transitions;
using Microsoft.Xna.Framework.Content;
using AuxLib;
using AuxLib.ScreenManagement;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using AuxLib.Camera;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Diagnostics;
using MonoGame.Extended.Sprites;
using System.Windows.Markup;
using AuxLib.Debug;

namespace LibTester.Screens
{
    public sealed class PlayScreen : GameState
    {
        private SpriteBatcher spriteBatch;

        private Player player;

        private Level activeLevel;
        private DebugMonitor debugger;


        private BoundedCamera<Vector2> camera;
        

        public PlayScreen(Game game) : base(game)
        {
            camera = new BoundedCamera<Vector2>(game.GraphicsDevice.Viewport);
            game.Services.AddService(camera);

            debugger = new DebugMonitor(game);
            debugger.Initialize();
            game.Services.AddService(debugger);            

            
        }

        public override void Initialize()
        {
            var content = Game.Content;            
            LoadContent(content);
        }

        protected override void LoadContent(ContentManager contentManager)
        {
            activeLevel = new Level(Game, "Maps/track2");
            
            spriteBatch = Game.Services.GetService<SpriteBatcher>();
            CreatePlayer(contentManager, activeLevel.CollisionWorld);
        }


        

        public override void Update(GameTime gameTime)
        {
            if (Input.WasPressed(0, Buttons.Start, Keys.Enter))
            {
                GameManager.PushState(new DialogScreen(Game));
            }
            
            if (Input.WasPressed(0, Buttons.LeftShoulder, Keys.OemMinus))
                camera.Zoom -= 0.2f;

            if (Input.WasPressed(0, Buttons.RightShoulder, Keys.OemPlus))
                camera.Zoom += 0.2f;




            activeLevel.Update(gameTime);

            player.Update(gameTime);

            camera.LookAt(player.m_car.Transform.Position);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {

            activeLevel.Draw(gameTime);

            
            spriteBatch.Begin(transformMatrix : camera.GetViewMatrix());
            
            player.Draw(gameTime);

            

            spriteBatch.End();

            if (true)
                debugger.Draw(gameTime);

        }


        

       

        private void CreatePlayer(ContentManager contentManager,World world)
        {
            player = new Player(Game,world);
            
            //player.LoadContent(contentManager, activeLevel.CollisionWorld);         
        }

        protected override void UnloadContent()
        {
            //font = null;
            base.UnloadContent();
        }

    }
}
