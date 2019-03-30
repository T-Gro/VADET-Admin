using System;

namespace KnnResults.Domain.Models
{
    public partial class AttributeRejection
    {
        public int Id { get; set; }
        public string Reason { get; set; }
        public DateTime Time { get; set; }
        public string Content { get; set; }
        public string AttributeSource { get; set; }
        public int OriginalProposalId { get; set; }
        public string User { get; set; }
    }
}