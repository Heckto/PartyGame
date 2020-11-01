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

namespace LibTester.Screens
{
    public sealed class IntroScreen : GameState
    {
        private SpriteBatcher spriteBatch;
        private SpriteFont font;


        public IntroScreen(Game game) : base(game)
        {

        }

        public override void Initialize()
        {            
            var content = Game.Content;            
            LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (Input.WasPressed(0, Buttons.Start, Keys.Enter))
            {
                GameManager.ChangeState(new PlayScreen(Game), new FadeTransition(Game.GraphicsDevice, Color.Blue, 2.0f));
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            var size = font.MeasureString("intro screen");
            var pos = new Vector2((Game.GraphicsDevice.Viewport.Width - size.X) / 2, (Game.GraphicsDevice.Viewport.Height - size.Y) / 2);

            var r = new Rectangle((int)(0.1 * Game.GraphicsDevice.Viewport.Width), (int)(0.1f * Game.GraphicsDevice.Viewport.Height), (int)(0.8 * Game.GraphicsDevice.Viewport.Width), (int)(0.8f * Game.GraphicsDevice.Viewport.Height));
            spriteBatch.DrawBoxFilled(r, new Color(Color.White, 0.2f));

            //spriteBatch.DrawBox(r, new Color(Color.White, 0.5f),2);

            spriteBatch.DrawString(font, "intro screen", pos, Color.Red);

            

            spriteBatch.End();
        }

        protected override void LoadContent(ContentManager contentManager)
        {
            
            spriteBatch = Game.Services.GetService<SpriteBatcher>();
            font = contentManager.Load<SpriteFont>("DialogFont");

        }
        
        protected override void UnloadContent()
        {
            font = null;
            base.UnloadContent();
        }

    }
}
