//#define SIMPLE
//#define COMPLEX
//#define ASS
#define sjaak

using AuxLib;
using AuxLib.Debug;
using AuxLib.Input;
using LibTester.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Particles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using Newtonsoft.Json.Schema;
using MonoGame.Extended.NuclexGui;

namespace LibTester.Controllers
{
    
    public abstract class Car : GameComponent
    {
        public Transform2 Transform { get; set; }

        public Body rb;

        protected IInputHandler Input;
        protected DebugMonitor debugger;


        protected readonly Vector2 leftRearWheelPosition = new Vector2(-.2f, .2f);
        protected readonly Vector2 rightRearWheelPosition = new Vector2(.2f, .2f);
        protected readonly Vector2 leftFrontWheelPosition = new Vector2(-.2f, -0.2f);
        protected readonly Vector2 rightFrontWheelPosition = new Vector2(.2f, -0.2f);

        public Car(Game game) : base(game)
        {
            Input = (IInputHandler)Game.Services.GetService(typeof(IInputHandler));
            debugger = game.Services.GetService<DebugMonitor>();
        }

        public abstract void DrawDebug(SpriteBatcher sb);
    }

    #region Model1
    public class Car1 : Car
    {


        private readonly Size2 carSize = new Size2(0.4f, 0.7f);
        private readonly Size2 tireSize = new Size2(0.1f, 0.15f);

        public float MAX_STEER_ANGLE = MathF.PI / 4;
        public float STEER_SPEED = 1.0f;
        public float SIDEWAYS_FRICTION_FORCE = 10;
        public float HORSEPOWERS = 120;
        public float DRIFT = 0.2f;


        float engineSpeed = 0f;
        float steeringAngle = 0f;
        bool EBrake = false;

        float speed = 0.0f;



        public Body leftFrontWheel;
        public Body rightFrontWheel;
        public Body leftRearWheel;
        public Body rightRearWheel;

        private RevoluteJoint leftJoint;
        private RevoluteJoint rightJoint;

        private Sprite sprite;
        private Sprite tireSprite;
        private SpriteBatcher sb;
        private Texture2D tireTex;

        private void GenerateTireTex()
        {

            sb = Game.Services.GetService<SpriteBatcher>();
            
            var text = Game.Content.Load<Texture2D>("Players/BlackCar");
            sprite = new Sprite(text);

            tireTex = new Texture2D(Game.GraphicsDevice, 10, 15);
            Color[] data = new Color[10 * 15];
            for (int pixel = 0; pixel<data.Length; pixel++)
            {
                //the function applies the color according to the specified pixel
                data[pixel] = Color.Black;
            }

            //set the color
            tireTex.SetData(data);
            tireSprite = new Sprite(tireTex);
        }


        public Car1(Game game, World world, Transform2 transform) : base(game)
        {
            Transform = transform;
            
            var simPosition = ConvertUnits.ToSimUnits(Transform.Position);
            var rotation = MathHelper.ToRadians(Transform.Rotation);
            //Transform = new Transform2(Position)
            //{
            //    //Rotation = MathHelper.ToRadians(0)
            //    Rotation = MathHelper.ToRadians(90)
            //    //Rotation = MathHelper.ToRadians(45)
            //    //Rotation = MathHelper.ToRadians(135)
            //    //Rotation = MathHelper.ToRadians(225)
            //};


            rb = world.CreateBody(simPosition, rotation, BodyType.Dynamic);
            rb.LinearDamping = .1f;
            rb.AngularDamping = .5f;

            rb.CreateRectangle(carSize.Width, carSize.Height, 1, Vector2.Zero);

            

            leftFrontWheel = world.CreateBody(simPosition + rb.GetWorldPoint(leftFrontWheelPosition), rotation, BodyType.Dynamic);
            leftFrontWheel.CreateRectangle(tireSize.Width, tireSize.Height, 1, Vector2.Zero);

            rightFrontWheel = world.CreateBody(simPosition + rb.GetWorldPoint(rightFrontWheelPosition), rotation, BodyType.Dynamic);
            rightFrontWheel.CreateRectangle(tireSize.Width, tireSize.Height, 1, Vector2.Zero);

            leftRearWheel = world.CreateBody(simPosition + rb.GetWorldPoint(leftRearWheelPosition), rotation, BodyType.Dynamic);
            leftRearWheel.CreateRectangle(tireSize.Width, tireSize.Height, 1, Vector2.Zero);

            rightRearWheel = world.CreateBody(simPosition + rb.GetWorldPoint(rightRearWheelPosition), rotation, BodyType.Dynamic);
            rightRearWheel.CreateRectangle(tireSize.Width, tireSize.Height, 1, Vector2.Zero);


            leftJoint = JointFactory.CreateRevoluteJoint(world, rb,leftFrontWheel, leftFrontWheelPosition,Vector2.Zero);
            leftJoint.MotorEnabled = true;
            leftJoint.MaxMotorTorque = 100;
            leftJoint.SetLimits(0, 0);

            rightJoint = JointFactory.CreateRevoluteJoint(world, rb, rightFrontWheel, rightFrontWheelPosition, Vector2.Zero);
            rightJoint.MotorEnabled = true;
            rightJoint.MaxMotorTorque = 100;
            rightJoint.SetLimits(0, 0);


            JointFactory.CreateWeldJoint(world, rb, leftRearWheel, leftRearWheelPosition, Vector2.Zero);
            JointFactory.CreateWeldJoint(world, rb, rightRearWheel, rightRearWheelPosition, Vector2.Zero);

            GenerateTireTex();
        }



        public void KillOrthogonalVelocity(Body targetBody)
        {
            var up = Vector2.Transform(new Vector2(0, -1), Matrix.CreateRotationZ(targetBody.Rotation));
            var right = Vector2.Transform(new Vector2(1, 0), Matrix.CreateRotationZ(targetBody.Rotation));

            Vector2 forwardVelocity = up * Vector2.Dot(targetBody.LinearVelocity, up);
            Vector2 rightVelocity = right * Vector2.Dot(targetBody.LinearVelocity, right);
            

            targetBody.LinearVelocity = forwardVelocity + rightVelocity * DRIFT;
        }

        public void handleInput(GameTime gameTime)
        {
            steeringAngle = Input.GamePads[0].ThumbSticks.Left.X * MAX_STEER_ANGLE;
            engineSpeed = 0f;
            if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.A, Microsoft.Xna.Framework.Input.Keys.Up))
                engineSpeed = 1.0f * HORSEPOWERS;
            if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.B, Microsoft.Xna.Framework.Input.Keys.Down))
                engineSpeed = -1.0f * HORSEPOWERS;
            EBrake = Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.RightTrigger, Microsoft.Xna.Framework.Input.Keys.Space);

        }

        public override void Update(GameTime gameTime)
        {

            handleInput(gameTime);

            KillOrthogonalVelocity(leftFrontWheel);
            KillOrthogonalVelocity(rightFrontWheel);
            KillOrthogonalVelocity(leftRearWheel);
            KillOrthogonalVelocity(rightRearWheel);

            //Driving
            var ldirection = Vector2.Transform(new Vector2(0, -1), Matrix.CreateRotationZ(leftFrontWheel.Rotation));
            ldirection *= (engineSpeed);

            var rdirection = Vector2.Transform(new Vector2(0, -1), Matrix.CreateRotationZ(rightFrontWheel.Rotation));
            rdirection *= (engineSpeed);

            speed = (ldirection + rdirection / 2).Length();

            leftFrontWheel.ApplyForce(ldirection * (float)gameTime.ElapsedGameTime.TotalSeconds, leftFrontWheel.Position);
            rightFrontWheel.ApplyForce(rdirection * (float)gameTime.ElapsedGameTime.TotalSeconds, rightFrontWheel.Position);

            if (EBrake)
            {
                rightRearWheel.LinearVelocity = Vector2.Zero;
                leftRearWheel.LinearVelocity = Vector2.Zero;
            }

            if (steeringAngle == 0)
            {
                leftJoint.LimitEnabled = true;
                rightJoint.LimitEnabled = true;
                
                
            }
            else
            {
                leftJoint.LimitEnabled = false;
                rightJoint.LimitEnabled = false;
            }
            //Steering
            var mspeed = 0.0f;
            mspeed = steeringAngle - leftJoint.JointAngle;
            leftJoint.MotorSpeed = (mspeed * STEER_SPEED);
            mspeed = steeringAngle - rightJoint.JointAngle;
            rightJoint.MotorSpeed = (mspeed * STEER_SPEED);

            doDebug();

            Transform.Position = ConvertUnits.ToDisplayUnits(rb.Position);
            Transform.Rotation = rb.Rotation;
        }

        void doDebug()
        {

            debugger.AddDebugValue("MAX_STEER_ANGLE", MAX_STEER_ANGLE.ToString());
            debugger.AddDebugValue("STEER_SPEED", STEER_SPEED.ToString());
            debugger.AddDebugValue("SIDEWAYS_FRICTION_FORCE", SIDEWAYS_FRICTION_FORCE.ToString());
            debugger.AddDebugValue("HORSEPOWERS", HORSEPOWERS.ToString());
            debugger.AddDebugValue("DRIFT", DRIFT);
            debugger.AddDebugValue("engineSpeed", engineSpeed.ToString());
            debugger.AddDebugValue("steeringAngle", steeringAngle.ToString());

            debugger.AddDebugValue("left angle", leftJoint.JointAngle.ToString());
            debugger.AddDebugValue("right angle ", rightJoint.JointAngle.ToString());

            debugger.AddDebugValue("Body Rotation", rb.Rotation);
            debugger.AddDebugValue("Body Speed", rb.LinearVelocity);
            

        }

        public override void DrawDebug(SpriteBatcher sb)
        {
            sb.Draw(tireSprite, ConvertUnits.ToDisplayUnits(leftFrontWheel.Position), leftFrontWheel.Rotation);
            sb.Draw(tireSprite, ConvertUnits.ToDisplayUnits(rightFrontWheel.Position), rightFrontWheel.Rotation);
            sb.Draw(tireSprite, ConvertUnits.ToDisplayUnits(leftRearWheel.Position), leftRearWheel.Rotation);
            sb.Draw(tireSprite, ConvertUnits.ToDisplayUnits(rightRearWheel.Position), rightRearWheel.Rotation);
            sb.Draw(sprite, Transform.Position, Transform.Rotation);
        }
    }
    #endregion

    #region Model2
    public class PlayerCar : Car
    {

        private readonly Size2 carSize = new Size2(0.4f, 0.7f);
        private readonly Size2 tireSize = new Size2(0.1f, 0.15f);

        float SteerSpeed = 5.5f;
        float SteerAdjustSpeed = 1f;
        float SpeedSteerCorrection = 300f;
        float WeightTransfer = 0.35f;
        float CGHeight = 0.55f;

        float Mass = 1200f;
        float LinearDamping = 0.5f;
        float AngularDamping = 0.5f;

        float BrakePower = 12000;
        float EBrakePower = 5000;

        float MaxSteerAngle = 0.85f;
        float InertiaScale = 1f;

        float EBrakeGripRatioFront = 0.9f;
        float TotalTireGripFront = 2.5f;

        float EBrakeGripRatioRear = 0.4f;
        float TotalTireGripRear = 2.5f;

        float CornerStiffnessFront = 5.0f;
        float CornerStiffnessRear = 5.2f;

        float AirResistance = 2.5f;
        float RollingResistance = 8.0f;

        float SpeedTurningStability = 1f;
        float AxleDistanceCorrection = 2f;

        float Inertia = 1;
        float WheelBase = 1.0f;
        float TrackWidth = 1.0f;

        float DistanceToGc = 0.5f;
        float Gravity = 10f;

        float HeadingAngle;
        public float AbsoluteVelocity;
        public float SteerDirection;
        public float SteerAngle;
        float AngularVelocity;

        Vector2 Velocity;
        Vector2 Acceleration;
        public Vector2 LocalVelocity;
        Vector2 LocalAcceleration;
        public Vector2 Up;

        public Axis FrontAxis;
        public Axis BackAxis;

        private float f_slipangle;
        private float b_slipangle;

        public Engine engine;

        public Transform2 CenterOfGravity;

        public Vector2 LocalVelocity2;

        float Throttle;
        float Brake;
        float EBrake;

        int i = 0;

        private Sprite sprite;
        private Sprite tireSprite;

        private bool SkidsActive = false;
        private readonly SkidMarkEffect skidsParticleEffect;

        public PlayerCar(Game game, World world, Transform2 transform) : base(game)
        {
            Transform = transform;

            var simUnits = ConvertUnits.ToSimUnits(Transform.Position);
            rb = world.CreateRoundedRectangle(carSize.Width,carSize.Height,0.2f,0.2f,4,1, simUnits, MathHelper.ToRadians(Transform.Rotation),BodyType.Dynamic);
            rb.Mass = Mass;
            rb.LinearDamping = LinearDamping;
            rb.AngularDamping = AngularDamping;
            
            CenterOfGravity = new Transform2(Vector2.Zero);

            FrontAxis = new Axis(rb, WheelBase, .5f, leftFrontWheelPosition, rightFrontWheelPosition,Gravity);
            BackAxis = new Axis(rb, WheelBase, .5f, leftRearWheelPosition, rightRearWheelPosition,Gravity);
            TrackWidth = Vector2.Distance(leftFrontWheelPosition, rightFrontWheelPosition);

            engine = new Engine();

            Velocity = Vector2.Zero;
            AbsoluteVelocity = 0;

            FrontAxis.DistanceToCG *= AxleDistanceCorrection;
            BackAxis.DistanceToCG *= AxleDistanceCorrection;

            Inertia = rb.Mass * InertiaScale;

            var text = Game.Content.Load<Texture2D>("Players/BlackCar");
            sprite = new Sprite(text);

            tireSprite = GenerateTireTex();


            skidsParticleEffect = new SkidMarkEffect(Game.Content);

        }

        private Sprite GenerateTireTex()
        {
            var s = new Vector2(ConvertUnits.ToDisplayUnits(carSize.Width * tireSize.Width), ConvertUnits.ToDisplayUnits(carSize.Height * tireSize.Height));

            var tireTex = new Texture2D(Game.GraphicsDevice, (int)s.X, (int)s.Y);
            Color[] data = new Color[(int)s.X * (int)s.Y];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                //the function applies the color according to the specified pixel
                data[pixel] = Color.Black;
            }

            //set the color
            tireTex.SetData(data);
            return new Sprite(tireTex);
        }

        public override void Update(GameTime gameTime)
        {

            Throttle = 0.0f;
            Brake = 0.0f;
            EBrake = 0f;
            var steerInput = Input.GamePads[0].ThumbSticks.Left.X;

            if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.A, Microsoft.Xna.Framework.Input.Keys.Up))
                Throttle = 1.0f;
            if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.B, Microsoft.Xna.Framework.Input.Keys.Down))
            { 
                //Brake = 1f;
                Throttle = -1.0f;
            }
            if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.RightTrigger, Microsoft.Xna.Framework.Input.Keys.Space))
                EBrake = 1;

            // Apply filters to our steer direction
            SteerDirection = SmoothSteering(steerInput, gameTime);
            SteerDirection = SpeedAdjustedSteering(SteerDirection);

            // Calculate the current angle the tires are pointing
            SteerAngle = SteerDirection * MaxSteerAngle;


            //FrontLeftTire.transform.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, SteerAngle).Z;
            //FrontRightTire.transform.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, SteerAngle).Z;

            FrontAxis.TireRight.transform.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, SteerAngle).Z;
            FrontAxis.TireLeft.transform.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, SteerAngle).Z;

            Vector2 pos = Vector2.Zero;
            if (LocalAcceleration.Length() > 1f)
            {

                float wfl = Math.Max(0, (FrontAxis.TireLeft.ActiveWeight - FrontAxis.TireLeft.RestingWeight));
                float wfr = Math.Max(0, (FrontAxis.TireRight.ActiveWeight - FrontAxis.TireRight.RestingWeight));
                float wrl = Math.Max(0, (BackAxis.TireLeft.ActiveWeight - BackAxis.TireLeft.RestingWeight));
                float wrr = Math.Max(0, (BackAxis.TireRight.ActiveWeight - BackAxis.TireRight.RestingWeight));

                pos = (FrontAxis.TireLeft.transform.Position) * wfl +
                    (FrontAxis.TireRight.transform.Position) * wfr +
                    (BackAxis.TireLeft.transform.Position) * wrl +
                    (BackAxis.TireRight.transform.Position) * wrr;

                float weightTotal = wfl + wfr + wrl + wrr;

                if (weightTotal > 0)
                {
                    pos /= weightTotal;
                    pos.Normalize();
                    pos.X = Math.Clamp(pos.X, -0.2f, 0.2f);
                }
                else
                {
                    pos = Vector2.Zero;
                }
            }

            // Update the "Center Of Gravity" dot to indicate the weight shift            
            CenterOfGravity.Position = Vector2.Lerp(CenterOfGravity.Position, pos, 0.1f);


            SkidsActive = false;

            // Skidmarks
            if (Math.Abs(LocalAcceleration.Y) > 18 || EBrake == 1)
            {
                SkidsActive = true;
                skidsParticleEffect.Trigger(ConvertUnits.ToDisplayUnits(rb.GetWorldPoint(leftRearWheelPosition)));
                skidsParticleEffect.Trigger(ConvertUnits.ToDisplayUnits(rb.GetWorldPoint(rightRearWheelPosition)));
            }

            // Automatic transmission
            engine.UpdateAutomaticTransmission(rb);

            if (true && i % 10 == 0)
            {
                doDebug();
                i = 0;
            }
            i++;


            skidsParticleEffect.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            FixedUpdate(gameTime);
        }

       
        void FixedUpdate(GameTime gameTime)
        {

            HeadingAngle = rb.Rotation;
            Up = Vector2.Transform(new Vector2(0, -1), Matrix.CreateRotationZ(rb.Rotation));

            Velocity = rb.LinearVelocity;
            var rotationMatrix = Matrix.CreateRotationZ(HeadingAngle - MathHelper.ToRadians(90));
            // Get local velocity
            LocalVelocity = Vector2.Transform(Velocity,Matrix.Invert(rotationMatrix));

            // Weight transfer
            float transferX = WeightTransfer * LocalAcceleration.X * CGHeight / WheelBase;
            float transferY = WeightTransfer * LocalAcceleration.Y * CGHeight / TrackWidth * 2;        //exagerate the weight transfer on the y-axis
            
            // Weight on each axle
            float weightFront = rb.Mass * (DistanceToGc * Gravity - transferX);
            float weightRear = rb.Mass * (DistanceToGc * Gravity + transferX);

            // Weight on each tire
            FrontAxis.TireLeft.ActiveWeight = weightFront - transferY;
            FrontAxis.TireRight.ActiveWeight = weightFront + transferY;
            BackAxis.TireLeft.ActiveWeight = weightRear - transferY;
            BackAxis.TireRight.ActiveWeight = weightRear + transferY;

            // Velocity of each tire
            FrontAxis.TireLeft.AngularVelocity = DistanceToGc * AngularVelocity;
            FrontAxis.TireRight.AngularVelocity = DistanceToGc * AngularVelocity;
            BackAxis.TireLeft.AngularVelocity = -DistanceToGc * AngularVelocity;
            BackAxis.TireRight.AngularVelocity = -DistanceToGc * AngularVelocity;

            // Math Sign UNITY / C# ????
            f_slipangle = MathF.Atan2(LocalVelocity.Y + FrontAxis.AngularVelocity, Math.Abs(LocalVelocity.X)) - Math.Sign(LocalVelocity.X) * SteerAngle;
            b_slipangle = MathF.Atan2(LocalVelocity.Y + BackAxis.AngularVelocity, Math.Abs(LocalVelocity.X));


            // Brake and Throttle power
            float activeBrake = Math.Min(Brake * BrakePower + EBrake * EBrakePower, BrakePower);
            float activeThrottle = (Throttle * engine.GetTorque(rb)) * (engine.GearRatio * engine.EffectiveGearRatio);

            // Torque of each tire (rear wheel drive)
            BackAxis.TireLeft.Torque = activeThrottle / BackAxis.TireLeft.Radius;
            BackAxis.TireRight.Torque = activeThrottle / BackAxis.TireRight.Radius;

            // Grip and Friction of each tire
            FrontAxis.TireLeft.Grip = TotalTireGripFront * (1.0f - EBrake * (1.0f - EBrakeGripRatioFront));
            FrontAxis.TireRight.Grip = TotalTireGripFront * (1.0f - EBrake * (1.0f - EBrakeGripRatioFront));
            BackAxis.TireLeft.Grip = TotalTireGripRear * (1.0f - EBrake * (1.0f - EBrakeGripRatioRear));
            BackAxis.TireRight.Grip = TotalTireGripRear * (1.0f - EBrake * (1.0f - EBrakeGripRatioRear));

            FrontAxis.TireLeft.FrictionForce = MathHelper.Clamp(-CornerStiffnessFront * f_slipangle, -FrontAxis.TireLeft.Grip, FrontAxis.TireLeft.Grip) * FrontAxis.TireLeft.ActiveWeight;
            FrontAxis.TireRight.FrictionForce = MathHelper.Clamp(-CornerStiffnessFront * f_slipangle, -FrontAxis.TireRight.Grip, FrontAxis.TireRight.Grip) * FrontAxis.TireRight.ActiveWeight;
            BackAxis.TireLeft.FrictionForce = MathHelper.Clamp(-CornerStiffnessRear * b_slipangle, -BackAxis.TireLeft.Grip, BackAxis.TireLeft.Grip) * BackAxis.TireLeft.ActiveWeight;
            BackAxis.TireRight.FrictionForce = MathHelper.Clamp(-CornerStiffnessRear * b_slipangle, -BackAxis.TireRight.Grip, BackAxis.TireRight.Grip) * BackAxis.TireRight.ActiveWeight;
         

            // Forces
            float tractionForceX = BackAxis.Torque - activeBrake * Math.Sign(LocalVelocity.X);
            float tractionForceY = 0;

            float dragForceX = -RollingResistance * LocalVelocity.X - AirResistance * LocalVelocity.X * Math.Abs(LocalVelocity.X);
            float dragForceY = -RollingResistance * LocalVelocity.Y - AirResistance * LocalVelocity.Y * Math.Abs(LocalVelocity.Y);


            float totalForceX = dragForceX + tractionForceX;
            float totalForceY = dragForceY + tractionForceY + MathF.Cos(SteerAngle) * FrontAxis.FrictionForce + BackAxis.FrictionForce;

            //adjust Y force so it levels out the car heading at high speeds
            if (AbsoluteVelocity > 10)
            {
                totalForceY *= (AbsoluteVelocity + 1) / (21f - SpeedTurningStability);
            }

            // If we are not pressing gas, add artificial drag - helps with simulation stability
            if (Throttle == 0)
            {
                Velocity = Vector2.Lerp(Velocity, Vector2.Zero, 0.005f);
            }

            // Acceleration
            LocalAcceleration.X = totalForceX / rb.Mass;
            LocalAcceleration.Y = totalForceY / rb.Mass;

            Acceleration = Vector2.Transform(LocalAcceleration, rotationMatrix);
            // Velocity and speed            
            Velocity += Acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;

            AbsoluteVelocity = Velocity.Length();

            // Angular torque of car
            float angularTorque = (FrontAxis.FrictionForce * DistanceToGc) - (BackAxis.FrictionForce * DistanceToGc);

            // Car will drift away at low speeds
            if (AbsoluteVelocity < 0.5f && activeThrottle == 0)
            {
                LocalAcceleration = Vector2.Zero;
                AbsoluteVelocity = 0;
                Velocity = Vector2.Zero;
                angularTorque = 0;
                AngularVelocity = 0;
                Acceleration = Vector2.Zero;
                rb.AngularVelocity = 0;
            }

            var angularAcceleration = angularTorque / Inertia;

            // Update 
            AngularVelocity += angularAcceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Simulation likes to calculate high angular velocity at very low speeds - adjust for this
            if (AbsoluteVelocity < 1 && Math.Abs(SteerAngle) < 0.05f)
            {
                AngularVelocity = 0;
            }
            else if (SpeedKilometersPerHour < 0.75f)
            {
                AngularVelocity = 0;
            }


            if (SteerAngle != 0)
                HeadingAngle += AngularVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;           

            rb.LinearVelocity = Velocity;
            rb.Rotation = HeadingAngle;

            Transform.Position = ConvertUnits.ToDisplayUnits(rb.Position);
            Transform.Rotation = rb.Rotation;

        }

        public float SpeedKilometersPerHour
        {
            get
            {
                return rb.LinearVelocity.Length() * 18f / 5f;
            }
        }

        float SmoothSteering(float steerInput, GameTime gameTime)
        {           

            float steer = 0;

            if (Math.Abs(steerInput) > 0.001f)
            {
                steer = Math.Clamp(SteerDirection + steerInput * (float)gameTime.ElapsedGameTime.TotalSeconds * SteerSpeed, -1.0f, 1.0f);
            }
            else
            {
                if (SteerDirection > 0)
                {
                    steer = Math.Max(SteerDirection - (float)gameTime.ElapsedGameTime.TotalSeconds * SteerAdjustSpeed, 0);
                }
                else if (SteerDirection < 0)
                {
                    steer = Math.Min(SteerDirection + (float)gameTime.ElapsedGameTime.TotalSeconds * SteerAdjustSpeed, 0);
                }
            }

            return steer;
        }

        float SpeedAdjustedSteering(float steerInput)
        {
            float activeVelocity = Math.Min(AbsoluteVelocity, 10.0f);
            float steer = steerInput * (1.0f - (activeVelocity / SpeedSteerCorrection));
            return steer;
        }

        void doDebug()
        {
            debugger.AddDebugValue("Speed", SpeedKilometersPerHour.ToString());
            debugger.AddDebugValue("RPM", engine.GetRPM(rb).ToString());
            debugger.AddDebugValue("Gear", (engine.CurrentGear + 1).ToString());
            debugger.AddDebugValue("LocalAcceleration", LocalAcceleration.ToString());
            debugger.AddDebugValue("Acceleration", Acceleration.ToString());
            debugger.AddDebugValue("LocalVelocity", LocalVelocity.ToString());
            debugger.AddDebugValue("LocalVelocity2", LocalVelocity2.ToString());

            

            debugger.AddDebugValue("Velocity", $"X {Velocity.X:0.0#} Y : {Velocity.Y:0.0#}");
            debugger.AddDebugValue("SteerAngle", SteerAngle.ToString());
            debugger.AddDebugValue("Throttle", Throttle.ToString());
            debugger.AddDebugValue("EBrake", EBrake.ToString());
            debugger.AddDebugValue("HeadingAngle", HeadingAngle.ToString());
            debugger.AddDebugValue("AngularVelocity", AngularVelocity.ToString());
            debugger.AddDebugValue("TrackWidth", TrackWidth.ToString());
            debugger.AddDebugValue("WheelBase", WheelBase.ToString());

            debugger.AddDebugValue("TireFL Weight", FrontAxis.TireLeft.ActiveWeight.ToString());
            debugger.AddDebugValue("TireFR Weight", FrontAxis.TireRight.ActiveWeight.ToString());
            debugger.AddDebugValue("TireRL Weight", BackAxis.TireLeft.ActiveWeight.ToString());
            debugger.AddDebugValue("TireRR Weight", BackAxis.TireRight.ActiveWeight.ToString());

            debugger.AddDebugValue("TireFL Friction", FrontAxis.TireLeft.FrictionForce.ToString());
            debugger.AddDebugValue("TireFR Friction", FrontAxis.TireRight.FrictionForce.ToString());
            debugger.AddDebugValue("TireRL Friction", BackAxis.TireLeft.FrictionForce.ToString());
            debugger.AddDebugValue("TireRR Friction", BackAxis.TireRight.FrictionForce.ToString());

            debugger.AddDebugValue("TireFL Grip", FrontAxis.TireLeft.Grip.ToString());
            debugger.AddDebugValue("TireFR Grip", FrontAxis.TireRight.Grip.ToString());
            debugger.AddDebugValue("TireRL Grip", BackAxis.TireLeft.Grip.ToString());
            debugger.AddDebugValue("TireRR Grip", BackAxis.TireRight.Grip.ToString());
            
            debugger.AddDebugValue("AxleF SlipAngle", f_slipangle.ToString());
            debugger.AddDebugValue("AxleR SlipAngle", b_slipangle.ToString());

            debugger.AddDebugValue("CG", CenterOfGravity.ToString());
            debugger.AddDebugValue("Skids", SkidsActive.ToString());

            debugger.AddDebugValue("Body Rotation", rb.Rotation);
            debugger.AddDebugValue("Body Speed", rb.LinearVelocity);
            debugger.AddDebugValue("Angular Speed", rb.AngularVelocity);
            debugger.AddDebugValue("Heading angle", HeadingAngle);

        }

        public void Draw(SpriteBatcher sb)
        {
            sb.Draw(skidsParticleEffect);

            sb.Draw(tireSprite, ConvertUnits.ToDisplayUnits(rb.GetWorldPoint(FrontAxis.TireLeft.transform.Position)), rb.Rotation + FrontAxis.TireLeft.transform.Rotation);
            sb.Draw(tireSprite, ConvertUnits.ToDisplayUnits(rb.GetWorldPoint(FrontAxis.TireRight.transform.Position)), rb.Rotation + FrontAxis.TireRight.transform.Rotation);
            sb.Draw(tireSprite, ConvertUnits.ToDisplayUnits(rb.GetWorldPoint(BackAxis.TireLeft.transform.Position)), rb.Rotation);
            sb.Draw(tireSprite, ConvertUnits.ToDisplayUnits(rb.GetWorldPoint(BackAxis.TireRight.transform.Position)), rb.Rotation);
            sb.Draw(sprite, Transform.Position, Transform.Rotation);

            if (true)
                DrawDebug(sb);
        }

        public override void DrawDebug(SpriteBatcher sb)
        {
            sb.DrawPoint(ConvertUnits.ToDisplayUnits(rb.GetWorldPoint((CenterOfGravity.Position))), Color.Red, 3);
            sb.DrawLine(Transform.Position, Transform.Position + rb.LinearVelocity * 20, Color.Green, 2);
            sb.DrawLine(Transform.Position, Transform.Position + LocalVelocity * 20, Color.Blue, 2);
            sb.DrawLine(Transform.Position, Transform.Position + Up * 20, Color.Pink, 2);
        }

        #endregion
    }

}

