using System;
using System.Collections.Generic;
namespace Nop.Core.Domain.Banners
{
    /// <summary>
    /// Represents a banner category mapping
    /// </summary>
    public partial class BannerCategoryMapping : BaseEntity
    {
        public int Id { get; set; }
        public int BannerId { get; set; }
        public int CategoryId { get; set; }
        public int Location { get; set; }
        public int DisplayOrder { get; set; }
        public int IsFeaturedBanner { get; set; }
        public string WidgetZone { get; set; }
        public virtual Banner Banner { get; set; }
    }

}
