using SqlSugar;

namespace FluUrl.Entity
{
    [SugarTable("thumailinfo")]
    public class ThumailInfo
    {
        [SugarColumn(IsPrimaryKey =true,IsIdentity =true)]
        public int Id { get; set; }
        [SugarColumn(ColumnName ="thumailid")]
        public int ThumailId { get; set; }
        [SugarColumn(ColumnName ="thumailname")]
        public string ThumailName { get; set; }
    }
}
