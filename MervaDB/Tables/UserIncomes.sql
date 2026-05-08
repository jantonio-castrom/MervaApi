CREATE TABLE [dbo].[UserIncomes]
(
    [IncomeId]   INT           NOT NULL IDENTITY(1,1),
    [TokenId]    INT           NOT NULL,
    [Name]       NVARCHAR(MAX) NOT NULL,
    [Amount]     NVARCHAR(MAX) NOT NULL,
    [Currency]   NVARCHAR(MAX) NOT NULL,
    [Category]   NVARCHAR(MAX) NULL,
    [IncomeDate] DATE          NOT NULL,
    [CreatedAt]  DATETIME2     NOT NULL CONSTRAINT [DF_UserIncomes_CreatedAt] DEFAULT GETUTCDATE(),
    [IsDeleted]  BIT           NOT NULL CONSTRAINT [DF_UserIncomes_IsDeleted] DEFAULT 0,
    [DeletedAt]  DATETIME2     NULL,

    CONSTRAINT [PK_UserIncomes] PRIMARY KEY CLUSTERED ([IncomeId] ASC),
    CONSTRAINT [FK_UserIncomes_UserTokens] FOREIGN KEY ([TokenId]) REFERENCES [dbo].[UserTokens] ([TokenId])
)
