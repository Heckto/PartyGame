﻿using Microsoft.Xna.Framework;
using System;
using AuxLib;
using tainicom.Aether.Physics2D.Dynamics;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuxLib.Input;
using Microsoft.Xna.Framework.Input;

namespace Game1
{
    public class Controller2D : RaycastController
    {
        private readonly float maxSlopeAngle = 65;        
        private readonly float fallThroughTimeout = 0.1f;

        
        public CollisionInfo collisions;

        public Vector2 latestVelocity;

        bool fallThrough;
        public Controller2D(Body body, Category collisionMask) : base(body,collisionMask)
        {            
            collisions.faceDirection = 1;
        }

        public virtual void Move(Vector2 moveAmount, bool standingOnPlatform = false)
        {
            latestVelocity = moveAmount;
            var simVelocity = ConvertUnits.ToSimUnits(moveAmount);
            
            this.fallThrough = InputHandler.Instance.IsPressed(0, Buttons.LeftThumbstickDown, Keys.Down);
            castList.Clear();

            UpdateRaycastOrigins();          
            collisions.Reset();
            collisions.moveAmountOld = moveAmount;            

            if (moveAmount.Y > 0)
            {
                DescendSlope(ref simVelocity);
            }

            if (moveAmount.X != 0)
            {
                collisions.faceDirection = Math.Sign(simVelocity.X);
            }

            HorizontalCollisions(ref simVelocity);

            if (moveAmount.Y != 0)
            {
                VerticalCollisions(ref simVelocity);
            }

            if (standingOnPlatform)
                collisions.below = true;

            collider.Position += simVelocity;            
        }

        void HorizontalCollisions(ref Vector2 moveAmount)
        {
            float directionX = collisions.faceDirection;
            float rayLength = Math.Abs(moveAmount.X) + skinWidth;

            if (Math.Abs(moveAmount.X) < skinWidth)
            {
                rayLength = 2 * skinWidth;
            }

            for (var i = 0; i < horizontalRayCount; i++)
            {
                var rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += UpVector * (horizontalRaySpacing * i);

                var from = ConvertUnits.ToDisplayUnits(rayOrigin);
                var to = ConvertUnits.ToDisplayUnits(rayOrigin + directionX * new Vector2(rayLength,0));
                castList.Add(new Ray(from, to,1));

                var hit = caster.RayCast(rayOrigin, RightVector * directionX, rayLength, out var hitInfo, collisionMask);
                if (hit)
                {
                    if (hitInfo.distance == 0)
                        continue;

                    var slopeAngle = MathHelper.ToDegrees((float)Math.Acos(Vector2.Dot(hitInfo.normal, UpVector)));
                    if (i == 0 && slopeAngle <= maxSlopeAngle)
                    {
                        if (collisions.descendingSlope)
                        {
                            collisions.descendingSlope = false;
                            moveAmount = collisions.moveAmountOld;
                        }
                        float distanceToSlopeStart = 0f;
                        if (slopeAngle != collisions.slopeAngleOld)
                        {
                            distanceToSlopeStart = hitInfo.distance - skinWidth;
                            moveAmount.X -= distanceToSlopeStart * directionX;
                        }

                        ClimbSlope(ref moveAmount, slopeAngle,hitInfo.normal);
                        moveAmount.X += distanceToSlopeStart * directionX;
                    }
                    if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
                    {                   
                        moveAmount.X = (hitInfo.distance - skinWidth) * directionX;
                        rayLength = hitInfo.distance;

                        if (collisions.climbingSlope)
                        {
                            moveAmount.Y = -(float)Math.Tan(MathHelper.ToRadians(collisions.slopeAngle)) * Math.Abs(moveAmount.X);
                        }

                        if (Math.Abs(hitInfo.normal.X) == 1 /*|| slopeAngle > maxSlopeAngle*/)
                        {
                            collisions.left = directionX == -1;
                            collisions.right = directionX == 1;
                        }
                    }
                }                
            }            
        }

        protected virtual void VerticalCollisions(ref Vector2 moveAmount)
        {
            float directionY = Math.Sign(moveAmount.Y);
            var rayLength = Math.Abs(moveAmount.Y) + skinWidth;

            for (var i = 0; i < verticalRayCount; i++)
            {
                var rayOrigin = (directionY == -1) ? raycastOrigins.topLeft : raycastOrigins.bottomLeft;
                rayOrigin += RightVector * (verticalRaySpacing * i + moveAmount.X);

                var from = ConvertUnits.ToDisplayUnits(rayOrigin);
                var to = ConvertUnits.ToDisplayUnits(rayOrigin + directionY * new Vector2(0,rayLength));
                castList.Add(new Ray(from, to, 2));
               
                var hit = caster.RayCast(rayOrigin, -UpVector * directionY, rayLength, out var hitInfo, collisionMask);
                if (hit)
                {
                    if ((hitInfo.fixture.CollisionCategories & Category.Cat4) == Category.Cat4)
                    {
                        if (directionY == -1 || hitInfo.distance == 0)
                            continue;
                    }
                    if (collisions.fallThroughPlatform)
                        continue;

                    if ((hitInfo.fixture.CollisionCategories & Category.Cat5) == Category.Cat5)
                    {
                        if (fallThrough)
                        {
                            collisions.fallThroughPlatform = true;
                            Task.Delay(TimeSpan.FromSeconds(fallThroughTimeout)).ContinueWith(t => ResetFallThrough());
                            continue;
                        }
                    }

                    moveAmount.Y = (hitInfo.distance - skinWidth) * directionY;
                    rayLength = hitInfo.distance;
                    if (collisions.climbingSlope)
                    {
                        moveAmount.X = (float)(-(moveAmount.Y / Math.Tan(MathHelper.ToRadians(collisions.slopeAngle))) * Math.Sign(moveAmount.X));
                    }
                    collisions.below = directionY == 1;
                    collisions.above = directionY == -1;
                }
                else
                    collisions.edgeCase = true;
            }

            if (collisions.climbingSlope)
            {
                var directionX = Math.Sign(moveAmount.X);
                rayLength = Math.Abs(moveAmount.X) + skinWidth;
                var rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + UpVector * moveAmount.Y;
                var hit = caster.RayCast(rayOrigin, RightVector * directionX, rayLength, out var hitInfo, collisionMask);
                if (hit)
                {
                    var slopeAngle = MathHelper.ToDegrees((float)Math.Acos(Vector2.Dot(UpVector, hitInfo.normal)));
                    if (slopeAngle != collisions.slopeAngle)
                    {

                        moveAmount.X = (hitInfo.distance - skinWidth) * directionX;
                        collisions.slopeAngle = slopeAngle;
                        collisions.slopeNormal = hitInfo.normal;
                    }
                }
            }
        }

        public void ClimbSlope(ref Vector2 moveAmount, float slopeAngle,Vector2 slopeNormal)
        {
            float distance = Math.Abs(moveAmount.X);

            float climbVelocityY = -(float)Math.Sin(MathHelper.ToRadians(slopeAngle)) * distance;
            if (moveAmount.Y > climbVelocityY)
            { 
                moveAmount.Y = climbVelocityY;

                moveAmount.X = (float)Math.Cos(MathHelper.ToRadians(slopeAngle)) * distance * Math.Sign(moveAmount.X);
                collisions.below = true;
                collisions.climbingSlope = true;
                collisions.slopeAngle = slopeAngle;
                collisions.slopeNormal = slopeNormal;
            }
        }

        public void DescendSlope(ref Vector2 moveAmount)
        {
            var downSlopeHitLeft = caster.RayCast(raycastOrigins.bottomLeft, -UpVector, Math.Abs(moveAmount.Y) + skinWidth, out var hitInfoLeft, collisionMask);
            var downSlopeHitRight = caster.RayCast(raycastOrigins.bottomRight, -UpVector, Math.Abs(moveAmount.Y) + skinWidth, out var hitInfoRight, collisionMask);
            if (downSlopeHitLeft ^ downSlopeHitRight)
            {
                SlideDownMaxSlope(hitInfoLeft, ref moveAmount);
                SlideDownMaxSlope(hitInfoRight, ref moveAmount);
            }

            if (!collisions.slidingDown)
            {


                float directionX = Math.Sign(moveAmount.X);
                var rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;

                var hit = caster.RayCast(rayOrigin, -UpVector, float.MaxValue, out var hitInfo, collisionMask);
                if (hit)
                {
                    var slopeAngle = MathHelper.ToDegrees((float)Math.Acos(Vector2.Dot(hitInfo.normal, UpVector)));
                    if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                    {
                        if (Math.Sign(hitInfo.normal.X) == directionX)
                        {
                            if (hitInfo.distance - skinWidth <= Math.Tan(MathHelper.ToRadians(slopeAngle)) * Math.Abs(moveAmount.X))
                            {
                                float moveDistance = Math.Abs(moveAmount.X);
                                float descendVelocityY = (float)Math.Sin(MathHelper.ToRadians(slopeAngle)) * moveDistance;
                                moveAmount.X = (float)Math.Cos(MathHelper.ToRadians(slopeAngle)) * moveDistance * Math.Sign(moveAmount.X);
                                moveAmount.Y += descendVelocityY;

                                collisions.slopeAngle = slopeAngle;
                                collisions.descendingSlope = true;
                                collisions.below = true;
                                collisions.slopeNormal = hitInfo.normal;
                            }
                        }
                    }
                }
            }
        }

        void SlideDownMaxSlope(RayCastHit hitInfo, ref Vector2 moveAmount)
        {
            var slopeAngle = MathHelper.ToDegrees((float)Math.Acos(Vector2.Dot(hitInfo.normal, UpVector)));
            if (slopeAngle < 90 && slopeAngle > maxSlopeAngle)
            {
                moveAmount.X = (float)(Math.Sign(hitInfo.normal.X) * (Math.Abs(moveAmount.Y) - hitInfo.distance) / Math.Tan(MathHelper.ToRadians(slopeAngle)));

                collisions.slopeAngle = slopeAngle;
                collisions.slidingDown = true;
                collisions.slopeNormal = hitInfo.normal;
            }
        }

        void ResetFallThrough()
        {
            collisions.fallThroughPlatform = false;
        }

        public struct CollisionInfo
        {
            public bool above, below;
            public bool left, right;

            public bool climbingSlope;
            public bool descendingSlope;
            public bool slidingDown;
            public float slopeAngle,slopeAngleOld;
            public Vector2 slopeNormal;

            public Vector2 moveAmountOld;
            public int faceDirection;

            public bool edgeCase;

            public bool fallThroughPlatform;
            public void Reset()
            {
                above = below = false;
                left = right = false;
                climbingSlope = false;
                descendingSlope = false;
                slidingDown = false;
                slopeNormal = Vector2.Zero;
                slopeAngleOld = slopeAngle;
                slopeAngle = 0;
                edgeCase = false;
            }
        }

       

    }

   



    public class RayCaster
    {
        readonly World world;

        public RayCaster(World world)
        {
            this.world = world;
        }
        public RayCastHit[] RayCastAll(Vector2 origin, Vector2 direction, float maxDistance, string colMask)
        {

            var hitList = new List<RayCastHit>();
            world.RayCast((f, p, n, fr) =>
            {
                hitList.Add(new RayCastHit { distance = fr * maxDistance, hitPoint = p, normal = n, fixture = f });
                return 1;
            }, origin, origin + direction * maxDistance);

            return hitList.ToArray();
        }

        public bool RayCast(Vector2 origin, Vector2 direction, float maxDistance, out RayCastHit hit, Category collisionMask)
        {
            var smallest = 1.0f;
            //direction.Normalize();
            var found = false;
            var localHit = default(RayCastHit);
            world.RayCast((f, p, n, fr) =>
            {
                if ((f.CollisionCategories & collisionMask) == f.CollisionCategories)
                {
                    found = true;
                    if (fr < smallest)
                    {
                        smallest = fr;
                        
                    }
                    localHit = new RayCastHit() { distance = fr * maxDistance, hitPoint = p, normal = n, fixture = f };
                    return fr;
                }
                return -1;
            }, origin, origin + direction * maxDistance);

            hit = localHit;
            return found;
        }
    }

    public struct RayCastHit
    {
        public Fixture fixture;
        public float distance;
        public Vector2 normal;
        public Vector2 hitPoint;
    }
}