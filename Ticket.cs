public abstract class Ticket: IEquatable<Ticket>, IComparable<Ticket>
{

    public enum STATUSES
    {
        OPEN,
        REOPENDED,
        RESOLVED,
        CLOSED,
        NO_STATUS_LISTED,
        ERROR_NOT_A_VALID_STATUS
    }
    public enum PRIORITIES
    {
        LOW,
        MEDIUM,
        HIGH,
        Urgent,
        EMERGENCY,
        NO_PRIORITY_LISTED,
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
    public List<string> Watching { get; set; } = new List<string>() { };

    // constructor
    public Ticket()
    {
    }

    // public method
    public virtual string Display()
    {
        return $"Id: {TicketId}\n" +
               $"Summary:   {Summary}\n" +
               $"Status:    {StatusesEnumToString(Status)}\n" +
               $"Status:    {PrioritiesEnumToString(Priority)}\n" +
               $"Submitter: {Submitter}\n" +
               $"Assigned:  {Assigned}\n" +
               $"Watching:  {string.Join(", ", Watching)}\n";
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Ticket objAsMovie = obj as Ticket;
        if (objAsMovie == null) return false;
        else return Equals(objAsMovie);
    }
    public bool Equals(Ticket other)
    {
        if (other == null) return false;
        return this.GetHashCode().Equals(other.GetHashCode());
    }

    // Default comparer for Ticket (id decending then alphabetically decending)
    public int CompareTo(Ticket compareTicket)
    {
        int compareResult = CompareToId(compareTicket);
        if(compareResult == 0){//If equal
            return CompareToSummary(compareTicket);;
        }else{
            return compareResult;
        }
    }

    // Primary comparer for Ticket (id decending)
    public int CompareToId(Ticket compareTicket)
    {
        // A null value means that this object is greater.
        if (compareTicket == null)
        {
            return 1;
        }
        else
        {
            return this.TicketId.CompareTo(compareTicket.TicketId);
        }
    }

    // Secondary comparer for Ticket (tile alphabetically decending)
    public int CompareToSummary(Ticket compareTicket)
    {
        // A null value means that this object is greater.
        if (compareTicket == null)
        {
            return 1;
        }
        else
        {
            return this.Summary.CompareTo(compareTicket.Summary);
        }
    }

    public virtual bool ContainsInExtras(string phraseStr, StringComparison stringComparison)
    {
        return false;
    }

    // PULL SORT HERE

    public static STATUSES GetEnumStatusFromString(string statuseStr)
    {
        switch (statuseStr.ToLower())
        {
            case "open": return STATUSES.OPEN;
            case "reopened": return STATUSES.REOPENDED;
            case "resolved": return STATUSES.RESOLVED;
            case "closed": return STATUSES.CLOSED;
            case "(no statuses listed)": return STATUSES.NO_STATUS_LISTED;
            default: return STATUSES.ERROR_NOT_A_VALID_STATUS;
        }
    }
    public static PRIORITIES GetEnumPriorityFromString(string priorityStr)
    {
        switch (priorityStr.ToLower())
        {
            case "low": return PRIORITIES.LOW;
            case "medium": return PRIORITIES.MEDIUM;
            case "high": return PRIORITIES.HIGH;
            case "urgent": return PRIORITIES.Urgent;
            case "emergency": return PRIORITIES.EMERGENCY;
            case "(no priorities listed)": return PRIORITIES.NO_PRIORITY_LISTED;
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
            case STATUSES.NO_STATUS_LISTED: return "(no statuses listed)";
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
            case PRIORITIES.NO_PRIORITY_LISTED: return "(no priorities listed)";
            default: return "ERROR: NOT A VALID PRIORITY";
        }
    }

}

public class BugDefect : Ticket
{
    public string Severity { get; set; }
    public override string Display()
    {
        return $"Id:        {TicketId}\n" +
               $"Summary:   {Summary}\n" +
               $"Status:    {StatusesEnumToString(Status)}\n" +
               $"Priority:  {PrioritiesEnumToString(Priority)}\n" +
               $"Submitter: {Submitter}\n" +
               $"Assigned:  {Assigned}\n" +
               $"Watching:  {string.Join(", ", Watching)}\n" +
               $"Severity:  {Severity}\n";
    }

    public override bool ContainsInExtras(string phraseStr, StringComparison stringComparison)
    {
        return Severity.Contains(phraseStr, stringComparison);
    }
}

public class Enhancement : Ticket
{
    public const string MONITORY_STARTER_ICON = "$"; //Should be the same as ":c" (currency) in use

    public string Software { get; set; }
    public double Cost { get; set; }
    public string Reason { get; set; }
    public string Estimate { get; set; }


    public override string Display()
    {
        return $"Id:        {TicketId}\n" +
               $"Summary:   {Summary}\n" +
               $"Status:    {StatusesEnumToString(Status)}\n" +
               $"Priority:  {PrioritiesEnumToString(Priority)}\n" +
               $"Submitter: {Submitter}\n" +
               $"Assigned:  {Assigned}\n" +
               $"Watching:  {string.Join(", ", Watching)}\n" +
               $"Software:  {Software}\n" +
               $"Cost:      {(false ? MONITORY_STARTER_ICON : "")}{Cost:c}\n" +
               $"Reason:    {Reason}\n" +
               $"Estimate:  {Estimate}\n";
    }

    public override bool ContainsInExtras(string phraseStr, StringComparison stringComparison)
    {
        return        Software.Contains(phraseStr, stringComparison)
            || Cost.ToString().Contains(phraseStr, stringComparison)
            ||          Reason.Contains(phraseStr, stringComparison)
            ||        Estimate.Contains(phraseStr, stringComparison);
    }
}

public class Task : Ticket
{
    public string ProjectName { get; set; }
    public DateOnly DueDate { get; set; }


    public override string Display()
    {
        return $"Id:          {TicketId}\n" +
               $"Summary:     {Summary}\n" +
               $"Status:      {StatusesEnumToString(Status)}\n" +
               $"Priority:    {PrioritiesEnumToString(Priority)}\n" +
               $"Submitter:   {Submitter}\n" +
               $"Assigned:    {Assigned}\n" +
               $"Watching:    {string.Join(", ", Watching)}\n" +
               $"ProjectName: {ProjectName}\n" +
               $"DueDate:     {DueDate}\n";
    }

    public override bool ContainsInExtras(string phraseStr, StringComparison stringComparison)
    {
        return        ProjectName.Contains(phraseStr, stringComparison)
            || DueDate.ToString().Contains(phraseStr, stringComparison);
    }
}
