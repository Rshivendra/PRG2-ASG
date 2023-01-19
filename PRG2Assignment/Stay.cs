using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG2Assignment
{
    internal class Stay
    {
        // Attributes for Stay Class
        public DateTime CheckinDate { get; set; }
        public DateTime CheckoutDate { get; set; }
        public List<Room> RoomList { get; set; } = new List<Room>();

        // Constructors for Stay Class
        public Stay() { }

        public Stay(DateTime checkinDate, DateTime checkoutDate)
        {
            this.CheckinDate = checkinDate;
            this.CheckoutDate = checkoutDate;
        }

        // Methods for Stay Class
        public void AddRoom(Room room)
        {
            RoomList.Add(room);
        }

        public double CalculateTotal()
        {
            double total = 0;
            int days = CheckoutDate.Subtract(CheckinDate).Days;

            foreach (Room room in RoomList)
            {
                total += room.CalculateCharges() * days + room.DailyRate * days;
            }
            return total;
        }

        public override string ToString()
        {
            return $"CheckIn:\n{this.CheckinDate.Date,-10}\n\nCheckOut:\n{this.CheckoutDate.Date,-10}\n\nDuration of Stay:\n{CheckoutDate.Subtract(CheckinDate).Days} Days";
        }

    }
}
