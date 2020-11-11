using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace XNACardemo
{
    class SpriteHelper
    {
        static SpriteBatch spriteBatch = null;
        Texture2D texture;
        Rectangle gfxRect;

        static List<SpriteToRender> sprites =
            new List<SpriteToRender>();

        #region SpriteToRender helper class
        class SpriteToRender
        {
            public Texture2D texture;
            public Rectangle rect;
            public Rectangle? sourceRect;
            public Color color;
            public float rotation = 0f;

            public SpriteToRender(Texture2D setTexture, Rectangle setRect, float setRotation,
                Rectangle? setSourceRect, Color setColor)
            {
                texture = setTexture;
                rotation = setRotation;
                rect = setRect;
                sourceRect = setSourceRect;
                color = setColor;
            } 
        } 
        #endregion

        public SpriteHelper(Texture2D setTexture, Rectangle? setGfxRect)
        {
            texture = setTexture;
            if (setGfxRect == null)
                gfxRect = new Rectangle(0, 0, texture.Width, texture.Height);
            else
                gfxRect = setGfxRect.Value;
        }


        public void Render(Rectangle rect, Color color, float rotation)
        {
            sprites.Add(new SpriteToRender(texture, rect, rotation, gfxRect, color));
        }

        #region DrawSprites
        public static void DrawSprites(int width, int height)
        {
            // No need to render if we got no sprites this frame
            if (sprites.Count == 0)
                return;

            // Create sprite batch if we have not done it yet.
            // Use device from texture to create the sprite batch.
            if (spriteBatch == null)
                spriteBatch = new SpriteBatch(sprites[0].texture.GraphicsDevice);

            Texture2D lastSpriteTexture = null;
            bool spriteBatchStarted = false;
            // Render all sprites
            foreach (SpriteToRender sprite in sprites)
            {
                // Start rendering sprites
                // Note: Now moved inside loop to fix most render sorting errors!
                if (lastSpriteTexture != sprite.texture)
                {
                    if (spriteBatchStarted)
                        spriteBatch.End();
                    spriteBatchStarted = true;
                    spriteBatch.Begin(SpriteSortMode.BackToFront,BlendState.AlphaBlend);
                } // if

                spriteBatch.Draw(sprite.texture,
                    // Rescale to fit resolution
                    new Rectangle(
                    sprite.rect.X,
                    sprite.rect.Y ,
                    sprite.rect.Width,
                    sprite.rect.Height),
                    sprite.sourceRect, sprite.color, sprite.rotation, new Vector2(sprite.rect.Width / 2, sprite.rect.Height / 2), SpriteEffects.None, 0);
            } // foreach

            // We are done, draw everything on screen with help of the end method.
            if (spriteBatchStarted)
                spriteBatch.End();

            // Kill list of remembered sprites
            sprites.Clear();
        } // DrawSprites()
        #endregion


    }
}
