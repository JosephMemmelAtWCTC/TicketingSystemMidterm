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
                    // quote = comma or quotes in ticket summary
                    // extract the TicketId
                    ticket.TicketId = UInt64.Parse(line.Substring(0, idx - 1));
                    // remove TicketId and first comma from string
                    line = line.Substring(idx);
                    // find the last quote
                    idx = line.LastIndexOf("\"");
                    // extract summary
                    ticket.Summary = line.Substring(0, idx + 1);
                    // remove summary and next comma from the string
                    line = line.Substring(idx + 2);
                    // split the remaining string based on commas
                    string[] details = line.Split(delimeter2);

                    ticketDetails = line.Split(delimeter1);
                    ticketDetails = new string[5];

                    // TODO:!!!!!!!!!!!!!!!!!!
                }

                ticket.TicketId = UInt64.Parse(ticketDetails[0]);
                ticket.Summary = ticketDetails[1];
                ticket.Status = Ticket.GetEnumStatusFromString(ticketDetails[2]);
                ticket.Priority = Ticket.GetEnumPriorityFromString(ticketDetails[3]);
                ticket.Submitter = ticketDetails[4];
                ticket.Assigned = ticketDetails[5];
                ticket.Watching = ticketDetails[6].Split(delimeter2).ToList();

                // if(typeof(T) == typeof(BugDefect)){
                //     BugDefect asBugDefect = ticket as BugDefect;

                //     additional = $"{additional}{delimeter1}{asBugDefect.Severity}";
                // }



                Tickets.Add(ticket);
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


    public bool AddTicket(T ticket)
    {
        try
        {
            // first generate ticket id
            ticket.TicketId = Tickets.Max(ticket => ticket.TicketId) + 1;
            // if summary contains a comma, wrap it in quotes
            string summary = ticket.Summary.IndexOf(delimeter2) != -1 || ticket.Summary.IndexOf("\"") != -1 ? $"\"{ticket.Summary}\"" : ticket.Summary;
            StreamWriter sw = new StreamWriter(filePath, true);
            // write ticket data to file
            //TODO: NEED TO WRITE LINE

            string lineToCore = $"{ticket.TicketId}{delimeter1}{ticket.Summary}{delimeter1}{Ticket.StatusesEnumToString(ticket.Status)}{delimeter1}{Ticket.PrioritiesEnumToString(ticket.Priority)}{delimeter1}{ticket.Submitter}{delimeter1}{ticket.Assigned}{delimeter1}{string.Join(delimeter2,ticket.Watching)}";
            string additional = "";

            if(typeof(T) == typeof(BugDefect)){
                BugDefect asBugDefect = ticket as BugDefect;

                additional = $"{additional}{delimeter1}{asBugDefect.Severity}";
            }


            sw.WriteLine($"{lineToCore}{additional}");

            sw.Close();
            // add ticket details to List
            Tickets.Add(ticket);
            // log transaction
            logger.Info("Media id {Id} added", ticket.TicketId);
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
            return false;
        }
        return true;
    }
}