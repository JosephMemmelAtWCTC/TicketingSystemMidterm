using Microsoft.AspNetCore.Mvc;
using NLog;

// Create instance of the Logger
NLog.Logger logger = UserInteractions.getLogger();
logger.Info("Main program is running and log mager is started, program is running on a " + (UserInteractions.IS_UNIX ? "" : "non-") + "unix-based device.\n");

const string ticketsPath = "Tickets.csv";
const string enhancementsPath = "Enhancements.csv";
const string tasksPath = "Tasks.csv";

// Scrub files
string bugDefectScrubbedFile = FileScrubber<BugDefect>.ScrubTickets(ticketsPath, logger, Ticket.DELIMETER_1, Ticket.DELIMETER_2);
logger.Info(bugDefectScrubbedFile);
string enhancementsScrubbedFile = FileScrubber<Enhancement>.ScrubTickets(enhancementsPath, logger, Ticket.DELIMETER_1, Ticket.DELIMETER_2);
logger.Info(enhancementsScrubbedFile);
string tasksScrubbedFile = FileScrubber<Task>.ScrubTickets(tasksPath, logger, Ticket.DELIMETER_1, Ticket.DELIMETER_2);
logger.Info(tasksScrubbedFile);


TicketFile<BugDefect> ticketFileBugDefects = new TicketFile<BugDefect>(bugDefectScrubbedFile, logger, Ticket.DELIMETER_1, Ticket.DELIMETER_2);
TicketFile<Enhancement> ticketFileEnhancements = new TicketFile<Enhancement>(enhancementsScrubbedFile, logger, Ticket.DELIMETER_1, Ticket.DELIMETER_2);
TicketFile<Task> ticketFileTasks = new TicketFile<Task>(tasksScrubbedFile, logger, Ticket.DELIMETER_1, Ticket.DELIMETER_2);


string[] MAIN_MENU_OPTIONS_IN_ORDER = { enumToStringMainMenuWorkarround(MAIN_MENU_OPTIONS.View_Tickets_No_Filter),
                                        // enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Tickets_Filter),
                                        enumToStringMainMenuWorkarround(MAIN_MENU_OPTIONS.Add_Ticket),
                                        enumToStringMainMenuWorkarround(MAIN_MENU_OPTIONS.Exit)};

string[] TICKET_TYPES_IN_ORDER = { enumToStringTicketTypeWorkArround(TICKET_TYPES.Bug_Defect),
                                   enumToStringTicketTypeWorkArround(TICKET_TYPES.Enhancment),
                                   enumToStringTicketTypeWorkArround(TICKET_TYPES.Task)};

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

    if (menuCheckCommand == enumToStringMainMenuWorkarround(MAIN_MENU_OPTIONS.Exit))
    {//If user intends to exit the program
        logger.Info("Program quiting...");
        return;
    }
    else if (menuCheckCommand == enumToStringMainMenuWorkarround(MAIN_MENU_OPTIONS.View_Tickets_No_Filter))
    {
        // UserInteractions.PrintTicketList(ticketFileBugDefects.Tickets);
        // UserInteractions.PrintTicketList(ticketFileEnhancements.Tickets);
        UserInteractions.PrintTicketList(ticketFileTasks.Tickets);
    }
    // else if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Tickets_Filter))
    // {
    // }
    else if (menuCheckCommand == enumToStringMainMenuWorkarround(MAIN_MENU_OPTIONS.Add_Ticket))
    {
        // Type newTicketType;
        string selectedTicketType = UserInteractions.OptionsSelector(TICKET_TYPES_IN_ORDER);
        Ticket newTicket = null;

        // Create newTicket as requested type
        if(selectedTicketType == enumToStringTicketTypeWorkArround(TICKET_TYPES.Bug_Defect)){
            // newTicketType = typeof(BugDefect);
            newTicket = userCreateNewTicket<BugDefect>();
        }else if(selectedTicketType == enumToStringTicketTypeWorkArround(TICKET_TYPES.Enhancment)){
            // newTicketType = typeof(Enhancement);
            newTicket = userCreateNewTicket<Enhancement>();
        }else if(selectedTicketType == enumToStringTicketTypeWorkArround(TICKET_TYPES.Task)){
            // newTicketType = typeof(Task);
            newTicket = userCreateNewTicket<Task>();
        }else{
            logger.Fatal("Somehow a ticket type was slected that did not fall under the the existing commands, this should never have been triggered. Improper ticket type is getting through");
        }

        bool savedSucuessfully = false;
        bool wasDuplicatSummary = false;

        // Store newTicket to respective file
        if(selectedTicketType == enumToStringTicketTypeWorkArround(TICKET_TYPES.Bug_Defect)){
            if(ticketFileBugDefects.isUniqueSummary(newTicket.Summary)){
                savedSucuessfully = ticketFileBugDefects.AddTicket(newTicket as BugDefect);
            }else{
                wasDuplicatSummary = true;
            }
        }else if(selectedTicketType == enumToStringTicketTypeWorkArround(TICKET_TYPES.Enhancment)){
            if(ticketFileEnhancements.isUniqueSummary(newTicket.Summary)){
                savedSucuessfully = ticketFileEnhancements.AddTicket(newTicket as Enhancement);
            }else{
                wasDuplicatSummary = true;
            }
        }else if(selectedTicketType == enumToStringTicketTypeWorkArround(TICKET_TYPES.Task)){
            if(ticketFileEnhancements.isUniqueSummary(newTicket.Summary)){
                savedSucuessfully = ticketFileTasks.AddTicket(newTicket as Task);
            }else{
                wasDuplicatSummary = true;
            }
        }


        if(savedSucuessfully && newTicket != null)
        {
            //Inform user that ticket was created and added    
            Console.WriteLine($"Your ticket with the summary of \"{newTicket.Summary}\" was successfully added to the records.");
        }
        else if(wasDuplicatSummary)
        {
            logger.Warn($"Duplicate ticket record summary on ticket \"{newTicket.Summary}\" with id \"{newTicket.TicketId}\". Not adding to records.");
        }else{
            logger.Warn("Was unable to save your record.");
        }
    }
    else
    {
        logger.Fatal("Somehow menuCheckCommand was slected that did not fall under the the existing commands, this should never have been triggered. Improper menuCheckCommand is getting through");
    }

} while (true);


T userCreateNewTicket<T>() where T : Ticket, new(){
    // Required by all

    string ticketSummary = UserInteractions.UserCreatedStringObtainer("Please enter the summary of the new ticket", 5, false, false);
    Ticket.STATUSES ticketStatus = Ticket.GetEnumStatusFromString(UserInteractions.OptionsSelector(TICKET_STATUSES_IN_ORDER));
    Ticket.PRIORITIES ticketPriority = Ticket.GetEnumPriorityFromString(UserInteractions.OptionsSelector(TICKET_PRIORITIES_IN_ORDER));
    string ticketSubmitter = UserInteractions.UserCreatedStringObtainer("Please enter the name of this ticket's submitter", 1, true, false);
    string ticketAssigned = UserInteractions.UserCreatedStringObtainer("Enter the person assigned to the ticket", 1, true, false);
    List<string> ticketWatching = new List<string>{ UserInteractions.UserCreatedStringObtainer("Enter the name of a person watching the ticket", 1, true, false)};


    if(typeof(T) == typeof(BugDefect))//TODO: Move to inside Ticket.cs so generics can be used without check
    {
        BugDefect userCreatedTicket = new BugDefect{
            Summary = ticketSummary,
            Status = ticketStatus,
            Priority = ticketPriority,
            Submitter = ticketSubmitter,
            Assigned = ticketAssigned,
            Watching = ticketWatching,
        };
        List<string> selectedWatchers = userCreatedTicket.Watching;
        do
        {
            selectedWatchers.Add(UserInteractions.UserCreatedStringObtainer("Enter the name of another person watching the ticket or leave blank to stop adding watchers", 0, false, true));
        }while (selectedWatchers.Last().Length != 0);
        selectedWatchers.RemoveAt(selectedWatchers.Count() - 1);
        userCreatedTicket.Watching = selectedWatchers;

        userCreatedTicket.Severity = UserInteractions.UserCreatedStringObtainer("Plese explain the severity of the bug/defect ticket", 5, false, false);

        return userCreatedTicket as T;
    }else if(typeof(T) == typeof(Enhancement))
    {
        Enhancement userCreatedTicket = new Enhancement{
            Summary = ticketSummary,
            Status = ticketStatus,
            Priority = ticketPriority,
            Submitter = ticketSubmitter,
            Assigned = ticketAssigned,
            Watching = ticketWatching,
        };
        List<string> selectedWatchers = userCreatedTicket.Watching;
        do
        {
            selectedWatchers.Add(UserInteractions.UserCreatedStringObtainer("Enter the name of another person watching the ticket or leave blank to stop adding watchers", 0, false, true));
        }while (selectedWatchers.Last().Length != 0);
        selectedWatchers.RemoveAt(selectedWatchers.Count() - 1);
        userCreatedTicket.Watching = selectedWatchers;

        userCreatedTicket.Software = UserInteractions.UserCreatedStringObtainer("Please enter the software of the enhancement ticket", 1, false, false);
        userCreatedTicket.Cost = UserInteractions.UserCreatedDoubleObtainer("Please enter the cost of the enhancement ticket", 0, 1_000_000_000,false,0,2);
        userCreatedTicket.Reason = UserInteractions.UserCreatedStringObtainer("Please enter the reason of the enhancement ticket", 5, false, false);
        userCreatedTicket.Estimate = UserInteractions.UserCreatedStringObtainer("Please enter the estimate of the enhancement ticket", 1, false, false);

        return userCreatedTicket as T;
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
//     summary = "Greatest Ticket Ever, The (2023)",
//     director = "Jeff Grissom",
//     // timespan (hours, minutes, seconds)
//     runningTime = new TimeSpan(2, 21, 23),
//     genres = { "Comedy", "Romance" }
// };

// Album album = new Album
// {
//     ticketId = 321,
//     summary = "Greatest Album Ever, The (2020)",
//     artist = "Jeff's Awesome Band",
//     recordLabel = "Universal Music Group",
//     genres = { "Rock" }
// };

// Book book = new Book
// {
//     ticketId = 111,
//     summary = "Super Cool Book",
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

string enumToStringMainMenuWorkarround(MAIN_MENU_OPTIONS mainMenuEnum)
{
    return mainMenuEnum switch
    {
        MAIN_MENU_OPTIONS.Exit => "Quit program",
        MAIN_MENU_OPTIONS.View_Tickets_No_Filter => $"View tickets on file in order (display max ammount is {UserInteractions.PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT:N0})",
        MAIN_MENU_OPTIONS.View_Tickets_Filter => "Filter tickets on file",
        MAIN_MENU_OPTIONS.Add_Ticket => "Add new ticket to file",
        _ => "ERROR_MAIN_MENU_OPTION_DOES_NOT_EXIST"
    };
}

string enumToStringTicketTypeWorkArround(TICKET_TYPES ticketTypeEnum)
{
    return ticketTypeEnum switch
    {
        TICKET_TYPES.Bug_Defect => "Bug/Defect",
        TICKET_TYPES.Enhancment => "Enhancement",
        TICKET_TYPES.Task => "Task",
        _ => "ERROR_TICKET_TYPE_DOES_NOT_EXIST"
    };
}

public enum MAIN_MENU_OPTIONS
{
    Exit,
    View_Tickets_No_Filter,
    View_Tickets_Filter,
    Add_Ticket
}

public enum TICKET_TYPES
{
    Bug_Defect,
    Enhancment,
    Task
}