using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG2Assignment
{
    internal class StandardRoom : Room
    {
        // Attributes for StandardRoom Class
        public bool RequireWifi { get; set; }
        public bool RequireBreakfast { get; set; }
      
        // Constructors for StandardRoom Class
        public StandardRoom() : base() { }
        public StandardRoom(int roomNo, string bedConfig, double dailyRate, bool isAvail) : base(roomNo, bedConfig, dailyRate, isAvail){ }

        // Methods for StandardRoom Class
        public override double CalculateCharges()
        {
            int wifiCharge = 0;
            int breakfastCharge = 0;

            if (RequireWifi == true)
            {
                wifiCharge = 10;
            }

            if (RequireBreakfast == true)
            {
                breakfastCharge = 20;
            }

            return wifiCharge + breakfastCharge;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
