using CommonApi.Models;
using CommonApi.Resopnse;
using CommonApi.Response;
using Infrastructure.Model.Provider;
using Manager;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("category")]
    public class CategoryController : Controller
    {
        protected readonly CategoryManager _categoryManager;

        public CategoryController(CategoryManager categoryManager)
        {
            _categoryManager = categoryManager;
        }

        [HttpPost]
        [ProducesResponseType(200, Type = typeof(IdNameModel))]
        public async Task<IActionResult> Create([FromBody] CategoryUpdateModel model)
        {
            return Json(await _categoryManager.Create(model));
        }

        [HttpGet]
        [Route("{skip}/{limit}")]
        [ProducesResponseType(200, Type = typeof(ApiListResponse<ProviderCategoryDisplayModel>))]
        public async Task<IActionResult> Get(int skip, int limit, [FromQuery] string q, [FromQuery] string languageCode)
        {
            return Json(await _categoryManager.Get(skip, limit, q, languageCode));
        }

        [HttpPut]
        [Route("{id}")]
        [ProducesResponseType(200, Type = typeof(IdNameModel))]
        public async Task<IActionResult> Update(string id, [FromBody] CategoryUpdateModel model)
        {
            return Json(await _categoryManager.Update(id, model));
        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(200, Type = typeof(OkResult))]
        public async Task<IActionResult> Delete(string id)
        {
            await _categoryManager.Delete(id);
            return Json(new OkResult());
        }
    }
}
