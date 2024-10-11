namespace JsonDatabase.DTO
{
    public class DivisionRuleDTO
    {
        public Guid Id { get; set; }
        public string UnitName { get; set; }
        public string DivisionName { get; set; }
        public int Count { get; set; }
        public bool IsValid => !string.IsNullOrEmpty(UnitName) && !string.IsNullOrEmpty(DivisionName) && Count > 0;
    }
}
