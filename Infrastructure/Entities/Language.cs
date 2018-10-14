using MRDb.Domain;
using MRDb.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Entities
{
    public class Language : Entity, IEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NativeName { get; set; }
    }
}
