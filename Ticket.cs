public abstract class Ticket//: IEquatable<Ticket>, IComparable<Ticket>
{

    public enum STATUSES
    {
        OPEN,
        REOPENDED,
        RESOLVED,
        CLOSED,
        NO_STATUSES_LISTED,
        ERROR_NOT_A_VALID_STATUS
    }
    public enum PRIORITIES
    {
        LOW,
        MEDIUM,
        HIGH,
        Urgent,
        EMERGENCY,
        NO_PRIORITIES_LISTED,//TODO: Rename to be singular
        ERROR_NOT_A_VALID_PRIORITY
    }

    public const string DELIMETER_1 = ",";
    public const string DELIMETER_2 = "|";
    public const string START_END_SUMMARY_WITH_DELIMETER1_INDICATOR = "\"";

    public UInt64 TicketId { get; set; }
    public string Summary { get; set; }
    public STATUSES Status { get; set; }
    public PRIORITIES Priority { get; set; }
    public string Submitter { get; set; }
    public string Assigned { get; set; }
    public List<string> Watching { get; set; } = new List<string>(){};

    // constructor
    public Ticket()
    {
    }

    // public method
    public virtual string Display()
    {
        return $"Id: {TicketId}\n" +
               $"Summary: {Summary}\n" +
               $"Status: {StatusesEnumToString(Status)}\n" +
               $"Status: {PrioritiesEnumToString(Priority)}\n" +
               $"Submitter: {Submitter}\n" +
               $"Assigned: {Assigned}\n" +
               $"Genres: {string.Join(", ", Watching)}\n";
    }

// PULL SORT HERE

    public static STATUSES GetEnumStatusFromString(string statuseStr)
    {
        switch (statuseStr)
        {
            case "Open": return STATUSES.OPEN;
            case "Reopened": return STATUSES.REOPENDED;
            case "Resolved": return STATUSES.RESOLVED;
            case "Closed": return STATUSES.CLOSED;
            case "(no statuses listed)": return STATUSES.NO_STATUSES_LISTED;
            default: return STATUSES.ERROR_NOT_A_VALID_STATUS;
        }
    }
    public static PRIORITIES GetEnumPriorityFromString(string priorityStr)
    {
        switch (priorityStr)
        {
            case "Low": return PRIORITIES.LOW;
            case "Medium": return PRIORITIES.MEDIUM;
            case "High": return PRIORITIES.HIGH;
            case "Urgent": return PRIORITIES.Urgent;
            case "Emergency": return PRIORITIES.EMERGENCY;
            case "(no priorities listed)": return PRIORITIES.NO_PRIORITIES_LISTED;
            default: return PRIORITIES.ERROR_NOT_A_VALID_PRIORITY;
        }
    }

    public static string StatusesEnumToString(STATUSES status)
    {
        switch (status)
        {
            case STATUSES.OPEN: return "Open";
            case STATUSES.REOPENDED: return "Reopened";
            case STATUSES.RESOLVED: return "Resolved";
            case STATUSES.CLOSED: return "Closed";
            case STATUSES.NO_STATUSES_LISTED: return "(no statuses listed)";
            default: return "ERROR: NOT A VALID STATUS";
        }
    }
    public static string PrioritiesEnumToString(PRIORITIES priority)
    {
        switch (priority)
        {
            case PRIORITIES.LOW: return "Low";
            case PRIORITIES.MEDIUM: return "Medium";
            case PRIORITIES.HIGH: return "High";
            case PRIORITIES.Urgent: return "Urgent";
            case PRIORITIES.EMERGENCY: return "Emergency";
            case PRIORITIES.NO_PRIORITIES_LISTED: return "(no priorities listed)";
            default: return "ERROR: NOT A VALID PRIORITY";
        }
    }

}

public class BugDefect : Ticket
{
    public string Severity { get; set; }
    public override string Display()
    {
        return $"Id: {TicketId}\n" +
               $"Summary: {Summary}\n" +
               $"Status: {StatusesEnumToString(Status)}\n" +
               $"Priority: {PrioritiesEnumToString(Priority)}\n" +
               $"Submitter: {Submitter}\n" +
               $"Assigned: {Assigned}\n" +
               $"Watching: {string.Join(", ", Watching)}\n" +
               $"Severity: {Severity}\n";
    }
}
