using System;
using System.Collections.Generic;
using System.Linq;
using Core.Helpers;
using Core.Models;
using Services.Interfaces;
using Spectre.Console;

namespace UI.Menus
{
    public class AdminMenu : MenuBase
    {
        private readonly AdminUser _admin;
        private readonly IAuthenticationService _authService;
        private readonly IBookService _bookService;
        private readonly IBorrowService _borrowService;
        private readonly INotificationService _notificationService;

        public AdminMenu(
            AdminUser admin,
            IAuthenticationService authService,
            IBookService bookService,
            IBorrowService borrowService,
            INotificationService notificationService)
        {
            _admin = admin;
            _authService = authService;
            _bookService = bookService;
            _borrowService = borrowService;
            _notificationService = notificationService;
        }

        protected override string Title => $"Admin Menu — {_admin.Username}";

        protected override List<string> GetMenuOptions() => new()
        {
            "View all books",
            "Add a book",
            "Remove a book",
            "Adjust book quantity",
            "Manage borrow requests",
            "View all borrowed books",
            "Check overdue & send notifications",
            "View all users",
            "Delete a user",
            "Settings",
            "Logout"
        };

        protected override bool HandleChoice(string choice)
        {
            switch (choice)
            {
                case "View all books": ShowCatalogue(); return true;
                case "Add a book": AddBook(); return true;
                case "Remove a book":
                    _bookService.RemoveBook(AnsiConsole.Ask<string>("ISBN to remove:"));
                    AnsiConsole.MarkupLine("[green]Removed.[/]");
                    return true;
                case "Adjust book quantity": AdjustQuantity(); return true;
                case "Manage borrow requests": ManageBorrowRequests(); return true;
                case "View all borrowed books": ShowAllBorrowRecords(); return true;
                case "Check overdue & send notifications": CheckOverdueAndNotify(); return true;
                case "View all users": ShowAllUsers(); return true;
                case "Delete a user": DeleteUser(); return true;
                case "Settings": return SettingsMenu.Show(_admin.Id, _admin.Username, _authService);
                case "Logout":
                    return false;
                default:
                    return true;
            }
        }

        private void ShowCatalogue()
        {
            var books = _bookService.GetAll();
            if (books.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]The library is empty.[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("ISBN");
            table.AddColumn("Title");
            table.AddColumn("Author");
            table.AddColumn("Quantity");

            foreach (var b in books)
                table.AddRow(b.Isbn, b.Title, b.Author, b.Quantity.ToString());

            AnsiConsole.Write(table);
        }

        private void AddBook()
        {
            string isbn = AnsiConsole.Ask<string>("ISBN:");
            string title = AnsiConsole.Ask<string>("Title:");

            string author;
            while (true)
            {
                author = AnsiConsole.Ask<string>("Author:");
                if (Validator.IsValidPersonName(author)) break;
                AnsiConsole.MarkupLine("[red]Author must start with a letter and contain only letters, spaces, or hyphens.[/]");
            }

            string quantityInput = AnsiConsole.Ask<string>("Quantity:");
            if (!Validator.TryParsePositiveInt(quantityInput, out int quantity))
            {
                AnsiConsole.MarkupLine("[red]Quantity must be a positive whole number.[/]");
                return;
            }

            _bookService.AddBook(isbn, title, author, quantity);
            AnsiConsole.MarkupLine("[green]Book added.[/]");
        }

        private void AdjustQuantity()
        {
            string isbn = AnsiConsole.Ask<string>("ISBN:");
            string deltaInput = AnsiConsole.Ask<string>("Change (e.g. 5 or -2):");

            if (!int.TryParse(deltaInput, out int delta))
            {
                AnsiConsole.MarkupLine("[red]Enter a whole number (can be negative).[/]");
                return;
            }

            _bookService.AdjustQuantity(isbn, delta);
            AnsiConsole.MarkupLine("[green]Quantity updated.[/]");
        }

        private void ManageBorrowRequests()
        {
            var pending = _borrowService.GetPendingRequests();
            if (pending.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No pending borrow requests.[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Borrow ID");
            table.AddColumn("User ID");
            table.AddColumn("ISBN");
            foreach (var r in pending)
                table.AddRow(r.BorrowId, r.UserId.ToString(), r.Isbn);
            AnsiConsole.Write(table);

            var choices = pending.Select(r => r.BorrowId).ToList();
            choices.Add("Cancel");
            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>().Title("Select a request:").AddChoices(choices));
            if (selected == "Cancel") return;

            string decision = AnsiConsole.Prompt(
                new SelectionPrompt<string>().Title("Decision:").AddChoices("Approve", "Reject"));

            if (decision == "Reject")
            {
                _borrowService.RejectRequest(selected);
                AnsiConsole.MarkupLine("[yellow]Rejected.[/]");
                return;
            }

            DateTime dueDate;
            while (true)
            {
                string input = AnsiConsole.Ask<string>("Due date for return (yyyy-MM-dd):");
                if (Validator.TryParseDate(input, out dueDate)) break;
                AnsiConsole.MarkupLine("[red]That's not a valid date.[/]");
            }

            _borrowService.ApproveRequest(selected, dueDate);
            AnsiConsole.MarkupLine("[green]Approved.[/]");
        }

        private void ShowAllBorrowRecords()
        {
            var records = _borrowService.GetAllBorrowRecords();
            if (records.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No borrow records yet.[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Borrow ID");
            table.AddColumn("User ID");
            table.AddColumn("ISBN");
            table.AddColumn("Due Date");
            table.AddColumn("Status");

            foreach (var r in records)
            {
                string dueDateStr = r.DueDate.HasValue ? r.DueDate.Value.ToString("yyyy-MM-dd") : "-";
                table.AddRow(r.BorrowId, r.UserId.ToString(), r.Isbn, dueDateStr, StatusColors.Colorize(r.Status));
            }

            AnsiConsole.Write(table);
        }

        // first catches up overdue marking + fines,
        // then sends notifications for everything currently due-tomorrow or overdue 
        private void CheckOverdueAndNotify()
        {
            var overdueMarked = _borrowService.CheckAndMarkOverdue();
            AnsiConsole.MarkupLine(overdueMarked.Count == 0
                ? "[green]No overdue books to mark or fine right now.[/]"
                : $"[yellow]{overdueMarked.Count} record(s) checked — fines updated where applicable.[/]");

            var notified = _notificationService.CheckDueDatesAndNotify();
            AnsiConsole.MarkupLine(notified.Count == 0
                ? "[green]No due-date notifications to send right now.[/]"
                : $"[green]{notified.Count} notification(s) sent.[/]");
            foreach (var line in notified) AnsiConsole.WriteLine(line);
        }

        private void ShowAllUsers()
        {
            var users = _authService.GetAllUsers();
            if (users.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No users registered yet.[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("ID");
            table.AddColumn("Username");
            table.AddColumn("Email");
            table.AddColumn("Role");
            table.AddColumn("Verified");
            table.AddColumn("Fines");

            foreach (var u in users)
            {
                string fines = u is ClientUser client ? client.Fines.ToString("F2") : "-";
                table.AddRow(u.Id.ToString(), u.Username, u.Email, u.Role.ToString(),
                    u.IsVerified ? "Yes" : "No", fines);
            }

            AnsiConsole.Write(table);
        }

        private void DeleteUser()
        {
            var users = _authService.GetAllUsers();
            if (users.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No users registered yet.[/]");
                return;
            }

            var choices = users.Select(u => $"{u.Id} — {u.Username} ({u.Role})").ToList();
            choices.Add("Cancel");
            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>().Title("Select a user to delete:").AddChoices(choices));
            if (selected == "Cancel") return;

            int id = int.Parse(selected.Split(" — ")[0]);

            if (id == _admin.Id)
            {
                AnsiConsole.MarkupLine("[red]Use 'Settings' to delete your own account.[/]");
                return;
            }

            var target = users.First(u => u.Id == id);
            if (!AnsiConsole.Confirm($"[red]Permanently delete user '{target.Username}'? This cannot be undone.[/]", false))
            {
                AnsiConsole.MarkupLine("[yellow]Cancelled.[/]");
                return;
            }

            _authService.DeleteUser(id);
            AnsiConsole.MarkupLine($"[green]User '{target.Username}' deleted.[/]");
        }
    }
}