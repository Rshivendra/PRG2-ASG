﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG2Assignment
{
    abstract class Room
    {
        // Attributes for Room Class
        public int RoomNumber { get; set; }
        public string BedConfiguration { get; set; }
        public double DailyRate { get; set; }
        public bool IsAvail { get; set; }

        // Constructors for Room Class
        public Room() { }
        public Room(int roomNo, string bedConfig, double dailyRate, bool isAvail)
        {
            RoomNumber = roomNo;
            BedConfiguration = bedConfig;
            DailyRate = dailyRate;
            IsAvail = isAvail;
        }

        // Methods for Room Class

        // Method to find the if a guest made extra charges during his stay
        public abstract double CalculateCharges();

        public override string ToString()
        {
            return $"{RoomNumber,-10}{BedConfiguration,-10}{DailyRate,-10}{IsAvail,-10}";
        }
    }
}