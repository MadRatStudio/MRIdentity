using Amazon;
using ConnectorS3;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dal
{
    public class TmpBucket : ConnectorS3Manager
    {
        public TmpBucket(RegionEndpoint region, string id, string secret) : base(region, id, secret)
        {
        }
    }

    public class OriginBucket : ConnectorS3Manager
    {
        public OriginBucket(RegionEndpoint region, string id, string secret) : base(region, id, secret)
        {
        }
    }
}
