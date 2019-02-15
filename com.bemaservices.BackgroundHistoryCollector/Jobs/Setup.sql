USE [RockNPM]
GO

INSERT INTO [dbo].[ServiceJob]
           ([IsSystem]
           ,[IsActive]
           ,[Name]
           ,[Description]
           ,[Assembly]
           ,[Class]
           ,[CronExpression]
           ,[LastSuccessfulRunDateTime]
           ,[LastRunDateTime]
           ,[LastRunDurationSeconds]
           ,[LastStatus]
           ,[LastStatusMessage]
           ,[LastRunSchedulerName]
           ,[NotificationEmails]
           ,[NotificationStatus]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId])
     VALUES
           (0
           ,0
           ,'Background History Collector'
           ,'Sweeps through the Attribute Values for background checks and keeps an historical record'
           ,NULL
           ,'com.bemaservices.BackgroundHistoryCollector.Jobs.BackgroundHistoryCollector'
           ,'0 0 12 1/1 * ? *'
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,3
           ,'27D27CF1-4D11-458E-98CC-CE03CC8FB64C'
           ,GETDATE()
           ,NULL
           ,NULL
           ,NULL
           ,'BEMA'
           ,NULL
           ,NULL)
GO


