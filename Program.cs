using Microsoft.AspNetCore.Mvc;
using NLog;

// Create instance of the Logger
NLog.Logger logger = UserInteractions.getLogger();
logger.Info("Main program is running and log mager is started, program is running on a " + (UserInteractions.IS_UNIX ? "" : "non-") + "unix-based device.\n");


const string databaseDirectory = "database/";
const string ticketsPath = $"{databaseDirectory}Tickets.csv";
const string enhancementsPath = $"{databaseDirectory}Enhancements.csv";
const string tasksPath = $"{databaseDirectory}Tasks.csv";

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
                                        enumToStringMainMenuWorkarround(MAIN_MENU_OPTIONS.View_Tickets_Filter),
                                        enumToStringMainMenuWorkarround(MAIN_MENU_OPTIONS.Add_Ticket),
                                        enumToStringMainMenuWorkarround(MAIN_MENU_OPTIONS.Exit)};

string[] FILTER_MENU_OPTIONS_IN_ORDER = { enumToStringFilterMenuWorkarround(FILTER_MENU_OPTIONS.Types),
                                          enumToStringFilterMenuWorkarround(FILTER_MENU_OPTIONS.Phrases),
                                          enumToStringFilterMenuWorkarround(FILTER_MENU_OPTIONS.Statuses),
                                          enumToStringFilterMenuWorkarround(FILTER_MENU_OPTIONS.Priorities),
                                          enumToStringFilterMenuWorkarround(FILTER_MENU_OPTIONS.Run_Filter)};


string[] TICKET_TYPES_IN_ORDER = { enumToStringTicketTypeWorkarround(TICKET_TYPES.Bug_Defect),
                                   enumToStringTicketTypeWorkarround(TICKET_TYPES.Enhancment),
                                   enumToStringTicketTypeWorkarround(TICKET_TYPES.Task)};

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
        List<Ticket> allTickets = getAllTickets();

        allTickets.Sort();
        UserInteractions.PrintTicketList(allTickets);
        Console.WriteLine($"There were {allTickets.Count} ticket{((allTickets.Count==1)? "" : "s" )} on record.");
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
        if (selectedTicketType == enumToStringTicketTypeWorkarround(TICKET_TYPES.Bug_Defect))
        {
            // newTicketType = typeof(BugDefect);
            newTicket = userCreateNewTicket<BugDefect>();
        }
        else if (selectedTicketType == enumToStringTicketTypeWorkarround(TICKET_TYPES.Enhancment))
        {
            // newTicketType = typeof(Enhancement);
            newTicket = userCreateNewTicket<Enhancement>();
        }
        else if (selectedTicketType == enumToStringTicketTypeWorkarround(TICKET_TYPES.Task))
        {
            // newTicketType = typeof(Task);
            newTicket = userCreateNewTicket<Task>();
        }
        else
        {
            logger.Fatal("Somehow a ticket type was slected that did not fall under the the existing commands, this should never have been triggered. Improper ticket type is getting through");
        }

        bool savedSucuessfully = false;
        bool wasDuplicatSummary = false;

        // Store newTicket to respective file
        if (selectedTicketType == enumToStringTicketTypeWorkarround(TICKET_TYPES.Bug_Defect))
        {
            if (ticketFileBugDefects.isUniqueSummary(newTicket.Summary))
            {
                savedSucuessfully = ticketFileBugDefects.AddTicket(newTicket as BugDefect, getAllTickets().Max(ticket => ticket.TicketId) + 1);
            }
            else
            {
                wasDuplicatSummary = true;
            }
        }
        else if (selectedTicketType == enumToStringTicketTypeWorkarround(TICKET_TYPES.Enhancment))
        {
            if (ticketFileEnhancements.isUniqueSummary(newTicket.Summary))
            {
                savedSucuessfully = ticketFileEnhancements.AddTicket(newTicket as Enhancement, getAllTickets().Max(ticket => ticket.TicketId) + 1);
            }
            else
            {
                wasDuplicatSummary = true;
            }
        }
        else if (selectedTicketType == enumToStringTicketTypeWorkarround(TICKET_TYPES.Task))
        {
            if (ticketFileEnhancements.isUniqueSummary(newTicket.Summary))
            {
                savedSucuessfully = ticketFileTasks.AddTicket(newTicket as Task, getAllTickets().Max(ticket => ticket.TicketId) + 1);
            }
            else
            {
                wasDuplicatSummary = true;
            }
        }


        if (savedSucuessfully && newTicket != null)
        {
            //Inform user that ticket was created and added    
            Console.WriteLine($"Your ticket with the summary of \"{newTicket.Summary}\" was successfully added to the records.");
        }
        else if (wasDuplicatSummary)
        {
            logger.Warn($"Duplicate ticket record summary on ticket \"{newTicket.Summary}\" with id \"{newTicket.TicketId}\". Not adding to records.");
        }
        else
        {
            logger.Warn("Was unable to save your record.");
        }
    }
    else if (true && menuCheckCommand == enumToStringMainMenuWorkarround(MAIN_MENU_OPTIONS.View_Tickets_Filter))
    {
        // Default, allow all

        TICKET_TYPES[] filterAllowTypes = (TICKET_TYPES[])Enum.GetValues(typeof(TICKET_TYPES)); //Get all enums of Ticket types

        List<string> filterSearchStrings = new List<string>() { }; //TODO: Optimize filtering

        Ticket.STATUSES[] filterAllowStatuses = (Ticket.STATUSES[])Enum.GetValues(typeof(Ticket.STATUSES)); //Get all enums of statuses
        Ticket.PRIORITIES[] filterAllowPriorities = (Ticket.PRIORITIES[])Enum.GetValues(typeof(Ticket.PRIORITIES)); //Get all enums of priorities

        string choosenFilterOption;
        do
        {
            Console.WriteLine("\n~<:{[ Current Filter Settings ]}:>~\n");

            // filterAllowTypes.ToList().Aggregate((current, next) => $"{enumToStringTicketTypeWorkArround(current)}{enumToStringTicketTypeWorkArround(next)}");
            string filterAllowTypesAsStr = "";
            string filterAllowStatusesAsStr = "";
            string filterAllowPrioritiesAsStr = "";
            foreach (TICKET_TYPES allowType in filterAllowTypes)
            {
                filterAllowTypesAsStr = $"{filterAllowTypesAsStr}, {enumToStringTicketTypeWorkarround(allowType)}";
            }
            if (filterAllowTypesAsStr.Length > 2)
            {
                filterAllowTypesAsStr = filterAllowTypesAsStr.Substring(2);
            }

            foreach (Ticket.STATUSES allowStatus in filterAllowStatuses)
            {
                filterAllowStatusesAsStr = $"{filterAllowStatusesAsStr}, {Ticket.StatusesEnumToString(allowStatus)}";
            }
            if (filterAllowStatusesAsStr.Length > 2)
            {
                filterAllowStatusesAsStr = filterAllowStatusesAsStr.Substring(2);
            }

            foreach (Ticket.PRIORITIES allowPriority in filterAllowPriorities)
            {
                filterAllowPrioritiesAsStr = $"{filterAllowPrioritiesAsStr}, {Ticket.PrioritiesEnumToString(allowPriority)}";
            }
            if (filterAllowPrioritiesAsStr.Length > 2)
            {
                filterAllowPrioritiesAsStr = filterAllowPrioritiesAsStr.Substring(2);
            }

            string indentStr = new string[3].Aggregate((c, n) => $"{c} "); //Blank space to make it act as a single character, needs to be 1 more then desired ammount
            string phraseIndentStr = new string[10].Aggregate((c, n) => $"{c} "); //Blank space to make it act as a single character

            string builtUpPhrases = (filterSearchStrings.Count == 0)? "" : filterSearchStrings.Aggregate((current, next) => $"{current}\n{indentStr}{phraseIndentStr}{next}");

            Console.WriteLine($"{indentStr}Types:      {filterAllowTypesAsStr}");
            Console.WriteLine($"{indentStr}Statuses:   {filterAllowStatusesAsStr}");
            Console.WriteLine($"{indentStr}Priorities: {filterAllowPrioritiesAsStr}");
            Console.WriteLine($"{indentStr}Types:      {filterAllowTypesAsStr}");
            Console.WriteLine($"{indentStr}Phrases:    {builtUpPhrases}");

            choosenFilterOption = UserInteractions.OptionsSelector(FILTER_MENU_OPTIONS_IN_ORDER);

            if (choosenFilterOption == enumToStringFilterMenuWorkarround(FILTER_MENU_OPTIONS.Types))
            {
                string[] choosenTypes = UserInteractions.RepeatingOptionsSelector(TICKET_TYPES_IN_ORDER);

                filterAllowTypes = new TICKET_TYPES[choosenTypes.Length];
                for (int i = 0; i < choosenTypes.Length; i++)
                {
                    filterAllowTypes[i] = stringToEnumTicketTypeWorkArround(choosenTypes[i]);
                }
            }
            else if (choosenFilterOption == enumToStringFilterMenuWorkarround(FILTER_MENU_OPTIONS.Phrases))
            {
                string[] keywordOptions = new string[] { "Add phrases", "Remove phrases", "Exit Phraes" };

                string selectedOption;
                do{
                    selectedOption = UserInteractions.OptionsSelector(keywordOptions);

                    if(selectedOption == keywordOptions[1]){
                        Console.WriteLine("Select the following to remove");
                        string[] leftAfterRemovel = UserInteractions.RepeatingOptionsSelector(filterSearchStrings.ToArray());
                        
                        filterSearchStrings = new List<string>() { }; //Reset options
                        foreach(string phrase in leftAfterRemovel)
                        {
                            filterSearchStrings.Add(phrase);
                        }
                    }else if(selectedOption == keywordOptions[0]){
                        Console.WriteLine("Add phrases to the filter (it's case-insensitive)");
                        string newPhrase = null;
                        do{
                            newPhrase = UserInteractions.UserCreatedStringObtainer("Please input a phrase to add to the search, or leave blank to exit",-1,false,true);
                            if(newPhrase.Length == 0){
                                newPhrase = null;
                            }else{
                                filterSearchStrings.Add(newPhrase);
                            }
                        }while(newPhrase != null);
                    }
                }while(selectedOption != keywordOptions[keywordOptions.Length-1]);

                Console.WriteLine($"Current search phrases: {filterSearchStrings.Aggregate((current,next) => $"{current}, \"{next}\"")}");

            }
            else if (choosenFilterOption == enumToStringFilterMenuWorkarround(FILTER_MENU_OPTIONS.Statuses))
            {
                string[] choosenStatuses = UserInteractions.RepeatingOptionsSelector(TICKET_STATUSES_IN_ORDER);

                filterAllowStatuses = new Ticket.STATUSES[choosenStatuses.Length];
                for (int i = 0; i < choosenStatuses.Length; i++)
                {
                    filterAllowStatuses[i] = Ticket.GetEnumStatusFromString(choosenStatuses[i]);
                }
            }
            else if (choosenFilterOption == enumToStringFilterMenuWorkarround(FILTER_MENU_OPTIONS.Priorities))
            {
                string[] choosenPriorities = UserInteractions.RepeatingOptionsSelector(TICKET_STATUSES_IN_ORDER);

                filterAllowStatuses = new Ticket.STATUSES[choosenPriorities.Length];
                for (int i = 0; i < choosenPriorities.Length; i++)
                {
                    filterAllowPriorities[i] = Ticket.GetEnumPriorityFromString(choosenPriorities[i]);
                }
            }
        } while (choosenFilterOption != enumToStringFilterMenuWorkarround(FILTER_MENU_OPTIONS.Run_Filter));
        // Run filter, TODO: Make more efficent, only adds, no removes
        List<Ticket> filterRemainingTickets = new List<Ticket>();
        // Filter by type - (add)
        if (filterAllowTypes.Contains(TICKET_TYPES.Bug_Defect))
        {
            filterRemainingTickets.AddRange(ticketFileBugDefects.Tickets);
        }
        if (filterAllowTypes.Contains(TICKET_TYPES.Enhancment))
        {
            filterRemainingTickets.AddRange(ticketFileEnhancements.Tickets);
        }
        if (filterAllowTypes.Contains(TICKET_TYPES.Task))
        {
            filterRemainingTickets.AddRange(ticketFileTasks.Tickets);
        }
        // Filter by summary - (remove)
        for(int i=0; i< filterRemainingTickets.Count; i++)
        {
            Ticket ticket = filterRemainingTickets[i];

            bool didNotFindAPhraseOrStatusPriority = true;

            foreach (Ticket.STATUSES status in filterAllowStatuses)
            {
                if(ticket.Status == status){
                    didNotFindAPhraseOrStatusPriority = false;
                    break;
                }
            }
            if(didNotFindAPhraseOrStatusPriority){
                foreach (Ticket.PRIORITIES priority in filterAllowPriorities)
                {
                    if(ticket.Priority == priority){
                        didNotFindAPhraseOrStatusPriority = false;
                        break;
                    }
                }
            }
            if(didNotFindAPhraseOrStatusPriority){
                foreach (string phrase in filterSearchStrings)
                {
                    if (ticket.Summary.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                    {
                        didNotFindAPhraseOrStatusPriority = false;
                    }
                    else if (ticket.Assigned.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                    {
                        didNotFindAPhraseOrStatusPriority = false;
                    }
                    else if (ticket.Submitter.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                    {
                        didNotFindAPhraseOrStatusPriority = false;
                    }
                    else if (ticket.Assigned.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                    {
                        didNotFindAPhraseOrStatusPriority = false;
                    }
                    else if (ticket.ContainsInExtras(phrase, StringComparison.OrdinalIgnoreCase))
                    {
                        didNotFindAPhraseOrStatusPriority = false;
                    }
                    if (!didNotFindAPhraseOrStatusPriority) { break; }
                }
            }
            if(didNotFindAPhraseOrStatusPriority)
            {
                filterRemainingTickets.Remove(ticket);
                i--;
            }
        }

        filterRemainingTickets.Sort();
        UserInteractions.PrintTicketList(filterRemainingTickets);
        Console.WriteLine($"There were {filterRemainingTickets.Count} ticket{((filterRemainingTickets.Count==1)? "" : "s" )} on record that satisfied your filter parameters.");
    }
    else
    {
        logger.Fatal("Somehow menuCheckCommand was slected that did not fall under the the existing commands, this should never have been triggered. Improper menuCheckCommand is getting through");
    }

} while (true);

List<Ticket> getAllTickets(){
    List<Ticket> allTickets = new List<Ticket>(){ };
    allTickets.AddRange(ticketFileBugDefects.Tickets);
    allTickets.AddRange(ticketFileEnhancements.Tickets);
    allTickets.AddRange(ticketFileTasks.Tickets);
    return allTickets;
}

T userCreateNewTicket<T>() where T : Ticket, new()
{
    // Required by all

    string ticketSummary = UserInteractions.UserCreatedStringObtainer("Please enter the summary of the new ticket", 5, false, false);
    Ticket.STATUSES ticketStatus = Ticket.GetEnumStatusFromString(UserInteractions.OptionsSelector(TICKET_STATUSES_IN_ORDER));
    Ticket.PRIORITIES ticketPriority = Ticket.GetEnumPriorityFromString(UserInteractions.OptionsSelector(TICKET_PRIORITIES_IN_ORDER));
    string ticketSubmitter = UserInteractions.UserCreatedStringObtainer("Please enter the name of this ticket's submitter", 1, true, false);
    string ticketAssigned = UserInteractions.UserCreatedStringObtainer("Enter the person assigned to the ticket", 1, true, false);
    List<string> ticketWatching = new List<string> { UserInteractions.UserCreatedStringObtainer("Enter the name of a person watching the ticket", 1, true, false) };


    if (typeof(T) == typeof(BugDefect))//TODO: Move to inside Ticket.cs so generics can be used without check
    {
        BugDefect userCreatedTicket = new BugDefect
        {
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
        } while (selectedWatchers.Last().Length != 0);
        selectedWatchers.RemoveAt(selectedWatchers.Count() - 1);
        userCreatedTicket.Watching = selectedWatchers;

        userCreatedTicket.Severity = UserInteractions.UserCreatedStringObtainer("Plese explain the severity of the bug/defect ticket", 5, false, false);

        return userCreatedTicket as T;
    }
    else if (typeof(T) == typeof(Enhancement))
    {
        Enhancement userCreatedTicket = new Enhancement
        {
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
        } while (selectedWatchers.Last().Length != 0);
        selectedWatchers.RemoveAt(selectedWatchers.Count() - 1);
        userCreatedTicket.Watching = selectedWatchers;

        userCreatedTicket.Software = UserInteractions.UserCreatedStringObtainer("Please enter the software of the enhancement ticket", 1, false, false);
        userCreatedTicket.Cost = UserInteractions.UserCreatedDoubleObtainer($"Please enter the cost of the enhancement ticket (in {Enhancement.MONITORY_STARTER_ICON})", 0, 1_000_000_000, false, 0, 2);
        userCreatedTicket.Reason = UserInteractions.UserCreatedStringObtainer("Please enter the reason of the enhancement ticket", 5, false, false);
        userCreatedTicket.Estimate = UserInteractions.UserCreatedStringObtainer("Please describe the time estimate of the enhancement ticket", 1, false, false);

        return userCreatedTicket as T;
    }
    else if (typeof(T) == typeof(Task))
    {
        Task userCreatedTicket = new Task
        {
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
        } while (selectedWatchers.Last().Length != 0);
        selectedWatchers.RemoveAt(selectedWatchers.Count() - 1);
        userCreatedTicket.Watching = selectedWatchers;

        userCreatedTicket.ProjectName = UserInteractions.UserCreatedStringObtainer("Please enter the project name of the task ticket", 1, false, false);
        int dueMonth = UserInteractions.UserCreatedIntObtainer("Please enter the due date (Month) of the task ticket", 1, 12, false, DateOnly.FromDateTime(DateTime.Now).Month);
        //TODO: Check if date is valid - place limit on days depending on month and year
        int dueDay = UserInteractions.UserCreatedIntObtainer("Please enter the due date (Day) of the task ticket", 1, 31, false, DateOnly.FromDateTime(DateTime.Now).Day);
        int dueYear = UserInteractions.UserCreatedIntObtainer("Please enter the due date (Year) of the task ticket", 2000, 2099, false, DateOnly.FromDateTime(DateTime.Now).Year);
        userCreatedTicket.DueDate = new DateOnly(dueYear, dueMonth, dueDay);

        return userCreatedTicket as T;
    }

    return new T
    {
        TicketId = 0,
        Summary = "",
        Status = Ticket.STATUSES.NO_STATUS_LISTED,
        Priority = Ticket.PRIORITIES.NO_PRIORITY_LISTED,
        Submitter = "",
        Assigned = "",
        Watching = new List<string>()
    };
}

logger.Info("Program ended");


// vvv UNUM STUFF vvv

string enumToStringMainMenuWorkarround(MAIN_MENU_OPTIONS mainMenuEnum)
{
    return mainMenuEnum switch
    {
        MAIN_MENU_OPTIONS.Exit => "Quit program",
        MAIN_MENU_OPTIONS.View_Tickets_No_Filter => $"View all tickets on file (display max ammount is {UserInteractions.PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT / 11:N0})",// Divide by 11 as 10 is the current max number of fields in a ticket and +1 for the empty spacing lines between
        MAIN_MENU_OPTIONS.View_Tickets_Filter => "Filter tickets on file",
        MAIN_MENU_OPTIONS.Add_Ticket => "Add new ticket to file",
        _ => "ERROR_MAIN_MENU_OPTION_DOES_NOT_EXIST"
    };
}

string enumToStringFilterMenuWorkarround(FILTER_MENU_OPTIONS filterMenuEnum)
{
    return filterMenuEnum switch
    {
        FILTER_MENU_OPTIONS.Types => "Add filter tickets by type",
        FILTER_MENU_OPTIONS.Phrases => "Modify filter tickets by phrase",
        FILTER_MENU_OPTIONS.Statuses => "Select filter tickets by status",
        FILTER_MENU_OPTIONS.Priorities => "Select filter tickets by priority",
        FILTER_MENU_OPTIONS.Run_Filter => "Run the compleated filters",
        _ => "ERROR_FILTER_MENU_OPTION_DOES_NOT_EXIST"
    };
}

string enumToStringTicketTypeWorkarround(TICKET_TYPES ticketTypeEnum)
{
    return ticketTypeEnum switch
    {
        TICKET_TYPES.Bug_Defect => "Bug/Defect",
        TICKET_TYPES.Enhancment => "Enhancement",
        TICKET_TYPES.Task => "Task",
        _ => "ERROR_TICKET_TYPE_DOES_NOT_EXIST"
    };
}

TICKET_TYPES stringToEnumTicketTypeWorkArround(string ticketTypeStr)
{
    return ticketTypeStr switch
    {
        "Bug/Defect" => TICKET_TYPES.Bug_Defect,
        "Enhancement" => TICKET_TYPES.Enhancment,
        "Task" => TICKET_TYPES.Task,
        _ => TICKET_TYPES.Bug_Defect //Default to orignal Bug/Defect when not found (should never happen if done correctly)
    };
    //TODO: Log error
}

public enum MAIN_MENU_OPTIONS
{
    Exit,
    View_Tickets_No_Filter,
    View_Tickets_Filter,
    Add_Ticket
}

public enum FILTER_MENU_OPTIONS
{
    Types,
    Phrases,
    Statuses,
    Priorities,
    Run_Filter
}

public enum TICKET_TYPES
{
    Bug_Defect,
    Enhancment,
    Task
}