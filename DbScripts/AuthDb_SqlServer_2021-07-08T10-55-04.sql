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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085409_2021-07-08T10-53-53')
BEGIN
    CREATE TABLE [AuthApplications] (
        [Id] uniqueidentifier NOT NULL,
        [AccessTokenLifeTime] int NULL,
        [RefreshTokenLifeTime] int NULL,
        [ClientId] nvarchar(100) NULL,
        [ClientSecret] nvarchar(max) NULL,
        [ConcurrencyToken] nvarchar(50) NULL,
        [ConsentType] nvarchar(50) NULL,
        [DisplayName] nvarchar(max) NULL,
        [DisplayNames] nvarchar(max) NULL,
        [Permissions] nvarchar(max) NULL,
        [PostLogoutRedirectUris] nvarchar(max) NULL,
        [Properties] nvarchar(max) NULL,
        [RedirectUris] nvarchar(max) NULL,
        [Requirements] nvarchar(max) NULL,
        [Type] nvarchar(50) NULL,
        CONSTRAINT [PK_AuthApplications] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085409_2021-07-08T10-53-53')
BEGIN
    CREATE TABLE [AuthScopes] (
        [Id] uniqueidentifier NOT NULL,
        [ConcurrencyToken] nvarchar(50) NULL,
        [Description] nvarchar(max) NULL,
        [Descriptions] nvarchar(max) NULL,
        [DisplayName] nvarchar(max) NULL,
        [DisplayNames] nvarchar(max) NULL,
        [Name] nvarchar(200) NULL,
        [Properties] nvarchar(max) NULL,
        [Resources] nvarchar(max) NULL,
        CONSTRAINT [PK_AuthScopes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085409_2021-07-08T10-53-53')
BEGIN
    CREATE TABLE [AuthAuthorizations] (
        [Id] uniqueidentifier NOT NULL,
        [ApplicationId] uniqueidentifier NULL,
        [ConcurrencyToken] nvarchar(50) NULL,
        [CreationDate] datetime2 NULL,
        [Properties] nvarchar(max) NULL,
        [Scopes] nvarchar(max) NULL,
        [Status] nvarchar(50) NULL,
        [Subject] nvarchar(400) NULL,
        [Type] nvarchar(50) NULL,
        CONSTRAINT [PK_AuthAuthorizations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AuthAuthorizations_AuthApplications_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [AuthApplications] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085409_2021-07-08T10-53-53')
BEGIN
    CREATE TABLE [AuthTokens] (
        [Id] uniqueidentifier NOT NULL,
        [ApplicationId] uniqueidentifier NULL,
        [AuthorizationId] uniqueidentifier NULL,
        [ConcurrencyToken] nvarchar(50) NULL,
        [CreationDate] datetime2 NULL,
        [ExpirationDate] datetime2 NULL,
        [Payload] nvarchar(max) NULL,
        [Properties] nvarchar(max) NULL,
        [RedemptionDate] datetime2 NULL,
        [ReferenceId] nvarchar(100) NULL,
        [Status] nvarchar(50) NULL,
        [Subject] nvarchar(400) NULL,
        [Type] nvarchar(50) NULL,
        CONSTRAINT [PK_AuthTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AuthTokens_AuthApplications_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [AuthApplications] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AuthTokens_AuthAuthorizations_AuthorizationId] FOREIGN KEY ([AuthorizationId]) REFERENCES [AuthAuthorizations] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085409_2021-07-08T10-53-53')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AuthApplications_ClientId] ON [AuthApplications] ([ClientId]) WHERE [ClientId] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085409_2021-07-08T10-53-53')
BEGIN
    CREATE INDEX [IX_AuthAuthorizations_ApplicationId_Status_Subject_Type] ON [AuthAuthorizations] ([ApplicationId], [Status], [Subject], [Type]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085409_2021-07-08T10-53-53')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AuthScopes_Name] ON [AuthScopes] ([Name]) WHERE [Name] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085409_2021-07-08T10-53-53')
BEGIN
    CREATE INDEX [IX_AuthTokens_ApplicationId_Status_Subject_Type] ON [AuthTokens] ([ApplicationId], [Status], [Subject], [Type]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085409_2021-07-08T10-53-53')
BEGIN
    CREATE INDEX [IX_AuthTokens_AuthorizationId] ON [AuthTokens] ([AuthorizationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085409_2021-07-08T10-53-53')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AuthTokens_ReferenceId] ON [AuthTokens] ([ReferenceId]) WHERE [ReferenceId] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085409_2021-07-08T10-53-53')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210708085409_2021-07-08T10-53-53', N'5.0.7');
END;
GO

COMMIT;
GO

