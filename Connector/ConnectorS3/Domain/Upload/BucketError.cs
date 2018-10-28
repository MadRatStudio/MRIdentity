using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectorS3.Domain.Upload
{
    public class BucketError
    {
        public List<string> Messages { get; set; }

        public BucketError(Exception ex)
        {
            Messages = new List<string>();
            do
            {
                Messages.Add(ex.Message);
            } while (ex.InnerException != null);
        }
    }
}
