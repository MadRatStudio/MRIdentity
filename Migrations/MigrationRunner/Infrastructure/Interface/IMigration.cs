using MigrationRunner.Infrastructure.Enum;
using System;
using System.Threading.Tasks;

namespace MigrationRunner.Infrastructure.Interface
{
    public interface IMigration
    {
        IMigration Init(Action<object, LogType> logAction, IServiceProvider serviceProvider);
        Task Run();
    }
}
