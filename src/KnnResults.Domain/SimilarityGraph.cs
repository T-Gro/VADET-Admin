using ProtoBuf;
using System.Collections.Generic;
using System.IO;

namespace KnnResults.Domain
{
    [ProtoContract]
    public class SimilarityGraph
    {
        [ProtoMember(1)]
        public Dictionary<string, DistancesToOldImages[]> ResultsForNewImages { get; set; }

        public static SimilarityGraph Load(string filename)
        {
            using (var file = File.OpenRead(filename))
            {
                return Serializer.Deserialize<SimilarityGraph>(file);
            }
        }
    }
}
