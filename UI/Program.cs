using Core.Interfaces;
using Repository;
using Services.Email;
using Repository.Files;
using Services;
using Services.Interfaces;
using Services.Security;
using UI.Menus;

namespace UI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            EnvLoader.Load();

            IFileManager fileManager = new FileManager();
            ILogger logger = new FileLogger(fileManager);
            IPasswordHasher passwordHasher = new BCryptPasswordHasher();
            IEmailService emailService = new SmtpEmailService(logger);

            IUserRepository userRepository = new UserRepository(fileManager, logger);
            IBookRepository bookRepository = new BookRepository(fileManager, logger);
            IBorrowRepository borrowRepository = new BorrowRepository(fileManager, logger);

            IAuthenticationService authService = new AuthenticationService(userRepository, passwordHasher, emailService, logger);
            IBookService bookService = new BookService(bookRepository, logger);
            IBorrowService borrowService = new BorrowService(borrowRepository, bookRepository, userRepository, logger);
            INotificationService notificationService = new NotificationService(
                borrowRepository, userRepository, bookRepository, emailService, logger);
            new LoginMenu(authService, bookService, borrowService, notificationService).Run();
        }
    }
}