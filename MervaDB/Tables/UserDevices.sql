CREATE TABLE [dbo].[UserDevices]
(
    [DeviceId]       INT           NOT NULL IDENTITY(1,1),
    [TokenId]        INT           NOT NULL,
    [UserAgent]      NVARCHAR(500) NULL,
    [Browser]        NVARCHAR(100) NULL,
    [BrowserVersion] NVARCHAR(50)  NULL,
    [OperatingSystem] NVARCHAR(100) NULL,
    [Language]       NVARCHAR(20)  NULL,
    [Timezone]       NVARCHAR(100) NULL,
    [IpAddress]      NVARCHAR(45)  NULL,
    [Country]        NVARCHAR(100) NULL,
    [Region]         NVARCHAR(100) NULL,
    [City]           NVARCHAR(100) NULL,
    [Isp]            NVARCHAR(200) NULL,
    [ConnectionType] NVARCHAR(50)  NULL,
    [RecordedAt]     DATETIME2     NOT NULL CONSTRAINT [DF_UserDevices_RecordedAt] DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_UserDevices] PRIMARY KEY CLUSTERED ([DeviceId] ASC),
    CONSTRAINT [FK_UserDevices_UserTokens] FOREIGN KEY ([TokenId]) REFERENCES [dbo].[UserTokens] ([TokenId])
)
