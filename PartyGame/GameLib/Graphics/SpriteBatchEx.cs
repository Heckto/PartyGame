using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AuxLib
{
    public class SpriteBatcher : SpriteBatch
    {
        public static Texture2D pixel;
        public static Texture2D circle;
        const int circleTextureRadius = 512;

        public SpriteBatcher(GraphicsDevice graphics) : base(graphics)
        {
            
            pixel = new Texture2D(graphics, 1, 1);
            pixel.SetData(new[] { Color.White });
            circle = CreateCircleTexture(graphics, circleTextureRadius, 0, 1, 1, Color.White, Color.White);

        }

        private Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int radius, int borderWidth,
                                                    int borderInnerTransitionWidth, int borderOuterTransitionWidth,
                                                    Color color, Color borderColor)
        {
            var diameter = radius * 2;
            var center = new Vector2(radius, radius);

            var circle = new Texture2D(graphicsDevice, diameter, diameter);
            var colors = new Color[diameter * diameter];
            var y = -1;
            for (var i = 0; i < colors.Length; i++)
            {
                var x = i % diameter;

                if (x == 0)
                {
                    y += 1;
                }

                var diff = new Vector2(x, y) - center;
                var length = diff.Length(); // distance.Length();

                if (length > radius)
                {
                    colors[i] = Color.Transparent;
                }
                else if (length >= radius - borderOuterTransitionWidth)
                {
                    var transitionAmount = (length - (radius - borderOuterTransitionWidth)) / borderOuterTransitionWidth;
                    transitionAmount = 255 * (1 - transitionAmount);
                    colors[i] = new Color(borderColor.R, borderColor.G, borderColor.B, (byte)transitionAmount);
                }
                else if (length > radius - (borderWidth + borderOuterTransitionWidth))
                {
                    colors[i] = borderColor;
                }
                else if (length >= radius - (borderWidth + borderOuterTransitionWidth + borderInnerTransitionWidth))
                {
                    var transitionAmount = (length -
                                              (radius -
                                               (borderWidth + borderOuterTransitionWidth + borderInnerTransitionWidth))) /
                                             (borderInnerTransitionWidth + 1);
                    colors[i] = new Color((byte)MathHelper.Lerp(color.R, borderColor.R, transitionAmount),
                                          (byte)MathHelper.Lerp(color.G, borderColor.G, transitionAmount),
                                          (byte)MathHelper.Lerp(color.B, borderColor.B, transitionAmount));
                }
                else
                {
                    colors[i] = color;
                }
            }
            circle.SetData(colors);
            return circle;
        }




        public void DrawPixel(int x, int y, Color c)
        {
            Draw(pixel, new Vector2(x, y), c);
        }


        public void DrawBox(Rectangle r, Color c, int linewidth)
        {
            DrawLine(r.Left, r.Top, r.Right, r.Top, c, linewidth);
            DrawLine(r.Right, r.Y, r.Right, r.Bottom, c, linewidth);
            DrawLine(r.Right, r.Bottom, r.Left, r.Bottom, c, linewidth);
            DrawLine(r.Left, r.Bottom, r.Left, r.Top, c, linewidth);
        }


        public void DrawBoxFilled(float x, float y, float w, float h, Color c)
        {
            Draw(pixel, new Rectangle((int)x, (int)y, (int)w, (int)h), c);
        }

        public void DrawBoxFilled(Vector2 upperLeft, Vector2 lowerRight, Color c)
        {
            var r = Extensions.RectangleFromVectors(upperLeft, lowerRight);
            Draw(pixel, r, c);
        }

        public void DrawBoxFilled(Rectangle r, Color c)
        {
            Draw(pixel, r, c);
        }

        public void DrawCircle(Vector2 position, float radius, Color c, int linewidth)
        {
            DrawPolygon(MakeCircle(position, radius, 32), c, linewidth);
        }

        public void DrawCircleFilled(Vector2 position, float radius, Color c)
        {
            Draw(circle, position, null, c, 0, new Vector2(circleTextureRadius, circleTextureRadius), radius / circleTextureRadius, SpriteEffects.None, 0);
        }


        public void DrawLine(float x1, float y1, float x2, float y2, Color c, int linewidth)
        {
            var v = new Vector2(x2 - x1, y2 - y1);
            var rot = (float)Math.Atan2(y2 - y1, x2 - x1);
            Draw(pixel, new Vector2(x1, y1), new Rectangle(1, 1, 1, linewidth), c, rot,
                new Vector2(0, linewidth / 2), new Vector2(v.Length(), 1), SpriteEffects.None, 0);
        }
        public void DrawLine(Vector2 startpos, Vector2 endpos, Color c, int linewidth)
        {
            DrawLine(startpos.X, startpos.Y, endpos.X, endpos.Y, c, linewidth);
        }


        public void DrawPath(Vector2[] points, Color c, int linewidth)
        {
            for (var i = 0; i < points.Length - 1; i++)
            {
                DrawLine(points[i], points[i + 1], c, linewidth);
            }
        }

        public void DrawPolygon(Vector2[] points, Color c, int linewidth)
        {
            DrawPath(points, c, linewidth);
            DrawLine(points[points.Length - 1], points[0], c, linewidth);
        }


        private Vector2[] MakeCircle(Vector2 position, float radius, int numpoints)
        {
            var polygon = new Vector2[numpoints];
            float angle = 0;
            for (var i = 0; i < numpoints; i++)
            {
                var x = (float)Math.Cos(angle) * radius;
                var y = (float)Math.Sin(angle) * radius;
                polygon[i] = position + new Vector2(x, y);
                angle += MathHelper.TwoPi / (float)numpoints;
            }
            return polygon;
        }



    }
}
