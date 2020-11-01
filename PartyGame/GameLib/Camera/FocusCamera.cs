using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AuxLib.Camera
{
    public class FocusCamera<T> : BoundedCamera<T>
    {
        public float verticalOffset = 1;
        public float lookAheadDstX = 300;
        public float lookSmoothTimeX = 0.5f;
        public float verticalSmoothTime= .2f;
        public Vector2 focusAreaSize = new Vector2(200, 300);

        public FocusArea focusArea;
        public Vector2 focusPosition = Vector2.Zero;

        float currentLookAheadX;
        float targetLookAheadX;
        float lookAheadDirX;
        float smoothLookVelocityX;
        float smoothVelocityY;
        bool lookAheadStopped;

        public bool focussedOnPlayer = true;

        public FocusCamera(Viewport viewport) : base(viewport) {

            var bb = new Rectangle(
                           (int)(Position.X - 55),
                           (int)(Position.Y - 55),
                           (int)110,
                           (int)180);

            focusArea = new FocusArea(bb, focusAreaSize);
        }

        //public Rectangle getFocusArea()
        //{
        //    return new Rectangle((int)(focusArea.left + Origin.X),(int)(focusArea.top + Origin.Y), (int)focusAreaSize.X,(int)focusAreaSize.Y)
        //}

        public Vector2[] getFocusArea()
        {
            return new Vector2[] {
                new Vector2(focusArea.left,focusArea.top) + Origin,
                new Vector2(focusArea.right, focusArea.top) + Origin,
                new Vector2(focusArea.right, focusArea.bottom) + Origin,
                new Vector2(focusArea.left, focusArea.bottom) + Origin
            };
        }

        public override void LookAt(Vector2 position)
        {
            base.LookAt(position);

            
        }

        public void UpdateCamera(GameTime gameTime, Vector2 latestVelocity)
        {
            
            if (Position != null)
            {

                var bb = new Rectangle(
                       (int)(Position.X - 55),
                       (int)(Position.Y - 55),
                       (int)110,
                       (int)180);

                focusArea.Update(bb);

                focusPosition = focusArea.center + new Vector2(0, 1) * verticalOffset;

                if (focusArea.velocity.X != 0)
                {
                    lookAheadDirX = Math.Sign(focusArea.velocity.X);
                    if (Math.Sign(latestVelocity.X) == Math.Sign(focusArea.velocity.X) && latestVelocity.X != 0)
                    {
                        lookAheadStopped = false;
                        targetLookAheadX = lookAheadDirX * lookAheadDstX;
                    }
                    else
                    {
                        if (!lookAheadStopped)
                        {
                            lookAheadStopped = true;
                            targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4f;
                        }
                    }
                }


                currentLookAheadX = Maths.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX, 1000, (float)gameTime.ElapsedGameTime.TotalSeconds);


                // Y lookout uit for now !!!!
                //focusPosition.Y = (Player.SmoothDamp(ConvertUnits.ToSimUnits(Position.Y), focusPosition.Y, ref smoothVelocityY, verticalSmoothTime,100, (float)gameTime.ElapsedGameTime.TotalSeconds));
                focusPosition += new Vector2(1, 0) * currentLookAheadX;

                if (focussedOnPlayer)
                    Position = focusPosition;
                //Position = ConvertUnits.ToDisplayUnits(focusPosition);
            }
        }
    }

    public struct FocusArea
    {
        public Vector2 velocity;
        public Vector2 center;
        public float left, right;
        public float top, bottom;

        public FocusArea(Rectangle targetBounds, Vector2 size)
        {
            
            left = targetBounds.Left - size.X / 2;
            right = targetBounds.Right + size.X / 2;
            bottom = targetBounds.Bottom;
            top = targetBounds.Top - size.Y;
            velocity = Vector2.Zero;
            center = new Vector2(left + right / 2, top + bottom / 2);
        }

        public void Update(Rectangle targetBounds)
        {
            float shiftX = 0;
            if (targetBounds.Left < left)
            {
                shiftX = targetBounds.Left - left;
            }
            else if (targetBounds.Right > right)
            {
                shiftX = targetBounds.Right - right;
            }
            left += shiftX;
            right += shiftX;

            float shiftY = 0;
            if (targetBounds.Top < top)
            {
                shiftY = targetBounds.Top - top;
            }
            else if (targetBounds.Bottom > bottom)
            {
                shiftY = targetBounds.Bottom - bottom;
            }
            top += shiftY;
            bottom += shiftY;
            center = new Vector2((left + right) / 2, (top + bottom) / 2);
            velocity = new Vector2(shiftX, shiftY);
        }
    }
}
