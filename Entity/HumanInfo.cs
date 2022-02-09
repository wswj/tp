using SqlSugar;

namespace FluUrl.Entity
{
    [SugarTable("humaninfo")]
    public class HumanInfo
    {
        public HumanInfo() { }
        [SugarColumn(IsPrimaryKey =true,IsIdentity =true)]
        public int Id { get; set; }
        [SugarColumn(ColumnName ="name")]
        public string Name { get; set; }
        [SugarColumn(ColumnName ="description")]
        public string Description { get; set; }
        [SugarColumn(ColumnName ="humanid")]
        public int HumanId { get; set; }
        public string? birthday { get; set; }
        public string? xz { get; set; }
        public string? sx { get; set; }
        public string? sg { get; set; }
        public string? weight { get; set; }
        public int? totalThumail { get; set; }

    }
}
