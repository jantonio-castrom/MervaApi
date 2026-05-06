CREATE TABLE [dbo].[UserPreferences]
(
    [PreferenceId]      INT          NOT NULL IDENTITY(1,1),
    [TokenId]           INT          NOT NULL,
    [DefaultCurrency]   NCHAR(3)     NOT NULL CONSTRAINT [DF_UserPreferences_DefaultCurrency] DEFAULT N'USD',
    [Theme]             NVARCHAR(20) NOT NULL CONSTRAINT [DF_UserPreferences_Theme] DEFAULT N'light',
    [UpdatedAt]         DATETIME2    NOT NULL CONSTRAINT [DF_UserPreferences_UpdatedAt] DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_UserPreferences] PRIMARY KEY CLUSTERED ([PreferenceId] ASC),
    CONSTRAINT [FK_UserPreferences_UserTokens] FOREIGN KEY ([TokenId]) REFERENCES [dbo].[UserTokens] ([TokenId]),
    CONSTRAINT [UQ_UserPreferences_TokenId] UNIQUE ([TokenId])
)
