using System.Web.Mvc;
using Nop.Web.Framework.Security;

using System.Collections.Generic;
using Nop.Core.Domain.Banners;
using Nop.Services.Catalog;

namespace Nop.Web.Controllers
{
    public partial class HomeController : BasePublicController
    {
        ICategoryService _CategoryService;

        public IList<Banner> bannerList = null;

       
        public HomeController(ICategoryService categoryService)
        {
            _CategoryService = categoryService;
        }
        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult Index()
        {
            bannerList = _CategoryService.GetBannerPicturesByLocationIdAndCategoryId(0, 0);

            return View(bannerList);

        }
    }
}
