using Nop.Core.Domain.Banners;

namespace Nop.Data.Mapping.Banners
{
    public partial class BannerMap : NopEntityTypeConfiguration<Banner>
    {
        public BannerMap()
        {
            this.ToTable("Banner");
            this.HasKey(p => p.Id);
            this.Property(p => p.BannerName).IsRequired();
            
            //this.HasRequired(p => p.Language)
            //    .WithMany()
            //    .HasForeignKey(p => p.LanguageId).WillCascadeOnDelete(true);
        }
    }
}