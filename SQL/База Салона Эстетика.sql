--go
--create database Эстетика COLLATE Cyrillic_General_CI_AS;
go
use Эстетика
create table Отдел
(
	Подразделение nvarchar(2) not null primary key,
	Наименование nvarchar(40) not null,
)
create table Клиентская_База
(
	Клиент_ИД Nvarchar(11) not null primary key,
	Подразделение nvarchar(2) not null,
	Фамилия Nvarchar(30) not null,
	Имя Nvarchar(30) not null,
	Отчество Nvarchar(30),
	Пол Nvarchar(1) not null,
	Дата_рождения date,
	Номер_телефона varchar(11),
	Паспорт varchar(10),
	Комментарий Nvarchar(100),
	Источник_обращения Nvarchar(30)
	Foreign key (Подразделение) references Отдел(Подразделение),
)
create table Врачи
(
	Врач_ИД nvarchar(2) not null primary key,
	ФИО Nvarchar(90) not null,
)
create table Администраторы
(
	Администратор_ИД nvarchar(2) not null primary key,
	ФИО Nvarchar(90) not null,
	Username NVARCHAR(50) NOT NULL UNIQUE
)
create table Бренды
(
	Бренд_ИД int not null primary key,
	Бренд Nvarchar(20) not null,
)
create table Товары
(
	Бренд_ИД int not null,
	Название Nvarchar(100) not null primary key,
	Стоимость money not null,
	Foreign key (Бренд_ИД) references Бренды (Бренд_ИД),
)
create table Материалы
(
	Название Nvarchar(100) not null primary key,
	Стоимость money not null,
)
create table Процедуры
(
	Название Nvarchar(50) not null primary key,
	Актуальность Nvarchar(1) not null, -- 0 делаем, 1 больше не делаем
);
create table Заказ
(
	Номер_заказа Nvarchar(11) not null primary key,
	Подразделение nvarchar(2) not null,
	Тип_заказа Nvarchar(1) not null, -- 0 отмена, 1 предварительный заказ, 2 исполнено
	Клиент_ИД Nvarchar(11) not null,
	Дата date not null,
	Foreign key (Клиент_ИД) references Клиентская_База (Клиент_ИД),
	Foreign key (Подразделение) references Отдел (Подразделение),
);
create table Услуга
(
	Номер_заказа Nvarchar(11) not null,
	Врач_ИД Nvarchar(2) not null,
	Название Nvarchar(50) not null,
	Стоимость money not null,
	Количество int not null,
	Количество_вспышек int,
	Время_нач Nvarchar(5),
	Время_кон Nvarchar(5)
	Foreign key (Название) references Процедуры (Название),
	Foreign key (Номер_заказа) references Заказ (Номер_заказа),
	Foreign key (Врач_ИД) references Врачи (Врач_ИД),
)
create table Расход
(
	Номер_заказа Nvarchar(11) not null,
	Название Nvarchar(100) not null,
	Количество int not null,
	Foreign key (Номер_заказа) references Заказ (Номер_заказа),
	Foreign key (Название) references Материалы (Название),
);
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