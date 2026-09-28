IF DB_ID('SqlPractice') IS NULL CREATE DATABASE SqlPractice;
GO
USE SqlPractice;
GO
DROP TABLE IF EXISTS WorkOrders;
DROP TABLE IF EXISTS Technicians;
DROP TABLE IF EXISTS Equipment;

CREATE TABLE Technicians (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Shift NVARCHAR(10) NOT NULL
);

CREATE TABLE Equipment (
    Id INT PRIMARY KEY,
    Tag NVARCHAR(20) NOT NULL,
    Area NVARCHAR(20) NOT NULL
);

CREATE TABLE WorkOrders (
    Id INT PRIMARY KEY,
    EquipmentId INT NOT NULL REFERENCES Equipment(Id),
    TechnicianId INT NULL REFERENCES Technicians(Id),
    Priority NVARCHAR(10) NOT NULL,
    Status NVARCHAR(12) NOT NULL,
    Hours DECIMAL(5,1) NOT NULL,
    OpenedOn DATE NOT NULL
);

INSERT INTO Technicians VALUES (1,'Arun','Day'),(2,'Bilal','Day'),(3,'Chitra','Night'),(4,'Deepak','Night');
INSERT INTO Equipment VALUES (1,'P-101','Unit 3'),(2,'P-102','Unit 3'),(3,'C-7','Unit 1'),(4,'V-20','Unit 1'),(5,'F-9','Unit 2');
INSERT INTO WorkOrders VALUES
(1,1,1,'High','Closed',4.0,'2026-09-01'),
(2,1,2,'Medium','Closed',2.5,'2026-09-03'),
(3,2,1,'Low','Open',0.0,'2026-09-05'),
(4,3,3,'High','InProgress',3.0,'2026-09-06'),
(5,3,3,'High','Closed',5.5,'2026-09-08'),
(6,4,NULL,'Medium','Open',0.0,'2026-09-10'),
(7,1,1,'High','Closed',6.0,'2026-09-12'),
(8,2,4,'Medium','Closed',1.5,'2026-09-15'),
(9,3,2,'Low','Closed',1.0,'2026-09-18'),
(10,1,3,'Medium','InProgress',2.0,'2026-09-20');

SELECT COUNT(*) AS WorkOrderRows FROM WorkOrders;