--go
--create database САРСК COLLATE Cyrillic_General_CI_AS;
go
use САРСК
create table Салоны
(
	Салон_ИД int not null,
	Наименование nvarchar(40) not null,
	Подключение nvarchar(100) not null
)

 INSERT INTO Салоны VALUES (1, 'Эстетика', 'Server=.\SQLEXPRESS;Database=Эстетика;')
 INSERT INTO Салоны VALUES (2, 'Орион', 'Server=.\SQLEXPRESS;Database=Орион;')


CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARBINARY(64) NOT NULL, -- Храним хеш, а не пароль!
    Saloon int not null
);
go
CREATE PROCEDURE add_user
@Username NVARCHAR(50),
@Password NVARCHAR(50),
@Saloon int
AS
BEGIN
    DECLARE @PasswordHash VARBINARY(64);
    SET @PasswordHash = HASHBYTES('SHA2_256', @Password);
    
    INSERT INTO Users (Username, PasswordHash, Saloon)
    VALUES (@Username, @PasswordHash, @Saloon);
END;
go
ALTER PROCEDURE check_user
@Username NVARCHAR(50),
@Password NVARCHAR(50)
AS
BEGIN
    DECLARE @PasswordHash VARBINARY(64);
    DECLARE @SALOON INT;
    SET @PasswordHash = HASHBYTES('SHA2_256', @Password);
    
    SET @SALOON = (SELECT Saloon 
    FROM Users
    WHERE @Username = Username AND @PasswordHash = PasswordHash);
    SELECT Подключение
    FROM Салоны
    WHERE @SALOON = Салон_ИД;
END;


go
exec add_user 'sa','123', 1;
go
exec add_user 'Dima','123', 1;
go
exec add_user 'Sergei','321', 1;

exec add_user 'sas','123', 2;
go

exec add_user 'Evil','666', 2;
go

exec check_user 'Dima','123';
go

GRANT SELECT ON Users TO public;
GO

