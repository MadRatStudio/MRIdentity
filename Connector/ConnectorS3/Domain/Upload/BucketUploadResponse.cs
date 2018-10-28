using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectorS3.Domain.Upload
{
    public class BucketUploadResponse
    {
        public BucketError Error { get; set; }
        public bool IsSuccess => Error == null;

        public string Key { get; set; }
        public string Url { get; set; }

        public BucketUploadResponse()
        {

        }

        public BucketUploadResponse(string key, string url)
        {
            Key = key;
            Url = url;
        }

        public BucketUploadResponse(Exception ex)
        {
            Error = new BucketError(ex);
        }
    }
}
