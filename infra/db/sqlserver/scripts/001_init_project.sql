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
   TABLE: users_plain
========================================== */
IF OBJECT_ID('dbo.users_plain', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.users_plain (
        id INT IDENTITY(1,1) PRIMARY KEY,
        username VARCHAR(50) NOT NULL,
        password VARCHAR(100) NOT NULL,
        created_at DATETIME NOT NULL
    );
END
GO

/* ==========================================
   INSERT ADMIN USER (texto plano)
========================================== */
IF NOT EXISTS (SELECT 1 FROM dbo.users_plain WHERE username = 'admin')
BEGIN
    INSERT INTO dbo.users_plain (username, password, created_at)
    VALUES ('admin', 'admin123', GETDATE());
END
GO