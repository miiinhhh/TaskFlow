USE TaskFlowDb;
GO

CREATE INDEX IX_Projects_OwnerId ON Projects(OwnerId);
CREATE INDEX IX_ProjectMembers_UserId ON ProjectMembers(UserId);
CREATE INDEX IX_Tasks_ProjectId ON Tasks(ProjectId);
CREATE INDEX IX_Tasks_StatusId ON Tasks(StatusId);
CREATE INDEX IX_Tasks_PriorityId ON Tasks(PriorityId);
CREATE INDEX IX_Tasks_DueDate ON Tasks(DueDate);
CREATE INDEX IX_TaskAssignments_UserId ON TaskAssignments(UserId);
CREATE INDEX IX_TaskComments_TaskId ON TaskComments(TaskId);
CREATE INDEX IX_TaskHistories_TaskId ON TaskHistories(TaskId);
CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);
GO
