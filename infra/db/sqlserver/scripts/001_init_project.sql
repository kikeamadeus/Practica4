/* ==========================================
   CREATE DATABASE
========================================== */
IF DB_ID('Practica4Db') IS NULL
BEGIN
    CREATE DATABASE Practica4Db;
END
GO

USE Practica4Db;
GO

/* ==========================================
   TABLE: users (texto plano)
========================================== */
IF OBJECT_ID('dbo.users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.users (
        id INT IDENTITY(1,1) PRIMARY KEY,
        username VARCHAR(50) NOT NULL,
        password VARCHAR(100) NOT NULL,
        created_at DATETIME NOT NULL
    );
END
GO