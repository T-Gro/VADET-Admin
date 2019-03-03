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
                var cnt = db.ZootBataProducts.Count(x => x.Brand == "Zoot");
                Console.WriteLine(cnt);
                Console.ReadLine();
            }
        }
    }
}
