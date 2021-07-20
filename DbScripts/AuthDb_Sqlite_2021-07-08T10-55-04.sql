CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;

CREATE TABLE "AuthApplications" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AuthApplications" PRIMARY KEY,
    "AccessTokenLifeTime" INTEGER NULL,
    "RefreshTokenLifeTime" INTEGER NULL,
    "ClientId" TEXT NULL,
    "ClientSecret" TEXT NULL,
    "ConcurrencyToken" TEXT NULL,
    "ConsentType" TEXT NULL,
    "DisplayName" TEXT NULL,
    "DisplayNames" TEXT NULL,
    "Permissions" TEXT NULL,
    "PostLogoutRedirectUris" TEXT NULL,
    "Properties" TEXT NULL,
    "RedirectUris" TEXT NULL,
    "Requirements" TEXT NULL,
    "Type" TEXT NULL
);

CREATE TABLE "AuthScopes" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AuthScopes" PRIMARY KEY,
    "ConcurrencyToken" TEXT NULL,
    "Description" TEXT NULL,
    "Descriptions" TEXT NULL,
    "DisplayName" TEXT NULL,
    "DisplayNames" TEXT NULL,
    "Name" TEXT NULL,
    "Properties" TEXT NULL,
    "Resources" TEXT NULL
);

CREATE TABLE "AuthAuthorizations" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AuthAuthorizations" PRIMARY KEY,
    "ApplicationId" TEXT NULL,
    "ConcurrencyToken" TEXT NULL,
    "CreationDate" TEXT NULL,
    "Properties" TEXT NULL,
    "Scopes" TEXT NULL,
    "Status" TEXT NULL,
    "Subject" TEXT NULL,
    "Type" TEXT NULL,
    CONSTRAINT "FK_AuthAuthorizations_AuthApplications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "AuthApplications" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "AuthTokens" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AuthTokens" PRIMARY KEY,
    "ApplicationId" TEXT NULL,
    "AuthorizationId" TEXT NULL,
    "ConcurrencyToken" TEXT NULL,
    "CreationDate" TEXT NULL,
    "ExpirationDate" TEXT NULL,
    "Payload" TEXT NULL,
    "Properties" TEXT NULL,
    "RedemptionDate" TEXT NULL,
    "ReferenceId" TEXT NULL,
    "Status" TEXT NULL,
    "Subject" TEXT NULL,
    "Type" TEXT NULL,
    CONSTRAINT "FK_AuthTokens_AuthApplications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "AuthApplications" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_AuthTokens_AuthAuthorizations_AuthorizationId" FOREIGN KEY ("AuthorizationId") REFERENCES "AuthAuthorizations" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_AuthApplications_ClientId" ON "AuthApplications" ("ClientId");

CREATE INDEX "IX_AuthAuthorizations_ApplicationId_Status_Subject_Type" ON "AuthAuthorizations" ("ApplicationId", "Status", "Subject", "Type");

CREATE UNIQUE INDEX "IX_AuthScopes_Name" ON "AuthScopes" ("Name");

CREATE INDEX "IX_AuthTokens_ApplicationId_Status_Subject_Type" ON "AuthTokens" ("ApplicationId", "Status", "Subject", "Type");

CREATE INDEX "IX_AuthTokens_AuthorizationId" ON "AuthTokens" ("AuthorizationId");

CREATE UNIQUE INDEX "IX_AuthTokens_ReferenceId" ON "AuthTokens" ("ReferenceId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210708085358_2021-07-08T10-53-53', '5.0.7');

COMMIT;

