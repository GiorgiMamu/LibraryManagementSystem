using System.Collections.Generic;

namespace Services.Interfaces
{
    public interface INotificationService
    {
        List<string> CheckDueDatesAndNotify();
    }
}