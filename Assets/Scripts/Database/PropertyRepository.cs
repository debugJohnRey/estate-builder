using System.Collections.Generic;
using EstateBuilder.Models;
using SQLite;

namespace EstateBuilder.Database
{
    public class PropertyRepository
    {
        private readonly SQLiteConnection db;

        public PropertyRepository()
        {
            db = DatabaseManager.Instance.GetDB();
        }

        public List<PropertyData> GetAllProperties()
        {
            return db.Query<PropertyData>("SELECT * FROM Property");
        }

        public PropertyData GetProperty(int propertyId)
        {
            List<PropertyData> rows = db.Query<PropertyData>(
                "SELECT * FROM Property WHERE property_id = ?", propertyId);
            return rows.Count > 0 ? rows[0] : null;
        }

        public List<PlayerPropertyData> GetOwnedProperties(int playerId)
        {
            return db.Query<PlayerPropertyData>(
                "SELECT * FROM PlayerProperty WHERE player_id = ?", playerId);
        }

        public bool BuyProperty(int playerId, int propertyId, double price, int currentDay)
        {
            List<PlayerData> matches = db.Query<PlayerData>(
                "SELECT * FROM Player WHERE player_id = ?", playerId);
            if (matches.Count == 0) return false;
            if (matches[0].cash_balance < price) return false;

            db.Execute(
                "UPDATE Player SET cash_balance = cash_balance - ? WHERE player_id = ?",
                price, playerId);

            db.Execute(
                "INSERT INTO PlayerProperty (player_id, property_id, purchase_price, current_value, status, game_day_purchased) VALUES (?, ?, ?, ?, 'owned', ?)",
                playerId, propertyId, price, price, currentDay);

            db.Execute(
                "INSERT INTO [Transaction] (player_property_id, type, amount, game_day) VALUES (last_insert_rowid(), 'buy', ?, ?)",
                -price, currentDay);

            return true;
        }

        public void SetRented(int playerPropertyId, double monthlyRent, int startDay, int endDay)
        {
            db.Execute(
                "UPDATE PlayerProperty SET status = 'rented' WHERE player_property_id = ?",
                playerPropertyId);

            db.Execute(
                "INSERT INTO RentContract (player_property_id, monthly_rent, start_day, end_day, is_active) VALUES (?, ?, ?, ?, 1)",
                playerPropertyId, monthlyRent, startDay, endDay);
        }

        public bool SellProperty(int playerId, int playerPropertyId, double salePrice, int currentDay)
        {
            db.Execute(
                "UPDATE Player SET cash_balance = cash_balance + ? WHERE player_id = ?",
                salePrice, playerId);

            db.Execute(
                "INSERT INTO [Transaction] (player_property_id, type, amount, game_day) VALUES (?, 'sell', ?, ?)",
                playerPropertyId, salePrice, currentDay);

            db.Execute(
                "DELETE FROM PlayerProperty WHERE player_property_id = ?",
                playerPropertyId);

            return true;
        }
    }
}
