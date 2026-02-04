BEGIN TRANSACTION;
GO

ALTER TABLE [Profesionales] ADD [Descripcion] nvarchar(max) NULL;
GO

ALTER TABLE [Profesionales] ADD [Especialidad] nvarchar(max) NULL;
GO

ALTER TABLE [Profesionales] ADD [FotoUrl] nvarchar(max) NULL;
GO

ALTER TABLE [Profesionales] ADD [InstagramUrl] nvarchar(max) NULL;
GO

ALTER TABLE [Profesionales] ADD [LinkedinUrl] nvarchar(max) NULL;
GO

ALTER TABLE [Personas] ADD [FailedLoginAttempts] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Personas] ADD [LockoutEnd] datetime2 NULL;
GO

ALTER TABLE [HorariosTrabajo] ADD [Fecha] datetime2 NULL;
GO

CREATE TABLE [AuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [Fecha] datetime2 NOT NULL,
    [UsuarioId] int NULL,
    [Accion] nvarchar(max) NOT NULL,
    [Entidad] nvarchar(max) NOT NULL,
    [Detalle] nvarchar(max) NULL,
    [IpAddress] nvarchar(max) NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [GlobalConfigs] (
    [Id] int NOT NULL IDENTITY,
    [Clave] nvarchar(max) NOT NULL,
    [Valor] nvarchar(max) NOT NULL,
    [Descripcion] nvarchar(max) NULL,
    CONSTRAINT [PK_GlobalConfigs] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [NotasClinicas] (
    [Id] int NOT NULL IDENTITY,
    [TurnoId] int NOT NULL,
    [Contenido] nvarchar(max) NOT NULL,
    [FechaCreacion] datetime2 NOT NULL,
    [VisibleParaPaciente] bit NOT NULL,
    CONSTRAINT [PK_NotasClinicas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_NotasClinicas_Turnos_TurnoId] FOREIGN KEY ([TurnoId]) REFERENCES [Turnos] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ArchivosAdjuntos] (
    [Id] int NOT NULL IDENTITY,
    [NotaClinicaId] int NOT NULL,
    [RutaArchivo] nvarchar(500) NOT NULL,
    [NombreOriginal] nvarchar(255) NOT NULL,
    [TipoArchivo] nvarchar(100) NOT NULL,
    [TamanioBytes] bigint NOT NULL,
    [FechaSubida] datetime2 NOT NULL,
    CONSTRAINT [PK_ArchivosAdjuntos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ArchivosAdjuntos_NotasClinicas_NotaClinicaId] FOREIGN KEY ([NotaClinicaId]) REFERENCES [NotasClinicas] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ArchivosAdjuntos_NotaClinicaId] ON [ArchivosAdjuntos] ([NotaClinicaId]);
GO

CREATE INDEX [IX_NotasClinicas_TurnoId] ON [NotasClinicas] ([TurnoId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260203174151_AddPersonaLockoutFields', N'8.0.11');
GO

COMMIT;
GO

