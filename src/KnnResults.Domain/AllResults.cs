using ProtoBuf;
using System.Collections.Generic;
using System.IO;

namespace KnnResults.Domain
{
    [ProtoContract]
    public class AllResults
    {
        public static Dictionary<int, Dictionary<int, int>> ReferenceMap;

        [ProtoMember(1)] public List<ResultsRow> Rows { get; set; }
        [ProtoMember(2)] public Dictionary<string, int> ImageEncoding { get; set; }
        [ProtoMember(3)] public Dictionary<string, int> PatchEncoding { get; set; }

        public AllResults()
        {
            Rows = new List<ResultsRow>(capacity: 1200000);
            ImageEncoding = new Dictionary<string, int>(capacity: 40000);
            PatchEncoding = new Dictionary<string, int>(capacity: 60);
        }

        public static AllResults Load(string filename)
        {
            using (var file = File.OpenRead(filename))
            {
                return Serializer.Deserialize<AllResults>(file);
            }
        }
    }
}
