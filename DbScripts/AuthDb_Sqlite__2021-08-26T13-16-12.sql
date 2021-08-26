CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;

CREATE TABLE "AuthenticationProviders" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AuthenticationProviders" PRIMARY KEY,
    "Type" TEXT NULL,
    "Enabled" INTEGER NOT NULL,
    "Name" TEXT NULL,
    "DisplayName" TEXT NULL,
    "Description" TEXT NULL,
    "Parameters" TEXT NULL
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

CREATE TABLE "Clients" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Clients" PRIMARY KEY,
    "ClientId" TEXT NULL,
    "ClientSecret" TEXT NULL,
    "ConcurrencyToken" TEXT NULL,
    "ConsentType" TEXT NULL,
    "DisplayName" TEXT NULL,
    "DisplayNames" TEXT NULL,
    "Permissions" TEXT NULL,
    "Properties" TEXT NULL,
    "Requirements" TEXT NULL,
    "Type" TEXT NULL,
    "AccessTokenLifeTime" INTEGER NULL,
    "RefreshTokenLifeTime" INTEGER NULL
);

CREATE TABLE "Roles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Roles" PRIMARY KEY,
    "Name" TEXT NULL,
    "DisplayName" TEXT NULL,
    "Description" TEXT NULL,
    "BuiltIn" INTEGER NOT NULL
);

CREATE TABLE "Users" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY,
    "Subject" TEXT NOT NULL,
    "UserName" TEXT NULL,
    "Email" TEXT NULL,
    "FirstName" TEXT NULL,
    "LastName" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "EmailConfirmed" INTEGER NOT NULL,
    "PhoneNumberConfirmed" INTEGER NOT NULL,
    "TwoFactorEnabled" INTEGER NOT NULL,
    "LockoutEnabled" INTEGER NOT NULL,
    "ExpiresOn" TEXT NULL,
    "Password" TEXT NULL,
    "Active" INTEGER NOT NULL,
    "SecurityCode" TEXT NULL,
    "SecurityCodeExpirationDate" TEXT NOT NULL,
    "ConcurrencyStamp" TEXT NULL
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
    CONSTRAINT "FK_AuthAuthorizations_Clients_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Clients" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ClientPostLogoutRedirectUri" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ClientPostLogoutRedirectUri" PRIMARY KEY,
    "PostLogoutRedirectUri" TEXT NULL,
    "ClientId" TEXT NOT NULL,
    CONSTRAINT "FK_ClientPostLogoutRedirectUri_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ClientRedirectUri" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ClientRedirectUri" PRIMARY KEY,
    "RedirectUri" TEXT NULL,
    "ClientId" TEXT NOT NULL,
    CONSTRAINT "FK_ClientRedirectUri_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ExternalClaims" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ExternalClaims" PRIMARY KEY,
    "Type" TEXT NOT NULL,
    "Value" TEXT NOT NULL,
    "Issuer" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "FK_ExternalClaims_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "MRoleMUser" (
    "RolesId" TEXT NOT NULL,
    "UsersId" TEXT NOT NULL,
    CONSTRAINT "PK_MRoleMUser" PRIMARY KEY ("RolesId", "UsersId"),
    CONSTRAINT "FK_MRoleMUser_Roles_RolesId" FOREIGN KEY ("RolesId") REFERENCES "Roles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_MRoleMUser_Users_UsersId" FOREIGN KEY ("UsersId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "UserClaims" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_UserClaims" PRIMARY KEY,
    "Type" TEXT NOT NULL,
    "Value" TEXT NOT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "FK_UserClaims_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "UserLogins" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_UserLogins" PRIMARY KEY,
    "Provider" TEXT NOT NULL,
    "ProviderIdentityKey" TEXT NOT NULL,
    "UserId" TEXT NOT NULL,
    "ConcurrencyStamp" TEXT NULL,
    CONSTRAINT "FK_UserLogins_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "UserSecrets" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_UserSecrets" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Secret" TEXT NOT NULL,
    "UserId" TEXT NOT NULL,
    "ConcurrencyStamp" TEXT NULL,
    CONSTRAINT "FK_UserSecrets_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
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
    CONSTRAINT "FK_AuthTokens_AuthAuthorizations_AuthorizationId" FOREIGN KEY ("AuthorizationId") REFERENCES "AuthAuthorizations" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_AuthTokens_Clients_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Clients" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_AuthAuthorizations_ApplicationId" ON "AuthAuthorizations" ("ApplicationId");

CREATE INDEX "IX_AuthTokens_ApplicationId" ON "AuthTokens" ("ApplicationId");

CREATE INDEX "IX_AuthTokens_AuthorizationId" ON "AuthTokens" ("AuthorizationId");

CREATE INDEX "IX_ClientPostLogoutRedirectUri_ClientId" ON "ClientPostLogoutRedirectUri" ("ClientId");

CREATE INDEX "IX_ClientRedirectUri_ClientId" ON "ClientRedirectUri" ("ClientId");

CREATE INDEX "IX_ExternalClaims_UserId" ON "ExternalClaims" ("UserId");

CREATE INDEX "IX_MRoleMUser_UsersId" ON "MRoleMUser" ("UsersId");

CREATE INDEX "IX_UserClaims_UserId" ON "UserClaims" ("UserId");

CREATE INDEX "IX_UserLogins_UserId" ON "UserLogins" ("UserId");

CREATE INDEX "IX_UserSecrets_UserId" ON "UserSecrets" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210826111517__2021-08-26T13-15-09', '5.0.7');

COMMIT;

