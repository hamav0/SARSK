use Эстетика
create table Продажи
(
	Пол Nvarchar(7) not null,
	Возраст Nvarchar(3),
	Услуга Nvarchar(50), 
	Повторяемость Nvarchar(3),
	Средний_чек money,
	Источник_обращения Nvarchar(30)
)
go
create procedure Продажи_ввод
	@период_от date,
	@период_до date,
	@Клиент_ИД Nvarchar(11)
as
begin 
	SET NOCOUNT ON;
	delete from Продажи;
	
	declare @Пол Nvarchar(7);
	declare @Возраст Nvarchar(3);
	declare @Дата_рождения date;

	DECLARE @ClientServices TABLE (
        Номер_заказа VARCHAR(11),
        Услуга_Название VARCHAR(50)
    );
	select @Дата_рождения = Клиентская_База.Дата_рождения from Клиентская_База
	where Клиентская_База.Клиент_ИД = @Клиент_ИД
    set @Возраст = DATEDIFF(YEAR, @Дата_рождения, GETDATE()) - 
    CASE 
        WHEN (MONTH(@Дата_рождения) > MONTH(GETDATE())) 
          OR (MONTH(@Дата_рождения) = MONTH(GETDATE()) AND DAY(@Дата_рождения) > DAY(GETDATE())) 
        THEN 1 
        ELSE 0 
    END

	select @Пол = CASE Клиентская_База.Пол
        WHEN 'м' THEN 'Мужской'
        WHEN 'ж' THEN 'Женский'
    END
	from Клиентская_База
	where Клиентская_База.Клиент_ИД = @Клиент_ИД 
	
	DECLARE @temp_Procedurs TABLE (
        Клиент_ID NVARCHAR(11),
        Название_услуги NVARCHAR(50),
        Повторяемость INT
    );
	INSERT INTO @temp_Procedurs (Клиент_ID, Название_услуги, Повторяемость)
    SELECT
        K.Клиент_ИД,
        U.Название,
        COUNT(*) AS Повторяемость
    FROM Клиентская_База AS K
    JOIN Заказ AS Z ON K.Клиент_ИД = Z.Клиент_ИД
    JOIN Услуга AS U ON Z.Номер_заказа = U.Номер_заказа
    WHERE Z.Дата >= @период_от AND Z.Дата <= @период_до and K.Клиент_ИД = @Клиент_ИД
    GROUP BY K.Клиент_ИД, U.Название;

	INSERT INTO Продажи (Пол, Возраст, Услуга, Повторяемость, Средний_чек, Источник_обращения)
    SELECT
         @Пол,
         @Возраст,
         TP.Название_услуги,
         TP.Повторяемость,
         (SELECT AVG(Стоимость) FROM Услуга AS U JOIN Заказ AS Z ON U.Номер_заказа = Z.Номер_заказа WHERE Z.Клиент_ИД = K.Клиент_ИД and Z.Дата >= @период_от AND Z.Дата <= @период_до),
         K.Источник_обращения
    FROM Клиентская_База AS K
    JOIN @temp_Procedurs AS TP ON K.Клиент_ИД = TP.Клиент_ID
    WHERE K.Клиент_ИД = @Клиент_ИД
end