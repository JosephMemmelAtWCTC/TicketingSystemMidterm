using NLog;

public class TicketFile<T> where T : Ticket, new()
{
    // public property
    public string filePath { get; set; }
    public List<T> Tickets { get; set; }
    private NLog.Logger logger;

    string delimeter1, delimeter2;

    // constructor is a special method that is invoked
    // when an instance of a class is created
    public TicketFile(string ticketFilePath, NLog.Logger logger, string delimeter1, string delimeter2)
    {
        this.logger = logger;
        this.delimeter1 = delimeter1;
        this.delimeter2 = delimeter2;

        filePath = ticketFilePath;
        Tickets = new List<T>();

        // to populate the list with data, read from the data file
        try
        {
            StreamReader sr = new StreamReader(filePath);
            while (!sr.EndOfStream)
            {
                // create instance of Ticket class
                T ticket = new T();
                string line = sr.ReadLine();
                // first look for quote(") in string
                // this indicates a comma(,) in ticket summary
                int idx = line.IndexOf("\"");

                string[] ticketDetails;
                if (idx == -1)
                {
                    // no quote = no comma in ticket summary
                    // ticket details are separated with comma(,)
                    ticketDetails = line.Split(delimeter1);
                }
                else
                {
                    // TODO: Make method as these are simmilar in the scrubber for picking appart from quotation marks
                    // quote = comma or quotes in ticket summary
                    string[] before = line.Substring(0,idx).Split(delimeter1);
                    string between = line.Substring(idx+1, line.LastIndexOf(Ticket.START_END_SUMMARY_WITH_DELIMETER1_INDICATOR)-3); //+1 & -3 are to remove the quotation marks
                    string[] after = line.Substring(line.LastIndexOf(Ticket.START_END_SUMMARY_WITH_DELIMETER1_INDICATOR)+1).Split(delimeter1);

                    ticketDetails = new string[before.Length + after.Length - 1];
                    int partIndex = 0;
                    for(int j = 0; j < before.Length; j++)
                    {
                        if(before[j].Length != 0){
                            ticketDetails[partIndex++] = before[j];
                        }
                    }
                    ticketDetails[partIndex++] = between;
                    for(int j = 0; j < after.Length; j++)
                    {
                        if(after[j].Length != 0){
                            ticketDetails[partIndex++] = after[j];
                        }
                    }
                }


                // Console.WriteLine("LINE = "+line);
                // for (int i = 0; i < ticketDetails.Length; i++)
                // {
                //     Console.WriteLine("ticketDetails["+i+"]="+ticketDetails[i]);
                // }

                
                // Console.WriteLine("LINE = "+line);
                // for (int i = 0; i < ticketDetails.Length; i++)
                // {
                //     Console.WriteLine("ticketDetails["+i+"]="+ticketDetails[i]);
                // }

                ticket.TicketId = UInt64.Parse(ticketDetails[0]);
                ticket.Summary = ticketDetails[1];
                ticket.Status = Ticket.GetEnumStatusFromString(ticketDetails[2]);
                ticket.Priority = Ticket.GetEnumPriorityFromString(ticketDetails[3]);
                ticket.Submitter = ticketDetails[4];
                ticket.Assigned = ticketDetails[5];
                ticket.Watching = ticketDetails[6].Split(delimeter2).ToList();

                Type ticketType = typeof(T);
                if(ticketType == typeof(BugDefect)){
                    BugDefect ticketAsBugDefect = ticket as BugDefect;
                    // Additional fields
                    ticketAsBugDefect.Severity = ticketDetails[7];

                    Tickets.Add(ticketAsBugDefect as T);
                }else if(ticketType == typeof(Enhancement)){
                    Enhancement ticketAsEnhancement = ticket as Enhancement;
                    // Additional fields
                    ticketAsEnhancement.Software = ticketDetails[7];
                    ticketAsEnhancement.Cost = Double.Parse(ticketDetails[8].Substring(1));
                    ticketAsEnhancement.Reason = ticketDetails[9];
                    ticketAsEnhancement.Estimate = ticketDetails[10];

                    Tickets.Add(ticketAsEnhancement as T);
                }else if(ticketType == typeof(Task)){
                    Task ticketAsTask = ticket as Task;
                    // Additional fields
                    ticketAsTask.ProjectName = ticketDetails[7];
                    ticketAsTask.DueDate = DateOnly.Parse(ticketDetails[8]);

                    Tickets.Add(ticketAsTask as T);
                }else{
                    Tickets.Add(ticket);
                }


            }
            // close file when done
            sr.Close();
            logger.Info($"Tickets in file ({Tickets.Count})");
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
        }
    }

    

    // public method
    public bool isUniqueSummary(string summary)
    {
        if (Tickets.ConvertAll(m => m.Summary.ToLower()).Contains(summary.ToLower()))
        {
            logger.Info("Duplicate ticket summary {summary}", summary);
            return false;
        }
        return true;
    }


    public bool AddTicket(T ticket, ulong id)
    {
        try
        {
            ticket.TicketId = id;
            // if summary contains a comma, wrap it in quotes
            string saveSummary = ticket.Summary.IndexOf(delimeter1) != -1 || ticket.Summary.IndexOf(delimeter1) != -1 ? $"\"{ticket.Summary}\"" : ticket.Summary;
            StreamWriter sw = new StreamWriter(filePath, true);
            // write ticket data to file

            string lineToCore = $"{ticket.TicketId}{delimeter1}{saveSummary}{delimeter1}{Ticket.StatusesEnumToString(ticket.Status)}{delimeter1}{Ticket.PrioritiesEnumToString(ticket.Priority)}{delimeter1}{ticket.Submitter}{delimeter1}{ticket.Assigned}{delimeter1}{string.Join(delimeter2,ticket.Watching)}";
            string additional = "";

            Type saveType = typeof(T);
            if(saveType == typeof(BugDefect)){
                BugDefect asBugDefect = ticket as BugDefect;

                additional = $"{additional}{delimeter1}{asBugDefect.Severity}";
            }else if(saveType == typeof(Enhancement)){
                Enhancement asEnhancement = ticket as Enhancement;

                additional = $"{additional}{delimeter1}{asEnhancement.Software}{delimeter1}{asEnhancement.Cost:c}{delimeter1}{asEnhancement.Reason}{delimeter1}{asEnhancement.Estimate}";
            }else if(saveType == typeof(Task)){
                Task asTask = ticket as Task;
                
                additional = $"{additional}{delimeter1}{asTask.ProjectName}{delimeter1}{asTask.DueDate}";
            }


            sw.WriteLine($"{lineToCore}{additional}");

            sw.Close();
            // add ticket details to List
            Tickets.Add(ticket);
            // log transaction
            logger.Info("Ticket id {Id} added", ticket.TicketId);
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
            return false;
        }
        return true;
    }
}