using AuxLib;
using AuxLib.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using System;
using System.Collections.Generic;
using System.Text;
using tainicom.Aether.Physics2D.Diagnostics;
using tainicom.Aether.Physics2D.Dynamics;

namespace LibTester
{
    public class Level : DrawableGameComponent
    {
        private readonly string LevelName;

        private SpriteBatcher spriteBatch;

        private BoundedCamera<Vector2> camera;

        public TiledMap map;

        private TiledMapRenderer mapRenderer;

        public World CollisionWorld;
        private DebugView debugView;

        public Dictionary<string, object> Properties;


        public Level(Game game, string lvl) : base(game)
        {
            LevelName = lvl;

            Initialize();

            LoadContent();
        }

        public override void Initialize()
        {
            CollisionWorld = new World(Vector2.Zero);
            debugView = new DebugView(CollisionWorld);

            spriteBatch = Game.Services.GetService<SpriteBatcher>();

            camera = Game.Services.GetService<BoundedCamera<Vector2>>();

            
        }

        protected override void LoadContent()
        {
            debugView.LoadContent(spriteBatch.GraphicsDevice, Game.Content);
            map = Game.Content.Load<TiledMap>(LevelName);

            mapRenderer = new TiledMapRenderer(GraphicsDevice);
            var bounds = new Rectangle(0, 0, map.WidthInPixels, map.HeightInPixels);

            camera.Limits = bounds;
            mapRenderer.LoadMap(map);

            CreateCollisionWorld(map);

            LoadProperties();
        }

        private void LoadProperties()
        {
            Properties = new Dictionary<string, object>();

            foreach(var item in map.Properties)
            {
                if (item.Key is "StartPos")
                {
                    var pos = item.Value.Split("|");
                    if (pos.Length == 3)
                    {
                        Properties.Add("StartPos", new Vector2(Convert.ToSingle(pos[0]), Convert.ToSingle(pos[1])));
                        Properties.Add("StartRotation", Convert.ToSingle(pos[2]));
                    }
                }
            }
        }

        public bool tryGetMapProperty<T>(string name,out T property)
        {
            if (Properties.ContainsKey(name))
            {
                property = (T)Properties[name];
                return true;
            }
            property = default(T);
            return false;
        }

        

        private void CreateCollisionWorld(TiledMap map)
        {
            var l = map.GetLayer<TiledMapObjectLayer>("Objectlaag 1");
            foreach (var obj in l.Objects)
            {
                switch (obj.Type)
                {
                    case "Polygon":
                        {
                            var polygon = (TiledMapPolygonObject)obj;
                            for (var idx = 0; idx < polygon.Points.Length - 1; idx++)
                            {
                                Body colBody;
                                if (idx < polygon.Points.Length - 2)
                                    colBody = CollisionWorld.CreateEdge(ConvertUnits.ToSimUnits(polygon.Points[idx] + polygon.Position), ConvertUnits.ToSimUnits(polygon.Points[idx + 1] + polygon.Position));
                                else                                
                                    colBody = CollisionWorld.CreateEdge(ConvertUnits.ToSimUnits(polygon.Points[polygon.Points.Length - 1] + polygon.Position), ConvertUnits.ToSimUnits(polygon.Points[0] + polygon.Position));                                   
                                colBody.SetCollisionCategories(Category.Cat2);
                            }

                            

                            break;
                        }
                    case "Rectangle":
                        {
                            var rect = (TiledMapRectangleObject)obj;
                            var origin = new Vector2(rect.Size.Width / 2, rect.Size.Height / 2);
                            var colBody = CollisionWorld.CreateRectangle(ConvertUnits.ToSimUnits(rect.Size.Width), ConvertUnits.ToSimUnits(rect.Size.Height), 1, ConvertUnits.ToSimUnits(rect.Position + origin), rect.Rotation);
                            colBody.SetCollisionCategories(Category.Cat2);
                            break;
                        }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            CollisionWorld.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
            mapRenderer.Update(gameTime);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var layer in map.Layers)
                mapRenderer.Draw(layer, camera.GetViewMatrix());


            debugView.RenderDebugData(getScaledProjMatrix(), getScaledViewMatrix());

            base.Draw(gameTime);
        }



        private Matrix getScaledProjMatrix()
        {
            
            return Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(spriteBatch.GraphicsDevice.Viewport.Width), ConvertUnits.ToSimUnits(spriteBatch.GraphicsDevice.Viewport.Height), 0f, 0f, 1f);
        }
        private Matrix getScaledViewMatrix()
        {
            return

                Matrix.CreateTranslation(ConvertUnits.ToSimUnits(new Vector3(-camera.Position, 0))) *
                Matrix.CreateTranslation(new Vector3(-ConvertUnits.ToSimUnits(camera.Origin), 0.0f)) *
                Matrix.CreateRotationZ(camera.Rotation) *
                Matrix.CreateScale(camera.Zoom) *
                Matrix.CreateTranslation(new Vector3(ConvertUnits.ToSimUnits(camera.Origin), 0.0f));
        }
    }
}
