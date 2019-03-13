using System;
using System.Linq;
using KnnResults.Domain.Models;

namespace DbAccessTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            using (var db = new VADETContext())
            {
                var cnt = db.VisualAttributeDefinition.Count(x => x.OriginalProposalId > 5);
                Console.WriteLine(cnt);
                Console.ReadLine();
            }
        }
    }
}
