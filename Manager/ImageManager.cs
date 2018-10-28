using AutoMapper;
using ConnectorS3;
using Dal;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Manager
{
    public class ImageManager : BaseManager
    {
        public TmpBucket _tmpBucket { get; set; }

        public ImageManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper, TmpBucket tmpBucket) : base(httpContextAccessor, appUserManager, mapper)
        {
            _tmpBucket = tmpBucket;
        }


    }
}
