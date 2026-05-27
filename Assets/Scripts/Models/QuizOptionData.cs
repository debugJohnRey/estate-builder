namespace EstateBuilder.Models
{
    public class QuizOptionData
    {
        public int    option_id   { get; set; }
        public int    question_id { get; set; }
        public string option_text { get; set; }
        public int    is_correct  { get; set; }
    }
}
