using Microsoft.VisualBasic;
using NLog;
public static class FileScrubber<T> where T: Ticket, new()
{

    //Debugging
    const bool DEBUGGING_ALWAYS_SCRUB_FILE = true;

    public static string ScrubTickets(string readFile, NLog.Logger logger, string delimeter1, string delimeter2)
    {
        //TODO: !!! Put back try catch
        try
        {
            // determine name of writeFile
            string ext = readFile.Split('.').Last();
            string writeFile = readFile.Replace(ext, $"scrubbed.{ext}");
            
            // if writeFile exists, the file has already been scrubbed
            if(!DEBUGGING_ALWAYS_SCRUB_FILE && File.Exists(writeFile))
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

                uint lineIndex = 0;
                while (!sr.EndOfStream)
                {
                    // create instance of Ticket class
                    Ticket ticket = new T();
                    string line = sr.ReadLine();

                    if(lineIndex == 0 && line.StartsWith("(") && line.EndsWith(")")){//TODO: Add more checks to avoid removing a record
                        // Ignore headers line 1
                    }else{
                        

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
                            string between = line.Substring(idx, line.LastIndexOf(Ticket.START_END_SUMMARY_WITH_DELIMETER1_INDICATOR)-1);
                            string[] after = line.Substring(line.LastIndexOf(Ticket.START_END_SUMMARY_WITH_DELIMETER1_INDICATOR)+1).Split(delimeter1);

                            ticketParts = new string[before.Length + after.Length - 1];
                            int partIndex = 0;
                            for(int j = 0; j < before.Length; j++)
                            {
                                if(before[j].Length != 0){
                                    ticketParts[partIndex++] = before[j];
                                }
                            }
                            ticketParts[partIndex++] = between;
                            for(int j = 0; j < after.Length; j++)
                            {
                                if(after[j].Length != 0){
                                    ticketParts[partIndex++] = after[j];
                                }
                            }
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

                        string storeBasicLine = $"{ticket.TicketId}{delimeter1}{ticket.Summary}{delimeter1}{Ticket.StatusesEnumToString(ticket.Status)}{delimeter1}{Ticket.PrioritiesEnumToString(ticket.Priority)}{delimeter1}{ticket.Submitter}{delimeter1}{ticket.Assigned}{delimeter1}{ticket.Watching.Aggregate((current, next) => $"{current}{delimeter2}{next}")}";
                        string additionalParts = "";

//TODO: !!!!! Make it so that parenthesies check is always made in any possible user string value +/ temp remove those from text when inputted

                        if(typeof(T) == typeof(BugDefect)){
                            ticket = (BugDefect)ticket;
                            if(ticket is BugDefect bugDefect){
                                bugDefect.Severity = ticketParts.Length > 7 ? ticketParts[7] : "[Severity not set]";
                                
                                additionalParts = $"{additionalParts}{delimeter1}{bugDefect.Severity}";
                            }
                        }else if(typeof(T) == typeof(Enhancement)){
                            ticket = (Enhancement)ticket;
                            if(ticket is Enhancement enhancement){
                                enhancement.Software = ticketParts.Length > 7 ? ticketParts[7] : "[Software not set]";
                                enhancement.Cost = ticketParts.Length > 8 ? Double.Parse(ticketParts[8].Substring((ticketParts[8].IndexOf(Enhancement.MONITORY_STARTER_ICON)==0)? 1 : 0)) : 0;
                                enhancement.Reason = ticketParts.Length > 9 ? ticketParts[9] : "[Reason not set]";
                                enhancement.Estimate = ticketParts.Length > 10 ? ticketParts[10] : "[Estimate not set]";

                                additionalParts = $"{additionalParts}{delimeter1}{enhancement.Software}{delimeter1}{enhancement.Cost:c}{delimeter1}{enhancement.Reason}{delimeter1}{enhancement.Estimate}";
                            }
                        }else if(typeof(T) == typeof(Task)){
                            ticket = (Task)ticket;
                            if(ticket is Task task){
                                task.ProjectName = ticketParts.Length > 7 ? ticketParts[7] : "[Project name not set]";
                                if(ticketParts.Length > 8){
                                    string[] dateParts = ticketParts[8].Split(delimeter2);
                                    task.DueDate = new DateOnly(int.Parse(dateParts[2]),int.Parse(dateParts[0]),int.Parse(dateParts[1]));
                                }else{
                                    task.DueDate = DateOnly.FromDateTime(DateTime.MaxValue.Date); //Sets to largest possible date if not found stored;
                                }

                                additionalParts = $"{additionalParts}{delimeter1}{task.ProjectName}{delimeter1}{task.DueDate}";
                            }
                        }
                        sw.WriteLine($"{storeBasicLine}{additionalParts}");
                    }
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
