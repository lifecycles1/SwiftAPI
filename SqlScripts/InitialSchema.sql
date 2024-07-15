CREATE TABLE IF NOT EXISTS SwiftMessages (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    BasicHeader TEXT NOT NULL,
    ApplicationHeader TEXT,
    UserHeader TEXT,
    TextBlock TEXT,
    Trailer TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS MT799 (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SwiftMessageId INTEGER NOT NULL,
--field20 (Transaction Reference Number) up to 16 characters, must not start or end with a slash "/", and contain 2 consecutive slashes "//"
    Field20 TEXT NOT NULL CHECK (LENGTH(Field20) <= 16 AND Field20 NOT LIKE '/%' AND Field20 NOT LIKE '%//' AND Field20 NOT LIKE '%/'),
--field21 (Related Reference) up to 16 characters
    Field21 TEXT CHECK (LENGTH(Field21) <= 16),
--field79 (Narrative) up to 35*50x (35 multiline each up to 50 characters) With the Narrative, there are no ‘Network Validated Rules’ for MT 799
    Field79 TEXT NOT NULL,
    FOREIGN KEY (SwiftMessageId) REFERENCES SwiftMessages(Id) ON DELETE CASCADE
);
