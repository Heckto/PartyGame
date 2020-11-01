using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended;

namespace AuxLib.Camera
{
    public class BoundedCamera<T> : Camera<Vector2>, IMovable, IRotatable
    {
        public bool focussed = true;

        protected Viewport viewport;
        private float _maximumZoom = float.MaxValue;
        private float _minimumZoom;

        private Rectangle? _limits;


        public BoundedCamera(Viewport viewport)
        {
            Rotation = 0;
            Origin = new Vector2(viewport.Width / 2.0f, viewport.Height / 2.0f);
            Zoom = 1.0f;
            Position = Vector2.Zero;
            this.viewport = viewport;
            
        }

        
        public override Vector2 Origin { get; set; }
        public override float Zoom { get; set; }
        public override float Rotation { get; set; }

        private Vector2 _position;

        public override Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;

                // If there's a limit set and the camera is not transformed clamp position to limits
                if (Limits != null && Zoom == 1.0f && Rotation == 0.0f)
                {
                    _position.X = MathHelper.Clamp(_position.X, Limits.Value.X, Limits.Value.X + Limits.Value.Width - viewport.Width);
                    _position.Y = MathHelper.Clamp(_position.Y, Limits.Value.Y, Limits.Value.Y + Limits.Value.Height - viewport.Height);
                }
            }
        }

        //public override Vector2 Position { get; set; }
        public override Vector2 Center => Position + Origin;

        public Rectangle? Limits
        {
            get { return _limits; }
            set
            {
                if (value != null)
                {
                    // Assign limit but make sure it's always bigger than the viewport
                    _limits = new Rectangle
                    {
                        X = value.Value.X,
                        Y = value.Value.Y,
                        Width = System.Math.Max(viewport.Width, value.Value.Width),
                        Height = System.Math.Max(viewport.Height, value.Value.Height)
                    };

                    // Validate camera position with new limit
                    Position = Position;
                }
                else
                {
                    _limits = null;
                }
            }
        }


        public override float MinimumZoom
        {
            get => _minimumZoom;
            set
            {
                if (value < 0)
                    throw new ArgumentException("MinimumZoom must be greater than zero");

                if (Zoom < value)
                    Zoom = MinimumZoom;

                _minimumZoom = value;
            }
        }

        public override float MaximumZoom
        {
            get => _maximumZoom;
            set
            {
                if (value < 0)
                    throw new ArgumentException("MaximumZoom must be greater than zero");

                if (Zoom > value)
                    Zoom = value;

                _maximumZoom = value;
            }
        }

        public override void Move(Vector2 direction)
        {
            Position += Vector2.Transform(direction, Matrix.CreateRotationZ(-Rotation));
        }

        public override void Rotate(float deltaRadians)
        {
            Rotation += deltaRadians;
        }

        public override void ZoomIn(float deltaZoom)
        {
            ClampZoom(Zoom + deltaZoom);
        }

        public override void ZoomOut(float deltaZoom)
        {
            ClampZoom(Zoom - deltaZoom);
        }

        private void ClampZoom(float value)
        {
            if (value < MinimumZoom)
                Zoom = MinimumZoom;
            else
                Zoom = value > MaximumZoom ? MaximumZoom : value;
        }


        public Matrix GetViewMatrix(Vector2 parallax)
        {
            return Matrix.CreateTranslation(new Vector3(-Position * parallax, 0.0f)) *
                Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Zoom, Zoom, 1) *
                Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }

        public override Matrix GetInverseViewMatrix()
        {
            return Matrix.Invert(GetViewMatrix());
        }



        public override Matrix GetViewMatrix()
        {
            return GetViewMatrix(Vector2.One);
        }

        public override void LookAt(Vector2 position)
        {
            Position = position - Origin;
        }

        public override RectangleF BoundingRectangle
        {
            get
            {
                var frustum = GetBoundingFrustum();
                var corners = frustum.GetCorners();
                var topLeft = corners[0];
                var bottomRight = corners[2];
                var width = bottomRight.X - topLeft.X;
                var height = bottomRight.Y - topLeft.Y;
                return new RectangleF(topLeft.X, topLeft.Y, width, height);
            }
        }

        private Matrix GetProjectionMatrix(Matrix viewMatrix)
        {
            
            var projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, -1, 0);
            Matrix.Multiply(ref viewMatrix, ref projection, out projection);
            return projection;
        }

        public Vector2 WorldToScreen(float x, float y)
        {
            return WorldToScreen(new Vector2(x, y));
        }

        public override Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition + new Vector2(viewport.X, viewport.Y), GetViewMatrix());
        }

        public Vector2 ScreenToWorld(float x, float y)
        {
            return ScreenToWorld(new Vector2(x, y));
        }

        public override Vector2 ScreenToWorld(Vector2 screenPosition)
        {            
            return Vector2.Transform(screenPosition - new Vector2(viewport.X, viewport.Y),
                Matrix.Invert(GetViewMatrix()));
        }


        public override BoundingFrustum GetBoundingFrustum()
        {
            var viewMatrix = GetViewMatrix();
            var projectionMatrix = GetProjectionMatrix(viewMatrix);
            return new BoundingFrustum(projectionMatrix);
        }

        public ContainmentType Contains(Point point)
        {
            return Contains(point.ToVector2());
        }

        public override ContainmentType Contains(Vector2 vector2)
        {
            return GetBoundingFrustum().Contains(new Vector3(vector2.X, vector2.Y, 0));
        }

        public override ContainmentType Contains(Rectangle rectangle)
        {
            var max = new Vector3(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height, 0.5f);
            var min = new Vector3(rectangle.X, rectangle.Y, 0.5f);
            var boundingBox = new BoundingBox(min, max);
            return GetBoundingFrustum().Contains(boundingBox);
        }
    }
}
