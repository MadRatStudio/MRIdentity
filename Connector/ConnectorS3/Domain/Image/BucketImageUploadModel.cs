using Amazon.S3.Transfer;
using ConnectorS3.Domain.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConnectorS3.Domain.Image
{
    public class BucketImageUploadModel : BucketUploadRequest
    {
        public string MimeType { get; set; }
        public Stream Stream { get; set; }

        public BucketImageUploadModel()
        {

        }

        public BucketImageUploadModel(Stream stream, string mimeType)
        {
            Stream = stream;
            MimeType = mimeType;
        }

        public override TransferUtilityUploadRequest CreateRequest(string bucket, string key)
        {
            var t = base.CreateRequest(bucket, key);
            t.InputStream = Stream;
            t.ContentType = MimeType;

            return t;
        }
    }

    public static class ImageMIMETypes
    {
        public const string GIF = "image/gif";
        public const string PNG = "image/png";
        public const string JPEG = "image/jpeg";
        public const string BMP = "image/bmp";
        public const string WEBP = "image/webp";
    }
}
