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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(max) NULL,
        [LastName] nvarchar(max) NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
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
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    CREATE TABLE [RefreshToken] (
        [Id] int NOT NULL IDENTITY,
        [Token] nvarchar(max) NULL,
        [Expires] datetime2 NOT NULL,
        [Created] datetime2 NOT NULL,
        [CreatedByIp] nvarchar(max) NULL,
        [Revoked] datetime2 NULL,
        [RevokedByIp] nvarchar(max) NULL,
        [ReplacedByToken] nvarchar(max) NULL,
        [ApplicationUserId] nvarchar(450) NULL,
        CONSTRAINT [PK_RefreshToken] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshToken_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_RefreshToken_ApplicationUserId] ON [RefreshToken] ([ApplicationUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250401141108_InitialMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250401141108_InitialMigration', N'8.0.14');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [DepartmantId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE TABLE [Faculties] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [Created] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModified] datetime2 NULL,
        CONSTRAINT [PK_Faculties] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE TABLE [Lectures] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [Created] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModified] datetime2 NULL,
        CONSTRAINT [PK_Lectures] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE TABLE [Departmants] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [FacultyId] int NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [Created] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModified] datetime2 NULL,
        CONSTRAINT [PK_Departmants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Departmants_Faculties_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE TABLE [Courses] (
        [Id] int NOT NULL IDENTITY,
        [LectureId] int NOT NULL,
        [TeacherId] nvarchar(450) NULL,
        [DepartmantId] int NOT NULL,
        [Schedule] datetime2 NOT NULL,
        [Location] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [Created] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModified] datetime2 NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Courses_AspNetUsers_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Courses_Departmants_DepartmantId] FOREIGN KEY ([DepartmantId]) REFERENCES [Departmants] ([Id]),
        CONSTRAINT [FK_Courses_Lectures_LectureId] FOREIGN KEY ([LectureId]) REFERENCES [Lectures] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE TABLE [CourseStaffs] (
        [CourseId] int NOT NULL,
        [StaffId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_CourseStaffs] PRIMARY KEY ([CourseId], [StaffId]),
        CONSTRAINT [FK_CourseStaffs_AspNetUsers_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_CourseStaffs_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE TABLE [CourseStudents] (
        [CourseId] int NOT NULL,
        [StudentId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_CourseStudents] PRIMARY KEY ([CourseId], [StudentId]),
        CONSTRAINT [FK_CourseStudents_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_CourseStudents_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE TABLE [Sessions] (
        [Id] int NOT NULL IDENTITY,
        [CourseId] int NOT NULL,
        [Date] datetime2 NOT NULL,
        [StartTime] datetime2 NOT NULL,
        [EndTime] datetime2 NOT NULL,
        [Token] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [Created] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModified] datetime2 NULL,
        CONSTRAINT [PK_Sessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Sessions_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE TABLE [Attendances] (
        [SessionId] int NOT NULL,
        [StudentId] nvarchar(450) NOT NULL,
        [Date] datetime2 NOT NULL,
        CONSTRAINT [PK_Attendances] PRIMARY KEY ([SessionId], [StudentId]),
        CONSTRAINT [FK_Attendances_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Attendances_Sessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Sessions] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_DepartmantId] ON [AspNetUsers] ([DepartmantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE INDEX [IX_Attendances_StudentId] ON [Attendances] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE INDEX [IX_Courses_DepartmantId] ON [Courses] ([DepartmantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE INDEX [IX_Courses_LectureId] ON [Courses] ([LectureId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE INDEX [IX_Courses_TeacherId] ON [Courses] ([TeacherId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE INDEX [IX_CourseStaffs_StaffId] ON [CourseStaffs] ([StaffId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE INDEX [IX_CourseStudents_StudentId] ON [CourseStudents] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE INDEX [IX_Departmants_FacultyId] ON [Departmants] ([FacultyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    CREATE INDEX [IX_Sessions_CourseId] ON [Sessions] ([CourseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Departmants_DepartmantId] FOREIGN KEY ([DepartmantId]) REFERENCES [Departmants] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412172254_Entities_Added'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250412172254_Entities_Added', N'8.0.14');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412174358_Faculty_Departmant_Seeds_Added'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Created', N'CreatedBy', N'Description', N'LastModified', N'LastModifiedBy', N'Name') AND [object_id] = OBJECT_ID(N'[Faculties]'))
        SET IDENTITY_INSERT [Faculties] ON;
    EXEC(N'INSERT INTO [Faculties] ([Id], [Created], [CreatedBy], [Description], [LastModified], [LastModifiedBy], [Name])
    VALUES (1, ''0001-01-01T00:00:00.0000000'', NULL, N''Engineering Faculty'', NULL, NULL, N''Engineering'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Created', N'CreatedBy', N'Description', N'LastModified', N'LastModifiedBy', N'Name') AND [object_id] = OBJECT_ID(N'[Faculties]'))
        SET IDENTITY_INSERT [Faculties] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412174358_Faculty_Departmant_Seeds_Added'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Created', N'CreatedBy', N'Description', N'FacultyId', N'LastModified', N'LastModifiedBy', N'Name') AND [object_id] = OBJECT_ID(N'[Departmants]'))
        SET IDENTITY_INSERT [Departmants] ON;
    EXEC(N'INSERT INTO [Departmants] ([Id], [Created], [CreatedBy], [Description], [FacultyId], [LastModified], [LastModifiedBy], [Name])
    VALUES (1, ''0001-01-01T00:00:00.0000000'', NULL, N''Software Engineering Departmant'', 1, NULL, NULL, N''Software Engineering'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Created', N'CreatedBy', N'Description', N'FacultyId', N'LastModified', N'LastModifiedBy', N'Name') AND [object_id] = OBJECT_ID(N'[Departmants]'))
        SET IDENTITY_INSERT [Departmants] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250412174358_Faculty_Departmant_Seeds_Added'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250412174358_Faculty_Departmant_Seeds_Added', N'8.0.14');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250413170857_Lecture_Code_Added'
)
BEGIN
    ALTER TABLE [Lectures] ADD [Code] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250413170857_Lecture_Code_Added'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Lectures_Code] ON [Lectures] ([Code]) WHERE [Code] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250413170857_Lecture_Code_Added'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250413170857_Lecture_Code_Added', N'8.0.14');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250427195543_Entities_Updated_For_Attend'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Courses]') AND [c].[name] = N'Schedule');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Courses] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Courses] DROP COLUMN [Schedule];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250427195543_Entities_Updated_For_Attend'
)
BEGIN
    EXEC sp_rename N'[Attendances].[Date]', N'MarkedAtUtc', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250427195543_Entities_Updated_For_Attend'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sessions]') AND [c].[name] = N'StartTime');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Sessions] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Sessions] ALTER COLUMN [StartTime] time NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250427195543_Entities_Updated_For_Attend'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sessions]') AND [c].[name] = N'EndTime');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Sessions] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Sessions] ALTER COLUMN [EndTime] time NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250427195543_Entities_Updated_For_Attend'
)
BEGIN
    ALTER TABLE [Sessions] ADD [Status] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250427195543_Entities_Updated_For_Attend'
)
BEGIN
    ALTER TABLE [Courses] ADD [DayOfWeek] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250427195543_Entities_Updated_For_Attend'
)
BEGIN
    ALTER TABLE [Courses] ADD [Duration] time NOT NULL DEFAULT '00:00:00';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250427195543_Entities_Updated_For_Attend'
)
BEGIN
    ALTER TABLE [Courses] ADD [StartTime] time NOT NULL DEFAULT '00:00:00';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250427195543_Entities_Updated_For_Attend'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [InformationMail] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250427195543_Entities_Updated_For_Attend'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250427195543_Entities_Updated_For_Attend', N'8.0.14');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    ALTER TABLE [Sessions] ADD [MinimumConfidence] float NOT NULL DEFAULT 0.0E0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    ALTER TABLE [Sessions] ADD [WifiVerificationRequired] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    ALTER TABLE [Courses] ADD [ClassroomId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    CREATE TABLE [Classrooms] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NULL,
        [Building] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [Created] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModified] datetime2 NULL,
        CONSTRAINT [PK_Classrooms] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    CREATE TABLE [StudentWifiScans] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] nvarchar(450) NULL,
        [SessionId] int NOT NULL,
        [ScannedAtUtc] datetime2 NOT NULL,
        [PredictedClassroomId] int NULL,
        [ConfidenceScore] float NULL,
        [CreatedBy] nvarchar(max) NULL,
        [Created] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModified] datetime2 NULL,
        CONSTRAINT [PK_StudentWifiScans] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StudentWifiScans_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_StudentWifiScans_Sessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Sessions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    CREATE TABLE [WifiTrainingSamples] (
        [Id] int NOT NULL IDENTITY,
        [ClassroomId] int NOT NULL,
        [CollectedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [Created] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModified] datetime2 NULL,
        CONSTRAINT [PK_WifiTrainingSamples] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WifiTrainingSamples_Classrooms_ClassroomId] FOREIGN KEY ([ClassroomId]) REFERENCES [Classrooms] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    CREATE TABLE [StudentWifiAccessPoints] (
        [Id] int NOT NULL IDENTITY,
        [StudentWifiScanId] int NOT NULL,
        [Bssid] nvarchar(max) NULL,
        [Rssi] int NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [Created] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModified] datetime2 NULL,
        CONSTRAINT [PK_StudentWifiAccessPoints] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StudentWifiAccessPoints_StudentWifiScans_StudentWifiScanId] FOREIGN KEY ([StudentWifiScanId]) REFERENCES [StudentWifiScans] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    CREATE TABLE [WifiTrainingAccessPoints] (
        [Id] int NOT NULL IDENTITY,
        [WifiTrainingSampleId] int NOT NULL,
        [Bssid] nvarchar(max) NULL,
        [Rssi] int NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [Created] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModified] datetime2 NULL,
        CONSTRAINT [PK_WifiTrainingAccessPoints] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WifiTrainingAccessPoints_WifiTrainingSamples_WifiTrainingSampleId] FOREIGN KEY ([WifiTrainingSampleId]) REFERENCES [WifiTrainingSamples] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    CREATE INDEX [IX_Courses_ClassroomId] ON [Courses] ([ClassroomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    CREATE INDEX [IX_StudentWifiAccessPoints_StudentWifiScanId] ON [StudentWifiAccessPoints] ([StudentWifiScanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    CREATE INDEX [IX_StudentWifiScans_SessionId] ON [StudentWifiScans] ([SessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    CREATE INDEX [IX_StudentWifiScans_StudentId] ON [StudentWifiScans] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    CREATE INDEX [IX_WifiTrainingAccessPoints_WifiTrainingSampleId] ON [WifiTrainingAccessPoints] ([WifiTrainingSampleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    CREATE INDEX [IX_WifiTrainingSamples_ClassroomId] ON [WifiTrainingSamples] ([ClassroomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    ALTER TABLE [Courses] ADD CONSTRAINT [FK_Courses_Classrooms_ClassroomId] FOREIGN KEY ([ClassroomId]) REFERENCES [Classrooms] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260301113056_AddWifiFingerprinting'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260301113056_AddWifiFingerprinting', N'8.0.14');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260303184657_AddClassroomId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260303184657_AddClassroomId', N'8.0.14');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260304102311_AddWifiScanColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260304102311_AddWifiScanColumns', N'8.0.14');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260304102932_WifiScanAuditColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260304102932_WifiScanAuditColumns', N'8.0.14');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260304103938_AddWifiScanAuditColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260304103938_AddWifiScanAuditColumns', N'8.0.14');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260304180505_AddWifiScanPrediction'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260304180505_AddWifiScanPrediction', N'8.0.14');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260426101911_AddSecurityEntities'
)
BEGIN
    CREATE TABLE [SecurityEvents] (
        [Id] int NOT NULL IDENTITY,
        [EventType] nvarchar(100) NOT NULL,
        [Severity] nvarchar(20) NOT NULL,
        [StudentId] nvarchar(450) NOT NULL,
        [AttendanceSessionId] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [BSSIDInvolved] nvarchar(50) NULL,
        [IpInvolved] nvarchar(50) NULL,
        [WifiSecurityScore] float NULL,
        [IsResolved] bit NOT NULL,
        [ResolutionNotes] nvarchar(max) NULL,
        [DetectedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [Created] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModified] datetime2 NULL,
        CONSTRAINT [PK_SecurityEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SecurityEvents_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260426101911_AddSecurityEntities'
)
BEGIN
    CREATE TABLE [StudentRiskScores] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] nvarchar(450) NOT NULL,
        [OverallRiskScore] float NOT NULL,
        [WifiSecurityScore] float NOT NULL,
        [IpSecurityScore] float NOT NULL,
        [SuspiciousEventCount] int NOT NULL,
        [IsUnderReview] bit NOT NULL,
        [ReviewStartedAt] datetime2 NULL,
        [ReviewReason] nvarchar(max) NULL,
        [ReviewNotes] nvarchar(max) NULL,
        [LastAttendanceWifiScore] float NULL,
        [LastAttendanceTime] datetime2 NULL,
        [LastUpdatedAt] datetime2 NOT NULL,
        [LowSecurityAttendanceCount] int NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [Created] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModified] datetime2 NULL,
        CONSTRAINT [PK_StudentRiskScores] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StudentRiskScores_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260426101911_AddSecurityEntities'
)
BEGIN
    CREATE INDEX [IX_SecurityEvents_StudentId] ON [SecurityEvents] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260426101911_AddSecurityEntities'
)
BEGIN
    CREATE INDEX [IX_StudentRiskScores_StudentId] ON [StudentRiskScores] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260426101911_AddSecurityEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260426101911_AddSecurityEntities', N'8.0.14');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428105917_AddAttendanceWifiColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260428105917_AddAttendanceWifiColumns', N'8.0.14');
END;
GO

COMMIT;
GO

