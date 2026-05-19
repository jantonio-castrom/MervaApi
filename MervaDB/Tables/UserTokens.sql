CREATE TABLE [dbo].[UserTokens]
(
    [TokenId]            INT           NOT NULL IDENTITY(1,1),
    [Token]              NVARCHAR(MAX) NOT NULL,
    [EncryptedValueHash] VARBINARY(32) NULL,
    [CreatedAt]          DATETIME2     NOT NULL CONSTRAINT [DF_UserTokens_CreatedAt] DEFAULT GETUTCDATE(),
    [IsPremium]          BIT           NOT NULL CONSTRAINT [DF_UserTokens_IsPremium] DEFAULT 0,

    CONSTRAINT [PK_UserTokens] PRIMARY KEY CLUSTERED ([TokenId] ASC),
    CONSTRAINT [UQ_UserTokens_EncryptedValueHash] UNIQUE NONCLUSTERED ([EncryptedValueHash])
)
