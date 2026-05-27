# Real Estate Simulator — Database Documentation

Unity 3D offline mobile game using SQLite for local data persistence.

---

## Table of Contents

1. [Overview](#overview)
2. [Database Schema](#database-schema)
   - [Player](#player)
   - [Property](#property)
   - [PlayerProperty](#playerproperty)
   - [Upgrade](#upgrade)
   - [PlayerPropertyUpgrade](#playerpropertyupgrade)
   - [RentContract](#rentcontract)
   - [Transaction](#transaction)
   - [Development](#development)
   - [QuizCategory](#quizcategory)
   - [QuizQuestion](#quizquestion)
   - [QuizOption](#quizoption)
   - [QuizAttempt](#quizattempt)
3. [Table Relationships](#table-relationships)
4. [Unity SQLite Integration](#unity-sqlite-integration)
   - [Project Structure](#project-structure)
   - [Plugin Setup](#plugin-setup)
   - [DatabaseManager](#databasemanager)
   - [Repositories](#repositories)
   - [Data Models](#data-models)
5. [Common Operations](#common-operations)
   - [Buy a Property](#buy-a-property)
   - [Record a Quiz Attempt](#record-a-quiz-attempt)
   - [Seed Static Data](#seed-static-data)
6. [Migration Guide](#migration-guide)

---

## Overview

This game stores three categories of data:

- **Player data** — save state, cash, progress
- **Property data** — catalog of all properties + what the player owns
- **Quiz data** — questions, options, and player attempts

Quizzes are connected to the simulation: answering correctly rewards the player with cash or unlocks specific properties.

---

## Database Schema

### Player

Stores the single player profile for the device.

| Column | Type | Description |
|---|---|---|
| `player_id` | INTEGER PK | Auto-incremented ID |
| `name` | TEXT | Player's name |
| `gender` | TEXT | `"male"`, `"female"`, or `"other"` |
| `cash_balance` | REAL | Current spendable cash |
| `net_worth` | REAL | Total calculated net worth |
| `game_day` | INTEGER | In-game day counter |
| `created_at` | TEXT | UTC datetime of profile creation |
| `last_saved_at` | TEXT | UTC datetime of last save |

```sql
CREATE TABLE IF NOT EXISTS Player (
    player_id     INTEGER PRIMARY KEY AUTOINCREMENT,
    name          TEXT,
    gender        TEXT,
    cash_balance  REAL DEFAULT 10000,
    net_worth     REAL DEFAULT 0,
    game_day      INTEGER DEFAULT 1,
    created_at    TEXT,
    last_saved_at TEXT
);
```

> Only one row ever exists in this table — one player per device.

---

### Property

The game's static catalog of all available properties. Pre-seeded on first launch, never modified at runtime.

| Column | Type | Description |
|---|---|---|
| `property_id` | INTEGER PK | Auto-incremented ID |
| `name` | TEXT | Display name |
| `type` | TEXT | e.g. `"residential"`, `"commercial"`, `"land"` |
| `base_price` | REAL | Starting market price |
| `zone` | TEXT | e.g. `"suburb"`, `"downtown"`, `"rural"` |
| `is_developable` | INTEGER | `1` if land can be developed, else `0` |
| `asset_ref` | TEXT | Unity prefab path e.g. `"Prefabs/SmallHouse"` |

```sql
CREATE TABLE IF NOT EXISTS Property (
    property_id    INTEGER PRIMARY KEY AUTOINCREMENT,
    name           TEXT,
    type           TEXT,
    base_price     REAL,
    zone           TEXT,
    is_developable INTEGER DEFAULT 0,
    asset_ref      TEXT
);
```

---

### PlayerProperty

Tracks which properties the player currently owns. Bridge table between `Player` and `Property`.

| Column | Type | Description |
|---|---|---|
| `player_property_id` | INTEGER PK | Auto-incremented ID |
| `player_id` | INTEGER FK | References `Player` |
| `property_id` | INTEGER FK | References `Property` |
| `purchase_price` | REAL | Price paid at time of purchase |
| `current_value` | REAL | Current market value (can change) |
| `status` | TEXT | See status values below |
| `game_day_purchased` | INTEGER | In-game day the property was bought |

**Valid `status` values:**

| Value | Meaning |
|---|---|
| `owned` | Player owns it, not active |
| `for_rent` | Listed for rent |
| `rented` | Currently rented out |
| `for_sale` | Listed for sale |
| `renovating` | Upgrade in progress |
| `developing` | Land development in progress |

```sql
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
);
```

---

### Upgrade

Catalog of all available upgrades/renovations. Static data, pre-seeded.

| Column | Type | Description |
|---|---|---|
| `upgrade_id` | INTEGER PK | Auto-incremented ID |
| `name` | TEXT | Display name e.g. `"New Roof"` |
| `cost` | REAL | Cash cost to apply |
| `value_increase` | REAL | How much it adds to property value |
| `duration_days` | INTEGER | In-game days to complete |

```sql
CREATE TABLE IF NOT EXISTS Upgrade (
    upgrade_id     INTEGER PRIMARY KEY AUTOINCREMENT,
    name           TEXT,
    cost           REAL,
    value_increase REAL,
    duration_days  INTEGER
);
```

---

### PlayerPropertyUpgrade

Tracks which upgrades have been applied to owned properties.

| Column | Type | Description |
|---|---|---|
| `id` | INTEGER PK | Auto-incremented ID |
| `player_property_id` | INTEGER FK | References `PlayerProperty` |
| `upgrade_id` | INTEGER FK | References `Upgrade` |
| `applied_day` | INTEGER | Day the upgrade started |
| `complete_day` | INTEGER | Day the upgrade finishes |
| `is_complete` | INTEGER | `1` when done, else `0` |

```sql
CREATE TABLE IF NOT EXISTS PlayerPropertyUpgrade (
    id                   INTEGER PRIMARY KEY AUTOINCREMENT,
    player_property_id   INTEGER,
    upgrade_id           INTEGER,
    applied_day          INTEGER,
    complete_day         INTEGER,
    is_complete          INTEGER DEFAULT 0,
    FOREIGN KEY(player_property_id) REFERENCES PlayerProperty(player_property_id),
    FOREIGN KEY(upgrade_id)         REFERENCES Upgrade(upgrade_id)
);
```

---

### RentContract

One active rent contract per rented property.

| Column | Type | Description |
|---|---|---|
| `rent_id` | INTEGER PK | Auto-incremented ID |
| `player_property_id` | INTEGER FK | References `PlayerProperty` |
| `monthly_rent` | REAL | Rent income per in-game month |
| `start_day` | INTEGER | Day rent started |
| `end_day` | INTEGER | Day rent ends |
| `is_active` | INTEGER | `1` if active, else `0` |

```sql
CREATE TABLE IF NOT EXISTS RentContract (
    rent_id              INTEGER PRIMARY KEY AUTOINCREMENT,
    player_property_id   INTEGER,
    monthly_rent         REAL,
    start_day            INTEGER,
    end_day              INTEGER,
    is_active            INTEGER DEFAULT 1,
    FOREIGN KEY(player_property_id) REFERENCES PlayerProperty(player_property_id)
);
```

---

### Transaction

Logs every financial event. Use this for a transaction history screen.

| Column | Type | Description |
|---|---|---|
| `transaction_id` | INTEGER PK | Auto-incremented ID |
| `player_property_id` | INTEGER FK | References `PlayerProperty` |
| `type` | TEXT | See type values below |
| `amount` | REAL | Cash amount (positive = income, negative = expense) |
| `game_day` | INTEGER | Day the transaction occurred |

**Valid `type` values:** `buy`, `sell`, `rent_income`, `upgrade_cost`, `dev_cost`, `quiz_reward`

```sql
CREATE TABLE IF NOT EXISTS [Transaction] (
    transaction_id       INTEGER PRIMARY KEY AUTOINCREMENT,
    player_property_id   INTEGER,
    type                 TEXT,
    amount               REAL,
    game_day             INTEGER,
    FOREIGN KEY(player_property_id) REFERENCES PlayerProperty(player_property_id)
);
```

---

### Development

Tracks land-to-property development projects.

| Column | Type | Description |
|---|---|---|
| `development_id` | INTEGER PK | Auto-incremented ID |
| `player_property_id` | INTEGER FK | The land being developed |
| `total_cost` | REAL | Total development cost |
| `complete_day` | INTEGER | Day the project finishes |
| `status` | TEXT | `"in_progress"` or `"complete"` |
| `result_property_id` | INTEGER FK | The `Property` it becomes when done |

```sql
CREATE TABLE IF NOT EXISTS Development (
    development_id       INTEGER PRIMARY KEY AUTOINCREMENT,
    player_property_id   INTEGER,
    total_cost           REAL,
    complete_day         INTEGER,
    status               TEXT DEFAULT 'in_progress',
    result_property_id   INTEGER,
    FOREIGN KEY(player_property_id)  REFERENCES PlayerProperty(player_property_id),
    FOREIGN KEY(result_property_id)  REFERENCES Property(property_id)
);
```

---

### QuizCategory

Groups quiz questions by topic.

| Column | Type | Description |
|---|---|---|
| `category_id` | INTEGER PK | Auto-incremented ID |
| `name` | TEXT | e.g. `"Real Estate Basics"`, `"Investment"` |

```sql
CREATE TABLE IF NOT EXISTS QuizCategory (
    category_id INTEGER PRIMARY KEY AUTOINCREMENT,
    name        TEXT
);
```

---

### QuizQuestion

Stores all quiz questions. Supports multiple choice and true/false.

| Column | Type | Description |
|---|---|---|
| `question_id` | INTEGER PK | Auto-incremented ID |
| `category_id` | INTEGER FK | References `QuizCategory` |
| `question_type` | TEXT | `"multiple_choice"` or `"true_false"` |
| `question_text` | TEXT | The question |
| `difficulty` | TEXT | `"easy"`, `"medium"`, `"hard"` |
| `reward_cash` | REAL | Cash awarded on correct answer |
| `reward_property_id` | INTEGER FK | Property unlocked on correct answer (nullable) |
| `explanation` | TEXT | Shown after answering |

```sql
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
);
```

---

### QuizOption

Stores answer choices for each question.

| Column | Type | Description |
|---|---|---|
| `option_id` | INTEGER PK | Auto-incremented ID |
| `question_id` | INTEGER FK | References `QuizQuestion` |
| `option_text` | TEXT | The answer choice |
| `is_correct` | INTEGER | `1` if correct, else `0` |

```sql
CREATE TABLE IF NOT EXISTS QuizOption (
    option_id   INTEGER PRIMARY KEY AUTOINCREMENT,
    question_id INTEGER,
    option_text TEXT,
    is_correct  INTEGER DEFAULT 0,
    FOREIGN KEY(question_id) REFERENCES QuizQuestion(question_id)
);
```

---

### QuizAttempt

Records every answer the player makes. `reward_given` prevents double-rewarding.

| Column | Type | Description |
|---|---|---|
| `attempt_id` | INTEGER PK | Auto-incremented ID |
| `question_id` | INTEGER FK | References `QuizQuestion` |
| `selected_option_id` | INTEGER FK | References `QuizOption` |
| `is_correct` | INTEGER | `1` if the answer was correct |
| `reward_given` | INTEGER | `1` once reward has been given |
| `game_day` | INTEGER | Day the attempt was made |

```sql
CREATE TABLE IF NOT EXISTS QuizAttempt (
    attempt_id         INTEGER PRIMARY KEY AUTOINCREMENT,
    question_id        INTEGER,
    selected_option_id INTEGER,
    is_correct         INTEGER,
    reward_given       INTEGER DEFAULT 0,
    game_day           INTEGER,
    FOREIGN KEY(question_id)        REFERENCES QuizQuestion(question_id),
    FOREIGN KEY(selected_option_id) REFERENCES QuizOption(option_id)
);
```

---

## Table Relationships

```
Player ──< PlayerProperty >── Property
                │
                ├──< PlayerPropertyUpgrade >── Upgrade
                ├──< RentContract
                ├──< Transaction
                └──< Development ──> Property (result)

QuizCategory ──< QuizQuestion >── Property (reward, nullable)
                     │
                     └──< QuizOption
                               │
                               └──< QuizAttempt
```

---

## Unity SQLite Integration

### Project Structure

```
Assets/
  Scripts/
    Database/
      DatabaseManager.cs      ← singleton, opens/closes DB
      PlayerRepository.cs     ← player CRUD
      PropertyRepository.cs   ← buy, sell, rent logic
      QuizRepository.cs       ← quiz attempts and rewards
    Models/
      PlayerData.cs
      PropertyData.cs
      PlayerPropertyData.cs
      QuizQuestionData.cs
      QuizOptionData.cs
  Plugins/
    SQLite.cs
    SQLiteAsync.cs
```

---

### Plugin Setup

Unity does not include SQLite by default. Download `sqlite-net-pcl` and place these two files in `Assets/Plugins/`:

- `SQLite.cs`
- `SQLiteAsync.cs`

Source: https://github.com/praeclarum/sqlite-net

---

### DatabaseManager

Singleton that opens the database connection and creates all tables on first launch.

```csharp
using SQLite;
using UnityEngine;
using System.IO;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance;
    private SQLiteConnection db;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitDatabase();
        }
        else Destroy(gameObject);
    }

    void InitDatabase()
    {
        string dbPath = Path.Combine(Application.persistentDataPath, "realestate.db");
        db = new SQLiteConnection(dbPath);
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
            CREATE TABLE IF NOT EXISTS QuizQuestion (
                question_id        INTEGER PRIMARY KEY AUTOINCREMENT,
                category_id        INTEGER,
                question_type      TEXT,
                question_text      TEXT,
                difficulty         TEXT,
                reward_cash        REAL DEFAULT 0,
                reward_property_id INTEGER,
                explanation        TEXT
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
                game_day           INTEGER
            )");

        db.Execute(@"
            CREATE TABLE IF NOT EXISTS [Transaction] (
                transaction_id     INTEGER PRIMARY KEY AUTOINCREMENT,
                player_property_id INTEGER,
                type               TEXT,
                amount             REAL,
                game_day           INTEGER
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

    void OnApplicationQuit() => db?.Close();
}
```

> Always use `Application.persistentDataPath` for the DB path. Never use `Application.dataPath` for saves — it is read-only on Android and iOS.

---

### Repositories

Keep all database logic out of your game scripts. One repository per feature area.

**PlayerRepository.cs**

```csharp
using SQLite;
using System;

public class PlayerRepository
{
    private SQLiteConnection db;

    public PlayerRepository()
    {
        db = DatabaseManager.Instance.GetDB();
    }

    public void CreatePlayer(string name, string gender)
    {
        db.Execute(
            "INSERT INTO Player (name, gender, cash_balance, net_worth, game_day, created_at, last_saved_at) VALUES (?, ?, ?, ?, ?, ?, ?)",
            name, gender, 10000.0, 0.0, 1,
            DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString()
        );
    }

    public PlayerData GetPlayer()
    {
        return db.Query<PlayerData>("SELECT * FROM Player LIMIT 1")[0];
    }

    public void UpdateCash(int playerId, double newBalance)
    {
        db.Execute(
            "UPDATE Player SET cash_balance = ?, last_saved_at = ? WHERE player_id = ?",
            newBalance, DateTime.UtcNow.ToString(), playerId
        );
    }

    public void UpdateNetWorth(int playerId, double newNetWorth)
    {
        db.Execute(
            "UPDATE Player SET net_worth = ? WHERE player_id = ?",
            newNetWorth, playerId
        );
    }

    public void AdvanceDay(int playerId)
    {
        db.Execute(
            "UPDATE Player SET game_day = game_day + 1 WHERE player_id = ?",
            playerId
        );
    }
}
```

**PropertyRepository.cs**

```csharp
using SQLite;
using System.Collections.Generic;

public class PropertyRepository
{
    private SQLiteConnection db;

    public PropertyRepository()
    {
        db = DatabaseManager.Instance.GetDB();
    }

    public List<PropertyData> GetAllProperties()
    {
        return db.Query<PropertyData>("SELECT * FROM Property");
    }

    public List<PlayerPropertyData> GetOwnedProperties(int playerId)
    {
        return db.Query<PlayerPropertyData>(
            "SELECT * FROM PlayerProperty WHERE player_id = ?", playerId);
    }

    public bool BuyProperty(int playerId, int propertyId, double price, int currentDay)
    {
        var player = db.Query<PlayerData>("SELECT * FROM Player WHERE player_id = ?", playerId)[0];
        if (player.cash_balance < price) return false;

        db.Execute(
            "UPDATE Player SET cash_balance = cash_balance - ? WHERE player_id = ?",
            price, playerId);

        db.Execute(
            "INSERT INTO PlayerProperty (player_id, property_id, purchase_price, current_value, status, game_day_purchased) VALUES (?, ?, ?, ?, 'owned', ?)",
            playerId, propertyId, price, price, currentDay);

        db.Execute(
            "INSERT INTO [Transaction] (player_property_id, type, amount, game_day) VALUES (last_insert_rowid(), 'buy', ?, ?)",
            price, currentDay);

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
```

**QuizRepository.cs**

```csharp
using SQLite;
using System.Collections.Generic;

public class QuizRepository
{
    private SQLiteConnection db;

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

    public void RecordAttempt(int questionId, int selectedOptionId, int currentDay)
    {
        var option = db.Query<QuizOptionData>(
            "SELECT * FROM QuizOption WHERE option_id = ?", selectedOptionId)[0];

        bool correct = option.is_correct == 1;

        db.Execute(
            "INSERT INTO QuizAttempt (question_id, selected_option_id, is_correct, reward_given, game_day) VALUES (?, ?, ?, 0, ?)",
            questionId, selectedOptionId, correct ? 1 : 0, currentDay);

        if (correct)
        {
            var question = db.Query<QuizQuestionData>(
                "SELECT * FROM QuizQuestion WHERE question_id = ?", questionId)[0];

            if (question.reward_cash > 0)
            {
                db.Execute(
                    "UPDATE Player SET cash_balance = cash_balance + ? WHERE player_id = 1",
                    question.reward_cash);
            }

            db.Execute(
                @"UPDATE QuizAttempt SET reward_given = 1
                  WHERE attempt_id = (
                      SELECT attempt_id FROM QuizAttempt
                      WHERE question_id = ?
                      ORDER BY attempt_id DESC LIMIT 1
                  )", questionId);
        }
    }
}
```

---

### Data Models

```csharp
// PlayerData.cs
public class PlayerData
{
    public int    player_id     { get; set; }
    public string name          { get; set; }
    public string gender        { get; set; }
    public double cash_balance  { get; set; }
    public double net_worth     { get; set; }
    public int    game_day      { get; set; }
}

// PropertyData.cs
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

// PlayerPropertyData.cs
public class PlayerPropertyData
{
    public int    player_property_id  { get; set; }
    public int    player_id           { get; set; }
    public int    property_id         { get; set; }
    public double purchase_price      { get; set; }
    public double current_value       { get; set; }
    public string status              { get; set; }
    public int    game_day_purchased  { get; set; }
}

// QuizQuestionData.cs
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

// QuizOptionData.cs
public class QuizOptionData
{
    public int    option_id   { get; set; }
    public int    question_id { get; set; }
    public string option_text { get; set; }
    public int    is_correct  { get; set; }
}
```

---

## Common Operations

### Buy a Property

```csharp
var repo = new PropertyRepository();
bool success = repo.BuyProperty(playerId: 1, propertyId: 2, price: 5000.0, currentDay: 14);

if (!success)
    Debug.Log("Not enough cash!");
```

### Record a Quiz Attempt

```csharp
var repo = new QuizRepository();
repo.RecordAttempt(questionId: 3, selectedOptionId: 7, currentDay: 14);
// Reward is applied automatically if the answer is correct
```

### Seed Static Data

Called once inside `DatabaseManager.InitDatabase()`. Checks row count first to avoid duplicate inserts on every launch.

```csharp
void SeedDataIfEmpty()
{
    int count = db.ExecuteScalar<int>("SELECT COUNT(*) FROM Property");
    if (count > 0) return;

    // Insert properties, upgrades, quiz questions, and options here
}
```

---

## Migration Guide

If you already have a `realestate.db` on a device and need to add a column, use `ALTER TABLE` instead of recreating the table — recreating it destroys existing save data.

**Add `gender` to an existing Player table:**

```csharp
db.Execute("ALTER TABLE Player ADD COLUMN gender TEXT");
```

Run this once on app launch, guarded by a version check:

```csharp
int dbVersion = db.ExecuteScalar<int>("PRAGMA user_version");

if (dbVersion < 2)
{
    db.Execute("ALTER TABLE Player ADD COLUMN gender TEXT");
    db.Execute("PRAGMA user_version = 2");
}
```

Increment `user_version` each time you make a schema change. This acts as a simple migration version tracker built into SQLite.
