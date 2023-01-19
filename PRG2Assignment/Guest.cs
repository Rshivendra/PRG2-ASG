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

        public Guest(string? name, string? passportNum, Stay? hotelStay, Membership? member)
        {
            this.Name = name;
            this.PassportNum = passportNum;
            this.HotelStay = hotelStay;
            this.Member = member;
            this.IsCheckedIn = false;
        }

        // Methods for Guest Class
        public override string ToString()
        {
            int daysLeft = this.HotelStay.CheckoutDate.Subtract(this.HotelStay.CheckinDate).Days;
            return $"{this.Name,-8} {this.PassportNum,-15} {daysLeft,-23} {this.Member.Status,-23} {this.IsCheckedIn,-10}";
        }

    }
}