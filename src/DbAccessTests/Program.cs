using System;
using System.Linq;
using KnnResults.Domain.Models;

namespace DbAccessTests
{
    class Program
    {
        static void Main(string[] args)
        {         
            using (var db = new VADETContext())
            {
                var tst =
                    from a in db.VisualAttributeDefinition
                    from b in db.Query<CandidatesOfAttributes>()
                    where a.Id == b.Id
                    where a.AttributeSource.Contains("conv5")                  
                    select new { b.PatchName, a.DistanceTreshold };


                var cnt = tst.ToList().Count;
                Console.WriteLine(cnt);
                Console.ReadLine();
            }
        }
    }
}
