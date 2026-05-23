use Орион
exec sp_addrole 'sys_admin'
exec sp_addrole 'Кассир'
grant delete, insert, update, select to sys_admin grant execute to sys_admin

grant UPDATE to Кассир;
grant SELECT to Кассир;
grant EXECUTE to Кассир;
-- пример
use master
create login sas with password = '123';
create login Evil with password = '666';
use Орион

create user sas_sys for login sas;
alter role sys_admin add member sas_sys;

create user Evil_k for login Evil;
alter role Кассир add member Evil_k;
