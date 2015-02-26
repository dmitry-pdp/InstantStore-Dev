USE master;
GO
ALTER DATABASE InstantStore
   SET RECOVERY FULL;
GO
-- Create Data and Log logical backup devices. 
USE master
GO
EXEC sp_addumpdevice 'disk', 'InstantStoreData', 
'D:\Work\Sakharau\InstantStoreDB_Backups\InstantStoreData.bak';
GO
EXEC sp_addumpdevice 'disk', 'InstantStoreLog', 
'D:\Work\Sakharau\InstantStoreDB_Backups\InstantStoreLog.bak';
GO

-- Back up the full AdventureWorks2012 database.
BACKUP DATABASE InstantStore TO InstantStoreData;
GO
-- Back up the AdventureWorks2012 log.
BACKUP LOG InstantStore
   TO InstantStoreLog;
GO