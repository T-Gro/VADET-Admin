using System;

namespace KnnResults.Domain.Models
{
    public partial class OfferedAttributeReaction
    {
        public int Id { get; set; }
        public int AttributeId { get; set; }
        public string ImageId { get; set; }
        public string User { get; set; }
        public DateTime CreatedAt { get; set; }
        public double DistanceToAttribute { get; set; }
        public string ReactionStatus { get; set; }
    }
}
