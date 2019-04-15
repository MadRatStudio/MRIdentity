using Infrastructure.Model.Common;
using Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("image")]
    public class ImageController : Controller
    {
        public ImageManager _imageManager { get; set; }

        public ImageController(ImageManager imageManager)
        {
            _imageManager = imageManager;
        }

        [Route("tmp")]
        [HttpPost]
        [ProducesResponseType(200, Type = typeof(ImageModel))]
        public async Task<IActionResult> UploadTmp([FromForm] IFormFile file)
        {
            return Ok(await _imageManager.UploadTmp(file));
        }
    }
}
