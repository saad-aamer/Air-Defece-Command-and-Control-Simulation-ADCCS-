BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS ActionStatuses (
    StatusId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT UNIQUE NOT NULL
);
CREATE TABLE IF NOT EXISTS ActionTypes (
    ActionTypeId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT UNIQUE NOT NULL
);
CREATE TABLE IF NOT EXISTS AlertLevels (
    AlertLevelId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT UNIQUE NOT NULL
);
CREATE TABLE IF NOT EXISTS AssetTypes (
    AssetTypeId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT UNIQUE NOT NULL
);
CREATE TABLE IF NOT EXISTS Assets (
    AssetId   INTEGER PRIMARY KEY AUTOINCREMENT,
    AssetName TEXT UNIQUE NOT NULL,
    AssetTypeId INTEGER NOT NULL,
    MaxSpeed    REAL,
    MaxRange    REAL,
    FOREIGN KEY (AssetTypeId) REFERENCES AssetTypes(AssetTypeId)
);
CREATE TABLE IF NOT EXISTS Classifications (
    ClassificationId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT UNIQUE NOT NULL
);
CREATE TABLE IF NOT EXISTS DefensiveActions (
    ActionId     INTEGER PRIMARY KEY AUTOINCREMENT,
    TargetId     INTEGER NOT NULL,
    ActionTypeId INTEGER NOT NULL,
    IssuedBy     INTEGER NOT NULL,
    IssuedAt     TEXT,
    StatusId     INTEGER,
    Notes        TEXT,
    CompletedAt  TEXT,
    AssetId      INTEGER REFERENCES Assets(AssetId),
    FOREIGN KEY (TargetId) REFERENCES Targets(TargetId),
    FOREIGN KEY (ActionTypeId) REFERENCES ActionTypes(ActionTypeId),
    FOREIGN KEY (IssuedBy) REFERENCES Users(UserId),
    FOREIGN KEY (StatusId) REFERENCES ActionStatuses(StatusId)
);
CREATE TABLE IF NOT EXISTS EventTypes (
    EventTypeId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT UNIQUE NOT NULL
);
CREATE TABLE IF NOT EXISTS MissionLogs (
    LogId       INTEGER PRIMARY KEY AUTOINCREMENT,
    MissionDate TEXT,
    EventTypeId INTEGER NOT NULL,
    Description TEXT NOT NULL,
    SeverityId  INTEGER,
    UserId      INTEGER,
    TargetId    INTEGER,
    ActionId    INTEGER,
    CreatedAt   TEXT,
    AssetId     INTEGER REFERENCES Assets(AssetId),
    FOREIGN KEY (EventTypeId) REFERENCES EventTypes(EventTypeId),
    FOREIGN KEY (SeverityId) REFERENCES SeverityLevels(SeverityId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (TargetId) REFERENCES Targets(TargetId),
    FOREIGN KEY (ActionId) REFERENCES DefensiveActions(ActionId)
);
CREATE TABLE IF NOT EXISTS RadarConfigurations (
    ConfigId INTEGER PRIMARY KEY AUTOINCREMENT,
    ConfigName TEXT NOT NULL,
    RadarRange REAL,
    ScanInterval INTEGER,
    MaxTargets INTEGER,
    AutoClassification INTEGER,
    AlertThreshold REAL,
    IsActive INTEGER,
    UpdatedBy INTEGER,
    UpdatedAt TEXT,
    FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId)
);
CREATE TABLE IF NOT EXISTS Roles (
    RoleId INTEGER PRIMARY KEY AUTOINCREMENT,
    RoleName TEXT UNIQUE NOT NULL
);
CREATE TABLE IF NOT EXISTS SeverityLevels (
    SeverityId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT UNIQUE NOT NULL
);
CREATE TABLE IF NOT EXISTS TargetTypes (
    TargetTypeId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT UNIQUE NOT NULL
);
CREATE TABLE IF NOT EXISTS Targets (
    TargetId         INTEGER PRIMARY KEY AUTOINCREMENT,
    TargetCode       TEXT UNIQUE NOT NULL,
    TargetTypeId     INTEGER NOT NULL,
    ClassificationId INTEGER,
    PositionX        REAL NOT NULL,
    PositionY        REAL NOT NULL,
    Speed            REAL NOT NULL,
    Altitude         REAL NOT NULL,
    Heading          REAL NOT NULL,
    DetectedAt       TEXT,
    LastUpdated      TEXT,
    IsActive         INTEGER,
    DetectedBy       INTEGER,
    AssetId          INTEGER REFERENCES Assets(AssetId),
    FOREIGN KEY (TargetTypeId) REFERENCES TargetTypes(TargetTypeId),
    FOREIGN KEY (ClassificationId) REFERENCES Classifications(ClassificationId),
    FOREIGN KEY (DetectedBy) REFERENCES Users(UserId)
);
CREATE TABLE IF NOT EXISTS ThreatAlerts (
    AlertId INTEGER PRIMARY KEY AUTOINCREMENT,
    TargetId INTEGER NOT NULL,
    AlertLevelId INTEGER NOT NULL,
    Message TEXT NOT NULL,
    IsAcknowledged INTEGER,
    AcknowledgedBy INTEGER,
    CreatedAt TEXT,
    AcknowledgedAt TEXT,
    FOREIGN KEY (TargetId) REFERENCES Targets(TargetId),
    FOREIGN KEY (AlertLevelId) REFERENCES AlertLevels(AlertLevelId),
    FOREIGN KEY (AcknowledgedBy) REFERENCES Users(UserId)
);
CREATE TABLE IF NOT EXISTS Users (
    UserId       INTEGER PRIMARY KEY AUTOINCREMENT,
    Username     TEXT UNIQUE NOT NULL,
    PasswordHash TEXT NOT NULL,
    RoleId       INTEGER NOT NULL,
    FullName     TEXT NOT NULL,
    Email        TEXT,
    IsActive     INTEGER,
    CreatedAt    TEXT,
    LastLogin    TEXT,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);
INSERT INTO "ActionStatuses" ("StatusId","Name") VALUES (1,'Pending'),
 (2,'Completed');
INSERT INTO "ActionTypes" ("ActionTypeId","Name") VALUES (1,'Scramble Jet'),
 (2,'Launch SAM'),
 (3,'Deploy Interceptor'),
 (4,'Track Only');
INSERT INTO "AlertLevels" ("AlertLevelId","Name") VALUES (2,'Low'),
 (3,'Moderate'),
 (4,'Medium'),
 (5,'High'),
 (6,'Critical');
INSERT INTO "AssetTypes" ("AssetTypeId","Name") VALUES (1,'Aircraft'),
 (2,'Missile'),
 (3,'Drone');
INSERT INTO "Assets" ("AssetId","AssetName","AssetTypeId","MaxSpeed","MaxRange") VALUES (1,'JF-17 Thunder Block III',1,1960.0,1352.0),
 (2,'F-16 Fighting Falcon',1,2410.0,1600.0),
 (3,'Mirage III ROSE',1,2350.0,1200.0),
 (4,'Mirage V Bahadur',1,2350.0,1300.0),
 (5,'F-7PG Skybolt',1,2175.0,1200.0),
 (6,'Saab 2000 AEW&C',1,680.0,3000.0),
 (7,'Erieye AEW&C',1,680.0,3200.0),
 (8,'K-8 Karakorum',1,800.0,800.0),
 (9,'Babur Cruise Missile',2,880.0,700.0),
 (10,'Ra''ad Air-Launched CM',2,950.0,350.0),
 (11,'Hatf-III Ghaznavi SRBM',2,5796.0,290.0),
 (12,'LY-80 (HQ-16) SAM',2,3000.0,40.0),
 (13,'Spada 2000 SAM',2,1400.0,25.0),
 (14,'AIM-120 AMRAAM',2,4248.0,160.0),
 (15,'SD-10A BVRAAM',2,4000.0,70.0),
 (16,'PL-5E II WVRAAM',2,3700.0,35.0),
 (17,'Burraq UCAV',3,463.0,500.0),
 (18,'Shahpar-I MALE UAV',3,180.0,750.0),
 (19,'Shahpar-II MALE UAV',3,220.0,1000.0),
 (20,'Uqab Tactical UAV',3,150.0,150.0),
 (21,'Jasoos Mini-UAV',3,80.0,50.0);
INSERT INTO "Classifications" ("ClassificationId","Name") VALUES (1,'Friendly'),
 (2,'Hostile'),
 (3,'Unknown');
INSERT INTO "EventTypes" ("EventTypeId","Name") VALUES (1,'User Login'),
 (2,'Target Detected'),
 (3,'Action Issued');
INSERT INTO "Roles" ("RoleId","RoleName") VALUES (1,'Admin'),
 (2,'Operator'),
 (3,'Commander');
INSERT INTO "SeverityLevels" ("SeverityId","Name") VALUES (1,'Info'),
 (2,'Warning');
INSERT INTO "TargetTypes" ("TargetTypeId","Name") VALUES (1,'Missile'),
 (2,'Helicopter'),
 (3,'Drone'),
 (4,'Aircraft');
INSERT INTO "Users" ("UserId","Username","PasswordHash","RoleId","FullName","Email","IsActive","CreatedAt","LastLogin") VALUES (1,'admin','$2a$11$usBTgOiRrJJFFklBsM4x9.gkHrILvPd7JVKJ7JtljqTBw0D802jdy',1,'System Administrator','admin@airdefence.com',1,'2025-11-23',NULL),
 (2,'operator1','hash',2,'John Operator','operator@airdefence.com',1,'2025-11-23',NULL),
 (3,'commander1','hash',3,'Jane Commander','commander@airdefence.com',1,'2025-11-23',NULL),
 (4,'saad','$2a$11$PQsLsWPSL5tVJXCDds3sTuhBnNtsaT9CnLv.fUCm76.ozpbw32TQ.',3,'Saad','saad@airdefence.com',1,'2026-04-29 17:27:15',NULL);
COMMIT;
