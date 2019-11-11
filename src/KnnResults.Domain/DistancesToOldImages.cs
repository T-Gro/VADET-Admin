using ProtoBuf;

namespace KnnResults.Domain
{
    [ProtoContract]
    public class DistancesToOldImages
    {
        [ProtoMember(1, AsReference = true)]
        public string OldPatchName { get; set; }
        [ProtoMember(2)]
        public float Distance { get; set; }
    }
}
