﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuxLib.Input;
using System.IO;
using AuxLib.Debug;
using Microsoft.Xna.Framework.Content;
using AuxLib;

namespace Game1.Screens
{
    public sealed class DialogState : BaseGameState, IIntroState
    {
        public TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        private Rectangle dialogRect;
        private SpriteFont dialogFont;
        private Rectangle pictureRect;
        private Texture2D pictureTex;

        private Rectangle textRect;
        private SpriteBatcher SpriteBatcher;        
        private SoundEffect sound;

        private string CappedMsg;
        
        private string assetPic;
        private readonly string dialog;
        private string wrappedMsg = String.Empty;
        private int lineCnt = 0;
        private string[] lines;
        private Vector2[] lineIdx;
        private int dialogIdx = 0;

        string currentText = "";
        int currentTextIdx = 0;
        float timeSinceLastIncrement = 0;


        public DialogState(Game game,string msg,string pic, bool blockUpdating = true) : base(game)
        {
            dialog = msg;
            assetPic = pic;
            BlockDrawing = false;
            BlockUpdating = blockUpdating;
        }

        public override void Initialize()
        {
            LoadContent();
        }

        private string[] WrapText(string text,float lineSize,int dialogHeigth)
        {
            var words = text.Split(new char[] { ' '
            });
            var sb = new StringBuilder();
            var linewidth = 0f;
            var spaceWidth = dialogFont.MeasureString(" ").X;
            var fontHeight = dialogFont.MeasureString(" ").Y;
            lineCnt = (int)((dialogHeigth - 20) / fontHeight);
            foreach (var word in words)
            {
                var size = dialogFont.MeasureString(word);
                if (linewidth + size.X < lineSize)
                {
                    sb.Append(word + " ");
                    linewidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    linewidth = size.X + spaceWidth;
                }
            }

            CappedMsg = sb.ToString();
            var strings = sb.ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            lineIdx = new Vector2[strings.Length];
            for (var idx = 0; idx < strings.Length; idx++)
            {
                lineIdx[idx] = new Vector2(textRect.X + 10, textRect.Y + (fontHeight * idx + 10));
            }

            return strings;
        }

        public override void Update(GameTime gameTime)
        {
            timeSinceLastIncrement += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastIncrement >= 0.1 && currentTextIdx < CappedMsg.Length)
            {
                
                currentText += CappedMsg[currentTextIdx];
                if (CappedMsg[currentTextIdx] != ' ')
                    sound.Play();

                currentTextIdx++;
                timeSinceLastIncrement = 0;
                
            }


            if (Input.WasPressed(0, Buttons.B, Keys.Escape))
            {
                
                GameManager.PopState();
                tcs.SetResult(true);
            }

            if (Input.WasPressed(0, Buttons.Start, Keys.Enter))
            {
                if (dialogIdx < lines.Length - lineCnt)
                    dialogIdx++;
            }
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            SpriteBatcher.Begin();
            SpriteBatcher.DrawBoxFilled(dialogRect, new Color(Color.Black, 0.5f));
            SpriteBatcher.Draw(pictureTex, pictureRect, Color.White);
            SpriteBatcher.DrawString(dialogFont, currentText, lineIdx[0], Color.White);
            base.Draw(gameTime);
            SpriteBatcher.End();
        }

        protected override void LoadContent(ContentManager contentManager)
        {
            SpriteBatcher = OurGame.Services.GetService<SpriteBatcher>();
            
            pictureTex = contentManager.Load<Texture2D>(@"Misc\" + assetPic);            
            dialogFont = contentManager.Load<SpriteFont>("Font/DialogFont");
            sound = contentManager.Load<SoundEffect>(@"sfx\typewriter");
            

            var x1 = (int)(0.1f * DemoGame.graphics.GraphicsDevice.DisplayMode.Width);
            var y1 = (int)(0.7f * DemoGame.graphics.GraphicsDevice.DisplayMode.Height);
            var width = (int)(0.8f * DemoGame.graphics.GraphicsDevice.DisplayMode.Width);
            var height = (int)(0.25f * DemoGame.graphics.GraphicsDevice.DisplayMode.Height);
            dialogRect = new Rectangle(x1, y1, width, height);

            pictureRect = new Rectangle(x1 + (int)(0.025f * dialogRect.Width), y1 + (int)(0.1f * dialogRect.Height), (int)(0.2 * dialogRect.Width), (int)(0.8 * dialogRect.Height));

            textRect = new Rectangle(pictureRect.X + pictureRect.Width + (int)(0.025f * dialogRect.Width), y1 + (int)(0.1f * dialogRect.Height), (int)(0.725 * dialogRect.Width), (int)(0.8 * dialogRect.Height));

            lines = WrapText(dialog, 0.9f * textRect.Width,textRect.Height);

           
        }
        protected override void UnloadContent()
        {
            pictureTex = null;
            base.UnloadContent();
        }

    }
}
