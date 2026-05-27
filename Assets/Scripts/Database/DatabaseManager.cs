using System.IO;
using SQLite;
using UnityEngine;

namespace EstateBuilder.Database
{
    public class DatabaseManager : MonoBehaviour
    {
        public static DatabaseManager Instance { get; private set; }

        private SQLiteConnection db;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitDatabase();
        }

        void InitDatabase()
        {
            string dbPath = Path.Combine(Application.persistentDataPath, "realestate.db");
            db = new SQLiteConnection(dbPath);
            Debug.Log($"[DB] Opened SQLite database at {dbPath}");

            CreateTables();
            SeedDataIfEmpty();
        }

        void CreateTables()
        {
            db.Execute(@"
                CREATE TABLE IF NOT EXISTS Player (
                    player_id     INTEGER PRIMARY KEY AUTOINCREMENT,
                    name          TEXT,
                    gender        TEXT,
                    cash_balance  REAL DEFAULT 10000,
                    net_worth     REAL DEFAULT 0,
                    game_day      INTEGER DEFAULT 1,
                    created_at    TEXT,
                    last_saved_at TEXT
                )");

            db.Execute(@"
                CREATE TABLE IF NOT EXISTS Property (
                    property_id    INTEGER PRIMARY KEY AUTOINCREMENT,
                    name           TEXT,
                    type           TEXT,
                    base_price     REAL,
                    zone           TEXT,
                    is_developable INTEGER DEFAULT 0,
                    asset_ref      TEXT
                )");

            db.Execute(@"
                CREATE TABLE IF NOT EXISTS PlayerProperty (
                    player_property_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    player_id          INTEGER,
                    property_id        INTEGER,
                    purchase_price     REAL,
                    current_value      REAL,
                    status             TEXT DEFAULT 'owned',
                    game_day_purchased INTEGER,
                    FOREIGN KEY(player_id)   REFERENCES Player(player_id),
                    FOREIGN KEY(property_id) REFERENCES Property(property_id)
                )");

            db.Execute(@"
                CREATE TABLE IF NOT EXISTS Upgrade (
                    upgrade_id     INTEGER PRIMARY KEY AUTOINCREMENT,
                    name           TEXT,
                    cost           REAL,
                    value_increase REAL,
                    duration_days  INTEGER
                )");

            db.Execute(@"
                CREATE TABLE IF NOT EXISTS PlayerPropertyUpgrade (
                    id                 INTEGER PRIMARY KEY AUTOINCREMENT,
                    player_property_id INTEGER,
                    upgrade_id         INTEGER,
                    applied_day        INTEGER,
                    complete_day       INTEGER,
                    is_complete        INTEGER DEFAULT 0,
                    FOREIGN KEY(player_property_id) REFERENCES PlayerProperty(player_property_id),
                    FOREIGN KEY(upgrade_id)         REFERENCES Upgrade(upgrade_id)
                )");

            db.Execute(@"
                CREATE TABLE IF NOT EXISTS RentContract (
                    rent_id            INTEGER PRIMARY KEY AUTOINCREMENT,
                    player_property_id INTEGER,
                    monthly_rent       REAL,
                    start_day          INTEGER,
                    end_day            INTEGER,
                    is_active          INTEGER DEFAULT 1,
                    FOREIGN KEY(player_property_id) REFERENCES PlayerProperty(player_property_id)
                )");

            db.Execute(@"
                CREATE TABLE IF NOT EXISTS [Transaction] (
                    transaction_id     INTEGER PRIMARY KEY AUTOINCREMENT,
                    player_property_id INTEGER,
                    type               TEXT,
                    amount             REAL,
                    game_day           INTEGER,
                    FOREIGN KEY(player_property_id) REFERENCES PlayerProperty(player_property_id)
                )");

            db.Execute(@"
                CREATE TABLE IF NOT EXISTS Development (
                    development_id     INTEGER PRIMARY KEY AUTOINCREMENT,
                    player_property_id INTEGER,
                    total_cost         REAL,
                    complete_day       INTEGER,
                    status             TEXT DEFAULT 'in_progress',
                    result_property_id INTEGER,
                    FOREIGN KEY(player_property_id)  REFERENCES PlayerProperty(player_property_id),
                    FOREIGN KEY(result_property_id)  REFERENCES Property(property_id)
                )");

            db.Execute(@"
                CREATE TABLE IF NOT EXISTS QuizCategory (
                    category_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name        TEXT
                )");

            db.Execute(@"
                CREATE TABLE IF NOT EXISTS QuizQuestion (
                    question_id        INTEGER PRIMARY KEY AUTOINCREMENT,
                    category_id        INTEGER,
                    question_type      TEXT,
                    question_text      TEXT,
                    difficulty         TEXT,
                    reward_cash        REAL DEFAULT 0,
                    reward_property_id INTEGER,
                    explanation        TEXT,
                    FOREIGN KEY(category_id)        REFERENCES QuizCategory(category_id),
                    FOREIGN KEY(reward_property_id) REFERENCES Property(property_id)
                )");

            db.Execute(@"
                CREATE TABLE IF NOT EXISTS QuizOption (
                    option_id   INTEGER PRIMARY KEY AUTOINCREMENT,
                    question_id INTEGER,
                    option_text TEXT,
                    is_correct  INTEGER DEFAULT 0,
                    FOREIGN KEY(question_id) REFERENCES QuizQuestion(question_id)
                )");

            db.Execute(@"
                CREATE TABLE IF NOT EXISTS QuizAttempt (
                    attempt_id         INTEGER PRIMARY KEY AUTOINCREMENT,
                    question_id        INTEGER,
                    selected_option_id INTEGER,
                    is_correct         INTEGER,
                    reward_given       INTEGER DEFAULT 0,
                    game_day           INTEGER,
                    FOREIGN KEY(question_id)        REFERENCES QuizQuestion(question_id),
                    FOREIGN KEY(selected_option_id) REFERENCES QuizOption(option_id)
                )");
        }

        void SeedDataIfEmpty()
        {
            int count = db.ExecuteScalar<int>("SELECT COUNT(*) FROM Property");
            if (count > 0) return;

            db.Execute(
                "INSERT INTO Property (name, type, base_price, zone, is_developable, asset_ref) VALUES (?, ?, ?, ?, ?, ?)",
                "Small House", "residential", 5000.0, "suburb", 0, "Prefabs/SmallHouse");

            db.Execute(
                "INSERT INTO Property (name, type, base_price, zone, is_developable, asset_ref) VALUES (?, ?, ?, ?, ?, ?)",
                "Empty Lot", "land", 2000.0, "suburb", 1, "Prefabs/EmptyLot");

            db.Execute(
                "INSERT INTO Property (name, type, base_price, zone, is_developable, asset_ref) VALUES (?, ?, ?, ?, ?, ?)",
                "Apartment Unit", "residential", 12000.0, "downtown", 0, "Prefabs/Apartment");
        }

        public SQLiteConnection GetDB() => db;

        void OnApplicationQuit()
        {
            db?.Close();
            db = null;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                db?.Close();
                db = null;
            }
        }
    }
}
