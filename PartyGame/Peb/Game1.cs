using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AuxLib;

namespace XNACardemo
{


    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatcher spriteBatch;

        SpriteFont font;
        Texture2D tailTexture;
        Texture2D circleTexture;
        Texture2D wheelTexture;
        Texture2D arcTexture;
        SpriteHelper tailSprites;
        SpriteHelper circleSprites;
        SpriteHelper wheelSprites;
        SpriteHelper arcSprites;


        List<LineInfo> lineList = new List<LineInfo>();
        List<DrawInfo> info = new List<DrawInfo>();
        CAR car = new CAR();

        #region Typedefs

        const int TRAIL_SIZE = 200;			/* number of dots in car trail */
        private int SCREEN_W;
        private int SCREEN_H;

        class CARTYPE
        {
            public float wheelbase;		// wheelbase in m
            public float b;				// in m, distance from CG to front axle
            public float c;				// in m, idem to rear axle
            public float h;				// in m, height of CM from ground
            public float mass;			// in kg
            public float inertia;		// in kg.m
            public float length, width;
            public float wheellength, wheelwidth;
        }

        class CAR
        {
            public CARTYPE cartype;			// pointer to static car data

            public Vector2 position_wc = new Vector2();		// position of car centre in world coordinates
            public Vector2 velocity_wc = new Vector2();		// velocity vector of car in world coordinates

            public float angle;				// angle of car body orientation (in rads)
            public float angularvelocity;

            public float steerangle;			// angle of steering (input)
            public float throttle;			// amount of throttle (input)
            public float brake;				// amount of braking (input)
        }

        struct TRAILPOINT
        {
            public float x, y;
            public float angle;
        }

        public static readonly Color[] GameColor = new Color[]
        {
            new Color(0, 160, 0), // Wheel
            new Color(230, 230, 230), // Trail
            new Color(160, 0, 0), // Car body
            new Color(0, 0, 160), // Wheels
            new Color(0, 100, 100), // Vector
            new Color(0, 0, 100), // Line
            new Color(0, 0, 0), // None
            new Color(0, 100, 0), // Line
            new Color(100, 0, 0), // Line
            new Color(100, 100, 100), // Line
        };


        CARTYPE[] cartypes = new CARTYPE[1];
        Vector2 screen_pos = new Vector2();
        float scale = 25;
        TRAILPOINT[] trail = new TRAILPOINT[TRAIL_SIZE];
        int num_trail = 0;



        /* Lots of globals, so their value may be printed on screen
         * normally most of these variables should be private to the physics function.
         */
        Vector2 velocity = new Vector2();
        Vector2 acceleration_wc = new Vector2();
        double rot_angle;
        double sideslip;
        double slipanglefront;
        double slipanglerear;
        Vector2 force = new Vector2();
        int rear_slip;
        int front_slip;
        Vector2 resistance = new Vector2();
        Vector2 acceleration = new Vector2();
        double torque;
        double angular_acceleration;
        double sn, cs;
        double yawspeed;
        double weight;
        Vector2 ftraction = new Vector2();
        Vector2 flatf = new Vector2(), flatr = new Vector2();
        #endregion


        /*
         * Trail module
         */


        void init_trail()
        {
            num_trail = 0;
        }


        void draw_trail(CAR car)
        {
            Color col;
            int i;
            int x, y;

            col = GameColor[1];

            for (i = 0; i < trail.Length; i++)
            {
                x = (int)((trail[i].x - car.position_wc.X) * scale + screen_pos.X);
                y = (int)(-(trail[i].y - car.position_wc.Y) * scale + screen_pos.Y);
                circleSimple(x, y, 2, col);
            }

        }

        private void DrawCircle(int x, int y, int p, Color col)
        {
            circleSprites.Render(new Rectangle(x, y, p, p), col, 0);

        }
        private void circleSimple(int x, int y, int p, Color col)
        {
            tailSprites.Render(new Rectangle(x , y, 12, 12), col, 0 );
        }

        void add_to_trail(float x, float y, float angle)
        {
            if (num_trail < TRAIL_SIZE - 1)
            {
                trail[num_trail].x = x;
                trail[num_trail].y = y;
                trail[num_trail].angle = angle;
                num_trail++;
            }
            else
            {
                num_trail = 0;
                //memcpy( trail+0, trail+1, sizeof(trail[0])*(TRAIL_SIZE-1));
                trail[num_trail].x = x;
                trail[num_trail].y = y;
                trail[num_trail].angle = angle;
            }
        }


        /*
         * End of Trail module
         */

        /*
         * Render module
         */

        void DrawRect(float angle, int w, int l, int x, int y, Color col, int crossed)
        {
            Vector2[] c = new Vector2[] { new Vector2(), new Vector2(), new Vector2(), new Vector2() }; ;
            Vector2[] c2 = new Vector2[] { new Vector2(), new Vector2(), new Vector2(), new Vector2() }; ;
            int i;

            float sn = (float)Math.Sin(angle);
            float cs = (float)Math.Cos(angle);

            c[0].X = -w / 2;
            c[0].Y = l / 2;

            c[1].X = w / 2;
            c[1].Y = l / 2;

            c[2].X = w / 2;
            c[2].Y = -l / 2;

            c[3].X = -w / 2;
            c[3].Y = -l / 2;

            for (i = 0; i <= 3; i++)
            {
                c2[i].X = cs * c[i].X - sn * c[i].Y;
                c2[i].Y = sn * c[i].X + cs * c[i].Y;
                c[i].X = c2[i].X;
                c[i].Y = c2[i].Y;
            }

            for (i = 0; i <= 3; i++)
            {
                c[i].X += x;
                c[i].Y += y;
            }
            //line(c[0].X, c[0].Y, c[1].X, c[1].Y, col);
            //line(c[1].X, c[1].Y, c[2].X, c[2].Y, col);
            //line(c[2].X, c[2].Y, c[3].X, c[3].Y, col);
            //line(c[3].X, c[3].Y, c[0].X, c[0].Y, col);

            wheelSprites.Render(
               new Rectangle(x, y, w, l), col, angle);
               //new Rectangle((int)c[0].X, (int)c[0].Y, w, l), col, angle);


            if (crossed == 1)
            {
                DrawLine(c[0].X, c[0].Y, c[2].X, c[2].Y, col);
                DrawLine(c[1].X, c[1].Y, c[3].X, c[3].Y, col);
            }
        }

        private void DrawLine(float p1, float p2, float p3, float p4, Color color)
        {
            LineInfo l = new LineInfo();
            l.lineStart = new Vector2(p1, p2);
            l.lineEnd = new Vector2(p3, p4);
            lineList.Add(l);
            //lineSprites.Render(new Rectangle((int)p1, (int)p2, (int)(p3 - p1), (int)(p4 - p2)), color, 0);

        }


        void DrawWheel(int nr, CAR car, int x, int y, int crossed)
        {
            Color col;

            col = GameColor[0];

            DrawRect(car.angle + (nr < 2 ? car.steerangle : 0),
                (int)(car.cartype.wheelwidth * scale), (int)(car.cartype.wheellength * scale), x, y, col, crossed);
        }


        void render(CAR car)
        {
            Color col;

            Vector2[] corners = new Vector2[] { new Vector2(), new Vector2(), new Vector2(), new Vector2() };
            Vector2[] wheels = new Vector2[] { new Vector2(), new Vector2(), new Vector2(), new Vector2() };
            Vector2[] w = new Vector2[] { new Vector2(), new Vector2(), new Vector2(), new Vector2() };

            int i;
            int y;

            double sn = Math.Sin(car.angle);
            double cs = Math.Cos(car.angle);

            screen_pos.X = car.position_wc.X * scale + SCREEN_W / 2;
            screen_pos.Y = -car.position_wc.Y * scale + SCREEN_H / 2;

            while (screen_pos.Y < 0)
                screen_pos.Y += SCREEN_H;
            while (screen_pos.Y > SCREEN_H)
                screen_pos.Y -= SCREEN_H;
            while (screen_pos.X < 0)
                screen_pos.X += SCREEN_W;
            while (screen_pos.X > SCREEN_W)
                screen_pos.X -= SCREEN_W;

            //draw_trail(car);


            //
            // Draw car body
            //

            col = GameColor[2];

            // corners: 0=fr left, 1=fr right, 2 =rear right, 3=rear left

            corners[0].X = -car.cartype.width / 2;
            corners[0].Y = -car.cartype.length / 2;

            corners[1].X = car.cartype.width / 2;
            corners[1].Y = -car.cartype.length / 2;

            corners[2].X = car.cartype.width / 2;
            corners[2].Y = car.cartype.length / 2;

            corners[3].X = -car.cartype.width / 2;
            corners[3].Y = car.cartype.length / 2;

            for (i = 0; i <= 3; i++)
            {
                w[i].X = (float)(cs * corners[i].X - sn * corners[i].Y);
                w[i].Y = (float)(sn * corners[i].X + cs * corners[i].Y);
                corners[i].X = w[i].X;
                corners[i].Y = w[i].Y;
            }

            for (i = 0; i <= 3; i++)
            {
                corners[i].X *= scale;
                corners[i].Y *= scale;
                corners[i].X += screen_pos.X;
                corners[i].Y += screen_pos.Y;
            }

            DrawLine(corners[0].X, corners[0].Y, corners[1].X, corners[1].Y, col);
            DrawLine(corners[1].X, corners[1].Y, corners[2].X, corners[2].Y, col);
            DrawLine(corners[2].X, corners[2].Y, corners[3].X, corners[3].Y, col);
            DrawLine(corners[3].X, corners[3].Y, corners[0].X, corners[0].Y, col);

            //wheelSprites.Render(
            //    new Rectangle((int)(screen_pos.X + car.cartype.width * scale / 2), (int)screen_pos.Y, (int)Math.Abs(car.cartype.width * scale), (int)Math.Abs(car.cartype.length * scale)), col, car.angle);



            //
            // Draw wheels
            //
            col = GameColor[3];


            // wheels: 0=fr left, 1=fr right, 2 =rear right, 3=rear left

            wheels[0].X = -car.cartype.width / 2;
            wheels[0].Y = -car.cartype.b;

            wheels[1].X = car.cartype.width / 2;
            wheels[1].Y = -car.cartype.b;

            wheels[2].X = car.cartype.width / 2;
            wheels[2].Y = car.cartype.c;

            wheels[3].X = -car.cartype.width / 2;
            wheels[3].Y = car.cartype.c;


            for (i = 0; i <= 3; i++)
            {
                w[i].X = (float)(cs * wheels[i].X - sn * wheels[i].Y);
                w[i].Y = (float)(sn * wheels[i].X + cs * wheels[i].Y);
                wheels[i].X = w[i].X;
                wheels[i].Y = w[i].Y;
            }

            for (i = 0; i <= 3; i++)
            {
                wheels[i].X *= scale;
                wheels[i].Y *= scale;
                wheels[i].X += screen_pos.X;
                wheels[i].Y += screen_pos.Y;
            }

            DrawWheel(0, car, (int)wheels[0].X, (int)wheels[0].Y, front_slip);
            DrawWheel(1, car, (int)wheels[1].X, (int)wheels[1].Y, front_slip);
            DrawWheel(2, car, (int)wheels[2].X, (int)wheels[2].Y, rear_slip);
            DrawWheel(3, car, (int)wheels[3].X, (int)wheels[3].Y, rear_slip);

            
            // "wheel spokes" to show Ackermann centre of turn
            //
            DrawLine(wheels[0].X, wheels[0].Y,
                (float)(wheels[0].X - Math.Cos(car.angle + car.steerangle) * 100f),
                (float)(wheels[0].Y - Math.Sin(car.angle + car.steerangle) * 100), col);
            DrawLine(wheels[3].X, wheels[3].Y,
                (float)(wheels[3].X - Math.Cos(car.angle) * 100f),
                (float)(wheels[3].Y - Math.Sin(car.angle) * 100), col);
            

            col = GameColor[4];


            // Velocity vector dial
            //
            int VDIAL_X = 550;      //Circle
            int VDIAL_Y = 40;
            int VWDIAL_X = 550;     //Circle
            int VWDIAL_Y = 100;
            int THROTTLE_X = 400;   //Line
            int THROTTLE_Y = 120;
            int BRAKE_X = 440;      //Line
            int BRAKE_Y = 120;
            int STEER_X = 160;
            int STEER_Y = 140;
            int SLIP_X = 50;
            int SLIP_Y = 140;
            int ROT_X = 50;
            int ROT_Y = 180;
            int AF_X = 240;
            int AF_Y = 140;
            int AR_X = 240;
            int AR_Y = 180;
            int TEXT_X = 10;
            DrawCircle(VDIAL_X, VDIAL_Y, 50, col);
            DrawLine(VDIAL_X, VDIAL_Y, VDIAL_X + velocity.X, VDIAL_Y - velocity.Y, col);

            DrawCircle(VWDIAL_X, VWDIAL_Y, 50, col);
            col = GameColor[5];
            DrawLine(VWDIAL_X, VWDIAL_Y, VWDIAL_X + car.velocity_wc.X, VWDIAL_Y - car.velocity_wc.Y, col);


            col = GameColor[6];
            DrawLine(THROTTLE_X, THROTTLE_Y, THROTTLE_X, THROTTLE_Y - 100, col);
            col = GameColor[8];
            DrawLine(THROTTLE_X + 1, THROTTLE_Y, THROTTLE_X + 1, THROTTLE_Y - car.throttle, col);

            col = GameColor[6];
            DrawLine(BRAKE_X, BRAKE_Y, BRAKE_X, BRAKE_Y - 100, col);
            col = GameColor[5];
            DrawLine(BRAKE_X + 1, BRAKE_Y, BRAKE_X + 1, BRAKE_Y - car.brake, col);

            col = GameColor[6];
            arc(STEER_X, STEER_Y, (20), (108), 35, col);
            col = GameColor[7];
            DrawLine(STEER_X, STEER_Y, STEER_X + (int)(Math.Sin(car.steerangle) * 30.0), STEER_Y - (int)(Math.Cos(car.steerangle) * 30.0), col);

            col = GameColor[6];
            arc(SLIP_X, SLIP_Y, (20), (108), 35, col);
            col = GameColor[7];
            DrawLine(SLIP_X, SLIP_Y, SLIP_X + (int)(Math.Sin(sideslip) * 30.0), SLIP_Y - (int)(Math.Cos(sideslip) * 30.0), col);

            col = GameColor[6];
            arc(ROT_X, ROT_Y, (20), (108), 35, col);
            col = GameColor[7];
            DrawLine(ROT_X, ROT_Y, ROT_X + (int)(Math.Sin(rot_angle) * 30.0), ROT_Y - (int)(Math.Cos(rot_angle) * 30.0), col);

            col = GameColor[6];
            arc(AF_X, AF_Y, 20, 108, 35, col);
            col = GameColor[7];
            DrawLine(AF_X, AF_Y, AF_X + (int)(Math.Sin(slipanglefront) * 30.0), AF_Y - (int)(Math.Cos(slipanglefront) * 30.0), col);

            col = GameColor[6];
            arc(AR_X, AR_Y, 20, 108, 35, col);
            col = GameColor[7];
            DrawLine(AR_X, AR_Y, AR_X + (int)(Math.Sin(slipanglerear) * 30.0), AR_Y - (int)(Math.Cos(slipanglerear) * 30.0), col);


            col = GameColor[9];
            y = 0;
            string infoText = string.Format("scale {0} pixels/m <Q,W>", scale);
            textout(infoText, TEXT_X, y += 10, col);

            infoText = string.Format("alpha front {0} deg", (float)slipanglefront * 180.0f / Math.PI);
            textout(infoText, TEXT_X, y += 10, col);
            infoText = string.Format("alpha rear  {0} deg", (float)slipanglerear * 180.0f / Math.PI);
            textout(infoText, TEXT_X, y += 10, col);

            infoText = string.Format("f.lat front {0} N", flatf.Y);
            textout(infoText, TEXT_X, y += 10, col);
            infoText = string.Format("f.lat rear  {0} N", flatr.Y);
            textout(infoText, TEXT_X, y += 10, col);

            infoText = string.Format("force.x    {0} N", force.X);
            textout(infoText, TEXT_X, y += 10, col);
            infoText = string.Format("force.y lat {0} N", force.Y);
            textout(infoText, TEXT_X, y += 10, col);

            infoText = string.Format("torque      {0} Nm", (float)torque);
            textout(infoText, TEXT_X, y += 10, col);

            infoText = string.Format("ang.vel.    {0} rad/s", car.angularvelocity);
            textout(infoText, TEXT_X, y += 10, col);

            infoText = string.Format("Esc=quit Q/W=zoom RCtrl=brake Up/Down=accelerator Space=4wheel slip", 1);
            textout(infoText, 0, SCREEN_H - 20, col);
        }

        private void textout(string infoText, int x, int y, Color col)
        {
            DrawInfo drawText = new DrawInfo();
            drawText.pos = new Vector2(x, y);
            drawText.text = infoText;
            drawText.color = col;
            info.Add(drawText);
        }



        private void arc(int x, int y, int angle1, int angle2, int radius, Color col)
        {
            arcSprites.Render(new Rectangle(x + 15, y - 10, 60, 40), col, 0);
        }

        /*
         * End of Render module
         */

        /*
         * Physics module
         */
        void init_cartypes()
        {
            CARTYPE cartype;
            cartypes[0] = new CARTYPE();

            cartype = cartypes[0];
            cartype.b = 1.0f;					// m							
            cartype.c = 1.0f;					// m
            cartype.wheelbase = cartype.b + cartype.c;
            cartype.h = 1.0f;					// m
            cartype.mass = 1500;				// kg			
            cartype.inertia = 1500;			// kg.m			
            cartype.width = 1.5f;				// m
            cartype.length = 3.0f;				// m, must be > wheelbase
            cartype.wheellength = 0.7f;
            cartype.wheelwidth = 0.3f;

        }

        void init_car(CAR car, CARTYPE cartype)
        {
            car.cartype = cartype;

            car.position_wc.X = 0;
            car.position_wc.Y = 0;
            car.velocity_wc.X = 0;
            car.velocity_wc.Y = 0;

            car.angle = 0;
            car.angularvelocity = 0;

            car.steerangle = 0;
            car.throttle = 0;
            car.brake = 0;
        }

        // These constants are arbitrary values, not realistic ones.

        float DRAG = 5.0f;		 		/* factor for air resistance (drag) 	*/
        float RESISTANCE = 30.0f;			/* factor for rolling resistance */
        float CA_R = -5.20f;			/* cornering stiffness */
        float CA_F = -5.0f;			/* cornering stiffness */
        float MAX_GRIP = 2.0f;				/* maximum (normalised) friction force, =diameter of friction circle */

        void do_physics(CAR car, float delta_t)
        {
            sn = Math.Sin(car.angle);
            cs = Math.Cos(car.angle);

            // SAE convention: x is to the front of the car, y is to the right, z is down

            // transform velocity in world reference frame to velocity in car reference frame
            velocity.X = (float)(cs * car.velocity_wc.Y + sn * car.velocity_wc.X);
            velocity.Y = (float)(-sn * car.velocity_wc.Y + cs * car.velocity_wc.X);

            // Lateral force on wheels
            //	
            // Resulting velocity of the wheels as result of the yaw rate of the car body
            // v = yawrate * r where r is distance of wheel to CG (approx. half wheel base)
            // yawrate (ang.velocity) must be in rad/s
            //
            yawspeed = car.cartype.wheelbase * 0.5 * car.angularvelocity;

            if (velocity.X == 0)		// TODO: fix singularity
                rot_angle = 0;
            else
                rot_angle = Math.Atan2(yawspeed, velocity.X);

            // Calculate the side slip angle of the car (a.k.a. beta)
            if (velocity.X == 0)		// TODO: fix singularity
                sideslip = 0;
            else
                sideslip = Math.Atan2(velocity.Y, velocity.X);

            // Calculate slip angles for front and rear wheels (a.k.a. alpha)
            slipanglefront = sideslip + rot_angle - car.steerangle;
            slipanglerear = sideslip - rot_angle;

            // weight per axle = half car mass times 1G (=9.8m/s^2) 
            weight = car.cartype.mass * 9.8 * 0.5;

            // lateral force on front wheels = (Ca * slip angle) capped to friction circle * load
            flatf.X = 0;
            flatf.Y = (float)(CA_F * slipanglefront);
            flatf.Y = (float)Math.Min(MAX_GRIP, flatf.Y);
            flatf.Y = (float)Math.Max(-MAX_GRIP, flatf.Y);
            flatf.Y *= (float)weight;
            if (front_slip == 1)
                flatf.Y *= 0.5f;

            // lateral force on rear wheels
            flatr.X = 0;
            flatr.Y = (float)(CA_R * slipanglerear);
            flatr.Y = (float)Math.Min(MAX_GRIP, flatr.Y);
            flatr.Y = (float)Math.Max(-MAX_GRIP, flatr.Y);
            flatr.Y *= (float)weight;
            if (rear_slip == 1)
                flatr.Y *= 0.5f;

            // longtitudinal force on rear wheels - very simple traction model
            ftraction.X = 100 * (car.throttle - car.brake * (((velocity.X) >= 0) ? 1 : -1));
            ftraction.Y = 0;
            if (rear_slip == 1)
                ftraction.X *= 0.5f;

            // Forces and torque on body

            // drag and rolling resistance
            resistance.X = (float)-(RESISTANCE * velocity.X + DRAG * velocity.X * Math.Abs(velocity.X));
            resistance.Y = (float)-(RESISTANCE * velocity.Y + DRAG * velocity.Y * Math.Abs(velocity.Y));

            // sum forces
            force.X = (float)(ftraction.X + Math.Sin(car.steerangle) * flatf.X + flatr.X + resistance.X);
            force.Y = (float)(ftraction.Y + Math.Cos(car.steerangle) * flatf.Y + flatr.Y + resistance.Y);

            // torque on body from lateral forces
            torque = car.cartype.b * flatf.Y - car.cartype.c * flatr.Y;

            // Acceleration

            // Newton F = m.a, therefore a = F/m
            acceleration.X = force.X / car.cartype.mass;
            acceleration.Y = force.Y / car.cartype.mass;

            angular_acceleration = torque / car.cartype.inertia;

            // Velocity and position

            // transform acceleration from car reference frame to world reference frame
            acceleration_wc.X = (float)(cs * acceleration.Y + sn * acceleration.X);
            acceleration_wc.Y = (float)(-sn * acceleration.Y + cs * acceleration.X);

            // velocity is integrated acceleration
            //
            car.velocity_wc.X += delta_t * acceleration_wc.X;
            car.velocity_wc.Y += delta_t * acceleration_wc.Y;

            // position is integrated velocity
            //
            car.position_wc.X += delta_t * car.velocity_wc.X;
            car.position_wc.Y += delta_t * car.velocity_wc.Y;


            // Angular velocity and heading

            // integrate angular acceleration to get angular velocity
            //
            car.angularvelocity += (float)(delta_t * angular_acceleration);

            // integrate angular velocity to get angular orientation
            //
            car.angle += delta_t * car.angularvelocity;

        }

        /*
         * End of Physics module
         */


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatcher(GraphicsDevice);
            font = Content.Load<SpriteFont>("Courier New");

//            primitiveBatch = new PrimitiveBatch(graphics.GraphicsDevice);


            tailTexture = Content.Load<Texture2D>("Circle");
            tailSprites = new SpriteHelper(tailTexture, null);
            wheelTexture = Content.Load<Texture2D>("Wheel");
            wheelSprites = new SpriteHelper(wheelTexture, null);
            circleTexture = Content.Load<Texture2D>("CircleBig");
            circleSprites = new SpriteHelper(circleTexture, null);
            arcTexture = Content.Load<Texture2D>("CircleHalf");
            arcSprites = new SpriteHelper(arcTexture, null);

            SCREEN_W = graphics.PreferredBackBufferWidth;
            SCREEN_H = graphics.PreferredBackBufferHeight;

            init_cartypes();
            init_car(car, cartypes[0]);

            init_trail();

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            KeyboardState kbd = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                    || kbd.IsKeyDown(Keys.Escape))
                this.Exit();

            render(car);
            draw_trail(car);

            if (kbd.IsKeyDown(Keys.F12)) // F12 to reset		
            {
                init_car(car, cartypes[0]);
                init_trail();
            }

            if (kbd.IsKeyDown(Keys.F6))			// F6 for screen shot
            {
                //take_screen_shot();
            }

            if (kbd.IsKeyDown(Keys.Up))	// throttle up
            {
                if (car.throttle < 100)
                    car.throttle += 5;
            }
            if (kbd.IsKeyDown(Keys.Down)) // throttle down
            {
                if (car.throttle >= 10)
                    car.throttle -= 5;
            }

            if (kbd.IsKeyDown(Keys.RightControl))	// brake
            {
                car.brake = 100;
                car.throttle = 0;
            }
            else
                car.brake = 0;

            // Steering 
            //
            if (kbd.IsKeyDown(Keys.Left))
            {
                if (car.steerangle > -Math.PI / 4.0)
                    car.steerangle -= (float)Math.PI / 128.0f;
            }
            else if (kbd.IsKeyDown(Keys.Right))
            {
                if (car.steerangle < Math.PI / 4.0)
                    car.steerangle += (float)Math.PI / 128.0f;
            }

            // Zoom in, zoom out
            if (kbd.IsKeyDown(Keys.Q))
                scale += 1.0f;
            if (kbd.IsKeyDown(Keys.W))
                scale -= 1.0f;

            // Let front, rear or both axles slip
            rear_slip = 0;
            front_slip = 0;
            if (kbd.IsKeyDown(Keys.R))
                rear_slip = 1;
            if (kbd.IsKeyDown(Keys.F))
                front_slip = 1;
            if (kbd.IsKeyDown(Keys.Space))
            {
                front_slip = 1;
                rear_slip = 1;
            }

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            do_physics(car, elapsed);
            add_to_trail(car.position_wc.X, car.position_wc.Y, car.angle);

            base.Update(gameTime);
        }

        //PrimitiveBatch primitiveBatch;

        private void DrawLines()
        {
            Vector2 where = new Vector2(50,50);

            // the sun is made from 4 lines in a circle.
            //spriteBatch.Begin(PrimitiveType.LineList);

            spriteBatch.Begin();

            foreach (LineInfo line in lineList)
            {
                spriteBatch.DrawLine(line.lineStart, line.lineEnd, Color.White, 2);
                //line.Render(spriteBatch);
                //primitiveBatch.AddVertex(line.lineStart, Color.White);
                //primitiveBatch.AddVertex(line.lineEnd, Color.White);
            }


            spriteBatch.End();

            //primitiveBatch.End();
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            //spriteBatch.Begin(SpriteBlendMode.AlphaBlend,
             //   SpriteSortMode.BackToFront, SaveStateMode.None);

            SpriteHelper.DrawSprites(SCREEN_W , SCREEN_H);

            foreach (DrawInfo drawInfo in info)
            {
                spriteBatch.DrawString(font,drawInfo.text,
                           drawInfo.pos, drawInfo.color);                
            }


            spriteBatch.End();

            DrawLines();

            lineList.Clear();
            info.Clear();

            base.Draw(gameTime);
        }
    }

    class DrawInfo
    {
        public string text;
        public Vector2 pos;
        public Color color;
    }
    class LineInfo
    {
        public Vector2 lineStart;
        public Vector2 lineEnd;
    }
}
