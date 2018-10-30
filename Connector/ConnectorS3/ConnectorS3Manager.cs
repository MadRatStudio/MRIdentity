using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using ConnectorS3.Domain.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorS3
{
    public class ConnectorS3Manager
    {
        protected readonly string _id;
        protected readonly string _secret;

        protected RegionEndpoint _region;
        protected string _bucketName;
        protected string _subdirectory;

        public string BucketFullPath => string.IsNullOrWhiteSpace(_subdirectory) ? _bucketName : $"{_bucketName}/{_subdirectory}";
        public string BucketLink(string key) => $"https://s3.amazonaws.com/{BucketFullPath}/{key}";

        protected TransferUtility _client => new TransferUtility(_id, _secret, _region);

        public ConnectorS3Manager(RegionEndpoint region, string id, string secret)
        {
            _region = region;
            _id = id;
            _secret = secret;
        }

        public ConnectorS3Manager SetBucket(string bucketName)
        {
            _bucketName = bucketName;
            return this;
        }

        public ConnectorS3Manager SetSubdirectory(string subdirectory)
        {
            if (!string.IsNullOrWhiteSpace(subdirectory))
                _subdirectory = subdirectory;
            else
                _subdirectory = string.Empty;

            return this;
        }

        public static ConnectorS3Manager Init(RegionEndpoint region, string id, string secret, string bucket, string subdirectory = null)
        {
            return new ConnectorS3Manager(region, id, secret).SetBucket(bucket).SetSubdirectory(subdirectory);
        }

        public async Task<BucketUploadResponse> PutObject(BucketUploadRequest model)
        {
            var key = _generateKey;

            try
            {
                await _client.UploadAsync(model.CreateRequest(BucketFullPath, key));
                return new BucketUploadResponse(key, BucketLink(key));
            }
            catch (Exception ex)
            {
                return new BucketUploadResponse(ex);
            }
        }

        public async Task<BucketUploadResponse> MoveFrom(string from, string key)
        {
            try
            {
                var client = _client;
                var response = await client.S3Client.CopyObjectAsync(from, key, BucketFullPath, key);
                var delete = await client.S3Client.DeleteObjectAsync(from, key);
                return new BucketUploadResponse
                {
                    Key = key,
                    Url = BucketLink(key)
                };
            }
            catch (Exception ex)
            {
                return new BucketUploadResponse(ex);
            }
        }

        public async Task<BucketUploadResponse> Delete(string key)
        {
            try
            {
                var client = _client;
                await client.S3Client.DeleteObjectAsync(BucketFullPath, key);
                return new BucketUploadResponse();
            }
            catch (Exception ex)
            {
                return new BucketUploadResponse(ex);
            }
        }

        protected String _generateKey => Guid.NewGuid().ToString();
    }

    public class ImageUploadModel
    {
        public string Name { get; set; }
        public Stream ImageStream { get; set; }
        public string ContentType { get; set; }
    }
}
