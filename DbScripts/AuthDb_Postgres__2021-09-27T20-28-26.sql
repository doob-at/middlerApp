CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE TABLE "AuthenticationProviders" (
        "Id" uuid NOT NULL,
        "Type" text NULL,
        "Enabled" boolean NOT NULL,
        "Name" text NULL,
        "DisplayName" text NULL,
        "Description" text NULL,
        "Parameters" text NULL,
        CONSTRAINT "PK_AuthenticationProviders" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE TABLE "AuthScopes" (
        "Id" uuid NOT NULL,
        "ConcurrencyToken" text NULL,
        "Description" text NULL,
        "Descriptions" text NULL,
        "DisplayName" text NULL,
        "DisplayNames" text NULL,
        "Name" text NULL,
        "Properties" text NULL,
        "Resources" text NULL,
        CONSTRAINT "PK_AuthScopes" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE TABLE "Clients" (
        "Id" uuid NOT NULL,
        "ClientId" text NULL,
        "ClientSecret" text NULL,
        "ConcurrencyToken" text NULL,
        "ConsentType" text NULL,
        "DisplayName" text NULL,
        "DisplayNames" text NULL,
        "Permissions" text NULL,
        "Properties" text NULL,
        "Requirements" text NULL,
        "Type" text NULL,
        "AccessTokenLifeTime" integer NULL,
        "RefreshTokenLifeTime" integer NULL,
        "Description" text NULL,
        CONSTRAINT "PK_Clients" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE TABLE "Roles" (
        "Id" uuid NOT NULL,
        "Name" text NULL,
        "DisplayName" text NULL,
        "Description" text NULL,
        "BuiltIn" boolean NOT NULL,
        CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "FirstName" text NULL,
        "LastName" text NULL,
        "ExpiresOn" timestamp without time zone NULL,
        "Password" character varying(200) NULL,
        "Active" boolean NOT NULL,
        "SecurityCode" character varying(200) NULL,
        "SecurityCodeExpirationDate" timestamp without time zone NOT NULL,
        "UserName" text NULL,
        "NormalizedUserName" text NULL,
        "Email" text NULL,
        "NormalizedEmail" text NULL,
        "EmailConfirmed" boolean NOT NULL,
        "PasswordHash" text NULL,
        "SecurityStamp" text NULL,
        "ConcurrencyStamp" text NULL,
        "PhoneNumber" text NULL,
        "PhoneNumberConfirmed" boolean NOT NULL,
        "TwoFactorEnabled" boolean NOT NULL,
        "LockoutEnd" timestamp with time zone NULL,
        "LockoutEnabled" boolean NOT NULL,
        "AccessFailedCount" integer NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE TABLE "AuthAuthorizations" (
        "Id" uuid NOT NULL,
        "ApplicationId" uuid NULL,
        "ConcurrencyToken" text NULL,
        "CreationDate" timestamp without time zone NULL,
        "Properties" text NULL,
        "Scopes" text NULL,
        "Status" text NULL,
        "Subject" text NULL,
        "Type" text NULL,
        CONSTRAINT "PK_AuthAuthorizations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AuthAuthorizations_Clients_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Clients" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE TABLE "ClientPostLogoutRedirectUri" (
        "Id" uuid NOT NULL,
        "PostLogoutRedirectUri" text NULL,
        "ClientId" uuid NOT NULL,
        CONSTRAINT "PK_ClientPostLogoutRedirectUri" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ClientPostLogoutRedirectUri_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE TABLE "ClientRedirectUri" (
        "Id" uuid NOT NULL,
        "RedirectUri" text NULL,
        "ClientId" uuid NOT NULL,
        CONSTRAINT "PK_ClientRedirectUri" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ClientRedirectUri_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE TABLE "ExternalClaims" (
        "Id" uuid NOT NULL,
        "Type" character varying(250) NOT NULL,
        "Value" character varying(250) NOT NULL,
        "Issuer" text NULL,
        "ConcurrencyStamp" text NULL,
        "UserId" uuid NOT NULL,
        CONSTRAINT "PK_ExternalClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ExternalClaims_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE TABLE "MRoleMUser" (
        "RolesId" uuid NOT NULL,
        "UsersId" uuid NOT NULL,
        CONSTRAINT "PK_MRoleMUser" PRIMARY KEY ("RolesId", "UsersId"),
        CONSTRAINT "FK_MRoleMUser_Roles_RolesId" FOREIGN KEY ("RolesId") REFERENCES "Roles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_MRoleMUser_Users_UsersId" FOREIGN KEY ("UsersId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE TABLE "UserClaims" (
        "Id" uuid NOT NULL,
        "Type" character varying(250) NOT NULL,
        "Value" character varying(250) NOT NULL,
        "ConcurrencyStamp" text NULL,
        "UserId" uuid NOT NULL,
        CONSTRAINT "PK_UserClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserClaims_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE TABLE "UserLogins" (
        "Id" uuid NOT NULL,
        "Provider" character varying(200) NOT NULL,
        "ProviderIdentityKey" character varying(200) NOT NULL,
        "UserId" uuid NOT NULL,
        "ConcurrencyStamp" text NULL,
        CONSTRAINT "PK_UserLogins" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserLogins_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE TABLE "UserSecrets" (
        "Id" uuid NOT NULL,
        "Name" text NOT NULL,
        "Secret" text NOT NULL,
        "UserId" uuid NOT NULL,
        "ConcurrencyStamp" text NULL,
        CONSTRAINT "PK_UserSecrets" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserSecrets_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE TABLE "AuthTokens" (
        "Id" uuid NOT NULL,
        "ApplicationId" uuid NULL,
        "AuthorizationId" uuid NULL,
        "ConcurrencyToken" text NULL,
        "CreationDate" timestamp without time zone NULL,
        "ExpirationDate" timestamp without time zone NULL,
        "Payload" text NULL,
        "Properties" text NULL,
        "RedemptionDate" timestamp without time zone NULL,
        "ReferenceId" text NULL,
        "Status" text NULL,
        "Subject" text NULL,
        "Type" text NULL,
        CONSTRAINT "PK_AuthTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AuthTokens_AuthAuthorizations_AuthorizationId" FOREIGN KEY ("AuthorizationId") REFERENCES "AuthAuthorizations" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_AuthTokens_Clients_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Clients" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE INDEX "IX_AuthAuthorizations_ApplicationId" ON "AuthAuthorizations" ("ApplicationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE INDEX "IX_AuthTokens_ApplicationId" ON "AuthTokens" ("ApplicationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE INDEX "IX_AuthTokens_AuthorizationId" ON "AuthTokens" ("AuthorizationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE INDEX "IX_ClientPostLogoutRedirectUri_ClientId" ON "ClientPostLogoutRedirectUri" ("ClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE INDEX "IX_ClientRedirectUri_ClientId" ON "ClientRedirectUri" ("ClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE INDEX "IX_ExternalClaims_UserId" ON "ExternalClaims" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE INDEX "IX_MRoleMUser_UsersId" ON "MRoleMUser" ("UsersId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE INDEX "IX_UserClaims_UserId" ON "UserClaims" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE INDEX "IX_UserLogins_UserId" ON "UserLogins" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    CREATE INDEX "IX_UserSecrets_UserId" ON "UserSecrets" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210927182723__2021-09-27T20-27-08') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210927182723__2021-09-27T20-27-08', '5.0.10');
    END IF;
END $EF$;
COMMIT;

