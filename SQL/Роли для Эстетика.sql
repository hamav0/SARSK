use Эстетика
exec sp_addrole 'sys_admin'
exec sp_addrole 'Кассир'
grant delete, insert, update, select to sys_admin grant execute to sys_admin

grant UPDATE to Кассир;
grant SELECT to Кассир;
grant EXECUTE to Кассир;
-- пример
use master
create login Sergei with password = '321';
create login Dima with password = '123';
use Эстетика

create user Sergei_sys for login Sergei;
alter role sys_admin add member Sergei_sys;

create user Dima_k for login Dima;
alter role Кассир add member Dima_k;
