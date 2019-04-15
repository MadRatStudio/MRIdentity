using AutoMapper;
using ConnectorS3;
using ConnectorS3.Domain.Image;
using ConnectorS3.Domain.Upload;
using Dal;
using Infrastructure.Model.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MRIdentityClient.Exception.Common;
using MRIdentityClient.Exception.MRSystem;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Manager
{
    public class ImageManager : BaseManager
    {
        public ImageTmpBucket _tmpBucket { get; set; }
        public ImageOriginBucket _originBucket { get; set; }

        public readonly string[] AVALIABLE_TYPES = new string[] {
            "image/gif", "image/png", "image/jpeg", "image/bmp", "image/webp"
        };

        public ImageManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper, ILoggerFactory loggerFactory, ImageTmpBucket tmpBucket, ImageOriginBucket imageOriginBucket) : base(httpContextAccessor, appUserManager, mapper, loggerFactory)
        {
            _tmpBucket = tmpBucket;
            _originBucket = imageOriginBucket;
        }

        public async Task<ImageModel> UploadTmp(IFormFile file)
        {
            if (file == null)
            {
                throw new ModelDamagedException("File is required");
            }

            var type = file.ContentType?.ToLower() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(type) || !AVALIABLE_TYPES.Contains(type))
                throw new ModelDamagedException("Bad image format");

            var name = file.Name;
            BucketUploadResponse bucketUploadResponse = null;

            using (var stream = file.OpenReadStream())
            {
                bucketUploadResponse = await _tmpBucket.PutObject(new BucketImageUploadModel(stream, name, type));
            }

            if (!bucketUploadResponse.IsSuccess)
            {
                throw new MRSystemException("Bucket upload model");
            }

            return new ImageModel
            {
                IsTmp = true,
                Key = bucketUploadResponse.Key,
                Name = name,
                Url = bucketUploadResponse.Url
            };
        }

        public async Task MoveToOrigin(string key)
        {
        }
    }
}
