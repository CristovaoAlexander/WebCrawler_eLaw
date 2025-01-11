CREATE TABLE WebCrawlerExecutionLog (
    ExecutionId INT IDENTITY(1,1) PRIMARY KEY,
    StartTime DATETIME,
    EndTime DATETIME,
    PageCount INT,
    LinhasCount INT,
    JsonFilePath NVARCHAR(255)
);

select * from WebCrawlerExecutionLog;
