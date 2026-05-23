use Орион
go
create procedure Ввод_клиент
	@Фамилия Nvarchar(30),
	@Имя Nvarchar(30),
	@Отчество Nvarchar(30),
	@пол Nvarchar(1),
	@Дата_рождения date = NULL,
	@Номер_телефона varchar(11),
	@Паспорт varchar(10)
as
begin
	declare @Клиент_ИД Nvarchar(11);
	DECLARE @NextSequenceNumber INT;
	DECLARE @FormattedNumber nvarCHAR(11);
	while 1=1
	begin
	SELECT @NextSequenceNumber = ISNULL(MAX(TRY_CAST(SUBSTRING(Клиент_ИД, PATINDEX('%[0-9]%', Клиент_ИД), LEN(Клиент_ИД)) AS INT)), 0) + 1
	FROM Клиентская_База;
	SET @FormattedNumber = FORMAT(@NextSequenceNumber, '00000000000');
	SET @Клиент_ИД = @FormattedNumber;
	IF NOT EXISTS (SELECT 1 FROM Клиентская_База WHERE Клиент_ИД = @Клиент_ИД)
    BEGIN -- пришлось разделить из-за размера
    INSERT INTO Клиентская_База(Клиент_ИД, Фамилия, Имя, Отчество,
	Пол, Дата_рождения, Номер_телефона, Паспорт)
    VALUES (@Клиент_ИД, @Фамилия, @Имя, @Отчество,
	@пол, @Дата_рождения, @Номер_телефона, @Паспорт);
	break;
	end;
end;
end
go
create procedure Ввод_парикмахеры
	@ФИО Nvarchar(90)
as
begin
	declare @Парикмахер_ИД int
	select @Парикмахер_ИД = ISNULL(MAX(Парикмахер_ИД), 0) + 1
	from Парикмахеры;
    INSERT INTO Парикмахеры(Парикмахер_ИД, ФИО)
    VALUES (@Парикмахер_ИД, @ФИО);
end
go
create procedure Товары_ввод
	@Название Nvarchar(100),
	@Стоимость money
as
begin
	INSERT INTO Товары(Название, Стоимость)
    VALUES (@Название, @Стоимость);
end
go
create procedure Ввод_стрижки
	@Название Nvarchar(50) -- 0 делаем, 1 больше не делаем
as
begin
	declare @Актуальность Nvarchar(1); 
	set @Актуальность = 0
	INSERT INTO Стрижки(Название, Актуальность)
    VALUES (@Название, @Актуальность);
end
go
create procedure Ввод_заказ
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
	FROM Заказ;
	SET @FormattedNumber = FORMAT(@NextSequenceNumber, '00000000000');
	SET @Номер_заказа = @FormattedNumber;
	IF NOT EXISTS (SELECT 1 FROM Заказ WHERE Номер_заказа = @Номер_заказа)
    BEGIN
    INSERT INTO Заказ(Номер_заказа, Тип_заказа, Клиент_ИД, Дата)
    VALUES (@Номер_заказа, @Тип_заказа, @Клиент_ИД, @Дата);
	INSERT INTO tempValue(CombinedValue)
	VALUES (@Номер_заказа)
	break;
	end
end
end
go
create procedure Ввод_услуги
	@Парикмахер_ИД Nvarchar(2),
	@Название Nvarchar(50),
	@Стоимость money,
	@Время_нач Nvarchar(5),
	@Время_кон Nvarchar(5)
as
begin
	declare @Номер_заказа Nvarchar(11);
	select @Номер_заказа = CombinedValue
	from tempValue -- пришлось разделить из-за размера
	insert into Услуга(Номер_заказа, Парикмахер_ИД, Название, Стоимость, Время_нач, Время_кон)
	values (@Номер_заказа, @Парикмахер_ИД, @Название, @Стоимость, @Время_нач, @Время_кон)
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
