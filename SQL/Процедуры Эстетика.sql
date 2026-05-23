use Эстетика
go
create procedure Ввод_клиент
	@Подразделение nvarchar(2),
	@Фамилия Nvarchar(30),
	@Имя Nvarchar(30),
	@Отчество Nvarchar(30),
	@пол Nvarchar(1),
	@Дата_рождения date,
	@Номер_телефона varchar(11),
	@Паспорт varchar(10),
	@Комментарий Nvarchar(100),
	@Источник_обращения Nvarchar(30)
as
begin
	declare @Клиент_ИД Nvarchar(11);
	DECLARE @NextSequenceNumber INT;
	DECLARE @FormattedNumber nvarCHAR(11);
	while 1=1
	begin
	select @NextSequenceNumber = ISNULL(MAX(TRY_CAST(SUBSTRING(Клиент_ИД, PATINDEX('%[0-9]%', Клиент_ИД), LEN(Клиент_ИД)) AS INT)), 0) + 1
	FROM Клиентская_База
	WHERE Подразделение = @Подразделение;
	SET @FormattedNumber = FORMAT(@NextSequenceNumber, '00000000');
	SET @Клиент_ИД = @Подразделение + '-' + @FormattedNumber;
	IF NOT EXISTS (SELECT 1 FROM Клиентская_База WHERE Клиент_ИД = @Клиент_ИД)
    BEGIN -- пришлось разделить из-за размера
    INSERT INTO Клиентская_База(Клиент_ИД, Подразделение, Фамилия, Имя, Отчество,
	Пол, Дата_рождения, Номер_телефона, Паспорт, Комментарий, Источник_обращения)
    VALUES (@Клиент_ИД, @Подразделение, @Фамилия, @Имя, @Отчество,
	@пол, @Дата_рождения, @Номер_телефона, @Паспорт, @Комментарий, @Источник_обращения);
	break;
	end;
end;
end
go
create procedure Ввод_врачи
	@ФИО Nvarchar(90)
as
begin
	declare @Врач_ИД int
	select @Врач_ИД = ISNULL(MAX(Врач_ИД), 0) + 1
	from Врачи;
    INSERT INTO Врачи(Врач_ИД, ФИО)
    VALUES (@Врач_ИД, @ФИО);
end
go
create procedure Ввод_администраторы
	@ФИО Nvarchar(90),
	@Username NVARCHAR(50)
as
begin
	declare @Администратор_ИД int
	select @Администратор_ИД = ISNULL(MAX(Администратор_ИД), 0) + 1
	from Администраторы;
    INSERT INTO Администраторы(Администратор_ИД, ФИО, Username)
    VALUES (@Администратор_ИД, @ФИО, @Username);
end
go
create procedure Бренд_ввод
	@Бренд Nvarchar(20)
as
begin
	declare @Бренд_ИД int
	select @Бренд_ИД = ISNULL(MAX(Бренд_ИД), 0) + 1
	from Бренды
	INSERT INTO Бренды(Бренд_ИД, Бренд)
    VALUES (@Бренд_ИД, @Бренд);
end
go
create procedure Товары_ввод
	@Бренд_ИД int,
	@Название Nvarchar(100),
	@Стоимость money
as
begin
	INSERT INTO Товары(Бренд_ИД, Название, Стоимость)
    VALUES (@Бренд_ИД, @Название, @Стоимость);
end
go
create procedure Материалы_ввод
	@Название Nvarchar(100),
	@Стоимость money
as
begin
	INSERT INTO Материалы(Название, Стоимость)
    VALUES (@Название, @Стоимость);
end
go
create procedure Ввод_процедуры
	@Название Nvarchar(50) -- 0 делаем, 1 больше не делаем
as
begin
	declare @Актуальность Nvarchar(1); 
	set @Актуальность = 0
	INSERT INTO Процедуры(Название, Актуальность)
    VALUES (@Название, @Актуальность);
end
go
create procedure Ввод_заказ
	@Подразделение nvarchar(2),
	@Тип_заказа Nvarchar(1), -- 0 отмена, 1 предварительный заказ, 2 исполнено
	@Клиент_ИД Nvarchar(11),
	@Дата date
as
begin
	declare @Номер_заказа Nvarchar(11);
	DECLARE @NextSequenceNumber INT;
	DECLARE @FormattedNumber nvarCHAR(11);
	while 1=1
	begin
	SELECT @NextSequenceNumber = ISNULL(MAX(TRY_CAST(SUBSTRING(Номер_заказа, PATINDEX('%[0-9]%', Номер_заказа), LEN(Номер_заказа)) AS INT)), 0) + 1
	FROM Заказ
	WHERE Подразделение = @Подразделение;
	SET @FormattedNumber = FORMAT(@NextSequenceNumber, '00000000');
	SET @Номер_заказа = @Подразделение + '-' + @FormattedNumber;
	IF NOT EXISTS (SELECT 1 FROM Заказ WHERE Номер_заказа = @Номер_заказа)
    BEGIN
    INSERT INTO Заказ(Номер_заказа,Подразделение, Тип_заказа, Клиент_ИД, Дата)
    VALUES (@Номер_заказа,@Подразделение, @Тип_заказа, @Клиент_ИД, @Дата);
	INSERT INTO tempValue(CombinedValue)
	VALUES (@Номер_заказа)
	break;
	end
end
end
go
create procedure Ввод_услуга
	@Врач_ИД Nvarchar(2),
	@Название Nvarchar(50),
	@Стоимость money,
	@Количество int,
	@Количество_вспышек int,
	@Время_нач Nvarchar(5),
	@Время_кон Nvarchar(5)
as
begin
	declare @Номер_заказа Nvarchar(11);
	select @Номер_заказа = CombinedValue
	from tempValue -- пришлось разделить из-за размера
	insert into Услуга(Номер_заказа, Врач_ИД, Название, Стоимость,
	Количество, Количество_вспышек, Время_нач, Время_кон)
	values (@Номер_заказа, @Врач_ИД, @Название, @Стоимость,
	@Количество, @Количество_вспышек, @Время_нач, @Время_кон)
end
go
create procedure Ввод_расход
	@Название Nvarchar(100),
	@Количество int
as
begin
	declare @Номер_заказа Nvarchar(11);
	select @Номер_заказа = CombinedValue
	from tempValue
	insert into Расход(Номер_заказа, Название, Количество)
	values (@Номер_заказа, @Название, @Количество)
end
go
create procedure Ввод_продажа
	@Товар Nvarchar(100),
	@Количество int,
	@Стоимость money
as
begin
	declare @Номер_заказа Nvarchar(11);
	select @Номер_заказа = CombinedValue
	from tempValue
	insert into Продажа(Номер_заказа, Товар, Количество, Стоимость)
	values (@Номер_заказа, @Товар, @Количество, @Стоимость)
end
go
create procedure Конец
as
begin
DELETE FROM tempValue;
end