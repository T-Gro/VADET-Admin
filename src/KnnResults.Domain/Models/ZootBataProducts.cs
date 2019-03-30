using System.Collections.Generic;

namespace KnnResults.Domain.Models
{
    public partial class ZootBataProducts
    {
        public ZootBataProducts()
        {
            ProductVisualAttributes = new HashSet<ProductVisualAttributes>();
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Brand { get; set; }
        public int Price { get; set; }
        public string Categories { get; set; }
        public string Tags { get; set; }

        public virtual ICollection<ProductVisualAttributes> ProductVisualAttributes { get; set; }
    }
}
