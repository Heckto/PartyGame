using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace AuxLib.Debug
{
    public class FpsMonitor
    {
        public float Value;
        public TimeSpan Sample { get; set; }
        private readonly Stopwatch sw;
        private int Frames;

        public FpsMonitor()
        {
            Sample = TimeSpan.FromSeconds(1);
            Value = 0;
            Frames = 0;
            sw = Stopwatch.StartNew();
        }

        public void Update()
        {
            Frames++;
            if (sw.Elapsed > Sample)
            {
                Value = (float)(Frames / sw.Elapsed.TotalSeconds);
                sw.Reset();
                sw.Start();
                Frames = 0;
            }
        }        

    }
}
