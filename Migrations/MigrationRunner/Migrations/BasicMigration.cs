using MigrationRunner.Infrastructure.Enum;
using MigrationRunner.Infrastructure.Interface;
using System;
using System.Threading.Tasks;

namespace MigrationRunner.Migrations
{
    public abstract class BasicMigration : IMigration
    {
        protected Action<object, LogType> _logAction;
        protected IServiceProvider _serviceProvider;

        public BasicMigration() { }

        public IMigration Init(Action<object, LogType> logAction, IServiceProvider serviceProvider)
        {
            _logAction = logAction;
            _serviceProvider = serviceProvider;

            return this;
        }

        public abstract Task Run();
    }
}
