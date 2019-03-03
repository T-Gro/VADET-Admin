using System;
using System.Collections.Generic;

namespace KnnResults.Domain.Models
{
    public partial class ProductVisualAttributes
    {
        public int Id { get; set; }
        public string ProductId { get; set; }
        public int AttributeId { get; set; }
        public double Distance { get; set; }
        public double Coverage { get; set; }

        public virtual VisualAttributeDefinition Attribute { get; set; }
        public virtual ZootBataProducts Product { get; set; }
    }
}
