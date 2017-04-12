using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Services
{
    public interface IMessageService
    {
        Task SendEmail(string email, string subject, string message);
    }
}
