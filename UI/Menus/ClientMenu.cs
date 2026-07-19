using System.Collections.Generic;
using System.Linq;
using Core.Helpers;
using Core.Models;
using Services.Interfaces;
using Spectre.Console;

namespace UI.Menus
{
    public class ClientMenu : MenuBase
    {
        // RefreshClient() swaps in the latest version
        // from storage before every action, so fines/status never go stale
        private ClientUser _client;
        private readonly IAuthenticationService _authService;
        private readonly IBookService _bookService;
        private readonly IBorrowService _borrowService;

        public ClientMenu(ClientUser client, IAuthenticationService authService,
            IBookService bookService, IBorrowService borrowService)
        {
            _client = client;
            _authService = authService;
            _bookService = bookService;
            _borrowService = borrowService;
        }

        protected override string Title => $"Client Menu — {_client.Username}";

        protected override List<string> GetMenuOptions() => new()
        {
            "Browse catalogue",
            "Search books",
            "Borrow a book",
            "Return a book",
            "My borrowed books",
            "View my fines",
            "Pay a fine",
            "Logout"
        };

        protected override bool HandleChoice(string choice)
        {
            RefreshClient();

            switch (choice)
            {
                case "Browse catalogue":
                    ShowCatalogue(_bookService.GetAll(), "The library is empty.");
                    return true;
                case "Search books":
                    string keyword = AnsiConsole.Ask<string>("Search for (title or author):");
                    ShowCatalogue(_bookService.Search(keyword), "No books match your search.");
                    return true;
                case "Borrow a book":
                    var record = _borrowService.RequestBorrow(_client, AnsiConsole.Ask<string>("ISBN to borrow:"));
                    AnsiConsole.MarkupLine($"[green]Request submitted — status: Pending. ID: {record.BorrowId}[/]");
                    return true;
                case "Return a book":
                    ReturnBookFlow();
                    return true;
                case "My borrowed books":
                    ShowMyBorrowedBooks();
                    return true;
                case "View my fines":
                    AnsiConsole.MarkupLine(_client.Fines == 0
                        ? "[green]You have no outstanding fines.[/]"
                        : $"Outstanding fines: [red]{_client.Fines:F2}[/]");
                    return true;
                case "Pay a fine":
                    PayFineFlow();
                    return true;
                case "Logout":
                    return false;
                default:
                    return true;
            }
        }

        // pulls the latest version of this client from storage 
        private void RefreshClient()
        {
            if (_authService.GetUserById(_client.Id) is ClientUser refreshed)
                _client = refreshed;
        }

        private void ShowCatalogue(List<Book> books, string emptyMessage)
        {
            if (books.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{emptyMessage}[/]");
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

        private void ReturnBookFlow()
        {
            var returnable = _borrowService.GetReturnableBooksForClient(_client.Id);
            if (returnable.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]You have no books currently borrowed to return.[/]");
                return;
            }

            var allBooks = _bookService.GetAll();
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Borrow ID");
            table.AddColumn("Title");
            table.AddColumn("Due Date");
            table.AddColumn("Status");

            foreach (var r in returnable)
            {
                var book = allBooks.FirstOrDefault(b => b.Isbn == r.Isbn);
                string title = book?.Title ?? r.Isbn;
                string dueDateStr = r.DueDate.HasValue ? r.DueDate.Value.ToString("yyyy-MM-dd") : "-";
                table.AddRow(r.BorrowId, title, dueDateStr, StatusColors.Colorize(r.Status));
            }
            AnsiConsole.Write(table);

            var choices = returnable.Select(r => r.BorrowId).ToList();
            choices.Add("Cancel");
            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>().Title("Select a borrow to return:").AddChoices(choices));
            if (selected == "Cancel") return;

            
            _borrowService.ReturnBook(_client, selected);
            AnsiConsole.MarkupLine("[green]Book returned.[/]");
        }

        private void ShowMyBorrowedBooks()
        {
            var records = _borrowService.GetBorrowedBooksForClient(_client.Id);
            if (records.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]You haven't borrowed any books yet.[/]");
                return;
            }

            var allBooks = _bookService.GetAll();
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Borrow ID");
            table.AddColumn("Title");
            table.AddColumn("Due Date");
            table.AddColumn("Status");

            foreach (var r in records)
            {
                var book = allBooks.FirstOrDefault(b => b.Isbn == r.Isbn);
                string title = book?.Title ?? r.Isbn;
                string dueDateStr = r.DueDate.HasValue ? r.DueDate.Value.ToString("yyyy-MM-dd") : "-";
                table.AddRow(r.BorrowId, title, dueDateStr, StatusColors.Colorize(r.Status));
            }

            AnsiConsole.Write(table);
        }

        private void PayFineFlow()
        {
            if (_client.Fines == 0)
            {
                AnsiConsole.MarkupLine("[green]You have no outstanding fines.[/]");
                return;
            }

            AnsiConsole.MarkupLine($"Current outstanding fine: [red]{_client.Fines:F2}[/]");
            string input = AnsiConsole.Ask<string>("Amount to pay:");

            if (!Validator.TryParsePositiveDecimal(input, out decimal amount))
            {
                AnsiConsole.MarkupLine("[red]Enter a positive amount.[/]");
                return;
            }

            _borrowService.PayFine(_client, amount);
            RefreshClient();

            AnsiConsole.MarkupLine(_client.Fines == 0
                ? "[green]Fine fully paid — you can borrow books again.[/]"
                : $"[yellow]Payment received. Remaining balance: {_client.Fines:F2}[/]");
        }
    }
}