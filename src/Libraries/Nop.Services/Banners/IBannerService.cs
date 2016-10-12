using Nop.Core;
using Nop.Core.Domain.Banners;
using Nop.Services.Banners;
using System.Collections.Generic;

namespace Nop.Services.Banners
{
    public partial interface IBannerService
    {
        Banner GetBannerById(int bannerId);
        Banner GetBannerByName(string bannerName);
        IPagedList<Banner> GetBanner(
             int pageIndex = 0, int pageSize = int.MaxValue, int isActive = 0);
        void DeleteBanner(Banner banner);
        void InsertBanner(Banner banner);
        void UpdateBanner(Banner banner);
        IList<Banner> GetBannerPicturesByLocationIdAndCategoryId(int location, int categoryId);
    }
}
