namespace MaProgramez.Website.Seo
{
    using System.Collections.Generic;
    using System.Xml.Linq;

    public interface ISitemapGenerator
    {
        XDocument GenerateSiteMap(IEnumerable<ISitemapItem> items);
    }
}