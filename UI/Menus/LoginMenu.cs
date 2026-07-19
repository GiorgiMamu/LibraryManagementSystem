using System;
using Core.Enums;
using Core.Exceptions;
using Core.Helpers;
using Core.Models;
using Services.Interfaces;
using Spectre.Console;

namespace UI.Menus
{
    public class LoginMenu
    {
        private readonly IAuthenticationService _authService;
        private readonly IBookService _bookService;
        private readonly IBorrowService _borrowService;
        private readonly INotificationService _notificationService;

        public LoginMenu(
            IAuthenticationService authService,
            IBookService bookService,
            IBorrowService borrowService,
            INotificationService notificationService)
        {
            _authService = authService;
            _bookService = bookService;
            _borrowService = borrowService;
            _notificationService = notificationService;
        }

        public void Run()
        {
            
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new FigletText("Library").Centered().Color(Color.Cyan1));

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .AddChoices("Login", "Register", "Verify account", "Exit"));

                try
                {
                    switch (choice)
                    {
                        case "Login": Login(); break;
                        case "Register": Register(); break;
                        case "Verify account": VerifyAccount(); break;
                        case "Exit": return;
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
                }

                if (choice != "Exit")
                {
                    AnsiConsole.MarkupLine("\n[grey]Press Enter to continue...[/]");
                    Console.ReadLine();
                }
            }
        }

        private void Login()
        {
            string username = AnsiConsole.Ask<string>("Username:");
            string password = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());

            var user = _authService.Login(username, password);
            AnsiConsole.MarkupLine($"[green]Welcome to the Library, {user.Username}![/]");

            if (user is ClientUser client)
                new ClientMenu(client, _authService, _bookService, _borrowService).Run();
            else if (user is AdminUser admin)
                new AdminMenu(admin, _bookService, _borrowService, _notificationService).Run();
        }

        private void Register()
        {
            string username;
            while (true)
            {
                username = AnsiConsole.Ask<string>("Choose a username:");
                if (Validator.IsValidUsername(username)) break;
                AnsiConsole.MarkupLine("[red]Username must start with a letter, and contain only letters, digits, or underscores.[/]");
            }

            string email;
            while (true)
            {
                email = AnsiConsole.Ask<string>("Email address:");
                if (Validator.IsValidEmail(email)) break;
                AnsiConsole.MarkupLine("[red]That doesn't look like a valid email address.[/]");
            }

            string password;
            while (true)
            {
                password = AnsiConsole.Prompt(new TextPrompt<string>("Choose a password:").Secret());

                if (!Validator.IsStrongPassword(password))
                {
                    AnsiConsole.MarkupLine("[red]Password must be at least 8 characters and include an uppercase letter, a lowercase letter, a number, and a symbol.[/]");
                    continue;
                }

                string confirm = AnsiConsole.Prompt(new TextPrompt<string>("Confirm password:").Secret());
                if (password == confirm) break;
                AnsiConsole.MarkupLine("[red]Passwords do not match — try again.[/]");
            }

            var role = AnsiConsole.Prompt(
                new SelectionPrompt<UserRole>().Title("Register as:").AddChoices(UserRole.Client, UserRole.Admin));

            _authService.Register(username, email, password, role);
            AnsiConsole.MarkupLine("[green]Registered! Check your email for a verification code.[/]");

            EnterVerificationCode(email);
        }


        private void EnterVerificationCode(string email)
        {
            while (true)
            {
                string code = AnsiConsole.Ask<string>("Enter the verification code sent to your email:");
                try
                {
                    _authService.VerifyAccount(email, code);
                    AnsiConsole.MarkupLine("[green]Account verified — you can log in now.[/]");
                    return;
                }
                catch (InvalidVerificationCodeException)
                {
                    AnsiConsole.MarkupLine("[red]That code is incorrect.[/]");
                    if (AnsiConsole.Confirm("Send a new code?"))
                    {
                        _authService.ResendVerificationCode(email);
                        AnsiConsole.MarkupLine("[green]A new code has been sent to your email.[/]");
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        // this is someone coming back later
        private void VerifyAccount()
        {
            string email = AnsiConsole.Ask<string>("Email address:");

            var user = _authService.FindByEmail(email);
            if (user == null)
            {
                AnsiConsole.MarkupLine("[red]No account found with that email.[/]");
                return;
            }
            if (user.IsVerified)
            {
                AnsiConsole.MarkupLine("[yellow]This account is already verified — you can log in.[/]");
                return;
            }

            _authService.ResendVerificationCode(email);
            AnsiConsole.MarkupLine("[green]A verification code has been sent to your email.[/]");

            EnterVerificationCode(email);
        }
    }
}