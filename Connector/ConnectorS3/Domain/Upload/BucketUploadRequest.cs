using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectorS3.Domain.Upload
{
    public abstract class BucketUploadRequest
    {
        public virtual TransferUtilityUploadRequest CreateRequest(string bucket, string key)
        {
            return new TransferUtilityUploadRequest()
            {
                BucketName = bucket,
                Key = key,
            };
        }
    }
}
