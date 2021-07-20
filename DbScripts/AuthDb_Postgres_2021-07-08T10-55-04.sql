CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210708085403_2021-07-08T10-53-53') THEN
    CREATE TABLE "AuthApplications" (
        "Id" uuid NOT NULL,
        "AccessTokenLifeTime" integer NULL,
        "RefreshTokenLifeTime" integer NULL,
        "ClientId" character varying(100) NULL,
        "ClientSecret" text NULL,
        "ConcurrencyToken" character varying(50) NULL,
        "ConsentType" character varying(50) NULL,
        "DisplayName" text NULL,
        "DisplayNames" text NULL,
        "Permissions" text NULL,
        "PostLogoutRedirectUris" text NULL,
        "Properties" text NULL,
        "RedirectUris" text NULL,
        "Requirements" text NULL,
        "Type" character varying(50) NULL,
        CONSTRAINT "PK_AuthApplications" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210708085403_2021-07-08T10-53-53') THEN
    CREATE TABLE "AuthScopes" (
        "Id" uuid NOT NULL,
        "ConcurrencyToken" character varying(50) NULL,
        "Description" text NULL,
        "Descriptions" text NULL,
        "DisplayName" text NULL,
        "DisplayNames" text NULL,
        "Name" character varying(200) NULL,
        "Properties" text NULL,
        "Resources" text NULL,
        CONSTRAINT "PK_AuthScopes" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210708085403_2021-07-08T10-53-53') THEN
    CREATE TABLE "AuthAuthorizations" (
        "Id" uuid NOT NULL,
        "ApplicationId" uuid NULL,
        "ConcurrencyToken" character varying(50) NULL,
        "CreationDate" timestamp without time zone NULL,
        "Properties" text NULL,
        "Scopes" text NULL,
        "Status" character varying(50) NULL,
        "Subject" character varying(400) NULL,
        "Type" character varying(50) NULL,
        CONSTRAINT "PK_AuthAuthorizations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AuthAuthorizations_AuthApplications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "AuthApplications" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210708085403_2021-07-08T10-53-53') THEN
    CREATE TABLE "AuthTokens" (
        "Id" uuid NOT NULL,
        "ApplicationId" uuid NULL,
        "AuthorizationId" uuid NULL,
        "ConcurrencyToken" character varying(50) NULL,
        "CreationDate" timestamp without time zone NULL,
        "ExpirationDate" timestamp without time zone NULL,
        "Payload" text NULL,
        "Properties" text NULL,
        "RedemptionDate" timestamp without time zone NULL,
        "ReferenceId" character varying(100) NULL,
        "Status" character varying(50) NULL,
        "Subject" character varying(400) NULL,
        "Type" character varying(50) NULL,
        CONSTRAINT "PK_AuthTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AuthTokens_AuthApplications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "AuthApplications" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_AuthTokens_AuthAuthorizations_AuthorizationId" FOREIGN KEY ("AuthorizationId") REFERENCES "AuthAuthorizations" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210708085403_2021-07-08T10-53-53') THEN
    CREATE UNIQUE INDEX "IX_AuthApplications_ClientId" ON "AuthApplications" ("ClientId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210708085403_2021-07-08T10-53-53') THEN
    CREATE INDEX "IX_AuthAuthorizations_ApplicationId_Status_Subject_Type" ON "AuthAuthorizations" ("ApplicationId", "Status", "Subject", "Type");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210708085403_2021-07-08T10-53-53') THEN
    CREATE UNIQUE INDEX "IX_AuthScopes_Name" ON "AuthScopes" ("Name");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210708085403_2021-07-08T10-53-53') THEN
    CREATE INDEX "IX_AuthTokens_ApplicationId_Status_Subject_Type" ON "AuthTokens" ("ApplicationId", "Status", "Subject", "Type");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210708085403_2021-07-08T10-53-53') THEN
    CREATE INDEX "IX_AuthTokens_AuthorizationId" ON "AuthTokens" ("AuthorizationId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210708085403_2021-07-08T10-53-53') THEN
    CREATE UNIQUE INDEX "IX_AuthTokens_ReferenceId" ON "AuthTokens" ("ReferenceId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210708085403_2021-07-08T10-53-53') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210708085403_2021-07-08T10-53-53', '5.0.7');
    END IF;
END $$;
COMMIT;

