//#define CAR1

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Text;
using MonoGame.Extended.Sprites;
using Microsoft.Xna.Framework.Content;
using AuxLib;
using AuxLib.Input;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using tainicom.Aether.Physics2D.Dynamics;
using LibTester.Controllers;

namespace LibTester
{
    public class Player : DrawableGameComponent
    {
        //public Body CollisionBox;

        public Sprite sprite;
        public Sprite tireSprite;

        public bool HasDied = false;

        protected Texture2D _texture;
        protected Texture2D debugTexture;
        private SpriteBatcher spriteBatch;

        

        private Texture2D tireTex;

        public bool IsRemoved = false;

        public Car m_car;
        //public Car2 m_car2;
        public Player(Game game, World world) : base(game)
        {
            spriteBatch = Game.Services.GetService<SpriteBatcher>();

            var transform = new Transform2(new Vector2(1400, 1500),90);

            m_car = new Car2(Game, world, transform);



        }

        //public void LoadContent(ContentManager contentManager,World world) {

        //    spriteBatch = Game.Services.GetService<SpriteBatcher>();

        //    var text = contentManager.Load<Texture2D>("Players/BlackCar");            
        //    sprite = new Sprite(text);

        //    tireTex = new Texture2D(Game.GraphicsDevice, 10, 15);
        //    Color[] data = new Color[10 * 15];
        //    for (int pixel = 0; pixel < data.Length; pixel++)
        //    {
        //        //the function applies the color according to the specified pixel
        //        data[pixel] = Color.Black;
        //    }

        //    //set the color
        //    tireTex.SetData(data);

        //    tireSprite = new Sprite(tireTex);

        //    Initialize(world);
        //}

        public override void Draw(GameTime gameTime)
        {
            m_car.DrawDebug(spriteBatch);
        }

        public void Initialize(World world)
        {
            

        }

        public override void Update(GameTime gameTime)
        {
            m_car.Update(gameTime);
        }
    }   
}
