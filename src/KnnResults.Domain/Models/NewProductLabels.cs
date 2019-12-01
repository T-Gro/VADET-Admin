using System;
using System.Collections.Generic;
using System.Linq;

namespace KnnResults.Domain.Models
{
    public partial class NewProductLabels
    {
        static char[] PipeSplitter = new char[] { '|' };
        static char[] CollectionSplitter = new char[] { ';', '"', '[', ']' };

        public string ProductId { get; set; }
        public string PipeDeliminitedEnglishNames { get; set; }

        public virtual string[] Names => PipeDeliminitedEnglishNames.Split(PipeSplitter);
       
        public bool IsMentionedIn(string collection)
        {
            if (collection == null || collection == "[]")
                return false;

            var splitted = collection.Split(CollectionSplitter, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
            var set = new HashSet<string>(splitted, StringComparer.OrdinalIgnoreCase);
                        
            foreach (var n in this.Names)
            {
                if (set.Contains(n))
                    return true;
            }

            return false;
        }
    }
}