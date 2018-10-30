using MRDb.Domain;
using MRDb.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Entities
{
    public class Image
    {
        public string Key { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
    }
}
