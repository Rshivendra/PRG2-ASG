using PRG2Assignment;
using System.Collections.Generic;

// global data lists/hashsets
List<Guest> guestList = new List<Guest>();
List<Room> roomList = new List<Room>();
HashSet<int> roomsBooked = new HashSet<int>();

// reason for hashset: only use it when involving a set of unique data

// used to check each person's stay history
IDictionary<string, Stay> stayDict = new Dictionary<string, Stay>();
// used to check each person's checkedIn status
IDictionary<string, bool> ischeckedInDict = new Dictionary<string, bool>();
// used to check the amount of room booked by each person
IDictionary<string, HashSet<int>> roomsBookedDict = new Dictionary<string, HashSet<int>>();
// used to check the addons for each room
IDictionary<int, List<bool>> roomAddonsDict = new Dictionary<int, List<bool>>();
// used to check the unavailability of the room in Stays.csv
IDictionary<int, bool> roomsNotAvailDict = new Dictionary<int, bool>();

// unused lists - do not delete until further notice -
//List<int> roomsBooked = new List<int>();
//List<int> roomsUnavailbleRooms = new List<int>();
//HashSet<int> roomsUnavailbleRooms = new HashSet<int>();

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

            guestList.Add(guest);

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
            stayDict.TryAdd(passPortNo, currentRowStay);
            ischeckedInDict.TryAdd(passPortNo, ischeckedIn);

            // run thru the row to check for the roomNos taken by each person and implement each add-on for each room
            for (int i = 0; i < currentRow.Length; i++)
            {
                int roomNo;
                List<bool> addons = new List<bool>();

                if (int.TryParse(currentRow[i], out roomNo))
                {

                    if (ischeckedIn == true)
                    {
                        roomsBooked.Add(roomNo);
                        roomsNotAvailDict.TryAdd(roomNo, ischeckedIn);

                        for (int j = 1; j <= 3 && i + j < currentRow.Length; j++)
                        {
                            addons.Add(bool.Parse(currentRow[i + j]));
                        }

                        roomAddonsDict.TryAdd(roomNo, addons);
                    }

                }
                else { continue; }
            }

            roomsBookedDict.TryAdd(passPortNo, roomsBooked);

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
            bool isAvail;
            int roomNo = Convert.ToInt32(record[1]);
            string bedConfig = record[2];
            double dailyRate = Convert.ToDouble(record[3]);

            if (roomsNotAvailDict.TryGetValue(roomNo, out isAvail))
            {
                // roomNumber was in dictionary;
                isAvail = false;
            }
            else
            {
                // roomNumber wasn't in dictionary;
                isAvail = true;
            }

            // run this code to check the availbility of all the rooms
            // press enter to continue
            /*
            Console.WriteLine($"Is Room Number {roomNo} Available?: {isAvail}");
            Console.ReadKey();
            */

            switch (record[0].ToUpper())
            {
                case "STANDARD":
                    Room stdRoom = new StandardRoom(roomNo, bedConfig, dailyRate, isAvail);
                    roomList.Add(stdRoom);
                    break;
                case "DELUXE":
                    Room dlxRoom = new DeluxeRoom(roomNo, bedConfig, dailyRate, isAvail);
                    roomList.Add(dlxRoom);
                    break;
                default:
                    Console.WriteLine("An error occured when searching for the rooms!");
                    break;
            }

        }
    }
}

void AvailRooms(List<Room> roomList)
{
    Console.WriteLine("-------------------------------------------------------------");
    Console.WriteLine("\t\t\tAvailable Rooms");
    Console.WriteLine("-------------------------------------------------------------");

    Console.WriteLine("Room\t Bed Configurations\t DailyRate\t Availability ");
    foreach(Room room in roomList)
    {
        if (room.IsAvail)
        {
            Console.WriteLine(room.ToString());
        } else { continue; }
    }
}

// Main Program
InitStay();
InitRoom();
InitGuest();

// Basic Feautres:
// Part 2)
AvailRooms(roomList);

void DisplayGuests()
{

}


void DisplayAvailableRooms()
{

}


void RegisterGuest()
{

}
