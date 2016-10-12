using System;
using System.Collections.Generic;

namespace Nop.Core.Domain.Banners
{

    public partial class Banner : BaseEntity
    {
        private ICollection<BannerCategoryMapping> _bannerCategories;
        public int Id { get; set; }
        public string BannerName { get; set; }
        public int IsActive { get; set; }
        public Nullable<int> PictureId { get; set; }
        public String PictureUrl { get; set; }
        public String BannerUrl { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public Nullable<DateTime> CreateDate { get; set; }
        public Nullable<int> UpdateBy { get; set; }
        public Nullable<DateTime> UpdateDate { get; set; }

        /// <summary>
        /// Gets or sets the collection of BannerCategory
        /// </summary>
        public virtual ICollection<BannerCategoryMapping> BannerCategories
        {
            get { return _bannerCategories ?? (_bannerCategories = new List<BannerCategoryMapping>()); }
            protected set { _bannerCategories = value; }
        }
    }

}