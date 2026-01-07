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

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251215180850_InitialBaseline', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE UNIQUE INDEX [IX_Personas_Dni] ON [Personas] ([Dni]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251218142143_UniqueDniIndex', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Personas_Dni] ON [Personas];
GO


        CREATE UNIQUE INDEX UX_Personas_Dni
        ON Personas (Dni)
        WHERE Activo = 1;
    
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251218143529_FixUniqueDniIndex', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Profesionales] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [Matricula] nvarchar(50) NOT NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_Profesionales] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_Profesionales_Matricula] ON [Profesionales] ([Matricula]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260106145347_AddProfesionales', N'8.0.11');
GO

COMMIT;
GO

