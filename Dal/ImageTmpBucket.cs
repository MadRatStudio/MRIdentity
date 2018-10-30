using Amazon;
using ConnectorS3;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Dal
{
    public class ImageTmpBucket : ConnectorS3Manager
    {
        public ImageTmpBucket(RegionEndpoint region, string id, string secret) : base(region, id, secret)
        {
        }

    }

    public class ImageOriginBucket : ConnectorS3Manager
    {
        public ImageOriginBucket(RegionEndpoint region, string id, string secret) : base(region, id, secret)
        {
        }
    }
}
