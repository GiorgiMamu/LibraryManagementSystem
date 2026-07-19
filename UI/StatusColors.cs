using Core.Enums;

namespace UI
{
    // One shared mapping so every table in the app colors statuses the same way.
    public static class StatusColors
    {
        public static string Colorize(BorrowStatus status)
        {
            string color = status switch
            {
                BorrowStatus.Pending => "yellow",
                BorrowStatus.Approved => "green",
                BorrowStatus.Rejected => "grey",
                BorrowStatus.Returned => "blue",
                BorrowStatus.Overdue => "red",
                _ => "white"
            };
            return $"[{color}]{status}[/]";
        }
    }
}