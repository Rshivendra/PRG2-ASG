using PRG2Assignment;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

// dictionaries:
IDictionary<string, Stay> stayDict = new Dictionary<string, Stay>();
IDictionary<int, Room> roomDict = new Dictionary<int, Room>();


// methods

// this method is used to update the room dictioanry
List<int> UpdateRoomDictFromFile(string filepath, IDictionary<int, Room> roomDict)
{
    List<int> roomNoList = new List<int>();
    roomDict.Clear();

    using (StreamReader reader = new StreamReader(filepath))
    {
        reader.ReadLine();

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();

            string[] parts = line.Split(',');

            int roomNumber = int.Parse(parts[1]);
            string bedConfiguration = parts[2];
            double dailyRate = double.Parse(parts[3]);
            // by default set to true
            bool isAvail = true;

            // Create a new Room object
            Room room;

            roomNoList.Add(roomNumber);

            switch (parts[0].ToUpper())
            {
                case "STANDARD":
                    room = new StandardRoom(roomNumber, bedConfiguration, dailyRate, isAvail);
                    roomDict.Add(roomNumber, room);
                    break;
                case "DELUXE":
                    room = new DeluxeRoom(roomNumber, bedConfiguration, dailyRate, isAvail);
                    roomDict.Add(roomNumber, room);
                    break;
                default:
                    Console.WriteLine("Error when trying to create Room types from room.csv");
                    break;
            }

        }
    }

    return roomNoList;

}

// this method is used to update the current Stay dict object
List<int> UpdateStayDictFromFile(string filepath, IDictionary<string, Stay> stayDict)
{

    List<int> roomNos = UpdateRoomDictFromFile("Rooms.csv", roomDict);
    stayDict.Clear();

    using (StreamReader reader = new StreamReader(filepath))
    {
        reader.ReadLine();

        while (!reader.EndOfStream)
        {
            string[] parts = reader.ReadLine().Split(',');

            string passportNum = parts[1];
            bool guestIsCheckedIn = bool.Parse(parts[2]);
            DateTime checkinDate = DateTime.Parse(parts[3]);
            DateTime checkoutDate = DateTime.Parse(parts[4]);

            Stay stay = new Stay(checkinDate, checkoutDate);

            // run thru the row to check for the roomNos taken by each person and implement each add-on for each room
            for (int i = 0; i < parts.Length; i++)
            {
                int roomNo;

                if (int.TryParse(parts[i], out roomNo))
                {
                    if (guestIsCheckedIn)
                    {
                        // set the room availability to false
                        roomDict[roomNo].IsAvail = false;
                    }

                    switch (roomDict[roomNo])
                    {
                        case StandardRoom:
                            StandardRoom stdroom = (StandardRoom)roomDict[roomNo];
                            stdroom.RequireWifi = bool.Parse(parts[i + 1]);
                            stdroom.RequireBreakfast = bool.Parse(parts[i + 2]);
                            break;

                        case DeluxeRoom:
                            DeluxeRoom dlxroom = (DeluxeRoom)roomDict[roomNo];
                            dlxroom.AdditionalBed = bool.Parse(parts[i + 3]);
                            break;

                        default:
                            Console.WriteLine("Error in updating the addons of the rooms in updateStayDict method!");
                            break;
                    }


                    stay.AddRoom(roomDict[roomNo]);
                }
                else { continue; }
            }

            stayDict.Add(passportNum, stay);
        }
    }

    return roomNos;
}

List<Guest> DisplayAllGuests(string filepath, IDictionary<string, Stay> stayDict,string todisplay = "yes")
{

    List<Guest> guestList = new List<Guest>();
    if(todisplay.ToLower() == "yes")
    {
        Console.WriteLine("Name\t PassPort\t Duration of Stay\t Membership Status\t CheckedIn Status");
    }

    using (StreamReader sr = new StreamReader(filepath))
    {

        sr.ReadLine();


        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();

            string[] parts = line.Split(',');

            // Extract the relevant information from the line
            string name = parts[0];
            string passportNum = parts[1];
            string status = parts[2];
            int points = int.Parse(parts[3]);
            // by default assume its false
            bool? checkedIn = guestCheckedInStatus(passportNum);

            if (checkedIn == null) { Console.WriteLine($"Unable to track {name}'s passportNum {passportNum}"); }


            // Create a new Membership object
            Membership member = new Membership(status, points);

            // Create a new Guest object
            Guest guest = new Guest(name, passportNum, stayDict[passportNum], member);
            guest.IsCheckedIn = checkedIn;

            guestList.Add(guest);

            // Display the information of the guest
            if(todisplay.ToLower() == "yes")
            {
                Console.WriteLine(guest.ToString());
            }
        }
    }

    return guestList;

}

bool? guestCheckedInStatus(string passPortNo)
{
    using (StreamReader sr = new StreamReader("Stays.csv"))
    {
        sr.ReadLine();

        while (!sr.EndOfStream)
        {

            string line = sr.ReadLine();

            string[] parts = line.Split(',');


            if (parts[1].Equals(passPortNo))
            {
                return bool.Parse(parts[2]);
            }
        }

    }

    return null;
}

void DisplayAvailRooms(IDictionary<int, Room> roomDict)
{
    List<int> roomNoList = UpdateStayDictFromFile("Stays.csv", stayDict);

    Console.WriteLine("-------------------------------------------------------------");
    Console.WriteLine("\t\t\tAvailable Rooms");
    Console.WriteLine("-------------------------------------------------------------");

    Console.WriteLine("Room\t Bed Configurations\t DailyRate\t Availability ");

    foreach (int roomNo in roomNoList)
    {

        if (roomDict[roomNo].IsAvail)
        {
            Console.WriteLine(roomDict[roomNo].ToString());
        }
        else { continue; }
    }

}

// NEEDS TESTING
void RegisterGuest()
{
    string name;
    string passportNo;
    Console.WriteLine();
    Console.WriteLine("REGISTER GUESTS SYSTEM");
    Console.WriteLine("----------------------\n");

    Console.Write("Please Enter Guest's Name: ");
    name = Console.ReadLine();
    Console.Write("Please Enter Guest's Passport Number: ");
    passportNo = Console.ReadLine();

    Membership membership = new Membership("Ordinary", 0);
    Guest guest = new Guest(name, passportNo, null, membership);

    // appending guest info to Guests.csv file
    string data = guest.Name + "," + guest.PassportNum + "," + guest.Member.Status + "," + guest.Member.Points;
    using (StreamWriter sw = new StreamWriter("Guests.csv", true))
    {
        sw.WriteLine(data);
    }

    Console.WriteLine();
    Console.WriteLine($"{guest.Name} has been Registered Successfully.");
}

void CheckInGuest()
{
    Console.WriteLine();
    Guest guest;
    Room room;
    bool selectAnotherRoom = true;
    string roomType;
    DateTime checkinDate;
    DateTime checkoutDate;

    DisplayAllGuests("Guests.csv", stayDict);

    Console.WriteLine();
    Console.WriteLine("CHECK IN SYSTEM");
    Console.WriteLine("---------------");

    do
    {
        Console.Write("Please Enter Guest's Passport Number to check in: ");
        string passportNum = Console.ReadLine().ToUpper();
        guest = retrieveGuest(passportNum);
        if (guest == null) { Console.WriteLine("Guest not found!\nGive a valid passport number!"); }


    } while (guest == null || guest.IsCheckedIn != false);

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
        DisplayAvailRooms(roomDict);
        Console.WriteLine();
        do
        {
            Console.Write("Select an Available Room: ");
            room = SearchAvailRoom(IntChecker(), roomDict);
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
        }

    }

    // updating the Stay of the guest
    guest.HotelStay = stay;

    // updating on guest's checkedIn status
    guest.IsCheckedIn = true;

    // need to update Stays.csv to update the room,checkedInStatus there
    // enter some code here

    Console.WriteLine();
    Console.WriteLine($"{guest.Name} has been CheckedIn: {guest.IsCheckedIn}");



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

void DisplayDetailsGuest()
{
    Guest guest;
    Stay stay;
    DisplayAllGuests("Guests.csv",stayDict);
    Console.WriteLine();
    Console.WriteLine("DETAILS OF GUEST");
    Console.WriteLine("----------------");
    Console.WriteLine();
    do
    {

        Console.Write("Please Enter Guest's Passport Number: ");
        string passportNum = Console.ReadLine().ToUpper();
        guest = retrieveGuest(passportNum);
        if (guest == null) { Console.WriteLine("Guest not found!\nGive a valid passport number!"); }
        else if (guest.IsCheckedIn == false) { Console.WriteLine("Please select a guest that is not checked in!"); }

    } while (guest == null || guest.IsCheckedIn == false);

    stay = guest.HotelStay;
    Console.WriteLine();
    Console.WriteLine($"{guest.Name}'s Stay");
    Console.WriteLine($"{RepeatStringForLoop("-", guest.Name.Length)}-------");
    Console.WriteLine(stay.ToString());


    Console.WriteLine();
    Console.WriteLine($"{guest.Name}'s Rooms");
    Console.WriteLine($"{RepeatStringForLoop("-", guest.Name.Length)}--------");

    foreach (Room room in stay.RoomList)
    {
        Console.WriteLine($"{room.RoomNumber}");
    }

}

void ExtendStay()
{
    Guest guest;
    Console.WriteLine();
    DisplayAllGuests("Guests.csv",stayDict);
    Console.WriteLine();
    Console.WriteLine("EXTEND STAY SYSTEM");
    Console.WriteLine("------------------");

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

    // implement updating of file code here

}

Room SearchAvailRoom(int roomNum, IDictionary<int, Room> roomDict)
{
    List<int> roomNoList = UpdateStayDictFromFile("Stays.csv", stayDict);

    foreach (int roomNo in roomNoList)
    {
        if (roomDict[roomNo].IsAvail)
        {
            // check if the roomNum entered by the user matches with the availble rooms in the dictionary
            if(roomNo == roomNum)
            {
                return roomDict[roomNum];
            }
        }
        else { continue; }
    }

    return null;
}

Guest retrieveGuest(string passportNum)
{
    List<Guest> guestList = DisplayAllGuests("Guests.csv", stayDict,"no");

    foreach (Guest guest in guestList)
    {
        if (passportNum.Equals(guest.PassportNum))
        {
            return guest;
        }
    }

    return null;
}


string RepeatStringForLoop(string s, int n)
{
    var result = s;

    for (var i = 0; i < n - 1; i++)
    {
        result += s;
    }

    return result;
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
            DisplayAllGuests("Guests.csv", stayDict);
            standardClearingConsole();
            break;
        case 2:
            DisplayAvailRooms(roomDict);
            standardClearingConsole();
            break;
        case 3:
            RegisterGuest();
            standardClearingConsole();
            break;
        case 4:
            CheckInGuest();
            standardClearingConsole();
            break;
        case 5:
            DisplayDetailsGuest();
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


// main program
int numbr;
try
{
    do
    {
        UpdateStayDictFromFile("Stays.csv", stayDict);
        menuShow();
        Console.Write("Enter your option : ");
        numbr = IntChecker();
        menuSelection(numbr);
    } while (numbr != 0);
} catch(Exception ex)
{
    Console.WriteLine(ex.Message);
}



Console.WriteLine("Press any Key to exit....");
Console.ReadKey();


