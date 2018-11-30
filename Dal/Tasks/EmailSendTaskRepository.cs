using Infrastructure.Entities.Tasks;
using MongoDB.Driver;
using MRDb.Infrastructure.Interface;
using MRDb.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dal.Tasks
{
    public class EmailSendTaskRepository : BaseRepository<EmailSendTask>, IRepository<EmailSendTask>
    {
        public EmailSendTaskRepository(IMongoDatabase mongoDatabase) : base(mongoDatabase) { }

        public async Task InsertEmail(string toEmail, string subject, string body, EmailTaskBot bot)
        {
            await Insert(new EmailSendTask
            {
                Bot = bot,
                Status = EmailSendStatus.New,
                Subject = subject,
                ToEmail = toEmail,
                Body = body
            });
        }

        public async Task<ICollection<EmailSendTask>> GetByStatus(EmailSendStatus status, int? limit = null)
        {
            var query = DbQuery
                .CustomSearch(x => x.And(
                    x.Eq(z => z.State, true),
                    x.Eq(z => z.Status, status)))
                .Ascending(x => x.CreatedTime);

            if (limit.HasValue && limit > 0)
                query.Limit = limit.Value;

            return await Get(query);
        }

        public async Task<UpdateResult> MultiplyUpdateStatus(IEnumerable<string> ids, EmailSendStatus newStatus)
        {
            var query = DbQuery
                .Eq(x => x.State, true)
                .Contains(x => x.Id, ids)
                .Update(x => x.Set(z => z.Status, newStatus));

            return await Update(query);
        }

        public async Task<UpdateResult> UpdateStatus(string id, EmailSendStatus newStatus, string failMessage = null)
        {
            var query = DbQuery
                .Eq(x => x.State, true)
                .Eq(x => x.Id, id)
                .Update(x => x.Set(z => z.Status, newStatus).Set(z => z.FailMessage, failMessage));

            return await Update(query);
        }
    }
}
