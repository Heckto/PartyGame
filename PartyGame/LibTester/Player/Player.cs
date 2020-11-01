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
        public Body CollisionBox;

        private Sprite sprite;

        public bool HasDied = false;

        public float RotationVelocity = 20f;
        public float LinearVelocity = 3f;

        public const float MaxSpeed = 5.0f;

        public Rectangle hitBox;


       // private float _rotation = 0.0f;

        private float Velocity;
        private float acc = 0.0f;

        protected Texture2D _texture;
        protected Texture2D debugTexture;
        private SpriteBatcher spriteBatch;
        private IInputHandler Input;

        public Transform2 Transform { get; set; }
        public CarController controller;

        

        public bool IsRemoved = false;
        public Player(Game game) : base(game)
        {
            Transform = new Transform2(1400, 1550);
            Transform.Rotation = MathHelper.ToRadians(90);
            
        }

        public void LoadContent(ContentManager contentManager,World world) {
            var text = contentManager.Load<Texture2D>("Players/BlackCar");
            spriteBatch = Game.Services.GetService<SpriteBatcher>();
            sprite = new Sprite(text);

            Initialize(world);
        }

        public void Initialize(World world)
        {

            Input = (IInputHandler)Game.Services.GetService(typeof(IInputHandler));
            CollisionBox = world.CreateRoundedRectangle(ConvertUnits.ToSimUnits(sprite.TextureRegion.Bounds.Width), ConvertUnits.ToSimUnits(sprite.TextureRegion.Bounds.Height), ConvertUnits.ToSimUnits(4), ConvertUnits.ToSimUnits(4),16, 1, ConvertUnits.ToSimUnits(Transform.Position), Transform.Rotation, BodyType.Dynamic);
            //CollisionBox = world.CreateRectangle(ConvertUnits.ToSimUnits(sprite.TextureRegion.Bounds.Width), ConvertUnits.ToSimUnits(sprite.TextureRegion.Bounds.Height), 1, ConvertUnits.ToSimUnits(Transform.Position), Transform.Rotation, BodyType.Dynamic);
            CollisionBox.Tag = this;
            CollisionBox.IgnoreGravity = true;

            controller = new CarController(Game, CollisionBox);
        }

        public void Draw(SpriteBatcher sb)
        {
            sb.Draw(sprite, Transform);
        }

        public override void Update(GameTime gameTime)
        {
            controller.Update(gameTime);

            //var _rotation = Transform.Rotation;
            //var _position = Transform.Position;
            
            //if (Input.IsPressed(0, Buttons.LeftThumbstickLeft, Keys.Left))
            //    _rotation -= MathHelper.ToRadians(RotationVelocity);
            //else if (Input.IsPressed(0, Buttons.LeftThumbstickRight, Keys.Right))
            //    _rotation += MathHelper.ToRadians(RotationVelocity);

            //var direction = new Vector2((float)Math.Sin(_rotation), -(float)Math.Cos(_rotation));



            //if (Input.IsPressed(0, Buttons.A, Keys.Up))
            //    acc += 0.2f;

            //if (Input.IsPressed(0, Buttons.B, Keys.Down))
            //    acc -= 0.2f;


            //acc -= 0.1f;

            //var velocity = MathHelper.Clamp(acc * LinearVelocity,-MaxSpeed,MaxSpeed);
            //CollisionBox.LinearVelocity = direction * velocity;
            //CollisionBox.AngularVelocity = _rotation;

            Transform.Position = ConvertUnits.ToDisplayUnits(CollisionBox.Position);
            Transform.Rotation = CollisionBox.Rotation;
        }
    }   
}
