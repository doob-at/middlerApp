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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085341_2021-07-08T10-53-26')
BEGIN
    CREATE TABLE [EndpointRules] (
        [Id] uniqueidentifier NOT NULL,
        [Order] decimal(18,2) NOT NULL,
        [Name] nvarchar(max) NULL,
        [Enabled] bit NOT NULL,
        [Scheme] nvarchar(max) NULL,
        [Hostname] nvarchar(max) NULL,
        [Path] nvarchar(max) NULL,
        [HttpMethods] nvarchar(max) NULL,
        CONSTRAINT [PK_EndpointRules] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085341_2021-07-08T10-53-26')
BEGIN
    CREATE TABLE [TypeDefinitions] (
        [Id] uniqueidentifier NOT NULL,
        [Module] nvarchar(max) NULL,
        [Content] nvarchar(max) NULL,
        CONSTRAINT [PK_TypeDefinitions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085341_2021-07-08T10-53-26')
BEGIN
    CREATE TABLE [Variables] (
        [Id] uniqueidentifier NOT NULL,
        [Parent] nvarchar(max) NULL,
        [Name] nvarchar(max) NULL,
        [IsFolder] bit NOT NULL,
        [Extension] nvarchar(max) NULL,
        [Content] nvarchar(max) NULL,
        [Bytes] varbinary(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Variables] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085341_2021-07-08T10-53-26')
BEGIN
    CREATE TABLE [EndpointActions] (
        [Id] uniqueidentifier NOT NULL,
        [Order] decimal(18,2) NOT NULL,
        [EndpointRuleEntityId] uniqueidentifier NOT NULL,
        [Terminating] bit NOT NULL,
        [WriteStreamDirect] bit NOT NULL,
        [Enabled] bit NOT NULL,
        [ActionType] nvarchar(max) NULL,
        [Parameters] nvarchar(max) NULL,
        CONSTRAINT [PK_EndpointActions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EndpointActions_EndpointRules_EndpointRuleEntityId] FOREIGN KEY ([EndpointRuleEntityId]) REFERENCES [EndpointRules] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085341_2021-07-08T10-53-26')
BEGIN
    CREATE TABLE [EndpointRulePermission] (
        [Id] uniqueidentifier NOT NULL,
        [Order] decimal(18,2) NOT NULL,
        [PrincipalName] nvarchar(max) NULL,
        [Type] nvarchar(max) NULL,
        [AccessMode] nvarchar(max) NULL,
        [Client] nvarchar(max) NULL,
        [SourceAddress] nvarchar(max) NULL,
        [EndpointRuleEntityId] uniqueidentifier NULL,
        CONSTRAINT [PK_EndpointRulePermission] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EndpointRulePermission_EndpointRules_EndpointRuleEntityId] FOREIGN KEY ([EndpointRuleEntityId]) REFERENCES [EndpointRules] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085341_2021-07-08T10-53-26')
BEGIN
    CREATE INDEX [IX_EndpointActions_EndpointRuleEntityId] ON [EndpointActions] ([EndpointRuleEntityId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085341_2021-07-08T10-53-26')
BEGIN
    CREATE INDEX [IX_EndpointRulePermission_EndpointRuleEntityId] ON [EndpointRulePermission] ([EndpointRuleEntityId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210708085341_2021-07-08T10-53-26')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210708085341_2021-07-08T10-53-26', N'5.0.7');
END;
GO

COMMIT;
GO

