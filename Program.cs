﻿using NLog;

// Create instance of the Logger
NLog.Logger logger = UserInteractions.getLogger();
logger.Info("Main program is running and log mager is started, program is running on a " + (UserInteractions.IS_UNIX ? "" : "non-") + "unix-based device.\n");

const string ticketsPath = "Tickets.csv";
const string enhancementsPath = "Enhancements.csv";
const string tasksPath = "Task.csv";

// Scrub files
string ticketsScrubbedFile = FileScrubber<BugDefect>.ScrubTickets(ticketsPath, logger, Ticket.DELIMETER_1, Ticket.DELIMETER_2);
logger.Info(ticketsScrubbedFile);

TicketFile<BugDefect> ticketFile = new TicketFile<BugDefect>(ticketsScrubbedFile, logger, Ticket.DELIMETER_1, Ticket.DELIMETER_2);


string[] MAIN_MENU_OPTIONS_IN_ORDER = { enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Tickets_No_Filter),
                                        // enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Tickets_Filter),
                                        enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Add_Ticket),
                                        enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Exit)};

string[] TICKET_STATUSES_IN_ORDER = {Ticket.StatusesEnumToString(Ticket.STATUSES.OPEN),
                                     Ticket.StatusesEnumToString(Ticket.STATUSES.REOPENDED),
                                     Ticket.StatusesEnumToString(Ticket.STATUSES.RESOLVED),
                                     Ticket.StatusesEnumToString(Ticket.STATUSES.CLOSED)};

string[] TICKET_PRIORITIES_IN_ORDER = {Ticket.PrioritiesEnumToString(Ticket.PRIORITIES.LOW),
                                       Ticket.PrioritiesEnumToString(Ticket.PRIORITIES.MEDIUM),
                                       Ticket.PrioritiesEnumToString(Ticket.PRIORITIES.HIGH),
                                       Ticket.PrioritiesEnumToString(Ticket.PRIORITIES.Urgent),
                                       Ticket.PrioritiesEnumToString(Ticket.PRIORITIES.EMERGENCY)};


// MAIN LOOP MENU
do
{
    // TODO: Move to switch with integer of place value and also make not relient on index by switching to enum for efficiency
    string menuCheckCommand = UserInteractions.OptionsSelector(MAIN_MENU_OPTIONS_IN_ORDER);

    logger.Info($"User choice: \"{menuCheckCommand}\"");

    if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Exit))
    {//If user intends to exit the program
        logger.Info("Program quiting...");
        return;
    }
    else if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Tickets_No_Filter))
    {
        UserInteractions.PrintTicketList<BugDefect>(ticketFile.Tickets);
    }
    // else if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Tickets_Filter))
    // {
    // }
    else if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Add_Ticket))
    {
        BugDefect newTicket = userCreateNewTicket<BugDefect>();//TODO: Check ticket title during creation
        if (ticketFile.isUniqueSummary(newTicket.Summary))
        {
            if(ticketFile.AddTicket(newTicket)){
                //Inform user that ticket was created and added    
                Console.WriteLine($"Your ticket with the title of \"{newTicket.Summary}\" was successfully added to the records.");
            }else{
                logger.Warn($"Your ticket was unable to be saved.");
            }
        }
        else
        {
            logger.Warn($"Duplicate ticket record on ticket \"{newTicket.Summary}\" with id \"{newTicket.TicketId}\". Not adding to records.");
        }
    }
    else
    {
        logger.Fatal("Somehow, menuCheckCommand was slected that did not fall under the the existing commands, this should never have been triggered. Improper menuCheckCommand is getting through");
    }

} while (true);


T userCreateNewTicket<T>() where T : Ticket, new() {
    string ticketTitle = UserInteractions.UserCreatedStringObtainer("Please enter the title of the new ticket", 1, false, false);

    if(typeof(T) == typeof(BugDefect))//TODO: Move to inside Ticket.cs so generics can be used without check
    {
        T userCreatedTicket = new T{
            TicketId = 0,
            Summary = UserInteractions.UserCreatedStringObtainer("Please enter the summary of the new ticket", 5, true, false),
            Status = Ticket.GetEnumStatusFromString(UserInteractions.OptionsSelector(TICKET_STATUSES_IN_ORDER)),
            Priority = Ticket.GetEnumPriorityFromString(UserInteractions.OptionsSelector(TICKET_PRIORITIES_IN_ORDER)),
            Submitter = UserInteractions.UserCreatedStringObtainer("Please enter the name of this ticket's submitter", 1, true, false),
            Assigned = UserInteractions.UserCreatedStringObtainer("Enter the person assigned to the ticket", 1, true, false),
            Watching = new List<string>{ UserInteractions.UserCreatedStringObtainer("Enter the name of a person watching the ticket", 1, true, false) }
        };
        List<string> selectedWatchers = userCreatedTicket.Watching;
        do
        {
            selectedWatchers.Add(UserInteractions.UserCreatedStringObtainer("Enter the name of another person watching the ticket or leave blank to compleate the ticket", 0, false, true));
        } while (selectedWatchers.Last().Length != 0);
        selectedWatchers.RemoveAt(selectedWatchers.Count() - 1);

        userCreatedTicket.Watching = selectedWatchers;
        return userCreatedTicket;
        // return new T
        // {
        //     TicketId = 123,
        //     title = ticketTitle,
        //     director = UserInteractions.UserCreatedStringObtainer("Please enter the director's name", 1, false, false),
        //     // timespan (hours, minutes, seconds)
        //     runningTime = new TimeSpan(
        //         UserInteractions.UserCreatedIntObtainer("Please enter the ticket's runtime (hours)", 0, 24/*int.MaxValue*/, false, 0),
        //         UserInteractions.UserCreatedIntObtainer("Please enter the ticket's runtime (minutes)", 0, 59, false, 0),
        //         UserInteractions.UserCreatedIntObtainer("Please enter the ticket's runtime (seconds)", 0, 59, false, 0)
        //         ),
        //     genres = UserInteractions.RepeatingGenreUserInteractions.OptionsSelector(false, false)
        // };
    }
    return new T{
        TicketId = 0,
        Summary = "",
        Status = Ticket.STATUSES.NO_STATUSES_LISTED,
        Priority = Ticket.PRIORITIES.NO_PRIORITIES_LISTED,
        Submitter = "",
        Assigned = "",
        Watching = new List<string>()
    };
}




// Console.WriteLine("--"+UserInteractions.userCreatedIntObtainer("Please enter an Id ", -50, 100, true, 42));

// Ticket ticket = new Ticket
// {
//     ticketId = 123,
//     title = "Greatest Ticket Ever, The (2023)",
//     director = "Jeff Grissom",
//     // timespan (hours, minutes, seconds)
//     runningTime = new TimeSpan(2, 21, 23),
//     genres = { "Comedy", "Romance" }
// };

// Album album = new Album
// {
//     ticketId = 321,
//     title = "Greatest Album Ever, The (2020)",
//     artist = "Jeff's Awesome Band",
//     recordLabel = "Universal Music Group",
//     genres = { "Rock" }
// };

// Book book = new Book
// {
//     ticketId = 111,
//     title = "Super Cool Book",
//     author = "Jeff Grissom",
//     pageCount = 101,
//     publisher = "",
//     genres = { "Suspense", "Mystery" }
// };

// Console.WriteLine(ticket.Display());
// Console.WriteLine(album.Display());
// Console.WriteLine(book.Display());

logger.Info("Program ended");


// vvv UNUM STUFF vvv

string enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS mainMenuEnum)
{
    return mainMenuEnum switch
    {
        MAIN_MENU_OPTIONS.Exit => "Quit program",
        MAIN_MENU_OPTIONS.View_Tickets_No_Filter => $"View tickets on file in order (display max ammount is {UserInteractions.PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT:N0})",
        MAIN_MENU_OPTIONS.View_Tickets_Filter => $"Filter tickets on file",
        MAIN_MENU_OPTIONS.Add_Ticket => "Add new ticket to file",
        _ => "ERROR"
    };
}

public enum MAIN_MENU_OPTIONS
{
    Exit,
    View_Tickets_No_Filter,
    View_Tickets_Filter,
    Add_Ticket
}