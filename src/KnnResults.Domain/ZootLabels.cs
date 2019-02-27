using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;

namespace KnnResults.Domain
{
    public class ZootLabels
    {
        public static List<ZootLabel> AllRecords;

        static ZootLabels()
        {
            using (var reader = new StreamReader(@"C:\sir-files\productData.csv"))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = ";";
                AllRecords = csv.GetRecords<ZootLabel>().GroupBy(x => x.id).Select(g => g.First()).ToList();
                AllRecords.ForEach(l => l.tags = l.tags.Trim());
            }
        }
    }
}
