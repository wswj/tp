using SqlSugar;

namespace FluUrl.Entity
{
    [SugarTable("thumail")]
    public class Thumail
    {
        [SugarColumn(IsIdentity =true,IsPrimaryKey =true)]
        public int Id { get; set; }
        [SugarColumn(ColumnName ="humanid")]
        public int HumanId { get; set; }
        [SugarColumn(ColumnName ="thumailid")]
        public int ThumailId { get; set; }
        public string ThumailName { get; set; }
    }
}
