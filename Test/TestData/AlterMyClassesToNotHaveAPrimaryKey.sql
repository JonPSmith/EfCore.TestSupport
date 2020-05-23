-- SQL script to create table without key

DROP TABLE IF EXISTS [MyClasses];
GO

CREATE TABLE [dbo].[MyClasses](
	[MyInt] [int] NOT NULL,
	[MyString] [nvarchar](max) NULL,
) 
GO
