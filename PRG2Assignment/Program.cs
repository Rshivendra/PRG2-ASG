using PRG2Assignment;

// global data lists
List<Guest> guestList = new List<Guest>();
List<Room> roomList = new List<Room>();
List<int> roomsBooked = new List<int>();
List<int> roomsUnavailbleRooms = new List<int>();

// used to check each person's stay history
IDictionary<string, Stay> stayDict = new Dictionary<string, Stay>();
// used to check each person's checkedIn status
IDictionary<string, bool> ischeckedInDict = new Dictionary<string, bool>();
// used to check the amount of room booked by each person
IDictionary<string, List<int>> roomsBookedDict = new Dictionary<string, List<int>>();
// used to check the availability of the room in Stays.csv
//IDictionary<int, bool> roomsAvailDict = new Dictionary<int, bool>();
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

void InitGuest()
{
    using (StreamReader eachLine = new StreamReader("Guests.csv"))
    {
        string? rowReader = eachLine.ReadLine();
        while ((rowReader = eachLine.ReadLine()) != null)
        {
            string[]? currentRow = rowReader.Split(",");

            string passPortNo = currentRow[1];

            Membership membership = new Membership(currentRow[2], int.Parse(currentRow[3]));
            Stay stay = stayDict[passPortNo];
            Guest guest = new Guest(currentRow[0], passPortNo, stay,membership, ischeckedInDict[passPortNo]);

        }
    }
}

void InitStay()
{
    using (StreamReader eachLine = new StreamReader("Stays.csv"))
    {
        string? rowReader = eachLine.ReadLine();
        while ((rowReader = eachLine.ReadLine()) != null)
        {
            string[]? currentRow = rowReader.Split(",");

            string passPortNo = currentRow[1];

            bool ischeckedIn = bool.Parse(currentRow[2]);
            Stay currentRowStay = new Stay(DateTime.Parse(currentRow[3]), DateTime.Parse(currentRow[4]));
            stayDict.Add(passPortNo, currentRowStay);
            ischeckedInDict.Add(passPortNo, ischeckedIn);
            roomsBookedDict.Add(passPortNo, roomsBooked);

            // run thru the row to check for the roomNos taken by each person and implement each add-on for each room
            for (int i = 0; i < currentRow.Length; i++)
            {
                int roomNo;
                List<bool> addons = new List<bool>();

                if (int.TryParse(currentRow[i], out roomNo))
                {

                    if(ischeckedIn != false)
                    {
                        roomsBooked.Add(roomNo);
                        roomsUnavailbleRooms.Add(roomNo);
                        //roomsAvailDict.Add(roomNo, !ischeckedIn);

                        for (int j = 1; i <= 3; j++)
                        {
                            addons.Add(bool.Parse(currentRow[i + j]));
                        }

                        roomAddonsDict.Add(roomNo, addons);
                    }

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
            

            // goes thru the list of unavil rooms 
            foreach(int unavailRoom in roomsUnavailbleRooms)
            {
                if(unavailRoom == roomNumber)
                {
                    isAvail = false;
                }
            }

            switch (record[0].ToUpper())
            {
                case "STANDARD":
                    Room stdRoom = new StandardRoom(roomNumber, bedConfig, dailyRate, isAvail);
                    roomList.Add(stdRoom);
                    break;
                case "DELUXE":
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

// Main Program
InitStay();
InitRoom();
InitGuest();