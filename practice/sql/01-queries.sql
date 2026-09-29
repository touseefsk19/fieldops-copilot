USE SqlPractice;

PRINT '--- Q1 ---';
SELECT Id, Priority, STATUS
FROM WorkOrders
WHERE Status <> 'Closed'
ORDER BY ID;

PRINT '--- Q2 ---';
SELECT w.Id, e.Tag, t.Name
FROM WorkOrders w
INNER JOIN Equipment e ON e.Id = w.EquipmentId
INNER JOIN Technicians t ON t.Id = w.TechnicianId
WHERE w.Priority = 'High'
ORDER BY w.Id;

PRINT '--- Q3 ---';
SELECT w.Id, e.Tag, t.Name
FROM WorkOrders w
INNER JOIN Equipment e ON e.Id = w.EquipmentId
LEFT JOIN Technicians t ON t.Id = w.TechnicianId
WHERE w.Status = 'Open'
ORDER BY w.Id;

PRINT '--- Q4 ---';
SELECT e.Tag, COUNT(w.Id) AS Orders
FROM Equipment e 
LEFT JOIN WorkOrders w ON w.EquipmentId = e.Id
GROUP BY e.Tag
ORDER BY Orders DESC, e.Tag;

PRINT '--- Q5 ---';
SELECT t.Name, SUM(w.Hours) AS TotalHours
FROM Technicians t
INNER JOIN WorkOrders w ON w.TechnicianId = t.Id
GROUP BY t.Name 
HAVING SUM(w.Hours) > 5
ORDER BY TotalHours Desc;

PRINT '--- Q6 ---';
SELECT e.Tag
FROM Equipment e
LEFT JOIN WorkOrders w ON w.EquipmentId = e.Id
WHERE w.Id IS NULL;

PRINT '--- Q7 ---';
SELECT e.Area, e.Tag, COUNT(w.Id) AS Orders,
Rank() OVER (PARTITION BY e.Area ORDER BY COUNT(w.Id) DESC) AS RankInArea
FROM Equipment e 
LEFT JOIN WorkOrders w ON w.EquipmentId = e.Id 
GROUP BY e.Area, e.Tag 
ORDER BY e.Area, RankInArea;

PRINT '--- Q8 ---';
WITH Latest AS(
    SELECT w.EquipmentId, w.Id, w.OpenedOn,
    ROW_NUMBER() OVER (PARTITION BY w.EquipmentId ORDER BY w.OpenedOn DESC) AS rn 
    FROM WorkOrders w
)
SELECT e.Tag, l.Id AS LatestOrder, l.OpenedOn
FROM Latest l
INNER JOIN Equipment e ON e.Id = l.EquipmentId
WHERE l.rn = 1
ORDER BY e.Tag;

PRINT '--- Q9 ---';
SELECT Id, OpenedON, Hours,
        SUM(Hours) OVER (ORDER BY OpenedOn) AS RunningHours
        FROM WorkOrders
        WHERE TechnicianId =1
        ORDER BY OpenedOn;

PRINT '--- Q10 ---';
SELECT Priority,
        COUNT(*) AS Total,
        SUM(CASE WHEN Status = 'Closed' THEN 1 ELSE 0 END) AS ClosedCount
        FROM WorkOrders
        Group BY Priority
        ORDER BY Priority;
