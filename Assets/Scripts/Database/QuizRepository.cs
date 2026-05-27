using System.Collections.Generic;
using EstateBuilder.Models;
using SQLite;

namespace EstateBuilder.Database
{
    public class QuizRepository
    {
        private readonly SQLiteConnection db;

        public QuizRepository()
        {
            db = DatabaseManager.Instance.GetDB();
        }

        public List<QuizQuestionData> GetQuestionsByCategory(int categoryId)
        {
            return db.Query<QuizQuestionData>(
                "SELECT * FROM QuizQuestion WHERE category_id = ?", categoryId);
        }

        public List<QuizOptionData> GetOptions(int questionId)
        {
            return db.Query<QuizOptionData>(
                "SELECT * FROM QuizOption WHERE question_id = ?", questionId);
        }

        public bool RecordAttempt(int playerId, int questionId, int selectedOptionId, int currentDay)
        {
            List<QuizOptionData> opts = db.Query<QuizOptionData>(
                "SELECT * FROM QuizOption WHERE option_id = ?", selectedOptionId);
            if (opts.Count == 0) return false;

            bool correct = opts[0].is_correct == 1;

            db.Execute(
                "INSERT INTO QuizAttempt (question_id, selected_option_id, is_correct, reward_given, game_day) VALUES (?, ?, ?, 0, ?)",
                questionId, selectedOptionId, correct ? 1 : 0, currentDay);

            int attemptId = db.ExecuteScalar<int>("SELECT last_insert_rowid()");

            if (!correct) return false;

            List<QuizQuestionData> questions = db.Query<QuizQuestionData>(
                "SELECT * FROM QuizQuestion WHERE question_id = ?", questionId);
            if (questions.Count == 0) return true;

            QuizQuestionData q = questions[0];

            if (q.reward_cash > 0)
            {
                db.Execute(
                    "UPDATE Player SET cash_balance = cash_balance + ? WHERE player_id = ?",
                    q.reward_cash, playerId);

                db.Execute(
                    "INSERT INTO [Transaction] (player_property_id, type, amount, game_day) VALUES (NULL, 'quiz_reward', ?, ?)",
                    q.reward_cash, currentDay);
            }

            db.Execute(
                "UPDATE QuizAttempt SET reward_given = 1 WHERE attempt_id = ?", attemptId);

            return true;
        }
    }
}
