using Microsoft.Xna.Framework;
using Game1.GameObjects.Levels;
using AuxLib.ScreenManagement;
using AuxLib.Camera;
using Microsoft.Xna.Framework.Input;
using System.IO;
using AuxLib.ScreenManagement.Transitions;
using Game1.Screens;
using System;
using System.Threading.Tasks;
using Game1.Scripting;
using AuxLib.Input;
using Game1.GameObjects.Sprite;
using AuxLib.Sound;
using Game1.HUD;
using AuxLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Game1.DataContext
{
    public partial class GameContext
    {
        public TaskCompletionSource<FrameNotifyer> currentFrameSource;

        public SpriteBatcher SpriteBatcher;
        public FocusCamera<Vector2> camera;
        public Level lvl;
        public TransitionManager transitionManager;
        public GameStateManager gameManager;
        public InputHandler input;
        public ScriptingEngine scripter;
        public HeadsUpDisplay HUD;
        public ContentManager content;
        public GraphicsDevice graphics;

        public LivingSpriteObject SpawnEnemy(string name, int x, int y)
        {            
            var location = new Vector2(x, y);
            return lvl.SpawnEnemy(name, location);
        }

        public void SpawnEnemy(string name)
        {
            var m = Matrix.Invert(camera.GetViewMatrix());
            var mousePos = Mouse.GetState().Position.ToVector2();
            var worldPos = Vector2.Transform(mousePos, m);
            lvl.SpawnEnemy(name, worldPos);
        }

        public void SpawnPlayer(int x, int y)
        {
            var location = new Vector2(x, y);
            lvl.SpawnPlayer(location);
        }

        public void SpawnPlayer()
        {
            var m = Matrix.Invert(camera.GetViewMatrix());
            var mousePos = Mouse.GetState().Position.ToVector2();
            var worldPos = Vector2.Transform(mousePos, m);
            lvl.SpawnPlayer(worldPos);
        }

        public string ListPlayers()
        {
            var msg = lvl.player.ToString();
            return msg;
        }

        public string ListEnemies()
        {
            var msg = String.Empty;
            //foreach(var entry in lvl.Sprites)
            //{
            //    msg += entry.Key + " " + entry.Value.Position.ToString() + Environment.NewLine;
            //}
            return msg;
        }

        public async Task MoveCamera(Vector2 dest, float speed)
        {
            var perc = 0f;
            var cameraStart = camera.Position;
            while (camera.Position != dest)
            {                
                var frameData = await currentFrameSource.Task;
                if (frameData.token.IsCancellationRequested)
                    frameData.token.Token.ThrowIfCancellationRequested();
                perc = MathHelper.Clamp(perc + speed, 0, 1);                
                var newPos = Vector2.Lerp(cameraStart, dest, perc);

                camera.Position = newPos;
                    
            }
        }

        public async Task MovePlayer(Vector2 dest, float speed)
        {
            var perc = 0f;
            var player = lvl.player;
            var playerStart = player.Transform.Position;
            while ((player.Transform.Position - dest).Length() > 3)
            {
                var frameData = await currentFrameSource.Task;
                if (frameData.token.IsCancellationRequested)
                    frameData.token.Token.ThrowIfCancellationRequested();
                perc = MathHelper.Clamp(perc + speed, 0, 1);
                //player.Ve = new Vector2(1,player.Trajectory.Y);
                if (player.Transform.Position.X > dest.X)
                    break;
                player.controller.Move(new Vector2(10.5f, 0));
            }

            //player.Trajectory = new Vector2(0f,0.00166667777f);
        }

        public void HaltPlayer()
        {            
            var player = lvl.player;        
            player.SetAnimation("Idle");
        }

        public void SetAnimation(string anim)
        {
            var player = lvl.player;
            
             player.SetAnimation(anim);
        }

        public async Task MovePlayerToMouse()
        {
            try
            {
                var speed = 0.02f;
                var m = Matrix.Invert(camera.GetViewMatrix());
                var mousePos = Mouse.GetState().Position.ToVector2();
                var dest = Vector2.Transform(mousePos, m);
                var perc = 0f;
                var player = lvl.player;
                var playerStart = player.Transform.Position;
                while ((player.Transform.Position - dest).Length() > 3)
                {
                    var ass = (player.Transform.Position - dest).Length();
                    var frameData = await currentFrameSource.Task;
                    if (frameData.token.IsCancellationRequested)
                        frameData.token.Token.ThrowIfCancellationRequested();
                    perc = MathHelper.Clamp(perc + speed, 0, 1);
                    var newPos = Vector2.Lerp(playerStart, dest, perc);
                    //player.Trajectory = new Vector2(5, player.Trajectory.Y);
                    if (player.Transform.Position.X > dest.X)
                        break;
                }

                //player.Trajectory = Vector2.Zero;
            }
            catch ( Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SetUserInput(bool active)
        {
            lvl.player.HandleInput = active;            
        }

        public void SetTransition(bool active)
        {
            transitionManager.canTransition = active;
        }



        public async Task DisplayDialog(string msg,string asset)
        {
            var dialogState = new DialogState(gameManager.Game,msg,asset, false);
            gameManager.PushState(dialogState);
            await dialogState.tcs.Task;
        }

        public void DisplayHUDText(string key,string msg,float TTL,float fadeTime)
        {
            if (HUD != null)
            {
                var textDisplay = new LevelIntroText(msg, TTL, fadeTime);
                textDisplay.LoadContent(this.content);
                HUD.AddHUDComponent(key, textDisplay);
            }
        }

        public void playSFX(string sfx)
        {
            AudioManager.PlaySoundEffect(sfx);
        }

        public GameContext() {}
    }

    
}
