namespace EstateBuilder.Models
{
    public class PlayerData
    {
        public int    player_id     { get; set; }
        public string name          { get; set; }
        public string gender        { get; set; }
        public double cash_balance  { get; set; }
        public double net_worth     { get; set; }
        public int    game_day      { get; set; }
        public string created_at    { get; set; }
        public string last_saved_at { get; set; }
    }
}
