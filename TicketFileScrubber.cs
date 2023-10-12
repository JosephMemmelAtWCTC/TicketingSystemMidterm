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
                    int idx = line.IndexOf('"');
                    string genres = "";
                    if (idx == -1)
                    {
                        // no quote = no comma or quote in ticket Summary
                        // ticket details are separated with comma(,)
                        string[] ticketDetails = line.Split(',');
                        ticket.TicketId = UInt64.Parse(ticketDetails[0]);
                        ticket.Summary = ticketDetails[1];
                        genres = ticketDetails[2];
                        // ticket.director = ticketDetails.Length > 3 ? ticketDetails[3] : "unassigned";
                        // ticket.runningTime = ticketDetails.Length > 4 ? TimeSpan.Parse(ticketDetails[4]) : new TimeSpan(0);
                    }
                    else
                    {
                        // quote = comma or quotes in ticket Summary
                        // extract the TicketId
                        ticket.TicketId = UInt64.Parse(line.Substring(0, idx - 1));
                        // remove TicketId and first comma from string
                        line = line.Substring(idx);
                        // find the last quote
                        idx = line.LastIndexOf('"');
                        // extract Summary
                        ticket.Summary = line.Substring(0, idx + 1);
                        // remove Summary and next comma from the string
                        line = line.Substring(idx + 2);
                        // split the remaining string based on commas
                        string[] details = line.Split(',');
                        // the first item in the array should be genres 
                        genres = details[0];
                        // // if there is another item in the array it should be director
                        // ticket.director = details.Length > 1 ? details[1] : "unassigned";
                        // // if there is another item in the array it should be run time
                        // ticket.runningTime = details.Length > 2 ? TimeSpan.Parse(details[2]) : new TimeSpan(0);
                    }
                    // sw.WriteLine($"{ticket.TicketId},{ticket.Summary},{genres},{ticket.director},{ticket.runningTime}");
                    sw.WriteLine($"{ticket.TicketId},{ticket.Summary},{genres}");
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
