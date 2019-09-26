
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 09/24/2019 09:10:21
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

IF OBJECT_ID(N'[dbo].[FK__jugador__idcuent__3B75D760]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Jugador] DROP CONSTRAINT [FK__jugador__idcuent__3B75D760];
GO
IF OBJECT_ID(N'[dbo].[FK__jugador__idparti__3C69FB99]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Jugador] DROP CONSTRAINT [FK__jugador__idparti__3C69FB99];
GO
IF OBJECT_ID(N'[dbo].[FK__respuesta__idjug__412EB0B6]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RespuestaCategoriaJugador] DROP CONSTRAINT [FK__respuesta__idjug__412EB0B6];
GO
IF OBJECT_ID(N'[dbo].[FK_respuestacategoria_jugador_categoria_partida]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RespuestaCategoriaJugador] DROP CONSTRAINT [FK_respuestacategoria_jugador_categoria_partida];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[CategoriaPartida]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CategoriaPartida];
GO
IF OBJECT_ID(N'[dbo].[Cuenta]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Cuenta];
GO
IF OBJECT_ID(N'[dbo].[Jugador]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Jugador];
GO
IF OBJECT_ID(N'[dbo].[Partida]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Partida];
GO
IF OBJECT_ID(N'[dbo].[RespuestaCategoriaJugador]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RespuestaCategoriaJugador];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'CategoriaPartida'
CREATE TABLE [dbo].[CategoriaPartida] (
    [idcategoria] int IDENTITY(1,1) NOT NULL,
    [idpartida] int  NOT NULL,
    [nombre] varchar(45)  NOT NULL
);
GO

-- Creating table 'Cuenta'
CREATE TABLE [dbo].[Cuenta] (
    [idcuenta] int IDENTITY(1,1) NOT NULL,
    [usuario] varchar(15)  NOT NULL,
    [password] varchar(64)  NOT NULL,
    [correo] varchar(50)  NOT NULL,
    [confirmada] int  NOT NULL,
    [monedas] int  NOT NULL,
    [fechaCreacion] datetime  NOT NULL,
    [codigoValidacion] varchar(64)  NOT NULL
);
GO

-- Creating table 'Jugador'
CREATE TABLE [dbo].[Jugador] (
    [idjugador] int IDENTITY(1,1) NOT NULL,
    [idcuenta] int  NOT NULL,
    [idpartida] int  NOT NULL,
    [puntuacion] int  NOT NULL,
    [anfitrion] int  NOT NULL
);
GO

-- Creating table 'Partida'
CREATE TABLE [dbo].[Partida] (
    [idpartida] int IDENTITY(1,1) NOT NULL,
    [ronda] int  NOT NULL,
    [fecha] datetime  NOT NULL
);
GO

-- Creating table 'RespuestaCategoriaJugador'
CREATE TABLE [dbo].[RespuestaCategoriaJugador] (
    [idrespuesta] int IDENTITY(1,1) NOT NULL,
    [idjugador] int  NOT NULL,
    [idcategoria] int  NOT NULL,
    [respuesta] varchar(25)  NOT NULL,
    [ronda] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [idcategoria] in table 'CategoriaPartida'
ALTER TABLE [dbo].[CategoriaPartida]
ADD CONSTRAINT [PK_CategoriaPartida]
    PRIMARY KEY CLUSTERED ([idcategoria] ASC);
GO

-- Creating primary key on [idcuenta] in table 'Cuenta'
ALTER TABLE [dbo].[Cuenta]
ADD CONSTRAINT [PK_Cuenta]
    PRIMARY KEY CLUSTERED ([idcuenta] ASC);
GO

-- Creating primary key on [idjugador] in table 'Jugador'
ALTER TABLE [dbo].[Jugador]
ADD CONSTRAINT [PK_Jugador]
    PRIMARY KEY CLUSTERED ([idjugador] ASC);
GO

-- Creating primary key on [idpartida] in table 'Partida'
ALTER TABLE [dbo].[Partida]
ADD CONSTRAINT [PK_Partida]
    PRIMARY KEY CLUSTERED ([idpartida] ASC);
GO

-- Creating primary key on [idrespuesta] in table 'RespuestaCategoriaJugador'
ALTER TABLE [dbo].[RespuestaCategoriaJugador]
ADD CONSTRAINT [PK_RespuestaCategoriaJugador]
    PRIMARY KEY CLUSTERED ([idrespuesta] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [idcategoria] in table 'RespuestaCategoriaJugador'
ALTER TABLE [dbo].[RespuestaCategoriaJugador]
ADD CONSTRAINT [FK_respuestacategoria_jugador_categoria_partida]
    FOREIGN KEY ([idcategoria])
    REFERENCES [dbo].[CategoriaPartida]
        ([idcategoria])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_respuestacategoria_jugador_categoria_partida'
CREATE INDEX [IX_FK_respuestacategoria_jugador_categoria_partida]
ON [dbo].[RespuestaCategoriaJugador]
    ([idcategoria]);
GO

-- Creating foreign key on [idcuenta] in table 'Jugador'
ALTER TABLE [dbo].[Jugador]
ADD CONSTRAINT [FK__jugador__idcuent__3B75D760]
    FOREIGN KEY ([idcuenta])
    REFERENCES [dbo].[Cuenta]
        ([idcuenta])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__jugador__idcuent__3B75D760'
CREATE INDEX [IX_FK__jugador__idcuent__3B75D760]
ON [dbo].[Jugador]
    ([idcuenta]);
GO

-- Creating foreign key on [idpartida] in table 'Jugador'
ALTER TABLE [dbo].[Jugador]
ADD CONSTRAINT [FK__jugador__idparti__3C69FB99]
    FOREIGN KEY ([idpartida])
    REFERENCES [dbo].[Partida]
        ([idpartida])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__jugador__idparti__3C69FB99'
CREATE INDEX [IX_FK__jugador__idparti__3C69FB99]
ON [dbo].[Jugador]
    ([idpartida]);
GO

-- Creating foreign key on [idjugador] in table 'RespuestaCategoriaJugador'
ALTER TABLE [dbo].[RespuestaCategoriaJugador]
ADD CONSTRAINT [FK__respuesta__idjug__412EB0B6]
    FOREIGN KEY ([idjugador])
    REFERENCES [dbo].[Jugador]
        ([idjugador])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__respuesta__idjug__412EB0B6'
CREATE INDEX [IX_FK__respuesta__idjug__412EB0B6]
ON [dbo].[RespuestaCategoriaJugador]
    ([idjugador]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------