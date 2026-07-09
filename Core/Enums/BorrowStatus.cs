namespace Core.Enums
{
    // tracks the status of a borrow request in the system
    public enum BorrowStatus
    {
        Pending,  // client has requested to borrow an item, but the request has not yet been reviewed by an admin
        Approved, // admin has approved the borrow request, and the client can now borrow the item
        Rejected, // admin said no
        Returned, // client has returned the item, and the borrow request is now complete
        Overdue // client has not returned the item by the due date, and the borrow request is now overdue
    }
}