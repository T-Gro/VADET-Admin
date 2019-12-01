using System;

namespace KnnResults.Domain.Models
{
    public partial class NewProductLabels
    {
        static char[] PipeSplitter = new char[] { '|' };

        public string ProductId { get; set; }
        public string PipeDeliminitedEnglishNames { get; set; }

        public virtual string[] Names => PipeDeliminitedEnglishNames.Split(PipeSplitter);
       
        public bool IsMentionedIn(string collection)
        {
            if (collection == null || collection == "[]")
                return false;

            foreach (var n in this.Names)
            {
                if (collection.IndexOf(n, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}