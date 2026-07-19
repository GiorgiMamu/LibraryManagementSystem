using System;
using System.Collections.Generic;
using Spectre.Console;

namespace UI.Menus
{
    public abstract class MenuBase
    {
        protected abstract string Title { get; }
        protected abstract List<string> GetMenuOptions();
        protected abstract bool HandleChoice(string choice);


        // run does not need to know or care which menu it's working on
        public void Run()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new FigletText("Library").Centered().Color(Color.Cyan1));


                //shows arrow-key meny using whatever Title and GetMenuOptins() the actual subclass provides
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title(Title)
                        .AddChoices(GetMenuOptions()));

                bool keepGoing;
                try
                {
                    // HandleChoice returns true if the menu should keep running, false if it should exit
                    keepGoing = HandleChoice(choice);
                }
                catch (Exception ex)
                {
                    // catch any exception thrown by HandleChoice and display it in red, then keep going
                    AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
                    keepGoing = true;
                }
                // if HandleChoice returned false, exit the menu loop// on logout
                if (!keepGoing) return;

                AnsiConsole.MarkupLine("\n[grey]Press Enter to continue...[/]");
                Console.ReadLine();
            }
        }
    }
}