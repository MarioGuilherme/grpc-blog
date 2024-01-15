CREATE DATABASE BlogGRPC;

USE BlogGRPC;

CREATE TABLE [BlogGRPC].[dbo].[Blogs] (
    [BlogId]   INT            IDENTITY (1, 1) NOT NULL,
    [AuthorId] INT            NOT NULL,
    [Title]    NVARCHAR (100) NOT NULL,
    [Content]  NVARCHAR (800) NOT NULL,
    PRIMARY KEY CLUSTERED ([BlogId] ASC)
);