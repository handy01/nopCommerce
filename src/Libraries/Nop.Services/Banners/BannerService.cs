using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Banners;
using Nop.Services.Events;
using System.Collections.Generic;

namespace Nop.Services.Banners
{

    public partial class BannerService : IBannerService
    {
        #region Fields

        private readonly IRepository<Banner> _bannerRepository;
        private readonly IRepository<BannerCategoryMapping> _bannerCategoryRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public BannerService(IRepository<Banner> bannerRepository, IRepository<BannerCategoryMapping> bannerCategoryRepository,
            IEventPublisher eventPublisher)
        {
            this._bannerRepository = bannerRepository;
            this._bannerCategoryRepository = bannerCategoryRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        public virtual Banner GetBannerById(int bannerid)
        {
            if (bannerid == 0)
                return null;

            return _bannerRepository.GetById(bannerid);
        }


        public virtual Banner GetBannerByName(string bannerName)
        {
            if (String.IsNullOrWhiteSpace(bannerName))
                return null;

            var query = from p in _bannerRepository.Table
                        where p.BannerName == bannerName
                        select p;
            var banner = query.FirstOrDefault();
            return banner;
        }


        public virtual IPagedList<Banner> GetBanner(int pageIndex = 0, int pageSize = int.MaxValue, int statusActive = 0)
        {
            var query = _bannerRepository.Table;

            query = query.Where(p => p.IsActive == statusActive).OrderBy(p => p);

            var el_banner = new PagedList<Banner>(query, pageIndex, pageSize);
            return el_banner;
        }

        public virtual void DeleteBanner(Banner banner)
        {
            if (banner == null)
                throw new ArgumentNullException("banner");

            _bannerRepository.Delete(banner);

            //event notification
            _eventPublisher.EntityDeleted(banner);
        }


        public virtual void InsertBanner(Banner banner)
        {
            if (banner == null)
                throw new ArgumentNullException("banner");

            _bannerRepository.Insert(banner);

            //event notification
            _eventPublisher.EntityInserted(banner);
        }


        public virtual void UpdateBanner(Banner banner)
        {
            if (banner == null)
                throw new ArgumentNullException("banner");

            _bannerRepository.Update(banner);

            //event notification
            _eventPublisher.EntityUpdated(banner);
        }
        #endregion


        public virtual IList<Banner> GetBannerPicturesByLocationIdAndCategoryId(int location, int categoryId)
        {
            var query = from b in _bannerRepository.Table
                        join bcm in _bannerCategoryRepository.Table on b.Id equals bcm.BannerId
                        where bcm.CategoryId == categoryId && bcm.Location == location
                        orderby bcm.DisplayOrder
                        select b;
            var bannerPictures = query.ToList();
            return bannerPictures;
        }
    }
}
