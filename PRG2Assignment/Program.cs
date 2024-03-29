﻿using PRG2Assignment;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;
using System.Text;

// connections
string staysConn = @"Stays.csv";
string guestsConn = @"Guests.csv";
string roomsConn = @"Rooms.csv";
string archiveConn = @"StayArchive.csv";
string pattern = @"^[A-Z]\d{7}[A-Z]$";

// dictionaries:
IDictionary<string, Stay> stayDict = new Dictionary<string, Stay>();
IDictionary<int, Room> roomDict = new Dictionary<int, Room>();

// list
List <Room> availableRoomList = new List<Room>();   

// methods

// this method is used to update the room dictioanry
List<int> InitRoom(string filepath, IDictionary<int, Room> roomDict)
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
List<int> UpdateStayDictFromFile(string filepath, IDictionary<string, Stay> stayDict, DateTime? currentDate, Stay? rangeOfStay, Guest guestToBeIgnored = null)
{

    List<int> roomNos = InitRoom(roomsConn, roomDict);
    stayDict.Clear();

    using (StreamReader reader = new StreamReader(filepath))
    {
        reader.ReadLine();

        while (!reader.EndOfStream)
        {
            string[] parts = reader.ReadLine().Split(',');
            string passportNum = parts[1];

            switch (guestToBeIgnored)
            {
                case not null:
                    // skip the guest to be ignored
                    if (passportNum.Equals(guestToBeIgnored.PassportNum))
                    {
                        continue;
                    }
                    break;
                case null:
                    break;
            }
            
            if (parts.Length > 3)
            {
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

                stayDict.Add(passportNum, stay);
            }
            else
            {
                Stay stay = new Stay();
                stayDict.Add(passportNum, stay);
            } 
        }
    }

    // adding all the rooms from Stay.csv
    return roomNos;
}

List<Guest> DisplayAllGuests(string filepath, IDictionary<string, Stay> stayDict,string todisplay = "yes")
{

    List<Guest> guestList = new List<Guest>();
    if(todisplay.ToLower() == "yes")
    {
        Console.WriteLine("Name\t Passport\t Membership Status\t Points\t\t Check-in Status");
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

            // Create a new Guest object
            checkedInStatus = searchForCheckedInStatus(staysConn, passportNum);
            Guest guest = new Guest(name, passportNum, stayDict[passportNum], member);

            //Console.WriteLine($"{guest.Name} has {stayDict[passportNum].RoomList.Count} rooms");

            guest.IsCheckedIn = checkedInStatus;
            guestList.Add(guest);

            if (todisplay.ToLower() == "yes")
            {
                Console.WriteLine(guest);
            }
        }
    }
    return guestList;
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

            string fileGuestKey = parts[1];

            if (fileGuestKey.Equals(guestKey))
            {
                return bool.Parse(parts[2]);
            }

        }
    }
    return null;
}

void DisplayAvailRooms(IDictionary<int, Room> roomDict, Stay? rangeOfStay, DateTime dateToBeChecked, bool todisplay = true, Guest guestobeIgnored = null)
{
    List<int> roomNoList = new List<int>();

    // if statement to determine whether need to display rooms based on current date or in a range of dates
    switch (rangeOfStay)
    {
        case null:
            roomNoList = UpdateStayDictFromFile(staysConn, stayDict, DateTime.Now, null);
            break;
        case not null:
            switch (guestobeIgnored)
            {
                case null:
                    roomNoList = UpdateStayDictFromFile(staysConn, stayDict, null, rangeOfStay);
                    break;
                case not null:
                    roomNoList = UpdateStayDictFromFile(staysConn, stayDict, null, rangeOfStay,guestobeIgnored);
                    break;
            }
            break;
        default:
            Console.WriteLine("An error has occured! at displaying Available Rooms");
            break;
    }

    if (todisplay)
    {
        Console.WriteLine("-------------------------------------------------------------");
        Console.WriteLine($"\tAvailable Rooms as of {dateToBeChecked.ToShortDateString()}");
        Console.WriteLine("-------------------------------------------------------------");

        Console.WriteLine("Room Type\t Room\t Bed Configurations\t DailyRate\t Availability ");
    }

    foreach (int roomNo in roomNoList)
    {

        if (roomDict[roomNo].IsAvail)
        {
            availableRoomList.Add(roomDict[roomNo]);
            if (todisplay) { Console.WriteLine(roomDict[roomNo].ToString()); }
        }
        else { continue; }
    }

    if(availableRoomList.Count == 0)
    {
        Console.WriteLine("There are no available rooms Sorry!");
    }

}

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
    // check if passport inputed is of correct format
    do
    {
        Console.Write("Please Enter Guest's Passport Number: ");
        passportNo = Console.ReadLine().ToUpper();
        similarityBetweenPassports = similarPassportChecker(passportNo);
        if (!Regex.IsMatch(passportNo, pattern)) { Console.WriteLine("Please input the passport number in the following format.\ne.g. S4236232Q"); }
    } while (!Regex.IsMatch(passportNo, pattern) || similarityBetweenPassports == true);


    Membership membership = new Membership("Ordinary", 0);
    Guest guest = new Guest(name, passportNo, null, membership);

    // appending guest info to Guests.csv file
    string data = guest.Name + "," + guest.PassportNum + "," + guest.Member.Status + "," + guest.Member.Points;
    using (StreamWriter sw = new StreamWriter(guestsConn, true))
    {
        sw.WriteLine(data);
    }

    // appending guest info to Stays.csv file
    data = guest.Name + "," + guest.PassportNum + "," + guest.IsCheckedIn;
    using (StreamWriter sw = new StreamWriter(staysConn, true))
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
    int numbrOfCheckInGuests = 0;
    int numberOfRoomsAdded = 0;
    DateTime checkinDate;
    DateTime checkoutDate;
    

    List<Guest> guestList = DisplayAllGuests(guestsConn, stayDict);

    foreach (Guest guestToBeCheckedIfCheckedOut in guestList)
    {
        if (guestToBeCheckedIfCheckedOut.IsCheckedIn == true) { numbrOfCheckInGuests++; }
    }
    if(numbrOfCheckInGuests != guestList.Count)
    {
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
        // clone the guest's current stay object before modifying
        Stay guestsOldStayObject = (Stay)guest.HotelStay.Clone();
        do
        {
            Console.Write("Enter Checkin Date (dd/MM/yyyy): ");
            checkinDate = exactDate();

            if (checkinDate < DateTime.Now)
            {
                Console.WriteLine("The check in date must not be later than today.");
            }

        } while (checkinDate < DateTime.Now);

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
        guest.HotelStay.RoomList.Clear();
        while (selectAnotherRoom)
        {
            availableRoomList.Clear();
            DisplayAvailRooms(roomDict, stay, stay.CheckinDate);
            // check to see if there are any available rooms
            if(availableRoomList.Count > 0)
            {
                Console.WriteLine();
                do
                {
                    Console.Write("Select an Available Room: ");
                    room = SearchAvailRoom(IntChecker(), roomDict, stay);
                    if (room == null || room.IsAvail == false) { Console.WriteLine("Room not found!"); }
                } while (room == null || room.IsAvail == false);

                // updating the availability
                room.IsAvail = false;
                guest.HotelStay = stay;
                guest.HotelStay.AddRoom(room);

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

                guest.IsCheckedIn = true;
                // check if the number of rooms being added is 0 or the dates are empty.
                if (numberOfRoomsAdded == 0)
                {
                    // check if the room objects are empty
                    if (guestsOldStayObject.CheckinDate == DateTime.MinValue || guestsOldStayObject.CheckoutDate == DateTime.MinValue)
                    {
                        // if it is delete the old data and override it, and dont add it to archive
                        startFileWritingProcess(guest, guestList, false);
                    }
                    // check if the roomlist contains no rooms
                    else if (guestsOldStayObject.RoomList.Count == 0)
                    {
                        // if it is delete the old data and override it, and dont add it to archive
                        startFileWritingProcess(guest, guestList, false);
                    }
                    else
                    {
                        //if it is delete the old data and override it, and dont add it to archive
                        startFileWritingProcess(guest, guestList);
                    }
                    numberOfRoomsAdded++;
                }
                else
                {
                    startFileWritingProcess(guest, guestList, false);
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
                    Console.WriteLine();
                    Console.WriteLine($"{guest.Name} has been CheckedIn: {guest.IsCheckedIn}");
                }
            }
            else { break; }
        
        }

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
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------------------------------------------");
        Console.WriteLine("All guests are checked in! sorry unable to check in a guest");
        Console.WriteLine("-----------------------------------------------------------");
    }

}

void CheckOutGuest()
{
    Console.WriteLine();
    Guest guest;
    double totalCost = 0;
    DateTime checkinDate;
    DateTime checkoutDate;
    int numbrOfCheckOutGuests = 0;

    List<Guest> guestList = DisplayAllGuests(guestsConn, stayDict);

    foreach(Guest guestToBeCheckedIfCheckedOut in guestList)
    {
        if(guestToBeCheckedIfCheckedOut.IsCheckedIn == false) { numbrOfCheckOutGuests++; }
    }
    if(numbrOfCheckOutGuests != guestList.Count)
    {
        Console.WriteLine();
        Console.WriteLine("CHECK OUT SYSTEM");
        Console.WriteLine("---------------");

        do
        {
            Console.Write("Please Enter Guest's Passport Number to check out: ");
            string passportNum = Console.ReadLine().ToUpper();
            guest = retrieveGuest(passportNum);
            if (guest == null) { Console.WriteLine("Guest not found! Give a valid passport number!"); }
            else if (guest.IsCheckedIn == false) { Console.WriteLine("Please select a guest that is checkedin!"); }
        } while (guest == null || guest.IsCheckedIn == false);

        totalCost = guest.HotelStay.CalculateTotal();
        Console.WriteLine($"\nTotal Amount: {totalCost:C2}");

        Console.WriteLine();
        Console.WriteLine($"{guest.Name}'s Details");
        Console.WriteLine($"{RepeatStringForLoop("-", guest.Name.Length)}----------");
        Console.WriteLine($"Status - {guest.Member.Status}");
        Console.WriteLine($"Points - {guest.Member.Points}");

        if (guest.Member.Status.ToUpper() == "GOLD" || guest.Member.Status.ToUpper() == "SILVER")
        {
            Console.WriteLine();
            Console.WriteLine($"Since you are a {guest.Member.Status} member, you can redeem points.");
            Console.Write("Would you like to redeem your points? [Y/N]: ");
            string acceptRedeem = Console.ReadLine();
            while (!ValidateInput(acceptRedeem))
            {
                Console.WriteLine("Invalid input! Please enter Y for Yes or N for No.");
                Console.Write("Would you like to redeem your points? [Y/N]: ");
                acceptRedeem = Console.ReadLine();
            }

            if (acceptRedeem.ToUpper() == "Y")
            {
                int redeemPoints = 0;
                bool successfulRedeem = false;

                do
                {
                    Console.WriteLine();
                    Console.Write("How many points would you like to redeem: ");
                    redeemPoints = IntChecker();
                    successfulRedeem = guest.Member.RedeemPoints(redeemPoints);
                    if (successfulRedeem == false) { Console.WriteLine("You do not have sufficent points.\nPlease try again."); }
                } while (successfulRedeem == false);

                int offsetCost = redeemPoints;
                totalCost = totalCost - offsetCost;
                Console.WriteLine();
                Console.WriteLine($"Final Total Amount: {totalCost:C2}");
            }
        }

        Console.WriteLine("Press any key to make payment.");
        Console.ReadKey();
        Console.WriteLine();
        Console.WriteLine("Transaction is successful.");

        guest.IsCheckedIn = false;
        guest.Member.EarnPoints(totalCost);

        Console.WriteLine();
        Console.WriteLine($"{guest.Name}'s membership has been updated.");

        if (guest.Member.Status.ToUpper() != "ORDINARY")
        {
            Console.WriteLine($"{RepeatStringForLoop("-", guest.Name.Length)}----------");
            Console.WriteLine($"Status - {guest.Member.Status}");
            Console.WriteLine($"Points - {guest.Member.Points}");
        }

        startFileWritingProcess(guest, guestList,false);

        using (StreamReader reader = new StreamReader(guestsConn))
        using (StreamWriter writer = File.CreateText("newfile.csv"))
        {
            reader.ReadLine();
            writer.WriteLine("Name,PassportNumber,MembershipStatus,MembershipPoints");
            while (!reader.EndOfStream)
            {
                StringBuilder sb = new StringBuilder();
                string line = reader.ReadLine();
                string[] parts = line.Split(',');
                if (parts[1].Equals(guest.PassportNum))
                {
                    string data = parts[0] + "," + parts[1] + "," + guest.Member.Status + "," + guest.Member.Points;
                    writer.WriteLine(data);
                }
                else
                {
                    for (int i = 0; i < parts.Length; i++)
                    {
                        sb.Append($"{parts[i]},");
                    }

                    writer.WriteLine(sb.ToString().TrimEnd(','));
                }
            }
        }
        File.Delete(guestsConn);
        File.Move("newfile.csv", guestsConn);

    }
    else
    {
        Console.WriteLine();
        Console.WriteLine("---------------------------------");
        Console.WriteLine("Sorry all guests are checked out!");
        Console.WriteLine("---------------------------------");
    }

}

void startFileWritingProcess(Guest guest, List<Guest> guestlist,bool toDeleteRow = true)
{
    List<string> columnsToReiterate = new List<string>();
    columnsToReiterate.Add(",RoomNumber,Wifi,Breakfast,ExtraBed");

    using (StreamReader reader = new StreamReader(staysConn))
    using (StreamWriter writer = File.CreateText("newfile.csv"))
    {
        int highestNumForHeaderPrintingCounter = guest.HotelStay.RoomList.Count;
        string headers = "Name,PassportNumber,IsCheckedIn,CheckinDate,CheckoutDate";

        foreach(Guest guestFromList in guestlist)
        {
            if(guestFromList.HotelStay.RoomList.Count > highestNumForHeaderPrintingCounter)
            {
                highestNumForHeaderPrintingCounter = guestFromList.HotelStay.RoomList.Count;
            }
            else
            {
                continue;
            }
        }

        // printing of the columns
        for(int i = 0; i < highestNumForHeaderPrintingCounter; i++)
        {
            foreach(string columns in columnsToReiterate)
            {
                headers += columns;
            }
        }
       
        reader.ReadLine();
        writer.WriteLine(headers);

        while (!reader.EndOfStream)
        {
            StringBuilder sb = new StringBuilder();
            string line = reader.ReadLine();
            string[] parts = line.Split(',');

            if (parts[1].Equals(guest.PassportNum))
            {
                string data = parts[0] + "," + parts[1] + "," + guest.IsCheckedIn + "," + guest.HotelStay.CheckinDate.ToShortDateString() + "," + guest.HotelStay.CheckoutDate.ToShortDateString(); ;

                foreach (Room room in guest.HotelStay.RoomList)
                {
                    switch (room)
                    {
                        case StandardRoom:
                            StandardRoom standardRoom = room as StandardRoom;
                            data += "," + standardRoom.RoomNumber + "," + standardRoom.RequireWifi + "," + standardRoom.RequireBreakfast + "," + false;
                            break;
                        case DeluxeRoom:
                            DeluxeRoom deluxeRoom = room as DeluxeRoom;
                            data += "," + deluxeRoom.RoomNumber + "," + false + "," + false + "," + deluxeRoom.AdditionalBed;
                            break;
                        default:
                            Console.WriteLine("Error @ file writing process");
                            break;
                    }
                }

                // appending previous data to the archive dataset
                if (toDeleteRow)
                {
                    using (StreamWriter archiveWriter = new StreamWriter(archiveConn, true))
                    {

                        for (int i = 0; i < parts.Length; i++)
                        {
                            sb.Append($"{parts[i]},");
                        }

                        archiveWriter.WriteLine(sb.ToString().TrimEnd(','));
                    }
                }

                writer.WriteLine(data);
            }
            else
            {
                // code to input orignal data
                for (int i = 0; i < parts.Length; i++)
                {
                    sb.Append($"{parts[i]},");
                }
                writer.WriteLine(sb.ToString().TrimEnd(','));
            }

        }
    }
    File.Delete(staysConn);
    File.Move("newfile.csv", staysConn);
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
    Console.WriteLine($"No\t Room\t\tStay Duration\t\t\t Number of Days");

    if (stayDict.ContainsKey(guest.PassportNum))
    {
        Stay stay = stayDict[guest.PassportNum];
        
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

void DisplayMonthlyBreakdown()
{
    List <Guest> guestList = DisplayAllGuests(guestsConn, stayDict, "no");

    int year = 0;
    double totalCharges = 0;
    IDictionary<string, double> monthlyCharges = new Dictionary<string, double>();

    Console.WriteLine("MONTHLY CHARGES");
    Console.WriteLine("---------------");
    Console.WriteLine();
    do
    {
        Console.Write("Enter the year: ");
        year = IntChecker();
        if (year < 2000 || year > 2500) { Console.WriteLine("Please input a year after 2000.\n"); }
    } while (year < 2000 || year > 2500);

    for (int i = 0; i < 12; i++)
    {
        monthlyCharges.Add(new DateTime(year, i + 1, 1).ToString("MMM"), 0);
    }

    foreach (Guest guest in guestList)
    {
        if (guest.HotelStay.CheckoutDate.Year == year)
        {
            if (monthlyCharges.ContainsKey(guest.HotelStay.CheckoutDate.ToString("MMM")))
            {
                monthlyCharges[guest.HotelStay.CheckoutDate.ToString("MMM")] += guest.HotelStay.CalculateTotal();
            }

            totalCharges += guest.HotelStay.CalculateTotal();
        }
    }

    if (new FileInfo(archiveConn).Length != 0)
    {
        LoadingPreviousChargesFromArchiveFileIntoMonthlyChargesDictionary();
    }
    
    foreach (KeyValuePair<string,double> kvp in monthlyCharges)
    {
        Console.WriteLine($"{kvp.Key} {year}:  ${kvp.Value}");
    }

    Console.WriteLine();
    Console.WriteLine($"Total:  ${totalCharges}");


    // method to get the charges from the archive file
    void LoadingPreviousChargesFromArchiveFileIntoMonthlyChargesDictionary()
    {
        List<Guest> archiveGuestList = InitArchive(archiveConn);

        foreach (Guest Archiveguest in archiveGuestList)
        {
            if (Archiveguest.HotelStay.CheckoutDate.Year == year)
            {
                if (monthlyCharges.ContainsKey(Archiveguest.HotelStay.CheckoutDate.ToString("MMM")))
                {
                    monthlyCharges[Archiveguest.HotelStay.CheckoutDate.ToString("MMM")] += Archiveguest.HotelStay.CalculateTotal();
                }

                totalCharges += Archiveguest.HotelStay.CalculateTotal();
            }
        }

    }

}

List<Guest> InitArchive(string filepath)
{
    List<Guest> archiveGuestList = new List<Guest>();

    using (StreamReader reader = new StreamReader(archiveConn))
    {
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            string[] parts = line.Split(',');

            string name = parts[0];
            string passportNum = parts[1];
            Stay stay = new Stay(DateTime.Parse(parts[3]), DateTime.Parse(parts[4]));

            for (int i = 0; i < parts.Length; i++)
            {
                int roomNo;

                if (int.TryParse(parts[i], out roomNo))
                {
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
                else {continue;}
            }

            Membership memebrship = null;
            Guest guest = new Guest(name,passportNum,stay,memebrship);
            archiveGuestList.Add(guest);
        }
    }
    return archiveGuestList;
}

// need to modify
void ExtendStay()
{
    Guest guest;
    List<Room?> guestsRoomsThatAreAvail = new List<Room?>();
    List<Guest> guestList = DisplayAllGuests(guestsConn, stayDict, "no");
    Console.WriteLine();
    DisplayAllGuests("Guests.csv", stayDict);
    Console.WriteLine();
    Console.WriteLine("EXTEND STAY SYSTEM");
    Console.WriteLine("------------------");

    do
    {
        Console.Write("Please Enter Guest's Passport Number for Extension: ");
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

    Stay clonedStayObject = (Stay)guest.HotelStay.Clone();

    // check if the room is available or not
    DateTime ClonednewCheckoutDate = clonedStayObject.CheckoutDate.AddDays(noOfDays);
    clonedStayObject.CheckoutDate = ClonednewCheckoutDate;

    DisplayAvailRooms(roomDict, clonedStayObject, clonedStayObject.CheckinDate, false, guest);
    foreach (Room roomOfGuest in guest.HotelStay.RoomList)
    {
        guestsRoomsThatAreAvail.Add(SearchAvailRoom(roomOfGuest.RoomNumber, roomDict, clonedStayObject));
    }

    int OccurencesOfNonAvailRooms = guestsRoomsThatAreAvail.Count(x => x == null);
    // refresh the dictionaries
    DisplayAvailRooms(roomDict, clonedStayObject, clonedStayObject.CheckinDate, false);
    // if the number of rooms that are not available is between 0 and the length of the guestsRoomsThatAreAvail list
    if (OccurencesOfNonAvailRooms <= guestsRoomsThatAreAvail.Count && OccurencesOfNonAvailRooms > 0)
    {
        Console.WriteLine();
        Console.WriteLine($"Sorry your rooms are not available at this checkout date {ClonednewCheckoutDate}");
        Console.WriteLine();
    }
    else
    {
        DateTime guestsNewCheckOutDate = guest.HotelStay.CheckoutDate.AddDays(noOfDays);
        guest.HotelStay.CheckoutDate = guestsNewCheckOutDate;
        Console.WriteLine($"{guest.Name}'s stay has been extended.");
        Console.WriteLine();
        Console.WriteLine("------------------------------------------------------");
        Console.WriteLine($"New Checkout Date: {guest.HotelStay.CheckoutDate}");
        Console.WriteLine("------------------------------------------------------");
        Console.WriteLine();
        startFileWritingProcess(guest, guestList, false);
    }
}

void CancelStay()
{
    Guest guest;
    Room cancelroom;
    int counter = 0;
    bool cancelAnotherRoom = true;
    int noOfRoomsCancelled = 0;
    Console.WriteLine();
    List<Guest> guestList = DisplayAllGuests("Guests.csv", stayDict);
    Console.WriteLine();
    Console.WriteLine("CANCEL ROOM SYSTEM");
    Console.WriteLine("------------------");

    do
    {
        Console.Write("Please Enter Guest's Passport Number to Cancel Room: ");
        string passportNum = Console.ReadLine().ToUpper();
        guest = retrieveGuest(passportNum);
        if (guest == null) { Console.WriteLine("Guest not found!\nGive a valid passport number!"); }
        else if (guest.IsCheckedIn == false) { Console.WriteLine("Please select a Guest that is checked in"); }
        else if (DateTime.Now >= guest.HotelStay.CheckinDate && DateTime.Now <= guest.HotelStay.CheckoutDate) { Console.WriteLine("Room cannot be cancelled"); }
    } while (guest == null || guest.IsCheckedIn == false);

    List<Room> roomsList = guest.HotelStay.RoomList;

    Console.WriteLine();
    Console.WriteLine($"{guest.Name}'s Details");
    Console.WriteLine($"{RepeatStringForLoop("-", guest.Name.Length)}----------");
    Console.WriteLine($"No\t Room");
    foreach (Room room in roomsList)
    {
        counter++;
        Console.WriteLine($"{counter})\t {room.RoomNumber}");
    }

    while (cancelAnotherRoom)
    {

        Console.WriteLine();
        Console.Write("Which room do you want to cancel booking: ");
        int cancelRoom = Convert.ToInt32(Console.ReadLine());
        foreach (Room room in roomsList)
        {
            while (true)
            {
                if (room.RoomNumber == cancelRoom)
                {
                    cancelroom = room;
                    roomsList.Remove(cancelroom);
                    guest.HotelStay.RoomList.Remove(cancelroom);
                    noOfRoomsCancelled += 1;
                    break;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Please enter a valid Room Number.");
                    Console.Write("Which room do you want to cancel booking: ");
                    cancelRoom = Convert.ToInt32(Console.ReadLine());
                }
            }
            break;
        }

        if (roomsList.Count == 0)
        {
            guest.IsCheckedIn = false;
            break;
        }
        else
        {
            Console.Write("Do you want to cancel another room? [Y/N]: ");
            string cancelAnotherRoomChoice = Console.ReadLine();
            while (!ValidateInput(cancelAnotherRoomChoice))
            {
                Console.WriteLine("Invalid input! Please enter Y for Yes or N for No.");
                Console.Write("Do you want to cancel another room? [Y/N]: ");
                cancelAnotherRoomChoice = Console.ReadLine();
            }

            if (cancelAnotherRoomChoice.Equals("N", StringComparison.OrdinalIgnoreCase))
            {
                cancelAnotherRoom = false;
            }
        }
    }

    startFileWritingProcess(guest, guestList, false);
    Console.WriteLine();
    Console.WriteLine($"You will have to pay a total of ${noOfRoomsCancelled * 100} for the cancellation fee.");
    Console.WriteLine("Press any key to make payment.");
    Console.ReadKey();
    Console.WriteLine();
    Console.WriteLine("Transaction is successful.");
}

Room SearchAvailRoom(int roomNum, IDictionary<int, Room> roomDict, Stay? rangeOfStay)
{
    foreach (Room availRoom in availableRoomList)
    {

        if (availRoom.IsAvail)
        {
            // check if the roomNum entered by the user matches with the availble rooms in the dictionary
            if(availRoom.RoomNumber == roomNum)
            {
                return roomDict[roomNum];
            }
        }
        else { continue; }
    }

    return null;
}

Guest retrieveGuest(string passportNum, bool searchBasedOnCheckInStatus = false)
{
    List<Guest> guestList = DisplayAllGuests(guestsConn, stayDict, "no");

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

// to format the output
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

        if (value < 0) { Console.WriteLine("Number must be postive, and > 0"); Console.Write("Please re-enter input value: "); }

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
        "Check-Out Guest",
        "Cancel Guest's Rooms",
        "Display stay details of a guest",
        "Extends the stay by numbers of day",
        "Display Monthly Charged Amounts Breakdown",
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

bool ValidateInput(string input)
{
    return input.ToUpper().Equals("Y", StringComparison.OrdinalIgnoreCase) || input.Equals("N", StringComparison.OrdinalIgnoreCase);
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
            CheckOutGuest();
            standardClearingConsole();
            break;
        case 6:
            CancelStay();
            standardClearingConsole();
            break;
        case 7:
            DisplayDetailsGuest();
            standardClearingConsole();
            break;
        case 8:
            ExtendStay();
            standardClearingConsole();
            break;
        case 9:
            DisplayMonthlyBreakdown();
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

if (!File.Exists(archiveConn))
{
    File.Create(archiveConn).Close();
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


