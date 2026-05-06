CREATE TABLE [dbo].[Expenses]
(
    [ExpenseId]   INT             NOT NULL IDENTITY(1,1),
    [TokenId]     INT             NOT NULL,
    [Name]        NVARCHAR(MAX)   NOT NULL,
    [Amount]      NVARCHAR(MAX)   NOT NULL,
    [Currency]    NVARCHAR(MAX)   NOT NULL,
    [Category]    NVARCHAR(MAX)   NULL,
    [ExpenseDate] DATE            NOT NULL,
    [CreatedAt]   DATETIME2       NOT NULL CONSTRAINT [DF_Expenses_CreatedAt] DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_Expenses] PRIMARY KEY CLUSTERED ([ExpenseId] ASC),
    CONSTRAINT [FK_Expenses_UserTokens] FOREIGN KEY ([TokenId]) REFERENCES [dbo].[UserTokens] ([TokenId])
)
