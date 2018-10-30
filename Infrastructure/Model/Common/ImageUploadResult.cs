using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Model.Common
{
    public class ImageUploadResult
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsTmp { get; set; }
    }
}
