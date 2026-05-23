--go
--create database Орион COLLATE Cyrillic_General_CI_AS;
go
use Орион
create table Клиентская_База
(
	Клиент_ИД Nvarchar(11) not null primary key,
	Фамилия Nvarchar(30) not null,
	Имя Nvarchar(30) not null,
	Отчество Nvarchar(30),
	Пол Nvarchar(1) not null,
	Дата_рождения date,
	Номер_телефона varchar(11),
	Паспорт varchar(10),
)
create table Парикмахеры
(
	Парикмахер_ИД nvarchar(2) not null primary key,
	ФИО Nvarchar(90) not null,
)
create table Товары
(
	Название Nvarchar(100) not null primary key,
	Стоимость money not null
)
create table Стрижки
(
	Название Nvarchar(50) not null primary key,
	Актуальность Nvarchar(1) not null, -- 0 делаем, 1 больше не делаем
);
create table Заказ
(
	Номер_заказа Nvarchar(11) not null primary key,
	Тип_заказа Nvarchar(1) not null, -- 0 отмена, 1 предварительный заказ, 2 исполнено
	Клиент_ИД Nvarchar(11) not null,
	Дата date not null,
	Foreign key (Клиент_ИД) references Клиентская_База (Клиент_ИД)
);
create table Услуга
(
	Номер_заказа Nvarchar(11) not null,
	Парикмахер_ИД Nvarchar(2) not null,
	Название Nvarchar(50) not null,
	Стоимость money not null,
	Время_нач Nvarchar(5),
	Время_кон Nvarchar(5)
	Foreign key (Название) references Стрижки (Название),
	Foreign key (Номер_заказа) references Заказ (Номер_заказа),
	Foreign key (Парикмахер_ИД) references Парикмахеры (Парикмахер_ИД),
)
create table Продажа
(
	Номер_заказа Nvarchar(11) not null,
	Товар Nvarchar(100) not null,
	Количество int not null,
	Стоимость money not null,
	Foreign key (Номер_заказа) references Заказ (Номер_заказа),
	Foreign key (Товар) references Товары (Название),
);
CREATE TABLE ErrorLog (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    ErrorTime DATETIME DEFAULT GETDATE(),
    ErrorNumber INT,
    ErrorMessage NVARCHAR(4000),
    ErrorProcedure NVARCHAR(128),
    ErrorLine INT
);
create table tempValue
( CombinedValue Nvarchar(11) primary key)