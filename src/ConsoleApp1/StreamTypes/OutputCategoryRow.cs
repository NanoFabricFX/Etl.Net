using Paillave.Etl.Helpers;

namespace ConsoleApp1.StreamTypes
{
    public class OutputCategoryRow
    {
        public string Category { get; set; }
        public int TotalAmount { get; set; }
        public int AmountOfEntries { get; set; }
    }
    public class OutputCategoryRowMapper : ColumnNameFlatFileDescriptor<OutputCategoryRow>
    {
        public OutputCategoryRowMapper()
        {
            this.MapColumnToProperty("Category", i => i.Category);
            this.MapColumnToProperty("Nb", i => i.AmountOfEntries);
            this.MapColumnToProperty("Tot", i => i.TotalAmount);
            this.IsFieldDelimited(',');
        }
    }
}
