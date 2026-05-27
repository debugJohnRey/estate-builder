namespace EstateBuilder.Models
{
    public class QuizQuestionData
    {
        public int    question_id        { get; set; }
        public int    category_id        { get; set; }
        public string question_type      { get; set; }
        public string question_text      { get; set; }
        public string difficulty         { get; set; }
        public double reward_cash        { get; set; }
        public int    reward_property_id { get; set; }
        public string explanation        { get; set; }
    }
}
