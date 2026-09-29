USE SqlPractice;
CREATE INDEX IX_WorkOrders_Status ON WorkOrders(Status) INCLUDE (Priority);
EXEC sp_helpindex 'WorkOrders';