using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AuxLib.ScreenManagement;
using AuxLib.Input;
using AuxLib;
using LibTester.Screens;

namespace LibTester
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatcher _spriteBatch;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {

            _graphics.PreferredBackBufferWidth = _graphics.GraphicsDevice.DisplayMode.Width;
            _graphics.PreferredBackBufferHeight = _graphics.GraphicsDevice.DisplayMode.Height;
            _graphics.ApplyChanges();
            Window.IsBorderless = false;

            _spriteBatch = new SpriteBatcher(GraphicsDevice);


            Services.AddService(_spriteBatch);

            // TODO: Add your initialization logic here
            var inputHandler = InputHandler.InitializeSingleton(this);
            Services.AddService(inputHandler);
            //Components.Add(inputHandler);


            var gameManager = new GameStateManager(this);
            Services.AddService(gameManager);





            gameManager.PushState(new IntroScreen(this));



            Components.Add(inputHandler);
            Components.Add(gameManager);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);           

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }

    
}
