using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Services
{
    public class FileMessageService : IMessageService
    {
        Task IMessageService.SendEmail(string email, string subject, string message)
        {
            // STUB to send an email (writes to file right now)
            var emailMessage = $"To: {email}\nSubject: {subject}\nMessage: {message}\n\n";
            File.AppendAllText("emails.txt", emailMessage);
            return Task.FromResult(0);
        }
    }
}
