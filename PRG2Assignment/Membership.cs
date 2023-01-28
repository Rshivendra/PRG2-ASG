﻿using System;
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
            if (this.Points >= 100 && this.Status.ToUpper() != "SILVER")
            {
                this.Status = "Silver";
            }
            else if (this.Points >= 200 && this.Status.ToUpper() != "GOLD")
            {
                this.Status = "Gold";
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