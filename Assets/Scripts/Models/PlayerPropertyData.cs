namespace EstateBuilder.Models
{
    public class PlayerPropertyData
    {
        public int    player_property_id  { get; set; }
        public int    player_id           { get; set; }
        public int    property_id         { get; set; }
        public double purchase_price      { get; set; }
        public double current_value       { get; set; }
        public string status              { get; set; }
        public int    game_day_purchased  { get; set; }
        public int    plot_id             { get; set; }
    }
}
