using System;
using System.Collections.Generic;
using EstateBuilder.Models;
using SQLite;

namespace EstateBuilder.Database
{
    public class PlayerRepository
    {
        private readonly SQLiteConnection db;

        public PlayerRepository()
        {
            db = DatabaseManager.Instance.GetDB();
        }

        public void CreatePlayer(string name, string gender)
        {
            string now = DateTime.UtcNow.ToString("o");
            db.Execute(
                "INSERT INTO Player (name, gender, cash_balance, net_worth, game_day, created_at, last_saved_at) VALUES (?, ?, ?, ?, ?, ?, ?)",
                name, gender, 10000.0, 0.0, 1, now, now);
        }

        public PlayerData GetPlayer()
        {
            List<PlayerData> rows = db.Query<PlayerData>("SELECT * FROM Player LIMIT 1");
            return rows.Count > 0 ? rows[0] : null;
        }

        public void UpdateCash(int playerId, double newBalance)
        {
            db.Execute(
                "UPDATE Player SET cash_balance = ?, last_saved_at = ? WHERE player_id = ?",
                newBalance, DateTime.UtcNow.ToString("o"), playerId);
        }

        public void UpdateNetWorth(int playerId, double newNetWorth)
        {
            db.Execute(
                "UPDATE Player SET net_worth = ? WHERE player_id = ?",
                newNetWorth, playerId);
        }

        public void AdvanceDay(int playerId)
        {
            db.Execute(
                "UPDATE Player SET game_day = game_day + 1 WHERE player_id = ?",
                playerId);
        }
    }
}
