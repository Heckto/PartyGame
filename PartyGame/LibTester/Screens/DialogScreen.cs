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
    public sealed class DialogScreen : GameState
    {
        private SpriteBatcher spriteBatch;
        private SpriteFont font;


        public DialogScreen(Game game) : base(game)
        {
            BlockUpdating = true;
            BlockDrawing = false;
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
                GameManager.PopState();                
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            var size = font.MeasureString("dialog screen");
            var pos = new Vector2((Game.GraphicsDevice.Viewport.Width - size.X) / 2, (Game.GraphicsDevice.Viewport.Height - size.Y) / 2);            
            var r = new Rectangle((int)(0.2 * Game.GraphicsDevice.Viewport.Width), (int)(0.2f * Game.GraphicsDevice.Viewport.Height), (int)(0.6 * Game.GraphicsDevice.Viewport.Width), (int)(0.6f * Game.GraphicsDevice.Viewport.Height));
            spriteBatch.DrawBoxFilled(r, new Color(Color.Black, 0.5f));
            spriteBatch.DrawString(font, "dialog screen", pos, Color.Green);
            base.Draw(gameTime);
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
