using PRG2Assignment;

// global data lists
List<Guest> guestList = new List<Guest>();
List<Stay> stayList = new List<Stay>();
List<Room> roomList = new List<Room>();
List<int> roomsBooked = new List<int>();

// used to check each person's stay history
IDictionary<string, List<Stay>> stayDict = new Dictionary<string, List<Stay>>();
// used to check each person's checkedIn status
IDictionary<string, bool> ischeckedInDict = new Dictionary<string, bool>();
// used to check the amount of room booked by each person
IDictionary<string, List<int>> roomsBookedDict = new Dictionary<string, List<int>>();
// used to check the availability of the room
IDictionary<int, bool> roomsAvailDict = new Dictionary<int, bool>();
// used to check the addons for each room
IDictionary<int, List<bool>> roomAddonsDict = new Dictionary<int, List<bool>>();

void DisplayGuests()
{

}


void DisplayAvailableRooms()
{

}


void RegisterGuest()
{

}


void InitStay()
{
    using (StreamReader eachLine = new StreamReader("Stays.csv"))
    {
        string? rowReader = eachLine.ReadLine();
        while ((rowReader = eachLine.ReadLine()) != null)
        {
            string[]? currentRow = rowReader.Split(",");

            bool ischeckedIn = bool.Parse(currentRow[2]);
            Stay currentStay = new Stay(DateTime.Parse(currentRow[3]), DateTime.Parse(currentRow[4]));
            stayList.Add(currentStay);
            ischeckedInDict.Add(currentRow[1], bool.Parse(currentRow[2]));
            roomsBookedDict.Add(currentRow[1], roomsBooked);

            // run thru the row to check for the roomNos taken by each person and implement each add-on for each room
            for (int i = 0; i < currentRow.Length; i++)
            {
                int roomNo;
                List<bool> addons = new List<bool>();

                if (int.TryParse(currentRow[i], out roomNo))
                {
                    roomsBooked.Add(roomNo);
                    roomsAvailDict.Add(roomNo, !ischeckedIn);

                    for (int j = 1; i <= 3; j++)
                    {
                        addons.Add(bool.Parse(currentRow[i + j]));
                    }

                    roomAddonsDict.Add(roomNo, addons);

                }
                else { continue; }
            }


            // reset the roomsBooked List
            roomsBooked.Clear();

        }
    }
}


void InitRoom()
{
    using (StreamReader sr = new StreamReader("Rooms.csv"))
    {
        string? s = sr.ReadLine();

        while ((s = sr.ReadLine()) != null)
        {
            string[] record = s.Split(',');

            int roomNumber = Convert.ToInt32(record[1]);
            string bedConfig = record[2];
            double dailyRate = Convert.ToDouble(record[3]);
            bool isAvail = true;

            // obtain the availbility of the room
            isAvail = roomsAvailDict[roomNumber];

            switch (record[0].ToUpper())
            {
                case "STANDARD":
                    Room stdRoom = new StandardRoom(roomNumber, bedConfig, dailyRate, isAvail);
                    roomList.Add(stdRoom);
                    break;
                case "DELUX":
                    Room dlxRoom = new DeluxeRoom(roomNumber, bedConfig, dailyRate, isAvail);
                    roomList.Add(dlxRoom);
                    break;
                default:
                    Console.WriteLine("An error occured when searching for the rooms!");
                    break;
            }


        }
    }
}
