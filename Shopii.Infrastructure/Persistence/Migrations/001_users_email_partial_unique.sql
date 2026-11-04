-- =============================================================================
-- One-time migration: make Users.Email unique only for live (non-deleted) rows.
--
-- Why: a deleted user is now soft-deleted (row kept, DeletedAt stamped) so that
-- historical operations that reference the user id stay resolvable. The row
-- keeps its real email, which means the original inline UNIQUE(Email) would
-- block that address from ever being registered again. Replacing it with a
-- partial unique index lets the archived row keep its real email while a new
-- account can still register the same address.
--
-- SQLite cannot drop a column-level UNIQUE constraint in place, so the Users
-- table must be rebuilt. Run this against the SQLite database
-- (appsettings.json -> ConnectionStrings:DefaultConnection)
-- with the API STOPPED. Take a backup copy of the .db file first.
--
-- All statements must be executed on a single connection, and no other
-- connection may be attached, otherwise the drop/rename will fail.
-- =============================================================================

PRAGMA foreign_keys = OFF;

BEGIN TRANSACTION;

-- 0. Clean up the staging table in case an earlier attempt was interrupted.
DROP TABLE IF EXISTS "Users_new";

-- 1. New table, identical to the current one but WITHOUT the inline
--    UNIQUE on Email, and with the soft-delete + audit columns.
CREATE TABLE "Users_new" (
    "UserId"         TEXT NOT NULL,
    "Email"          TEXT NOT NULL,               -- uniqueness handled by index below
    "PasswordHash"   TEXT NOT NULL,
    "Salt"           TEXT NOT NULL,
    "RoleId"         INTEGER NOT NULL,
    "IsVerified"     INTEGER NOT NULL DEFAULT 0 CHECK("IsVerified" IN (0, 1)),
    "LastLogin"      DATETIME,
    "CreatedAt"      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt"      DATETIME DEFAULT CURRENT_TIMESTAMP,
    "DeletedAt"      DATETIME,
    "DeletedByUserId" TEXT,
    PRIMARY KEY("UserId"),
    FOREIGN KEY("RoleId") REFERENCES "User_Roles"("RoleId")
);

-- 2. Copy every row, preserving all existing values.
INSERT INTO "Users_new" (
    "UserId", "Email", "PasswordHash", "Salt", "RoleId",
    "IsVerified", "LastLogin", "CreatedAt", "UpdatedAt",
    "DeletedAt", "DeletedByUserId"
)
SELECT
    "UserId", "Email", "PasswordHash", "Salt", "RoleId",
    "IsVerified", "LastLogin", "CreatedAt", "UpdatedAt",
    "DeletedAt", "DeletedByUserId"
FROM "Users";

-- 3. Swap the tables.
DROP TABLE "Users";
ALTER TABLE "Users_new" RENAME TO "Users";

-- 4. Email is unique among live users only. Two deleted users may share an
--    email, and a deleted user's email does not block a new registration.
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email_Live"
    ON "Users"("Email")
    WHERE "DeletedAt" IS NULL;

-- 5. Helper index for admin/restore queries over deleted users.
CREATE INDEX IF NOT EXISTS "IX_Users_DeletedAt"
    ON "Users"("DeletedAt");

COMMIT;

PRAGMA foreign_keys = ON;

-- Sanity checks after running:
--   SELECT sql FROM sqlite_master WHERE name = 'Users';
--   SELECT sql FROM sqlite_master WHERE name = 'IX_Users_Email_Live';
--   PRAGMA foreign_key_check;
