# AGENT INSTRUCTION: Property Table Seed Data

## Purpose
This file tells the AI agent exactly what data to insert into the `Property` table, what files to create, and what code to write. Follow every step exactly as written.

---

## Step 1 — Verify the Property Table Schema

Before inserting data, confirm the `Property` table has these exact columns. If any are missing, add them via migration.

**Required columns:**

| Column | SQLite Type | Default |
|---|---|---|
| `property_id` | INTEGER PRIMARY KEY AUTOINCREMENT | — |
| `name` | TEXT | — |
| `description` | TEXT | — |
| `type` | TEXT | — |
| `base_price` | REAL | — |
| `zone` | TEXT | — |
| `is_developable` | INTEGER | 0 |
| `asset_ref` | TEXT | — |
| `image_ref` | TEXT | — |

**CREATE TABLE statement to use in `DatabaseManager.cs`:**

```csharp
db.Execute(@"
    CREATE TABLE IF NOT EXISTS Property (
        property_id    INTEGER PRIMARY KEY AUTOINCREMENT,
        name           TEXT,
        description    TEXT,
        type           TEXT,
        base_price     REAL,
        zone           TEXT,
        is_developable INTEGER DEFAULT 0,
        asset_ref      TEXT,
        image_ref      TEXT
    )");
```

**If the table already exists, run this migration inside `DatabaseManager.cs`:**

```csharp
int dbVersion = db.ExecuteScalar<int>("PRAGMA user_version");

if (dbVersion < 2)
{
    db.Execute("ALTER TABLE Property ADD COLUMN description TEXT");
    db.Execute("ALTER TABLE Property ADD COLUMN image_ref TEXT");
    db.Execute("PRAGMA user_version = 2");
}
```

---

## Step 2 — Verify the PropertyData Model

The model file is located at `Assets/Scripts/Models/PropertyData.cs` inside the `EstateBuilder.Models` namespace.

Make sure it has exactly these fields:

```csharp
namespace EstateBuilder.Models
{
    public class PropertyData
    {
        public int    property_id    { get; set; }
        public string name           { get; set; }
        public string description    { get; set; }
        public string type           { get; set; }
        public double base_price     { get; set; }
        public string zone           { get; set; }
        public string is_developable { get; set; }
        public string asset_ref      { get; set; }
        public string image_ref      { get; set; }
    }
}
```

If any field is missing, add it. Do not remove or rename existing fields.

---

## Step 3 — Create the Folder Structure

### 3D Prefab Folder
The 3D prefab models are stored here. This folder already exists.

```
Assets/Prefabs/Properties/
```

Confirm these prefab files exist inside it:

```
Assets/Prefabs/Properties/TinyHouse.prefab
Assets/Prefabs/Properties/BasicFamily.prefab
Assets/Prefabs/Properties/ModernHome.prefab
```

### 2D Sprite Folder
This folder does not exist yet. Create it.

```
Assets/Sprites/Properties/
```

To create it in Unity:
1. In the Project panel, right-click `Assets`
2. Click `Create → Folder` → name it `Sprites`
3. Right-click `Sprites` → `Create → Folder` → name it `Properties`

Then place these image files inside it:

```
Assets/Sprites/Properties/tiny_house.png
Assets/Sprites/Properties/basic_family.png
Assets/Sprites/Properties/modern_home.png
```

---

## Step 4 — Property Data to Insert

Insert exactly these 3 rows. Do not change values unless explicitly instructed.

### Property 1
| Field | Value |
|---|---|
| `name` | `Tiny House` |
| `description` | `A tiny, bare-bones house tucked in a busy urban street. Small space, big opportunity.` |
| `type` | `single_family` |
| `base_price` | `50000.0` |
| `zone` | `urban` |
| `is_developable` | `0` |
| `asset_ref` | `Prefabs/Properties/TinyHouse` |
| `image_ref` | `Sprites/Properties/tiny_house` |

### Property 2
| Field | Value |
|---|---|
| `name` | `Basic Family Home` |
| `description` | `A simple three-bedroom house in a quiet city neighborhood. Reliable and low maintenance.` |
| `type` | `single_family` |
| `base_price` | `190000.0` |
| `zone` | `urban` |
| `is_developable` | `0` |
| `asset_ref` | `Prefabs/Properties/BasicFamily` |
| `image_ref` | `Sprites/Properties/basic_family` |

### Property 3
| Field | Value |
|---|---|
| `name` | `Modern City Home` |
| `description` | `A newly built modern home with clean architecture and quality finishes.` |
| `type` | `single_family` |
| `base_price` | `480000.0` |
| `zone` | `urban` |
| `is_developable` | `0` |
| `asset_ref` | `Prefabs/Properties/ModernHome` |
| `image_ref` | `Sprites/Properties/modern_home` |

---

## Step 5 — Write the Seed Method

Add this method to `DatabaseManager.cs`. Call it at the end of `InitDatabase()`.

```csharp
void SeedDataIfEmpty()
{
    int count = db.ExecuteScalar<int>("SELECT COUNT(*) FROM Property");
    if (count > 0) return;

    db.Execute(
        "INSERT INTO Property (name, description, type, base_price, zone, is_developable, asset_ref, image_ref) VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
        "Tiny House",
        "A tiny, bare-bones house tucked in a busy urban street. Small space, big opportunity.",
        "single_family", 50000.0, "urban", 0,
        "Prefabs/Properties/TinyHouse", "Sprites/Properties/tiny_house");

    db.Execute(
        "INSERT INTO Property (name, description, type, base_price, zone, is_developable, asset_ref, image_ref) VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
        "Basic Family Home",
        "A simple three-bedroom house in a quiet city neighborhood. Reliable and low maintenance.",
        "single_family", 190000.0, "urban", 0,
        "Prefabs/Properties/BasicFamily", "Sprites/Properties/basic_family");

    db.Execute(
        "INSERT INTO Property (name, description, type, base_price, zone, is_developable, asset_ref, image_ref) VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
        "Modern City Home",
        "A newly built modern home with clean architecture and quality finishes.",
        "single_family", 480000.0, "urban", 0,
        "Prefabs/Properties/ModernHome", "Sprites/Properties/modern_home");
}
```

Call it inside `InitDatabase()`:

```csharp
void InitDatabase()
{
    string dbPath = Path.Combine(Application.persistentDataPath, "realestate.db");
    db = new SQLiteConnection(dbPath);
    CreateTables();
    SeedDataIfEmpty(); // must be called after CreateTables()
}
```

---

## Step 6 — Verify After Seeding

After running the game, confirm the data was inserted correctly by running this query:

```sql
SELECT * FROM Property;
```

Expected result: exactly 3 rows with the values defined in Step 4.

If the table has 0 rows, `SeedDataIfEmpty()` was not called. Check that it is called inside `InitDatabase()` after `CreateTables()`.

If the table has duplicate rows, the row count check failed. Confirm the check `SELECT COUNT(*) FROM Property` runs before any INSERT.

---

## Rules for the Agent

- Do not insert more than 3 rows into the `Property` table.
- Do not change `asset_ref` or `image_ref` paths. Use exactly the values in Step 4.
- Do not remove the `if (count > 0) return;` guard in `SeedDataIfEmpty()`.
- Do not use `Resources.Load` — the project does not have a `Resources` folder.
- If a file already exists, do not overwrite it. Read it first, then add only what is missing.
