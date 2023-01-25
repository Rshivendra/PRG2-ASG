using PRG2Assignment;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;

// connections
string staysConn = @"C:\Polytechnic_Year1\Polytechnic_sem_2\PRG_2\Assignment\Stays.csv";
string guestsConn = @"Guests.csv";
string roomsConn = @"Rooms.csv";
string pattern = @"^[A-Z]\d{7}[A-Z]$";

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
List<int> UpdateStayDictFromFile(string filepath, IDictionary<string, Stay> stayDict, DateTime? currentDate, Stay? rangeOfStay)
{

    List<int> roomNos = UpdateRoomDictFromFile(roomsConn, roomDict);
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
            DateTime checkoutDate = DateTime.Parse(parts[4]).Date;

            Stay stay = new Stay(checkinDate, checkoutDate);

            // run thru the row to check for the roomNos taken by each person and implement each add-on for each room
            for (int i = 0; i < parts.Length; i++)
            {
                int roomNo;

                if (int.TryParse(parts[i], out roomNo))
                {

                    // check if need to use current date or a range of dates
                    switch (currentDate)
                    {
                        case null:
                            switch (guestIsCheckedIn)
                            {
                                case false:
                                    break;
                                case true:
                                    if (((rangeOfStay.CheckinDate.Date >= checkinDate && rangeOfStay.CheckinDate.Date <= checkoutDate) || (rangeOfStay.CheckoutDate.Date >= checkinDate && rangeOfStay.CheckoutDate.Date <= checkoutDate)) || (checkinDate >= rangeOfStay.CheckinDate.Date && checkoutDate <= rangeOfStay.CheckoutDate.Date))
                                    {
                                        //Console.WriteLine($"room number: {roomNo} {checkinDate} {checkoutDate} user checkin date: {rangeOfStay.CheckinDate.Date} occupied");
                                        roomDict[roomNo].IsAvail = false;
                                        //Console.ReadKey();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    break;
                                default:
                                    Console.WriteLine("Error when using switch case for guestisCheckedIn at updateStayDict");
                                    break;


                            }
                            break;

                        // if guest is checkedIn and is not within the checkIn and checkOut date range
                        case not null:

                            if (guestIsCheckedIn && !((currentDate >= checkinDate) && (currentDate <= checkoutDate)))
                            {
                                // set the room availability to true
                                roomDict[roomNo].IsAvail = true;
                            }
                            else if ((currentDate >= checkinDate) && (currentDate <= checkoutDate))
                            {
                                // else set the room availability to false
                                roomDict[roomNo].IsAvail = false;
                            }
                            break;
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

            stayDict.Add(passportNum+"-"+checkinDate.ToShortDateString()+"_"+checkoutDate.ToShortDateString(), stay);
        }
    }

    return roomNos;
}

List<Guest> DisplayAllGuests(string filepath, IDictionary<string, Stay> stayDict,string todisplay = "yes")
{

    List<Guest> guestList = new List<Guest>();
    if(todisplay.ToLower() == "yes")
    {
        Console.WriteLine("Name\t PassPort\t Membership Status\t Points\t\t Check-in Status");
    }

    using (StreamReader sr = new StreamReader(filepath))
    {

        sr.ReadLine();

        while (!sr.EndOfStream)
        {
            int guestCounter = 0;
            string line = sr.ReadLine();

            string[] parts = line.Split(',');

            // Extract the relevant information from the line
            string name = parts[0];
            string passportNum = parts[1];
            string status = parts[2];
            int points = int.Parse(parts[3]);
            bool? checkedInStatus = false;
            // Create a new Membership object
            Membership member = new Membership(status, points);

            List<string> guestsStays = SearchForCheckInCheckOutDates(staysConn, passportNum);

            // Create a new Guest object
            foreach (string stayDuration in guestsStays)
            {
                string guestKey = passportNum + "-" + stayDuration;
                checkedInStatus = searchForCheckedInStatus(staysConn, guestKey);

                Guest guest = new Guest(name, passportNum, stayDict[passportNum+"-"+stayDuration], member);
                guest.IsCheckedIn = checkedInStatus;
                guestList.Add(guest);

                // Display the information of the guest
                if (todisplay.ToLower() == "yes" && guestCounter == 0)
                {
                    Console.WriteLine(guest.ToString());
                }
                guestCounter++;
            }

        }
    }
    return guestList;
}

List<string> SearchForCheckInCheckOutDates(string filepath, string passPortNum)
{

    List<string> differentCheckOutPeriodForSameGuestList = new List<string>();

    using (StreamReader reader = new StreamReader(filepath))
    {
        reader.ReadLine();

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();

            string[] parts = line.Split(',');

            if (parts[1].Equals(passPortNum))
            {
                differentCheckOutPeriodForSameGuestList.Add(parts[3] +"_"+ parts[4]);
            }

        }
    }

    return differentCheckOutPeriodForSameGuestList;

}

bool? searchForCheckedInStatus(string filepath, string guestKey)
{
    using (StreamReader reader = new StreamReader(filepath))
    {
        reader.ReadLine();

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();

            string[] parts = line.Split(',');

            string fileGuestKey = parts[1] + "-" + guestKey.Split("-")[1];

            if (fileGuestKey.Equals(guestKey))
            {
                return bool.Parse(parts[2]);
            }

        }
    }

    return null;
}


void DisplayAvailRooms(IDictionary<int, Room> roomDict, Stay? rangeOfStay, DateTime dateToBeChecked)
{
    List<int> roomNoList = new List<int>();

    // if statement to determine whether need to display rooms based on current date or in a range of dates
    switch (rangeOfStay)
    {
        case null:
            roomNoList = UpdateStayDictFromFile(staysConn, stayDict, DateTime.Now, null);
            break;
        case not null:
            roomNoList = UpdateStayDictFromFile(staysConn, stayDict, null, rangeOfStay);
            break;
        default:
            Console.WriteLine("An error has occured! at displaying Available Rooms");
            break;
    }

    Console.WriteLine("-------------------------------------------------------------");
    Console.WriteLine($"\tAvailable Rooms as of {dateToBeChecked.ToShortDateString()}");
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
    List<Guest> guestList = DisplayAllGuests(guestsConn, stayDict, "no");
    string name;
    string passportNo;
    bool? similarityBetweenPassports;
    Console.WriteLine();
    Console.WriteLine("REGISTER GUESTS SYSTEM");
    Console.WriteLine("----------------------\n");

    Console.Write("Please Enter Guest's Name: ");
    name = Console.ReadLine();
    // check if passport inputted is of correct format
    do
    {
        Console.Write("Please Enter Guest's Passport Number: ");
        passportNo = Console.ReadLine().ToUpper();
        similarityBetweenPassports = similarPassportChecker(passportNo);
        if (!Regex.IsMatch(passportNo, pattern)) { Console.WriteLine("Passport number is not valid"); }
    } while (!Regex.IsMatch(passportNo, pattern) || similarityBetweenPassports == true);


    Membership membership = new Membership("Ordinary", 0);
    Guest guest = new Guest(name, passportNo, null, membership);

    // appending guest info to Guests.csv file
    string data = guest.Name + "," + guest.PassportNum + "," + guest.Member.Status + "," + guest.Member.Points;
    using (StreamWriter sw = new StreamWriter(guestsConn, true))
    {
        sw.WriteLine(data);
    }

    Console.WriteLine();
    Console.WriteLine($"{guest.Name} has been Registered Successfully.");

    bool? similarPassportChecker(string userInputPassportNumber)
    {
        foreach(Guest guest in guestList)
        {
            if (guest.PassportNum.Equals(userInputPassportNumber))
            {
                Console.WriteLine($"Inputted passport number {userInputPassportNumber} already exists...");
                return true;
            }
        }

        return null;
    }


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

    DisplayAllGuests(guestsConn, stayDict);

    Console.WriteLine();
    Console.WriteLine("CHECK IN SYSTEM");
    Console.WriteLine("---------------");

    do
    {
        Console.Write("Please Enter Guest's Passport Number to check in: ");
        string passportNum = Console.ReadLine().ToUpper();
        guest = retrieveGuest(passportNum);
        if (guest == null) { Console.WriteLine("Guest not found! Give a valid passport number!"); }
        else if (guest.IsCheckedIn == true) { Console.WriteLine("Please select a guest that is not checkedin!"); }
    } while (guest == null || guest.IsCheckedIn == true);

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
        DisplayAvailRooms(roomDict, stay,stay.CheckinDate);
        Console.WriteLine();
        do
        {
            Console.Write("Select an Available Room: ");
            room = SearchAvailRoom(IntChecker(), roomDict,stay);
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
    int counter = 0;
    Guest guest;
    DisplayAllGuests(guestsConn,stayDict);
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

    } while (guest == null);

    Console.WriteLine();
    Console.WriteLine($"{guest.Name}'s Details");
    Console.WriteLine($"{RepeatStringForLoop("-", guest.Name.Length)}----------");
    Console.WriteLine();
    Console.WriteLine($"No\t Room\t\tStay Duration\t\t\t Number of Days");

    foreach (string key in stayDict.Keys)
    {
        // key has passportNum-checkinDate_checkoutDate
        // keyArray[0] means passportNum and keyArray[1] means checkinDate_checkoutDate
        string[] keyArray = key.Split("-");

        if (keyArray[0].Equals(guest.PassportNum))
        {
            displayRoomDetails(keyArray[1]);
        }

    }


    void displayRoomDetails(string? stayDate)
    {
        
        string[] stayDatesArray = stayDate.Split("_");
        Stay stay = stayDict[guest.PassportNum + "-" + stayDatesArray[0] + "_" + stayDatesArray[1]];

        if (stay.RoomList.Count > 0)
        {
            foreach (Room room in stay.RoomList)
            {
                counter++;
                Console.WriteLine($"{counter})\t {room.RoomNumber}\t{stay}");
            }
        }
        else
        {
            Console.WriteLine($"{guest.Name} has no rooms checked in.");
        }
    }

}

// need to modify
void ExtendStay()
{
    Guest guest;
    Console.WriteLine();
    DisplayAllGuests("Guests.csv", stayDict);
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

    // check if the same room is available when extended
    bool? roomsAvailableToExtend = isRoomAvailAbleForExtensions(guest.HotelStay.RoomList, roomDict, guest.HotelStay,noOfDays);

    if (roomsAvailableToExtend == true)
    {
        Console.WriteLine("Your rooms are available for extension.");
        DateTime newCheckoutDate = guest.HotelStay.CheckoutDate.AddDays(noOfDays);
        guest.HotelStay.CheckoutDate = newCheckoutDate;

        Console.WriteLine($"{guest.Name}'s stay has been extended.");
        Console.WriteLine();
        Console.WriteLine("------------------------------------------------------");
        Console.WriteLine($"New Checkout Date: {guest.HotelStay.CheckoutDate}");
        Console.WriteLine("------------------------------------------------------");
        Console.WriteLine();
    }
    else
    {
        Console.WriteLine("Your rooms are not available for extension.");
    }


    // implement updating of file code here

}


Room SearchAvailRoom(int roomNum, IDictionary<int, Room> roomDict, Stay? rangeOfStay)
{
    List<int> roomNoList = new List<int>();

    // if statement to determine whether need to display rooms based on current date or in a range of dates
    switch (rangeOfStay)
    {
        case null:
            roomNoList = UpdateStayDictFromFile(staysConn, stayDict, DateTime.Now, null);
            break;
        case not null:
            roomNoList = UpdateStayDictFromFile(staysConn, stayDict, null, rangeOfStay);
            break;
    }

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

bool? isRoomAvailAbleForExtensions(List<Room> roomlist, IDictionary<int, Room> roomDict, Stay? rangeOfStay, int noOfDaysToExtend)
{
    List<int> roomNoList = new List<int>();
    List<bool> allRoomsAvailableList = new List<bool>();
    DateTime newCheckoutDate = rangeOfStay.CheckoutDate.AddDays(noOfDaysToExtend);
    rangeOfStay.CheckoutDate = newCheckoutDate;

    Console.WriteLine(rangeOfStay.CheckoutDate);

    // if statement to determine whether need to display rooms based on current date or in a range of dates
    switch (rangeOfStay)
    {
        case null:
            roomNoList = UpdateStayDictFromFile(staysConn, stayDict, DateTime.Now, null);
            break;
        case not null:
            roomNoList = UpdateStayDictFromFile(staysConn, stayDict, null, rangeOfStay);
            break;
    }

    foreach (int roomNo in roomNoList)
    {
        if (roomDict[roomNo].IsAvail)
        {
            // check if the roomNum entered by the user matches with the availble rooms in the dictionary
            foreach(Room room in roomlist)
            {
                if (roomNo == room.RoomNumber)
                {
                    allRoomsAvailableList.Add(true);
                }
            }

        }
        else { continue; }
    }

    if(roomlist.Count == allRoomsAvailableList.Count)
    {
        if (allRoomsAvailableList.Contains(true))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    return null;
}

Guest retrieveGuest(string passportNum,bool searchBasedOnCheckInStatus = false)
{
    List<Guest> guestList = DisplayAllGuests(guestsConn, stayDict,"no");

    foreach (Guest guest in guestList)
    {
        switch (searchBasedOnCheckInStatus)
        {
            case false:
                if (passportNum.Equals(guest.PassportNum))
                {
                    return guest;
                }
                break;
            case true:
                if (passportNum.Equals(guest.PassportNum) && guest.IsCheckedIn == true)
                {
                    return guest;
                }
                break;
            default:
                Console.WriteLine("Error at retrieveGuest method!");
                break;
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
            DisplayAllGuests(guestsConn, stayDict);
            standardClearingConsole();
            break;
        case 2:
            DisplayAvailRooms(roomDict,null,DateTime.Now);
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
            Console.WriteLine("System exited..");
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
        UpdateStayDictFromFile(staysConn, stayDict, DateTime.Now,null);
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


