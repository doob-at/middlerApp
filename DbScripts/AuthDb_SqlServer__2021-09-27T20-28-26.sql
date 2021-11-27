IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE TABLE [AuthenticationProviders] (
        [Id] uniqueidentifier NOT NULL,
        [Type] nvarchar(max) NULL,
        [Enabled] bit NOT NULL,
        [Name] nvarchar(max) NULL,
        [DisplayName] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [Parameters] nvarchar(max) NULL,
        CONSTRAINT [PK_AuthenticationProviders] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE TABLE [AuthScopes] (
        [Id] uniqueidentifier NOT NULL,
        [ConcurrencyToken] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [Descriptions] nvarchar(max) NULL,
        [DisplayName] nvarchar(max) NULL,
        [DisplayNames] nvarchar(max) NULL,
        [Name] nvarchar(max) NULL,
        [Properties] nvarchar(max) NULL,
        [Resources] nvarchar(max) NULL,
        CONSTRAINT [PK_AuthScopes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE TABLE [Clients] (
        [Id] uniqueidentifier NOT NULL,
        [ClientId] nvarchar(max) NULL,
        [ClientSecret] nvarchar(max) NULL,
        [ConcurrencyToken] nvarchar(max) NULL,
        [ConsentType] nvarchar(max) NULL,
        [DisplayName] nvarchar(max) NULL,
        [DisplayNames] nvarchar(max) NULL,
        [Permissions] nvarchar(max) NULL,
        [Properties] nvarchar(max) NULL,
        [Requirements] nvarchar(max) NULL,
        [Type] nvarchar(max) NULL,
        [AccessTokenLifeTime] int NULL,
        [RefreshTokenLifeTime] int NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE TABLE [Roles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NULL,
        [DisplayName] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [BuiltIn] bit NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(max) NULL,
        [LastName] nvarchar(max) NULL,
        [ExpiresOn] datetime2 NULL,
        [Password] nvarchar(200) NULL,
        [Active] bit NOT NULL,
        [SecurityCode] nvarchar(200) NULL,
        [SecurityCodeExpirationDate] datetime2 NOT NULL,
        [UserName] nvarchar(max) NULL,
        [NormalizedUserName] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [NormalizedEmail] nvarchar(max) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE TABLE [AuthAuthorizations] (
        [Id] uniqueidentifier NOT NULL,
        [ApplicationId] uniqueidentifier NULL,
        [ConcurrencyToken] nvarchar(max) NULL,
        [CreationDate] datetime2 NULL,
        [Properties] nvarchar(max) NULL,
        [Scopes] nvarchar(max) NULL,
        [Status] nvarchar(max) NULL,
        [Subject] nvarchar(max) NULL,
        [Type] nvarchar(max) NULL,
        CONSTRAINT [PK_AuthAuthorizations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AuthAuthorizations_Clients_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE TABLE [ClientPostLogoutRedirectUri] (
        [Id] uniqueidentifier NOT NULL,
        [PostLogoutRedirectUri] nvarchar(max) NULL,
        [ClientId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ClientPostLogoutRedirectUri] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClientPostLogoutRedirectUri_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE TABLE [ClientRedirectUri] (
        [Id] uniqueidentifier NOT NULL,
        [RedirectUri] nvarchar(max) NULL,
        [ClientId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ClientRedirectUri] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClientRedirectUri_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE TABLE [ExternalClaims] (
        [Id] uniqueidentifier NOT NULL,
        [Type] nvarchar(250) NOT NULL,
        [Value] nvarchar(250) NOT NULL,
        [Issuer] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ExternalClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExternalClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE TABLE [MRoleMUser] (
        [RolesId] uniqueidentifier NOT NULL,
        [UsersId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_MRoleMUser] PRIMARY KEY ([RolesId], [UsersId]),
        CONSTRAINT [FK_MRoleMUser_Roles_RolesId] FOREIGN KEY ([RolesId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MRoleMUser_Users_UsersId] FOREIGN KEY ([UsersId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE TABLE [UserClaims] (
        [Id] uniqueidentifier NOT NULL,
        [Type] nvarchar(250) NOT NULL,
        [Value] nvarchar(250) NOT NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE TABLE [UserLogins] (
        [Id] uniqueidentifier NOT NULL,
        [Provider] nvarchar(200) NOT NULL,
        [ProviderIdentityKey] nvarchar(200) NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_UserLogins] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE TABLE [UserSecrets] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Secret] nvarchar(max) NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_UserSecrets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserSecrets_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE TABLE [AuthTokens] (
        [Id] uniqueidentifier NOT NULL,
        [ApplicationId] uniqueidentifier NULL,
        [AuthorizationId] uniqueidentifier NULL,
        [ConcurrencyToken] nvarchar(max) NULL,
        [CreationDate] datetime2 NULL,
        [ExpirationDate] datetime2 NULL,
        [Payload] nvarchar(max) NULL,
        [Properties] nvarchar(max) NULL,
        [RedemptionDate] datetime2 NULL,
        [ReferenceId] nvarchar(max) NULL,
        [Status] nvarchar(max) NULL,
        [Subject] nvarchar(max) NULL,
        [Type] nvarchar(max) NULL,
        CONSTRAINT [PK_AuthTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AuthTokens_AuthAuthorizations_AuthorizationId] FOREIGN KEY ([AuthorizationId]) REFERENCES [AuthAuthorizations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AuthTokens_Clients_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE INDEX [IX_AuthAuthorizations_ApplicationId] ON [AuthAuthorizations] ([ApplicationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE INDEX [IX_AuthTokens_ApplicationId] ON [AuthTokens] ([ApplicationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE INDEX [IX_AuthTokens_AuthorizationId] ON [AuthTokens] ([AuthorizationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE INDEX [IX_ClientPostLogoutRedirectUri_ClientId] ON [ClientPostLogoutRedirectUri] ([ClientId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE INDEX [IX_ClientRedirectUri_ClientId] ON [ClientRedirectUri] ([ClientId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE INDEX [IX_ExternalClaims_UserId] ON [ExternalClaims] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE INDEX [IX_MRoleMUser_UsersId] ON [MRoleMUser] ([UsersId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE INDEX [IX_UserClaims_UserId] ON [UserClaims] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE INDEX [IX_UserLogins_UserId] ON [UserLogins] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    CREATE INDEX [IX_UserSecrets_UserId] ON [UserSecrets] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210927182728__2021-09-27T20-27-08')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210927182728__2021-09-27T20-27-08', N'5.0.10');
END;
GO

COMMIT;
GO

