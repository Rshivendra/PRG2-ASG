using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG2Assignment
{
    internal class DeluxeRoom : Room
    {
        // Attributes for DeluxeRoom Class
        public bool AdditionalBed { get; set; }

        // Constructors for DeluxeRoom Class
        public DeluxeRoom() { }
        public DeluxeRoom(int roomNo, string bedConfig, double dailyRate, bool isAvail) : base(roomNo, bedConfig, dailyRate, isAvail) { }

        // Methods for DeluxeRoom Class
        public override double CalculateCharges()
        {
            int bedCharge = 0;

            if (AdditionalBed == true)
            {
                bedCharge = 25;
            }

            return bedCharge;
        }

        public override string ToString()
        {
            return base.ToString() + $"{CalculateCharges():2C,-10}";
        }
    }
}
