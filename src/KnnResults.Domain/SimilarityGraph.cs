using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KnnResults.Domain
{
    [ProtoContract]
    public class SimilarityGraph
    {
        [ProtoMember(1)]
        public Dictionary<string, DistancesToOldImages[]> ResultsForNewImages { get; set; }
        [ProtoMember(2)]
        public Dictionary<string, float> OldNameTreshold512 { get; set; }

        public static SimilarityGraph Load(string filename)
        {         
            using (var file = File.OpenRead(filename))
            {
                var t =  Serializer.Deserialize<SimilarityGraph>(file);
                foreach (var kvp in t.ResultsForNewImages.Where(x => x.Value is null).ToList())
                {
                    t.ResultsForNewImages[kvp.Key] = new DistancesToOldImages[0];
                }               
                return t;
            }
        }
    }
}
