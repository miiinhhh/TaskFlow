USE TaskFlowDb;
GO

INSERT INTO Roles (Name, Description)
VALUES
(N'Admin', N'Quản trị hệ thống'),
(N'Manager', N'Quản lý dự án'),
(N'Member', N'Thành viên dự án');
GO

INSERT INTO TaskStatuses (Name, Description, DisplayOrder, IsCompleted)
VALUES
(N'To Do', N'Chưa bắt đầu', 1, 0),
(N'In Progress', N'Đang thực hiện', 2, 0),
(N'Review', N'Đang chờ kiểm tra', 3, 0),
(N'Done', N'Đã hoàn thành', 4, 1),
(N'Cancelled', N'Đã hủy', 5, 0);
GO

INSERT INTO TaskPriorities (Name, Description, Level)
VALUES
(N'Low', N'Ưu tiên thấp', 1),
(N'Medium', N'Ưu tiên trung bình', 2),
(N'High', N'Ưu tiên cao', 3),
(N'Critical', N'Khẩn cấp', 4);
GO

-- Demo users only. PasswordHash will be replaced by the ASP.NET Core
-- authentication implementation with a real BCrypt/PBKDF2/Identity hash.
INSERT INTO Users (RoleId, Username, Email, PasswordHash, FullName)
VALUES
(1, N'admin', N'admin@taskflow.com', N'DEMO_PASSWORD_HASH', N'System Administrator'),
(2, N'manager', N'manager@taskflow.com', N'DEMO_PASSWORD_HASH', N'TaskFlow Manager'),
(3, N'minh', N'minh@taskflow.com', N'DEMO_PASSWORD_HASH', N'Minh');
GO

INSERT INTO Projects (OwnerId, Name, Description, StartDate)
VALUES
(2, N'TaskFlow Development', N'Dự án quản lý công việc TaskFlow', CAST(GETDATE() AS DATE));
GO

INSERT INTO ProjectMembers (ProjectId, UserId)
VALUES
(1, 2),
(1, 3);
GO

INSERT INTO Tasks
(
    ProjectId, CreatedById, StatusId, PriorityId,
    Title, Description, StartDate, DueDate
)
VALUES
(
    1, 2, 1, 3,
    N'Build Task API',
    N'Xây dựng REST API cho chức năng quản lý Task',
    GETDATE(),
    DATEADD(DAY, 7, GETDATE())
);
GO

INSERT INTO TaskAssignments (TaskId, UserId, AssignedById)
VALUES
(1, 3, 2);
GO
