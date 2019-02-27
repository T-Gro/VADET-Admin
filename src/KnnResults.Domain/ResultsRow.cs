using ProtoBuf;

namespace KnnResults.Domain
{
    [ProtoContract]
    public class ResultsRow 
    {
        [ProtoMember(1)] public Patch Query { get; set; }
        [ProtoMember(2)] public SearchHit[] Hits { get; set; }
        [ProtoMember(3)] public ClusterLabel[] Labels { get; set; }
    }
}
