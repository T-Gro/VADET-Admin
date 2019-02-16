using ProtoBuf;
using System.Diagnostics;

namespace KnnResults.Domain
{
    [DebuggerDisplay("Label = {Label}, Corr = {Correlation}")]
    [ProtoContract]
    public class ClusterLabel
    {
        [ProtoMember(1)]
        public string Label { get; set; }
        [ProtoMember(2)]
        public double Correlation { get; set; }
        [ProtoMember(3)]
        public int Count { get; set; }
    }
}
