
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 10/10/2019 11:05:00
-- Generated from EDMX file: C:\Users\Mauricio CP\Desktop\DOST\DOST.DataAccess\DOSTModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [dost];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_AccountPlayer]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Player] DROP CONSTRAINT [FK_AccountPlayer];
GO
IF OBJECT_ID(N'[dbo].[FK_GamePlayer]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Player] DROP CONSTRAINT [FK_GamePlayer];
GO
IF OBJECT_ID(N'[dbo].[FK_PlayerCategoryPlayerAnswer]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CategoryPlayerAnswer] DROP CONSTRAINT [FK_PlayerCategoryPlayerAnswer];
GO
IF OBJECT_ID(N'[dbo].[FK_GameCategoryCategoryPlayerAnswer]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CategoryPlayerAnswer] DROP CONSTRAINT [FK_GameCategoryCategoryPlayerAnswer];
GO
IF OBJECT_ID(N'[dbo].[FK_GameGameCategory]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GameCategory] DROP CONSTRAINT [FK_GameGameCategory];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[GameCategory]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GameCategory];
GO
IF OBJECT_ID(N'[dbo].[Account]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Account];
GO
IF OBJECT_ID(N'[dbo].[Player]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Player];
GO
IF OBJECT_ID(N'[dbo].[Game]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Game];
GO
IF OBJECT_ID(N'[dbo].[CategoryPlayerAnswer]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CategoryPlayerAnswer];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'GameCategory'
CREATE TABLE [dbo].[GameCategory] (
    [idcategory] int IDENTITY(1,1) NOT NULL,
    [idgame] int  NOT NULL,
    [name] varchar(45)  NOT NULL
);
GO

-- Creating table 'Account'
CREATE TABLE [dbo].[Account] (
    [idaccount] int IDENTITY(1,1) NOT NULL,
    [username] varchar(15)  NOT NULL,
    [password] varchar(64)  NOT NULL,
    [email] varchar(50)  NOT NULL,
    [isVerified] int  NOT NULL,
    [coins] int  NOT NULL,
    [creationDate] datetime  NOT NULL,
    [validationCode] varchar(64)  NOT NULL
);
GO

-- Creating table 'Player'
CREATE TABLE [dbo].[Player] (
    [idplayer] int IDENTITY(1,1) NOT NULL,
    [idaccount] int  NOT NULL,
    [idgame] int  NOT NULL,
    [score] int  NOT NULL,
    [isHost] int  NOT NULL
);
GO

-- Creating table 'Game'
CREATE TABLE [dbo].[Game] (
    [idgame] int IDENTITY(1,1) NOT NULL,
    [round] int  NOT NULL,
    [date] datetime  NOT NULL
);
GO

-- Creating table 'CategoryPlayerAnswer'
CREATE TABLE [dbo].[CategoryPlayerAnswer] (
    [idanswer] int IDENTITY(1,1) NOT NULL,
    [idplayer] int  NOT NULL,
    [idcategory] int  NOT NULL,
    [answer] varchar(25)  NOT NULL,
    [round] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [idcategory] in table 'GameCategory'
ALTER TABLE [dbo].[GameCategory]
ADD CONSTRAINT [PK_GameCategory]
    PRIMARY KEY CLUSTERED ([idcategory] ASC);
GO

-- Creating primary key on [idaccount] in table 'Account'
ALTER TABLE [dbo].[Account]
ADD CONSTRAINT [PK_Account]
    PRIMARY KEY CLUSTERED ([idaccount] ASC);
GO

-- Creating primary key on [idplayer] in table 'Player'
ALTER TABLE [dbo].[Player]
ADD CONSTRAINT [PK_Player]
    PRIMARY KEY CLUSTERED ([idplayer] ASC);
GO

-- Creating primary key on [idgame] in table 'Game'
ALTER TABLE [dbo].[Game]
ADD CONSTRAINT [PK_Game]
    PRIMARY KEY CLUSTERED ([idgame] ASC);
GO

-- Creating primary key on [idanswer] in table 'CategoryPlayerAnswer'
ALTER TABLE [dbo].[CategoryPlayerAnswer]
ADD CONSTRAINT [PK_CategoryPlayerAnswer]
    PRIMARY KEY CLUSTERED ([idanswer] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [idaccount] in table 'Player'
ALTER TABLE [dbo].[Player]
ADD CONSTRAINT [FK_AccountPlayer]
    FOREIGN KEY ([idaccount])
    REFERENCES [dbo].[Account]
        ([idaccount])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AccountPlayer'
CREATE INDEX [IX_FK_AccountPlayer]
ON [dbo].[Player]
    ([idaccount]);
GO

-- Creating foreign key on [idgame] in table 'Player'
ALTER TABLE [dbo].[Player]
ADD CONSTRAINT [FK_GamePlayer]
    FOREIGN KEY ([idgame])
    REFERENCES [dbo].[Game]
        ([idgame])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GamePlayer'
CREATE INDEX [IX_FK_GamePlayer]
ON [dbo].[Player]
    ([idgame]);
GO

-- Creating foreign key on [idplayer] in table 'CategoryPlayerAnswer'
ALTER TABLE [dbo].[CategoryPlayerAnswer]
ADD CONSTRAINT [FK_PlayerCategoryPlayerAnswer]
    FOREIGN KEY ([idplayer])
    REFERENCES [dbo].[Player]
        ([idplayer])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PlayerCategoryPlayerAnswer'
CREATE INDEX [IX_FK_PlayerCategoryPlayerAnswer]
ON [dbo].[CategoryPlayerAnswer]
    ([idplayer]);
GO

-- Creating foreign key on [idcategory] in table 'CategoryPlayerAnswer'
ALTER TABLE [dbo].[CategoryPlayerAnswer]
ADD CONSTRAINT [FK_GameCategoryCategoryPlayerAnswer]
    FOREIGN KEY ([idcategory])
    REFERENCES [dbo].[GameCategory]
        ([idcategory])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GameCategoryCategoryPlayerAnswer'
CREATE INDEX [IX_FK_GameCategoryCategoryPlayerAnswer]
ON [dbo].[CategoryPlayerAnswer]
    ([idcategory]);
GO

-- Creating foreign key on [idgame] in table 'GameCategory'
ALTER TABLE [dbo].[GameCategory]
ADD CONSTRAINT [FK_GameGameCategory]
    FOREIGN KEY ([idgame])
    REFERENCES [dbo].[Game]
        ([idgame])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GameGameCategory'
CREATE INDEX [IX_FK_GameGameCategory]
ON [dbo].[GameCategory]
    ([idgame]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------