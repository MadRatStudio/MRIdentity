using AutoMapper;
using CommonApi.Errors;
using CommonApi.Resopnse;
using ConnectorS3;
using ConnectorS3.Domain.Image;
using ConnectorS3.Domain.Upload;
using Dal;
using Infrastructure.Model.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public ImageManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper, ImageTmpBucket tmpBucket, ImageOriginBucket imageOriginBucket) : base(httpContextAccessor, appUserManager, mapper)
        {
            _tmpBucket = tmpBucket;
            _originBucket = imageOriginBucket;
        }

        public async Task<ApiResponse<ImageUploadResult>> UploadTmp(IFormFile file)
        {
            if (file == null)
            {
                return Fail(ECollection.Select(ECollection.MODEL_DAMAGED));
            }

            var type = file.ContentType?.ToLower() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(type) || !AVALIABLE_TYPES.Contains(type))
                return Fail(ECollection.BAD_DATA_FORMAT);

            var name = file.Name;
            BucketUploadResponse bucketUploadResponse = null;

            using (var stream = file.OpenReadStream())
            {
                bucketUploadResponse = await _tmpBucket.PutObject(new BucketImageUploadModel(stream, name, type));
            }

            if (!bucketUploadResponse.IsSuccess)
            {
                return Fail(ECollection.Select(ECollection.UNDEFINED_ERROR, new ModelError("Image", string.Join(", ", bucketUploadResponse.Error.Messages))));
            }

            return Ok(new ImageUploadResult
            {
                IsTmp = true,
                Key = bucketUploadResponse.Key,
                Name = name,
                Url = bucketUploadResponse.Url
            });

        }
    }
}
