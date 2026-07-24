using Services.Interfaces;
using Spectre.Console;

namespace UI.Menus
{
    // Shared "Settings" flow used by both AdminMenu and ClientMenu, so
    // change-password / delete-account logic only lives in one place.
    // Returns false only when the caller's own account was deleted —
    // the menu that called this should treat that as a logout.
    public static class SettingsMenu
    {
        public static bool Show(int userId, string username, IAuthenticationService authService)
        {
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Settings")
                    .AddChoices("Change password", "Delete my account", "Back"));

            switch (choice)
            {
                case "Change password":
                    ChangePassword(userId, authService);
                    return true;
                case "Delete my account":
                    return !DeleteAccount(userId, username, authService);
                default:
                    return true;
            }
        }

        private static void ChangePassword(int userId, IAuthenticationService authService)
        {
            string current = AnsiConsole.Prompt(new TextPrompt<string>("Current password:").Secret());

            string newPassword;
            while (true)
            {
                newPassword = AnsiConsole.Prompt(new TextPrompt<string>("New password:").Secret());
                string confirm = AnsiConsole.Prompt(new TextPrompt<string>("Confirm new password:").Secret());

                if (newPassword == confirm) break;
                AnsiConsole.MarkupLine("[red]Passwords do not match — try again.[/]");
            }

            authService.ChangePassword(userId, current, newPassword);
            AnsiConsole.MarkupLine("[green]Password changed successfully.[/]");
        }

        // returns true only if the account was actually deleted
        private static bool DeleteAccount(int userId, string username, IAuthenticationService authService)
        {
            bool confirmed = AnsiConsole.Confirm(
                $"[red]Are you sure you want to permanently delete the account '{username}'? This cannot be undone.[/]",
                false);

            if (!confirmed)
            {
                AnsiConsole.MarkupLine("[yellow]Cancelled.[/]");
                return false;
            }

            string password = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter your password to confirm deletion:").Secret());

            authService.DeleteAccount(userId, password);
            AnsiConsole.MarkupLine("[green]Your account has been deleted. You will be logged out.[/]");
            return true;
        }
    }
}