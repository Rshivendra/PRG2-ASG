using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG2Assignment
{
    internal class Guest
    {
        // Attributes for Guest Class
        public string? Name { get; set; }
        public string? PassportNum { get; set; }
        public Stay? HotelStay { get; set; }
        public Membership? Member { get; set; }
        public bool? IsCheckedIn { get; set; }

        // Constructors for Guest Class
        public Guest() { }

        public Guest(string? name, string? passportNum, Stay? hotelStay, Membership? member, bool? isCheckedIn)
        {
            this.Name = name;
            this.PassportNum = passportNum;
            this.HotelStay = hotelStay;
            this.Member = member;
            this.IsCheckedIn = isCheckedIn;
        }

        // Methods for Guest Class
        public override string ToString()
        {
            return $"{this.Name,-10} {this.PassportNum,-10} {this.HotelStay,-10} {this.Member,-10} {this.IsCheckedIn,-10}";
        }

    }
}