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

        //protected readonly Vector2 leftRearWheelPosition = new Vector2(-0.64f, 0.865f);
        //protected readonly Vector2 rightRearWheelPosition = new Vector2(0.64f, 0.865f);
        //protected readonly Vector2 leftFrontWheelPosition = new Vector2(-0.64f, -0.865f);
        //protected readonly Vector2 rightFrontWheelPosition = new Vector2(0.64f, -0.865f);

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
    public class Car2 : Car
    {
        

        private readonly Vector2 frontAxisPosition = new Vector2(0.0f, -0.2f);
        private readonly Vector2 rearAxisPosition = new Vector2(0.0f, -0.2f);

        private readonly Size2 carSize = new Size2(0.4f, 0.7f);
        private readonly Size2 tireSize = new Size2(0.1f, 0.15f);

        float SteerSpeed = 5.5f;
        float SteerAdjustSpeed = 1f;
        float SpeedSteerCorrection = 300f;
        float WeightTransfer = 0.35f;
        float CGHeight = 0.55f;
        
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

        public Tire FrontLeftTire;
        public Tire FrontRightTire;
        public Tire RearLeftTire;
        public Tire RearRightTire;

        private float f_slipangle;
        private float b_slipangle;

        private float f_friction;
        private float r_friction;

        public Engine engine;

        public Transform2 CenterOfGravity;

        public Vector2 LocalVelocity2;

        float Throttle;
        float Brake;
        float EBrake;

        int i = 0;

        private Sprite sprite;
        private bool Skids = false;

        private SkidMarkEffect skids;

        public Car2(Game game, World world, Transform2 transform) : base(game)
        {
            Transform = transform;

            var simUnits = ConvertUnits.ToSimUnits(Transform.Position);
            rb = world.CreateRoundedRectangle(carSize.Width,carSize.Height,0.2f,0.2f,4,1, simUnits, MathHelper.ToRadians(Transform.Rotation),BodyType.Dynamic);
            rb.Mass = 1200;
            rb.LinearDamping = .5f;
            rb.AngularDamping = 0.5f;
            
            CenterOfGravity = new Transform2(Vector2.Zero);

            FrontAxis = new Axis(rb, WheelBase, .5f, leftFrontWheelPosition, rightFrontWheelPosition);
            BackAxis = new Axis(rb, WheelBase, .5f, leftRearWheelPosition, rightRearWheelPosition);

            FrontLeftTire = new Tire(leftFrontWheelPosition);
            FrontRightTire = new Tire(rightFrontWheelPosition);
            RearLeftTire = new Tire(leftRearWheelPosition);
            RearRightTire = new Tire(rightRearWheelPosition);

            float weight = rb.Mass * (DistanceToGc * Gravity);
            FrontLeftTire.RestingWeight = weight;
            FrontRightTire.RestingWeight = weight;
            RearLeftTire.RestingWeight = weight;
            RearRightTire.RestingWeight = weight;

            //FrontAxis = new Axis(rb, WheelBase, frontAxisPosition, CenterOfGravity.Position);
            //BackAxis = new Axis(rb, WheelBase, rearAxisPosition, CenterOfGravity.Position);

            TrackWidth = Vector2.Distance(leftFrontWheelPosition, rightFrontWheelPosition);

            engine = new Engine();

            Velocity = Vector2.Zero;
            AbsoluteVelocity = 0;

            FrontAxis.DistanceToCG *= AxleDistanceCorrection;
            BackAxis.DistanceToCG *= AxleDistanceCorrection;

            //WheelBase = 0.4f;
            Inertia = rb.Mass * InertiaScale;

            var text = Game.Content.Load<Texture2D>("Players/BlackCar");
            sprite = new Sprite(text);

            skids = new SkidMarkEffect(Game.Content,Game.GraphicsDevice);
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

//            FrontAxis.TireRight.transform.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, SteerAngle).Z;
//            FrontAxis.TireLeft.transform.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, SteerAngle).Z;

            Vector2 pos = Vector2.Zero;
            if (LocalAcceleration.Length() > 1f)
            {

                float wfl = Math.Max(0, (FrontLeftTire.ActiveWeight - FrontLeftTire.RestingWeight));
                float wfr = Math.Max(0, (FrontRightTire.ActiveWeight - FrontRightTire.RestingWeight));
                float wrl = Math.Max(0, (RearLeftTire.ActiveWeight - RearLeftTire.RestingWeight));
                float wrr = Math.Max(0, (RearRightTire.ActiveWeight - RearRightTire.RestingWeight));

                pos = (FrontLeftTire.transform.Position) * wfl +
                    (FrontRightTire.transform.Position) * wfr +
                    (RearLeftTire.transform.Position) * wrl +
                    (RearRightTire.transform.Position) * wrr;

                float weightTotal = wfl + wfr + wrl + wrr;

                if (weightTotal > 0)
                {
                    pos /= weightTotal;
                    pos.Normalize();
                    pos.X = Math.Clamp(pos.X, -0.4f, 0.4f);
                }
                else
                {
                    pos = Vector2.Zero;
                }
            }

            // Update the "Center Of Gravity" dot to indicate the weight shift            
            CenterOfGravity.Position = Vector2.Lerp(CenterOfGravity.Position, pos, 0.1f);



            // Skidmarks
            if (Math.Abs(LocalAcceleration.Y) > 18 || EBrake == 1)
            {
                skids.Trigger(ConvertUnits.ToDisplayUnits(rb.GetWorldPoint(leftRearWheelPosition)));
                skids.Trigger(ConvertUnits.ToDisplayUnits(rb.GetWorldPoint(rightRearWheelPosition)));
            }

            // Automatic transmission
            engine.UpdateAutomaticTransmission(rb);

            if (true && i % 10 == 0)
            {
                doDebug();
                i = 0;
            }
            i++;


            skids.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            FixedUpdate(gameTime);
        }

       
        void FixedUpdate(GameTime gameTime)
        {

            HeadingAngle = rb.Rotation;
            Up = Vector2.Transform(new Vector2(0, -1), Matrix.CreateRotationZ(rb.Rotation));

            Velocity = rb.LinearVelocity;

            //var sin = MathF.Sin(HeadingAngle);
            //var cos = -MathF.Cos(HeadingAngle);

            var rotationMatrix = Matrix.CreateRotationZ(HeadingAngle - MathHelper.ToRadians(90));

            LocalVelocity = Vector2.Transform(Velocity,Matrix.Invert(rotationMatrix));
            // Get local velocity
            //LocalVelocity.X = cos * Velocity.Y + sin * Velocity.X;
            //LocalVelocity.Y = -sin * Velocity.Y + cos * Velocity.X;

            // Weight transfer
            float transferX = WeightTransfer * LocalAcceleration.X * CGHeight / WheelBase;
            float transferY = WeightTransfer * LocalAcceleration.Y * CGHeight / TrackWidth * 2;        //exagerate the weight transfer on the y-axis

            
            // Weight on each axle
            float weightFront = rb.Mass * (DistanceToGc * Gravity - transferX);
            float weightRear = rb.Mass * (DistanceToGc * Gravity + transferX);

            // Weight on each tire
            FrontLeftTire.ActiveWeight = weightFront - transferY;
            FrontRightTire.ActiveWeight = weightFront + transferY;
            RearLeftTire.ActiveWeight = weightRear - transferY;
            RearRightTire.ActiveWeight = weightRear + transferY;

            // Velocity of each tire
            FrontLeftTire.AngularVelocity = DistanceToGc * AngularVelocity;
            FrontRightTire.AngularVelocity = DistanceToGc * AngularVelocity;
            RearLeftTire.AngularVelocity = -DistanceToGc * AngularVelocity;
            RearRightTire.AngularVelocity = -DistanceToGc * AngularVelocity;

            // Math Sign UNITY / C# ????
            
            //if (LocalVelocity.X == 0)
            //    ass = 1;
            
            // Slip angle

            var frontAxisAngularVelocity = (FrontLeftTire.AngularVelocity + FrontRightTire.AngularVelocity) / 2f;
            var rearAxisAngularVelocity = (RearLeftTire.AngularVelocity + RearRightTire.AngularVelocity) / 2f;

            // HIER !!!

            var sign = Math.Sign(LocalVelocity.X);
            f_slipangle = MathF.Atan2(LocalVelocity.Y + frontAxisAngularVelocity, Math.Abs(LocalVelocity.X)) - sign * SteerAngle;
            b_slipangle = MathF.Atan2(LocalVelocity.Y + rearAxisAngularVelocity, Math.Abs(LocalVelocity.X));


            // Brake and Throttle power
            float activeBrake = Math.Min(Brake * BrakePower + EBrake * EBrakePower, BrakePower);
            float activeThrottle = (Throttle * engine.GetTorque(rb)) * (engine.GearRatio * engine.EffectiveGearRatio);

            // Torque of each tire (rear wheel drive)
            RearLeftTire.Torque = activeThrottle / RearLeftTire.Radius;
            RearRightTire.Torque = activeThrottle / RearRightTire.Radius;

            // Grip and Friction of each tire
            FrontLeftTire.Grip = TotalTireGripFront * (1.0f - EBrake * (1.0f - EBrakeGripRatioFront));
            FrontRightTire.Grip = TotalTireGripFront * (1.0f - EBrake * (1.0f - EBrakeGripRatioFront));
            RearLeftTire.Grip = TotalTireGripRear * (1.0f - EBrake * (1.0f - EBrakeGripRatioRear));
            RearRightTire.Grip = TotalTireGripRear * (1.0f - EBrake * (1.0f - EBrakeGripRatioRear));

            FrontLeftTire.FrictionForce = MathHelper.Clamp(-CornerStiffnessFront * f_slipangle, -FrontLeftTire.Grip, FrontLeftTire.Grip) * FrontLeftTire.ActiveWeight;
            FrontRightTire.FrictionForce = MathHelper.Clamp(-CornerStiffnessFront * f_slipangle, -FrontRightTire.Grip, FrontRightTire.Grip) * FrontRightTire.ActiveWeight;
            RearLeftTire.FrictionForce = MathHelper.Clamp(-CornerStiffnessRear * b_slipangle, -RearLeftTire.Grip, RearLeftTire.Grip) * RearLeftTire.ActiveWeight;
            RearRightTire.FrictionForce = MathHelper.Clamp(-CornerStiffnessRear * b_slipangle, -RearRightTire.Grip, RearRightTire.Grip) * RearRightTire.ActiveWeight;


            var b_torque = (RearLeftTire.Torque + RearRightTire.Torque) / 2f;
            // Forces
            float tractionForceX = b_torque - activeBrake * Math.Sign(LocalVelocity.X);
            //float tractionForceX = b_torque - activeBrake * ass;
            float tractionForceY = 0;

            float dragForceX = -RollingResistance * LocalVelocity.X - AirResistance * LocalVelocity.X * Math.Abs(LocalVelocity.X);
            float dragForceY = -RollingResistance * LocalVelocity.Y - AirResistance * LocalVelocity.Y * Math.Abs(LocalVelocity.Y);

            
            f_friction = (FrontLeftTire.FrictionForce + FrontRightTire.FrictionForce) / 2f;
            r_friction = (RearLeftTire.FrictionForce + RearRightTire.FrictionForce) / 2f;

            float totalForceX = dragForceX + tractionForceX;
            float totalForceY = dragForceY + tractionForceY + MathF.Cos(SteerAngle) * f_friction + r_friction;

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
            
            Velocity += Acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Velocity and speed
            //Velocity.X += Acceleration.X * (float)gameTime.ElapsedGameTime.TotalSeconds;
            //Velocity.Y += Acceleration.Y * (float)gameTime.ElapsedGameTime.TotalSeconds;

            AbsoluteVelocity = Velocity.Length();

            // Angular torque of car
            float angularTorque = (f_friction * DistanceToGc) - (r_friction * DistanceToGc);

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
            

            // AngularVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;



            rb.LinearVelocity = Velocity;
            //rb.LinearVelocity = Vector2.Zero;
            //rb.AngularVelocity = AngularVelocity;
            //rb.AngularVelocity = AngularVelocity;
            rb.Rotation = HeadingAngle;
            //rb.AngularVelocity = 0.0f;

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
            //if (Math.Abs(steerInput) < 0.001f)
            //{
            //    return 0;
            //}

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

            debugger.AddDebugValue("TireFL Weight", FrontLeftTire.ActiveWeight.ToString());
            debugger.AddDebugValue("TireFR Weight", FrontRightTire.ActiveWeight.ToString());
            debugger.AddDebugValue("TireRL Weight", RearLeftTire.ActiveWeight.ToString());
            debugger.AddDebugValue("TireRR Weight", RearRightTire.ActiveWeight.ToString());

            debugger.AddDebugValue("TireFL Friction", FrontLeftTire.FrictionForce.ToString());
            debugger.AddDebugValue("TireFR Friction", FrontRightTire.FrictionForce.ToString());
            debugger.AddDebugValue("TireRL Friction", RearLeftTire.FrictionForce.ToString());
            debugger.AddDebugValue("TireRR Friction", RearRightTire.FrictionForce.ToString());

            debugger.AddDebugValue("TireFL Grip", FrontLeftTire.Grip.ToString());
            debugger.AddDebugValue("TireFR Grip", FrontRightTire.Grip.ToString());
            debugger.AddDebugValue("TireRL Grip", RearLeftTire.Grip.ToString());
            debugger.AddDebugValue("TireRR Grip", RearRightTire.Grip.ToString());
            
            debugger.AddDebugValue("AxleF SlipAngle", f_slipangle.ToString());
            debugger.AddDebugValue("AxleR SlipAngle", b_slipangle.ToString());

//            debugger.AddDebugValue("AxleF Torque", FrontAxis.Torque.ToString());
            //debugger.AddDebugValue("AxleR Torque", BackAxis.Torque.ToString());

            debugger.AddDebugValue("CG", CenterOfGravity.ToString());
            debugger.AddDebugValue("Skids", Skids.ToString());

            debugger.AddDebugValue("Body Rotation", rb.Rotation);
            debugger.AddDebugValue("Body Speed", rb.LinearVelocity);
            debugger.AddDebugValue("Angular Speed", rb.AngularVelocity);
            debugger.AddDebugValue("Heading angle", HeadingAngle);

        }

        public override void DrawDebug(SpriteBatcher sb)
        {
            sb.Draw(skids);
            //sb.Draw(sprite, Transform.Position, Transform.Rotation,new Vector2(3.375f,3.2f));
            sb.Draw(sprite, Transform.Position, Transform.Rotation);
            sb.DrawPoint(ConvertUnits.ToDisplayUnits(rb.GetWorldPoint((CenterOfGravity.Position))), Color.Red, 3);
            sb.DrawLine(Transform.Position, Transform.Position + rb.LinearVelocity * 20, Color.Green, 2);
            sb.DrawLine(Transform.Position, Transform.Position + LocalVelocity * 20, Color.Blue, 2);
            sb.DrawLine(Transform.Position, Transform.Position + Up * 20, Color.Pink, 2);
        }

        #endregion

#if SIMPLE
        public override void Update(GameTime gameTime)
        {
            const float maxSpeed = 10;
            const float braking = 0.5f;
            const float acceleration = 0.5f;
            const float steering = 0.1f;
            const float drag = 0.05f;

            var h = -Input.GamePads[0].ThumbSticks.Left.X;
            var v = 0.0f;
            if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.A, Microsoft.Xna.Framework.Input.Keys.Up))
                v = 1.0f;
            if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.B, Microsoft.Xna.Framework.Input.Keys.Down))
                v = -1.0f;

            var rot = rb.Rotation - (h * steering);
            rb.Rotation = rot;
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
#endif

#if COMPLEX
        public override void Update(GameTime gameTime)
        {
            const float acceleration = 1500;
            const float steering = 3;
            float h = -Input.GamePads[0].ThumbSticks.Left.X;
            float v = 0.0f;
            if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.A, Microsoft.Xna.Framework.Input.Keys.Up))
                v = 1.0f;
            if (Input.IsPressed(0, Microsoft.Xna.Framework.Input.Buttons.B, Microsoft.Xna.Framework.Input.Keys.Down))
                v = -1.0f;


            var transformed = Vector2.Transform(new Vector2(0, -1), Matrix.CreateRotationZ(rb.Rotation));
            Vector2 speed = transformed * (v * acceleration);
            rb.ApplyForce(speed);

            float direction = Vector2.Dot(rb.LinearVelocity, transformed);


            if (direction > 0.0f)
            {

                rb.Rotation += h * steering * (rb.LinearVelocity.Length() / 5.0f);
                //rb.ApplyTorque((-h * steering) * (rb.LinearVelocity.Length() / 10.0f));

            }
            else
            {
                rb.Rotation -= h * steering * (rb.LinearVelocity.Length() / 5.0f);
                //rb.ApplyTorque((h * steering) * (rb.LinearVelocity.Length() / 10.0f));
            }


            Vector2 forward = new Vector2(0.0f, -0.5f);
            float steeringRightAngle;
            if (rb.AngularVelocity > 0)
            {
                steeringRightAngle = -90;
            }
            else
            {
                steeringRightAngle = 90;
            }

            var ass = Quaternion.CreateFromAxisAngle(Vector3.Forward, steeringRightAngle);

            //Quaternion.A
            //Quaternion.CreateFromYawPitchRoll(0, 0, steeringRightAngle).Z;
            Vector2 rightAngleFromForward = new Vector2(ass.X, ass.Y) * forward;
            ////Debug.DrawLine((Vector2)rb.Position, (Vector2)rb.GetWorldPoint(rightAngleFromForward), Color.Green);

            
            float driftForce = Vector2.Dot(rb.LinearVelocity, rb.GetWorldVector(Vector2.Normalize(rightAngleFromForward + rb.Position)));

            Vector2 relativeForce = (Vector2.Normalize(rightAngleFromForward) * -1.0f) * (driftForce * 10.0f);


            ////Debug.DrawLine((Vector2)rb.Position, (Vector2)rb.GetWorldPoint(relativeForce), Color.Red);

            rb.ApplyForce(rb.GetWorldVector(relativeForce));
        }
#endif
    }

}

