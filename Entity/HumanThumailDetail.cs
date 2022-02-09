using SqlSugar;

namespace FluUrl.Entity
{
    [SugarTable("humanthumaildetail")]
    public class HumanThumailDetail
    {
        [SugarColumn(IsPrimaryKey =true,IsIdentity =true)]
        public int Id { get; set; }
        [SugarColumn(ColumnName ="name")]
        public string Name { get; set; }
        [SugarColumn(ColumnName ="humanid")]
        public int HumanId { get; set; }
        [SugarColumn(ColumnName ="firstimg")]
        public string FirstImg { get; set; }
        [SugarColumn(ColumnName ="thumailid")]
        public int ThumailId { get; set; }
    }
}
