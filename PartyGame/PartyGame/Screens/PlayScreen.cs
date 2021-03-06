﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using AuxLib.Debug;
using AuxLib;
using System.IO;
using Game1.Settings;
using AuxLib.Camera;
using Game1.GameObjects.Levels;
using Game1.DataContext;
using Game1.Scripting;
using Game1.HUD;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Game1.Screens
{
    public sealed class PlayState : BaseGameState, IPlayGameState
    {
        private readonly GameContext context;
        private FocusCamera<Vector2> camera;
        private readonly SpriteBatcher SpriteBatcher;
        private ScriptingEngine scriptingEngine;
        string lvlFile;
        GameSettings settings;
        public static DebugMonitor DebugMonitor;
        HeadsUpDisplay hud;
        SpriteFont font;

        
        

//        public static DebugMonitor DebugMonitor = new DebugMonitor();


        public PlayState(DemoGame game,string LevelFile) : base(game)
        {            
            SpriteBatcher = game.Services.GetService<SpriteBatcher>();
            camera = game.Services.GetService<FocusCamera<Vector2>>();
            settings = game.Services.GetService<GameSettings>();
            scriptingEngine = game.Services.GetService<ScriptingEngine>();
            context = game.Services.GetService<GameContext>();
            DebugMonitor = new DebugMonitor(game);
            
            lvlFile = LevelFile;

            

            hud = new HeadsUpDisplay();


            LoadContent(game.ContentManager);
            Initialize();
        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            scriptingEngine.Update(gameTime);

            if (Input.WasPressed(0, Buttons.Start, Keys.Escape))
            {
                // push our start menu onto the stack
                GameManager.PushState(new OptionsMenuState(OurGame));
            }
            if (Input.WasPressed(0,Buttons.DPadRight,Keys.F1))
                context.lvl.SpawnPlayer(null);
            if (Input.WasPressed(0, Buttons.LeftShoulder, Keys.OemMinus))
                camera.Zoom -= 0.2f;

            if (Input.WasPressed(0, Buttons.RightShoulder, Keys.OemPlus))
                camera.Zoom += 0.2f;

            if (Input.WasPressed(0, Buttons.LeftStick, Keys.OemTilde))
            {
                GameManager.PushState(new ConsoleScreen(OurGame));
            }
            context.lvl.Update(gameTime);

            //if (camera.focussedOnPlayer)
              //  camera.Position = context.lvl.player.Position;

            hud.Update(gameTime);

            DebugMonitor.Update(gameTime);
            //DebugMonitor.Update();

            //DebugMonitor.AddDebugValue("FrameRate", monitor.Value);
        }


        public override void Draw(GameTime gameTime)
        {
            DemoGame.graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            context.lvl.Draw(SpriteBatcher, camera);

            hud.Draw(SpriteBatcher, gameTime);

            if (settings.debugMode)
            {
                context.lvl.DrawDebug(SpriteBatcher, font, camera);
                DebugMonitor.Draw(gameTime);
            }

            base.Draw(gameTime);
        }

        protected override void LoadContent(ContentManager contentManager)
        {
            font = contentManager.Load<SpriteFont>("Font/DiagnosticFont");
            DebugMonitor.Initialize();

            if (!String.IsNullOrEmpty(lvlFile) && File.Exists(lvlFile))
            {
                context.lvl = Level.FromFile(lvlFile);
                // ????
                context.lvl.context = context;
                

                context.lvl.LoadContent(contentManager);

                //var bounds = (Rectangle)context.lvl.CustomProperties["bounds"].value;

                context.lvl.GenerateCollision();

                context.lvl.SpawnPlayer(null);

                context.HUD = hud;
                if (camera.focussedOnPlayer)
                    camera.Position = context.lvl.player.Transform.Position;

                //camera.Bounds = bounds;

                var dir = Path.Combine(Path.Combine(contentManager.RootDirectory, "Scripts"), Path.GetFileNameWithoutExtension(lvlFile)); ;
                var files = Directory.GetFiles(dir);
                scriptingEngine.LoadScript(files);
                var intro_script = "Intro";
                if (!String.IsNullOrEmpty(intro_script) && scriptingEngine.hasScriptLoaded(intro_script))
                {
                    scriptingEngine.ExecuteScript(intro_script);
                }                
            }
        }

        public override void Initialize()
        {

        }

        protected override void UnloadContent()                    
        {            
            base.UnloadContent();
        }
    }
}
