using System;
using System.Collections.Generic;

namespace KnnResults.Domain.Models
{
    public partial class VisualAttributeDefinition
    {
        public VisualAttributeDefinition()
        {
            ProductVisualAttributes = new HashSet<ProductVisualAttributes>();
        }

        public int Id { get; set; }
        public int OriginalProposalId { get; set; }
        public string AttributeSource { get; set; }
        public string User { get; set; }
        public string Name { get; set; }
        public string Quality { get; set; }
        public string Candidates { get; set; }
        public DateTime? CreatedAt { get; set; }
        public double? DistanceTreshold { get; set; }
        public string DiscardedProducts { get; set; }
        public string DiscardedCategories { get; set; }
        public string WhitelistedCategories { get; set; }

        public virtual ICollection<ProductVisualAttributes> ProductVisualAttributes { get; set; }
    }
}
