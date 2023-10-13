using NLog;
public static class FileScrubber<T> where T: Ticket, new()
{

    public static string ScrubTickets(string readFile, NLog.Logger logger, string delimeter1, string delimeter2)
    {
        try
        {
            // determine name of writeFile
            string ext = readFile.Split('.').Last();
            string writeFile = readFile.Replace(ext, $"scrubbed.{ext}");
            // if writeFile exists, the file has already been scrubbed
            if (File.Exists(writeFile))
            {
                // file has already been scrubbed
                logger.Info("File already scrubbed");
            }
            else
            {
                // file has not been scrubbed
                logger.Info("File scrub started");
                // open write file
                StreamWriter sw = new StreamWriter(writeFile);
                // open read file
                StreamReader sr = new StreamReader(readFile);
                // remove first line - column headers
                sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    // create instance of Ticket class
                    Ticket ticket = new T();
                    string line = sr.ReadLine();
                    // look for quote(") in string
                    // this indicates a comma(,) or quote(") in ticket Summary
                    int idx = line.IndexOf(Ticket.START_END_SUMMARY_WITH_DELIMETER1_INDICATOR);
                    string[] ticketParts;
                    if (idx == -1)
                    {
                        // no quote = no comma or quote in ticket Summary
                        ticketParts = line.Split(delimeter1);
                    }
                    else
                    {
                        // string merged = ticketParts[1..(ticketParts.Length - 5)].Aggregate((current, next) => $"{current}{delimeter1}{next}"); //Put delimeter back in
                        string[] before = line.Substring(0,idx).Split(delimeter1);
                        string between = line.Substring(idx, line.LastIndexOf(Ticket.START_END_SUMMARY_WITH_DELIMETER1_INDICATOR));
                        string[] after = line.Substring(line.LastIndexOf(Ticket.START_END_SUMMARY_WITH_DELIMETER1_INDICATOR)).Split(delimeter1);

                        ticketParts = new string[before.Length + 1 + after.Length];
                        int partIndex = 0;
                        for(int j = 0; j < before.Length; j++)
                        {
                            ticketParts[partIndex++] = before[j];
                        }
                        ticketParts[partIndex++] = between;
                        for(int j = 0; j < after.Length; j++)
                        {
                            ticketParts[partIndex++] = after[j];
                        }

                        // // if there is another item in the array it should be director
                        // ticket.director = details.Length > 1 ? details[1] : "unassigned";
                        // // if there is another item in the array it should be run time
                        // ticket.runningTime = details.Length > 2 ? TimeSpan.Parse(details[2]) : new TimeSpan(0);
                    }

                    // Ticket base object
                    // TicketID, Summary, Status, Priority, Submitter, Assigned, Watching
                    ticket.TicketId = UInt64.Parse(ticketParts[0]);
                    ticket.Summary = ticketParts[1];
                    ticket.Status = Ticket.GetEnumStatusFromString(ticketParts[2]);
                    ticket.Priority = Ticket.GetEnumPriorityFromString(ticketParts[3]);
                    ticket.Submitter = ticketParts[4];
                    ticket.Assigned = ticketParts[5];
                    ticket.Watching = ticketParts[6].Split(delimeter2).ToList();

                    string storeBasicLine = $"{ticket.TicketId}{delimeter1}{ticket.Summary}{delimeter1}{Ticket.StatusesEnumToString(ticket.Status)}{delimeter1}{Ticket.PrioritiesEnumToString(ticket.Priority)}{delimeter1}{ticket.Submitter}{delimeter1}{ticket.Assigned}{delimeter1}{ticket.Watching}";
                    
                    sw.WriteLine(storeBasicLine);
                }
                sw.Close();
                sr.Close();
                logger.Info("File scrub ended");
            }
            return writeFile;
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
        }
        return "";
    }
}
