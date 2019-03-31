using System;

namespace MigrationRunner.Infrastructure.Attr
{
    public class MRMigrationAttribute : Attribute
    {
        public string Name { get; set; }
        public string CreatedDate { get; set; }

        public MRMigrationAttribute(string name, string createdTime)
        {
            Name = name;
            CreatedDate = createdTime;
        }
    }
}
