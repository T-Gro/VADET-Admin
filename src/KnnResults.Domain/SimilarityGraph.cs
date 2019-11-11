using ProtoBuf;
using System.Collections.Generic;

namespace KnnResults.Domain
{
    [ProtoContract]
    public class SimilarityGraph
    {
        [ProtoMember(1)]
        public Dictionary<string, DistancesToOldImages[]> ResultsForNewImages { get; set; }
    }
}
