using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG2Assignment
{
    internal class Membership
    {
        // Attributes for Membership Class
        public string? Status { get; set; }
        public int Points { get; set; }

        // Constructors for Membership Class
        public Membership() { }

        public Membership(string? status, int points)
        {
            this.Status = status;
            this.Points = points;
        }

        // Methods for Membership Class
        public void EarnPoints(double totalCharges)
        {
            // formula: (nights_stayed * dailyRate) /10 
            // then append points to the Points variable
            // then change the status if reached quota

            // append points from checkout
            this.Points += (int)totalCharges / 10;

            // check if the status has been changed
            if (Status.ToUpper() == "ORDINARY")
            {
                if (Points >= 100)
                {
                    Status = "Silver";
                    Console.WriteLine("You have been upgraded to Silver Membership.\n");
                }
                else if (Points >= 200)
                {
                    Status = "Gold";
                    Console.WriteLine("You have been upgraded to Gold Membership.\n");
                }
            }
            else if (Status.ToUpper() == "SILVER")
            {
                if (Points >= 200)
                {
                    Status = "Gold";
                    Console.WriteLine("You have been upgraded to Gold Membership.\n");
                }
            }

        }

        public bool RedeemPoints(int points)
        {
            if (Points - points < 0)
            {
                return false;
            }
            else 
            {
                Points = Points - points;
                return true; 
            }
        }

        public override string ToString()
        {
            return $"{this.Status,-23} {this.Points}";
        }


    }
}