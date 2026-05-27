namespace EstateBuilder.Models
{
    public class PropertyData
    {
        public int    property_id    { get; set; }
        public string name           { get; set; }
        public string type           { get; set; }
        public double base_price     { get; set; }
        public string zone           { get; set; }
        public int    is_developable { get; set; }
        public string asset_ref      { get; set; }
    }
}
