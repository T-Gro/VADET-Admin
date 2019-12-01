namespace KnnResults.Domain.Models
{
    public partial class NewProductLabels
    {
        static char[] PipeSplitter = new char[] { '|' };

        public string ProductId { get; set; }
        public string PipeDeliminitedEnglishNames { get; set; }

        public virtual string[] Names => PipeDeliminitedEnglishNames.Split(PipeSplitter);
    }
}