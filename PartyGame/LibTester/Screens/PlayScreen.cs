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

namespace LibTester.Screens
{
    public sealed class PlayScreen : GameState
    {
        private SpriteBatcher spriteBatch;
        //private SpriteFont font;
        //private Texture2D tex;

        // The tile map
        private TiledMap map;
        private Player player;
        // The renderer for the map
        private TiledMapRenderer mapRenderer;
        private BoundedCamera<Vector2> camera;
        private World CollisionWorld;
        private DebugView debugView;

        public PlayScreen(Game game) : base(game)
        {
            camera = new BoundedCamera<Vector2>(game.GraphicsDevice.Viewport);
        }

        public override void Initialize()
        {
            var content = Game.Content;
            CollisionWorld = new World();
            debugView = new DebugView(CollisionWorld);
            LoadContent(content);
        }

        protected override void LoadContent(ContentManager contentManager)
        {

            spriteBatch = Game.Services.GetService<SpriteBatcher>();
            //font = contentManager.Load<SpriteFont>("DialogFont");
            //tex = contentManager.Load<Texture2D>("test");

            debugView.LoadContent(spriteBatch.GraphicsDevice, contentManager);

            map = contentManager.Load<TiledMap>("Maps/track1");

            createCollisionWorld(map);

            // Create the map renderer
            mapRenderer = new TiledMapRenderer(GraphicsDevice);
            var bounds = new Rectangle(0, 0, map.WidthInPixels, map.HeightInPixels);
            camera.Limits = bounds;
            mapRenderer.LoadMap(map);

            CreatePlayer(contentManager);
        }


        private void createCollisionWorld(TiledMap map)
        {            
            var l = map.GetLayer<TiledMapObjectLayer>("Objectlaag 1");
            foreach (var obj in l.Objects)
            { 
                switch (obj.Type)
                {
                    case "Polygon":
                        {
                            var polygon = (TiledMapPolygonObject)obj;
                            for (var idx= 0; idx < polygon.Points.Length - 1; idx++)
                            {
                                
                                var colBody = CollisionWorld.CreateEdge(ConvertUnits.ToSimUnits(polygon.Points[idx] + polygon.Position), ConvertUnits.ToSimUnits(polygon.Points[idx + 1] + polygon.Position));
                                colBody.SetCollisionCategories(Category.Cat2);
                            }
                            break;
                        }
                    case "Rectangle":
                        {
                            var rect = (TiledMapRectangleObject)obj;
                            var origin = new Vector2(rect.Size.Width / 2, rect.Size.Height / 2);
                            var colBody = CollisionWorld.CreateRectangle(ConvertUnits.ToSimUnits(rect.Size.Width), ConvertUnits.ToSimUnits(rect.Size.Height),1, ConvertUnits.ToSimUnits(rect.Position+ origin),rect.Rotation);
                            colBody.SetCollisionCategories(Category.Cat2);
                            break;
                        }
                }
            }

            
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




            base.Update(gameTime);

            player.Update(gameTime);

            CollisionWorld.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
            mapRenderer.Update(gameTime);
            

            
            camera.LookAt(player.Transform.Position);
        }

        public override void Draw(GameTime gameTime)
        {
            
            foreach(var layer in map.Layers)
                mapRenderer.Draw(layer, camera.GetViewMatrix());

            spriteBatch.Begin(transformMatrix : camera.GetViewMatrix());
            player.Draw(spriteBatch);

            var vp = spriteBatch.GraphicsDevice.Viewport;

            var Projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(vp.Width), ConvertUnits.ToSimUnits(vp.Height), 0f, 0f, 1f);
            var View = getScaledViewMatrix();

            debugView.RenderDebugData(Projection, View);
            spriteBatch.End();
        }


        public Matrix getScaledViewMatrix()
        {
            return

                Matrix.CreateTranslation(ConvertUnits.ToSimUnits(new Vector3(-camera.Position, 0))) *
                Matrix.CreateRotationZ(camera.Rotation) *
                Matrix.CreateScale(camera.Zoom);
        }

       

        private void CreatePlayer(ContentManager contentManager)
        {
            player = new Player(Game);
            player.LoadContent(contentManager,CollisionWorld);         
        }

        protected override void UnloadContent()
        {
            //font = null;
            base.UnloadContent();
        }

    }
}
