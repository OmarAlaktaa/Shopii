BEGIN TRANSACTION;

CREATE TABLE IF NOT EXISTS "AuditLogs" (
    "AuditLogId" INTEGER NOT NULL CONSTRAINT "PK_AuditLogs" PRIMARY KEY AUTOINCREMENT,
    "Timestamp" DATETIME NOT NULL,
    "ActorUserId" TEXT NULL,
    "Action" TEXT NOT NULL,
    "EntityType" TEXT NOT NULL,
    "EntityId" TEXT NULL,
    "CorrelationId" TEXT NULL,
    "Details" TEXT NULL
);

CREATE INDEX IF NOT EXISTS "IX_AuditLogs_Timestamp"
    ON "AuditLogs" ("Timestamp");

CREATE INDEX IF NOT EXISTS "IX_AuditLogs_ActorUserId"
    ON "AuditLogs" ("ActorUserId");

CREATE INDEX IF NOT EXISTS "IX_AuditLogs_EntityType_EntityId"
    ON "AuditLogs" ("EntityType", "EntityId");

CREATE INDEX IF NOT EXISTS "IX_AuditLogs_CorrelationId"
    ON "AuditLogs" ("CorrelationId");

COMMIT;
