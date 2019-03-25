CREATE TABLE [MyEntites] (
    [MyEntityId] int NOT NULL IDENTITY,
    [MyDateTime] datetime2 NOT NULL,
    [MyInt] int NOT NULL,
    [MyString] nvarchar(450) NULL,
    CONSTRAINT [PK_MyEntites] PRIMARY KEY ([MyEntityId]),
    CONSTRAINT [MySpecialName] UNIQUE NONCLUSTERED ([MyInt])
);
GO

CREATE INDEX [IX_MyEntites_MyString] ON [MyEntites] ([MyString]);
GO
