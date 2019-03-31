using Dal.Tasks;
using Infrastructure.Entities.Tasks;
using Manager.Email;
using Service.Instance;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{
    public class EmailMadRatBotService : IBaseService
    {
        public TimeSpan Schedule => TimeSpan.FromSeconds(30);

        const int LIMIT = 10;

        protected readonly EmailSendTaskRepository _emailSendTaskRepository;
        protected readonly EmailMadRatBotManager _emailMadRatBotManager;

        public EmailMadRatBotService(EmailSendTaskRepository emailSendTaskRepository, EmailMadRatBotManager emailMadRatBotManager)
        {
            _emailSendTaskRepository = emailSendTaskRepository;
            _emailMadRatBotManager = emailMadRatBotManager;
        }

        public void SendEmailsSync()
        {
            SendEmails().Wait();
        }

        public async Task SendEmails()
        {
            var tasks = await _emailSendTaskRepository.GetByStatus(EmailSendStatus.New, LIMIT);
            if (tasks == null || !tasks.Any()) return;

            await _emailSendTaskRepository.MultiplyUpdateStatus(tasks.Select(x => x.Id), EmailSendStatus.InProgress);

            foreach(var task in tasks)
            {
                var result = await _emailMadRatBotManager.Send(task.ToEmail, task.Subject, task.Body);
                if (result)
                    await _emailSendTaskRepository.UpdateStatus(task.Id, EmailSendStatus.Delivered);
                else
                    await _emailSendTaskRepository.UpdateStatus(task.Id, EmailSendStatus.Failed, "Fail logs not implemented");
            }
        }
    }
}
