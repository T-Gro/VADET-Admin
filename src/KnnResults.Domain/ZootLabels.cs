using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;

namespace KnnResults.Domain
{
    public class ZootLabels
    {
        public static Dictionary<string, ZootLabel> AllRecords;

        static ZootLabels()
        {
            using (var reader = new StreamReader(@"C:\sir-files\productData.csv"))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = ";";
                AllRecords = csv.GetRecords<ZootLabel>().GroupBy(x => x.id).Select(g => g.First()).ToDictionary(x => x.id);
                foreach (var v in AllRecords.Values)
                {
                    v.tags = v.tags.Trim();
                }
            }
        }

        public static ZootLabel Get(string imageName)
        {
            var cleanName = Path.GetFileNameWithoutExtension(imageName);
            return AllRecords[cleanName];
        }
    }
}
