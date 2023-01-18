using PRG2Assignment;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

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

void DisplayAvailRooms(List<Room> roomList)
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

Room SearchAvailRoom(int roomNum, List<Room> roomList)
{
    foreach (Room room in roomList)
    {
        if (roomNum == room.RoomNumber)
        {
            if (!room.IsAvail){continue;}
            else{return room;}
        }
        else { continue; }
    }

    return null;
}

void DisplayGuests(List<Guest> guestList)
{
    Console.WriteLine("-------------------------------------------------------------");
    Console.WriteLine("\t\t\tGuests");
    Console.WriteLine("-------------------------------------------------------------");

    Console.WriteLine("Name\t PassPort\t Duration of Stay\t Membership Status\t CheckedIn Status");
    foreach (Guest guest in guestList)
    {
        Console.WriteLine(guest.ToString());
    }
}

Guest retrieveGuest(string passportNum)
{
    foreach(Guest guest in guestList)
    {
        if (passportNum.Equals(guest.PassportNum))
        {
            return guest;
        }
    }

    return null;
}

void CheckInGuest()
{

    Guest guest;
    Room room;
    bool selectAnotherRoom = true;
    string roomType;
    DateTime checkinDate;
    DateTime checkoutDate;

    DisplayGuests(guestList);
    Console.WriteLine();
    Console.WriteLine("CHECK IN SYSTEM");
    Console.WriteLine("---------------");

    do
    {
        Console.Write("Please Enter Guest's Passport Number to check in: ");
        string passportNum = Console.ReadLine().ToUpper();
        guest = retrieveGuest(passportNum);
        if (guest == null) { Console.WriteLine("Guest not found!\nGive a valid passport number!"); }
        else if (guest.IsCheckedIn != false) { Console.WriteLine("Please select a guest that is not checked in!"); }

    } while (guest == null || guest.IsCheckedIn != false);
    
    // retrieving previous rooms booked if any
    roomsBooked = roomsBookedDict[guest.PassportNum];

    Console.Write("Enter Checkin Date (dd/MM/yyyy): ");
    checkinDate = exactDate();

    do
    {
        Console.Write("Enter Checkout Date (dd/MM/yyyy): ");
        checkoutDate = exactDate();
        if (checkoutDate.Subtract(checkinDate).Days < 0) 
        { 
            Console.WriteLine($"Checkout date is behind the checkin date!");
            Console.WriteLine("Re-enter the checkout date!");
            Console.WriteLine();
        }

    } while (checkoutDate.Subtract(checkinDate).Days < 0);

    Console.WriteLine();
    Stay stay = new Stay(checkinDate, checkoutDate);
    while (selectAnotherRoom)
    {
        DisplayAvailRooms(roomList);
        Console.WriteLine();
        do
        {
            Console.Write("Select an Available Room: ");
            room = SearchAvailRoom(IntChecker(), roomList);
            if (room == null) { Console.WriteLine("Room not found!"); }
        } while (room == null);

        // updating the availability
        room.IsAvail = false;

        switch (room)
        {

            case StandardRoom:
                Console.WriteLine();
                Console.WriteLine("-----------------------");
                Console.WriteLine("Standard Room Selected!");
                Console.WriteLine("-----------------------");
                Console.WriteLine();
                StandardRoom stdRoom = (StandardRoom)room;

                Console.Write("Would you require Wifi? [Y/N]: ");
                string wifi = Console.ReadLine();
                while (!ValidateInput(wifi))
                {
                    Console.WriteLine("Invalid input! Please enter Y for Yes or N for No.");
                    Console.Write("Would you require Wifi? [Y/N]: ");
                    wifi = Console.ReadLine();
                }

                Console.Write("Would you require Breakfast? [Y/N]: ");
                string breakfast = Console.ReadLine();
                while (!ValidateInput(breakfast))
                {
                    Console.WriteLine("Invalid input! Please enter Y for Yes or N for No.");
                    Console.Write("Would you require Breakfast? [Y/N]: ");
                    breakfast = Console.ReadLine();
                }

                // assigns True or False accordingly, and StringComparison.OrdinalIgnoreCase makes it so that it is case-sensitive
                stdRoom.RequireWifi = wifi.Equals("Y", StringComparison.OrdinalIgnoreCase);
                stdRoom.RequireBreakfast = breakfast.Equals("Y", StringComparison.OrdinalIgnoreCase);

                break;

            case DeluxeRoom:
                Console.WriteLine();
                Console.WriteLine("---------------------");
                Console.WriteLine("Deluxe Room Selected!");
                Console.WriteLine("---------------------");
                Console.WriteLine();
                DeluxeRoom dlxRoom = (DeluxeRoom)room;

                Console.Write("Would you require Additional Bed? [Y/N]: ");
                string bed = Console.ReadLine();
                while (!ValidateInput(bed))
                {
                    Console.WriteLine("Invalid input! Please enter Y for Yes or N for No.");
                    Console.Write("Would you require Additional Bed? [Y/N]: ");
                    bed = Console.ReadLine();
                }

                dlxRoom.AdditionalBed = bed.Equals("Y", StringComparison.OrdinalIgnoreCase);

                break;

            default:
                Console.WriteLine("An error has occured");
                break;
        }
        // adding the new room to the roomsbooked
        roomsBooked.Add(room.RoomNumber);

        Console.Write("Do you want to select another room? [Y/N]: ");
        string anotherRoomChoice = Console.ReadLine();
        while (!ValidateInput(anotherRoomChoice))
        {
            Console.WriteLine("Invalid input! Please enter Y for Yes or N for No.");
            Console.Write("Do you want to select another room? [Y/N]: ");
            anotherRoomChoice = Console.ReadLine();
        }

        if (anotherRoomChoice.Equals("N", StringComparison.OrdinalIgnoreCase))
        {
            selectAnotherRoom = false;
            // updating on user's rooms
            if (roomsBookedDict.TryGetValue(guest.PassportNum, out HashSet<int> existingRoomsBooked))
            {
                existingRoomsBooked.UnionWith(roomsBooked);
            }
            else
            {
                roomsBookedDict.Add(guest.PassportNum, roomsBooked);
            }

        }


    }

    // updating the Stay of the guest
    guest.HotelStay = stay;

    // updating on guest's checkedIn status
    guest.IsCheckedIn = true;
    Console.WriteLine();
    Console.WriteLine($"{guest.Name} has been CheckedIn: {guest.IsCheckedIn}");

    // clear the values in the roomsBooked list
    roomsBooked.Clear();

    // methods for this method
    DateTime exactDate()
    {
        string format = "dd/MM/yyyy";

        DateTime formatteddate;

        while (!DateTime.TryParseExact(Console.ReadLine(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out formatteddate))
        {
            Console.WriteLine();
            Console.WriteLine("Input is not a valid date");
            Console.WriteLine("Please follow the exact format listed below!");
            Console.WriteLine("dd/MM/yyyy");
            Console.WriteLine("E.g, 15/11/2022");
            Console.WriteLine();
            Console.Write("Please Re-Enter a Valid Date: ");
        }

        return formatteddate;
    }

    bool ValidateInput(string input)
    {
        return input.ToUpper().Equals("Y", StringComparison.OrdinalIgnoreCase) || input.Equals("N", StringComparison.OrdinalIgnoreCase);
    }

}

void ExtendStay()
{
    Guest guest;
    DisplayGuests(guestList);
    Console.WriteLine();
    Console.WriteLine("EXTEND STAY SYSTEM");
    Console.WriteLine("---------------");

    do
    {
        Console.Write("Please Enter Guest's Passport Number to check in: ");
        string passportNum = Console.ReadLine().ToUpper();
        guest = retrieveGuest(passportNum);
        if (guest == null) { Console.WriteLine("Guest not found!\nGive a valid passport number!"); }
        else if (guest.IsCheckedIn == false) { Console.WriteLine("Guest is not checked in"); }

    } while (guest == null || guest.IsCheckedIn == false);

    Console.WriteLine("------------------------------------------------------");
    Console.WriteLine($"Current Checkout Date: {guest.HotelStay.CheckoutDate}");
    Console.WriteLine("------------------------------------------------------");
    Console.WriteLine();
    Console.Write("Please enter the number of days to extend: ");
    int noOfDays = IntChecker();

    DateTime newCheckoutDate = guest.HotelStay.CheckoutDate.AddDays(noOfDays);
    guest.HotelStay.CheckoutDate = newCheckoutDate;

    Console.WriteLine($"{guest.Name}'s stay has been extended.");
    Console.WriteLine();
    Console.WriteLine("------------------------------------------------------");
    Console.WriteLine($"New Checkout Date: {guest.HotelStay.CheckoutDate}");
    Console.WriteLine("------------------------------------------------------");
    Console.WriteLine();
}

void RegisterGuest()
{

}

void DisplayDetailsGuest()
{

}

int IntChecker()
{
    int value;

    do
    {
        while (!int.TryParse(Console.ReadLine(), out value))
        {
            ErrorMsg();
        }

        if (value < 0) { Console.WriteLine("Nunber must be postive, and > 0"); Console.Write("Please re-enter input value: "); }

    } while (value < 0);

    return (value);

    void ErrorMsg()
    {
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine($"| Must be of a numeric value! Please try again. |");
        Console.WriteLine("--------------------------------------------------");
        Console.Write("Please re-enter input value: ");
    }
}

void menuShow()
{
    List<string> listforMenu = new List<string>()
    {
        "List All Guests",
        "List All Available Rooms",
        "Register Guest",
        "Check-in Guest",
        "Display stay details of a guest",
        "Extends the stay by numbers of day",
        "Exit"
    };

    Console.WriteLine("------------- MENU -------------");

    for (int i = 0; i < listforMenu.Count; i++)
    {
        if (listforMenu[i].ToUpper().Equals("EXIT"))
        {
            Console.WriteLine($"[{i * 0}] {listforMenu[i]}");
        }
        else
        {
            Console.WriteLine($"[{i + 1}] {listforMenu[i]}");
        }
    }

    Console.WriteLine("--------------------------------");

}

void menuSelection(int numb)
{
    switch (numb)
    {
        case 1:
            DisplayGuests(guestList);
            standardClearingConsole();
            break;
        case 2:
            DisplayAvailRooms(roomList);
            standardClearingConsole();
            break;
        case 3:
            Console.WriteLine("Not done yet");
            standardClearingConsole();
            break;
        case 4:
            CheckInGuest();
            standardClearingConsole();
            break;
        case 5:
            Console.WriteLine("Not done yet");
            standardClearingConsole();
            break;
        case 6:
            ExtendStay();
            standardClearingConsole();
            break;
        case 0:
            Console.WriteLine("Bye");
            break;
        default:
            Console.WriteLine("Please input a number from the options shown!");
            standardClearingConsole();
            break;

    }

    void standardClearingConsole()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to continue..");
        Console.ReadKey();
        Console.Clear();
    }

}

// Main Program
InitStay();
InitRoom();
InitGuest();

int numbr;

try
{
    do
    {
        menuShow();
        Console.Write("Enter your option : ");
        numbr = IntChecker();
        menuSelection(numbr);
    } while (numbr != 0);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

Console.WriteLine("Press any Key to exit....");
Console.ReadKey();

// Basic Feautres:

// Part 1) Done
//DisplayGuests(guestList);

// Part 2) Done
//DisplayAvailRooms(roomList);

// Part 3) Havent Done
//RegisterGuest();

// Part 4) Done
// CheckInGuest();

// Part 5) Havent Done
// DisplayDetailsGuest()

// Part 6) Done
// ExtendStay();

