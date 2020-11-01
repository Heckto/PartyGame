using AuxLib.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Numerics;
using System.Text;
using tainicom.Aether.Physics2D.Dynamics;

namespace LibTester.Controllers
{
    public class CarController : GameComponent
    {
        public float maxSpeed = 10;
        public float braking = 0.5f;
        public float acceleration= 0.5f;
        public float steering = 0.1f;
        public float drag = 0.05f;
        private Body rb;
        private IInputHandler Input;

        public CarController(Game game, Body body) : base(game)
        {
            rb = body;
            Input = (IInputHandler)game.Services.GetService(typeof(IInputHandler));
        }

        //public override void Update(GameTime gameTime)
        //{

        //    var engine_power = 200;
        //    var friction = -0.9f;
        //    var drag = -0.001f;
        //    var braking = -450;
        //    var max_speed_reverse = 250;
        //    var slip_speed = 400;
        //    var traction_fast = 0.1;
        //    var traction_slow = 0.7;


        //    var wheel_base = 0.02f;  //Distance from front to rear wheel
        //    var steering_angle = 15;  //Amount that front wheel turns, in degrees
        //    var acc = Vector2.Zero;
        //    var velocity = Vector2.Zero;

        //    var turn = -Input.GamePads[0].ThumbSticks.Left.X;

        //    var transformed = Vector2.Transform(new Vector2(0, -1), Matrix.CreateRotationZ(rb.Rotation));



        //    var steer_direction = turn * MathHelper.ToRadians(steering_angle);

        //    if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.A, Microsoft.Xna.Framework.Input.Keys.Up))
        //        acc = transformed * engine_power;
        //    if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.B, Microsoft.Xna.Framework.Input.Keys.Down))
        //        acc = transformed * braking;


        //    if (velocity.Length() < 5)
        //        velocity = Vector2.Zero;
        //    var friction_force = velocity * friction;
        //    var drag_force = velocity * velocity.Length() * drag;
        //    acc += drag_force + friction_force;




        //    //var rear_wheel = rb.Position - transformed * wheel_base / 2.0f;
        //    //var front_wheel = rb.Position + transformed * wheel_base / 2.0f;
        //    //rear_wheel += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

        //    //front_wheel += Vector2.Transform(velocity, Matrix.CreateRotationZ(steer_angle)) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        //    //var new_heading = Vector2.Normalize(rear_wheel- front_wheel);

        //    //rb.LinearVelocity = new_heading * velocity.Length();
        //    //rb.Rotation = (float)Math.Atan2(front_wheel.Y - rear_wheel.Y, front_wheel.X - rear_wheel.X);



        //    velocity += acc * (float)gameTime.ElapsedGameTime.TotalSeconds;

        //    rb.LinearVelocity = velocity;


        //    //velocity = move_and_slide(velocity)
        //}


        public override void Update(GameTime gameTime)
        {


            var h = -Input.GamePads[0].ThumbSticks.Left.X;
            var v = 0.0f;
            if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.A, Microsoft.Xna.Framework.Input.Keys.Up))
                v = 1.0f;
            if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.B, Microsoft.Xna.Framework.Input.Keys.Down))
                v = -1.0f;

            var rot = rb.Rotation - (h * steering);
            rb.Rotation = rot;
            //transform.localEulerAngles = new Vector3(0.0f, 0.0f, rot);
            // acceleration/braking

            var transformed = Vector2.Transform(new Vector2(0, -1), Matrix.CreateRotationZ(rb.Rotation));

            float velocity = rb.LinearVelocity.Length();
            if (v > 0.01f)
            {
                velocity += v * acceleration;

            }
            else if (v < -0.01f)
            {
                velocity += v * braking;
            }

            if (velocity > 0)
                velocity -= drag;
            // apply car movement

            velocity = MathHelper.Clamp(velocity, -maxSpeed, maxSpeed);

            rb.LinearVelocity = (transformed * velocity);
            rb.AngularVelocity = 0.0f;
        }


        //public override void Update(GameTime gameTime)
        //{

        //    float h = -Input.GamePads[0].ThumbSticks.Left.X;
        //    float v = 0.0f;
        //    if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.A, Microsoft.Xna.Framework.Input.Keys.Up))
        //        v = 1.0f;
        //    if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.B, Microsoft.Xna.Framework.Input.Keys.Down))
        //        v = -1.0f;
        //    Debug.WriteLine(v);



        //    Vector2 speed = transformed * (v * acceleration);
        //    rb.ApplyForce(speed);

        //    float direction = Vector2.Dot(rb.LinearVelocity, transformed);


        //    if (direction > 0.0f)
        //    {          

        //        //rb.Rotation += h * steering * (rb.LinearVelocity.Length() / 5.0f);
        //        rb.ApplyTorque((-h * steering) * (rb.LinearVelocity.Length() / 10.0f));

        //    }
        //    else
        //    {
        //        //rb.Rotation -= h * steering * (rb.LinearVelocity.Length() / 5.0f);
        //        rb.ApplyTorque((h * steering) * (rb.LinearVelocity.Length() / 10.0f));
        //    }


        //    Vector2 forward = new Vector2(0.0f, 0.5f);
        //    float steeringRightAngle;
        //    if (rb.AngularVelocity > 0)
        //    {
        //        steeringRightAngle = -90;
        //    }
        //    else
        //    {
        //        steeringRightAngle = 90;
        //    }

        //    var ass = Quaternion.CreateFromAxisAngle(Vector3.Forward, steeringRightAngle);
        //    Vector2 rightAngleFromForward = new Vector2(ass.X, ass.Y) * forward;
        //    //Debug.DrawLine((Vector2)rb.Position, (Vector2)rb.GetWorldPoint(rightAngleFromForward), Color.Green);


        //    float driftForce = Vector2.Dot(rb.LinearVelocity, rb.GetWorldVector(Vector2.Normalize(rightAngleFromForward)));

        //    Vector2 relativeForce = (Vector2.Normalize(rightAngleFromForward) * -1.0f) * (driftForce * 10.0f);


        //    //Debug.DrawLine((Vector2)rb.Position, (Vector2)rb.GetWorldPoint(relativeForce), Color.Red);

        //    rb.ApplyForce(rb.GetWorldVector(relativeForce));
        //}
    }
}
