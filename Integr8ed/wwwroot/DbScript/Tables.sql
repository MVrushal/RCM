USE [{dynamicDatanase}]

CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[BookingDetails](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[UserGroupId] [bigint] NOT NULL,
	[MeetingTypeId] [bigint] NULL,
	[RoomTypeId] [bigint] NOT NULL,
	[BookingDate] [datetime] NULL,
	[StartTime] [nvarchar](50) NOT NULL,
	[FinishTime] [nvarchar](50) NOT NULL,
	[TitleOfMeeting] [nvarchar](max) NULL,
	[CatererRemark] [nvarchar](max) NULL,
	[NumberOfAttending] [int] NULL,
	[CarSpaceRequired] [int] NULL,
	[HouseKeepingRequired] [bit] NULL,
	[SecurityRequired] [bit] NULL,
	[Cost] [decimal](18, 0) NOT NULL,
	[LayoutInformation] [nvarchar](max) NULL,
	[TobeInvoiced] [bit] NULL,
	[TechnicianOnSite] [bit] NULL,
	[DisabledAccess] [bit] NULL,
	[ReturnedBookingForm] [bit] NULL,
	[BookingContact] [nvarchar](50) NULL,
	[Email] [nvarchar](256) NULL,
	[Mobile] [nvarchar](50) NULL,
	[DateFromSent] [nvarchar](50) NULL,
	[BookingStatus] [int] NOT NULL,
	[NotifyDays] [int] NOT NULL,
	[AdditionalInformation] [nvarchar](max) NULL,
	[CancellationDetail] [nvarchar](max) NULL,
	[ExternalBookingClientId] [bigint] NULL,
	[BranchId] [bigint] NOT NULL,
 CONSTRAINT [PK_BookingDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[BookingRequest](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[UserGroupId] [bigint] NOT NULL,
	[MeetingTypeId] [bigint] NULL,
	[RoomTypeId] [bigint] NOT NULL,
	[BookingDate] [datetime] NULL,
	[StartTime] [nvarchar](50) NOT NULL,
	[FinishTime] [nvarchar](50) NOT NULL,
	[TitleOfMeeting] [nvarchar](max) NULL,
	[CatererRemark] [nvarchar](max) NULL,
	[NumberOfAttending] [int] NULL,
	[CarSpaceRequired] [int] NULL,
	[HouseKeepingRequired] [bit] NULL,
	[SecurityRequired] [bit] NULL,
	[Cost] [decimal](18, 0) NOT NULL,
	[LayoutInformation] [nvarchar](max) NULL,
	[TobeInvoiced] [bit] NULL,
	[TechnicianOnSite] [bit] NULL,
	[DisabledAccess] [bit] NULL,
	[ReturnedBookingForm] [bit] NULL,
	[BookingContact] [nvarchar](50) NULL,
	[Mobile] [nvarchar](50) NULL,
	[DateFromSent] [nvarchar](50) NULL,
	[BookingStatus] [int] NOT NULL,
	[AdditionalInformation] [nvarchar](max) NULL,
	[CancellationDetail] [nvarchar](max) NULL,
	[ExternalBookingClientId] [bigint] NULL,
	[BranchId] [bigint] NOT NULL,
	[RequestStatus] [int] NULL,
 CONSTRAINT [PK_BookingRequest] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[BookingStatus](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Status] [nvarchar](max) NULL,
	[ColorCode][nvarchar](max)NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
 CONSTRAINT [PK_BookingStatus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[BranchMaster](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[BranchName] [varchar](max) NOT NULL,
 CONSTRAINT [PK_BranchMaster] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[CallLogs](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[Subject] [nvarchar](max) NULL,
	[EntryType] [int] NOT NULL,
	[DateOfentry] [date] NOT NULL,
	[Time] [nvarchar](max) NULL,
	[Contact] [nvarchar](max) NULL,
	[Address] [nvarchar](max) NULL,
	[Comments] [nvarchar](max) NULL,
	[PostCode] [nvarchar](max) NULL,
	[TakenBy] [nvarchar](max) NULL,
	[TakenFor] [nvarchar](max) NULL,
	[NextContactDate] [date] NOT NULL,
	[ISCompleted] [bit] NOT NULL,
	[Associated] [int] NULL,
	[BookingDetailId] [bigint] NULL,
 CONSTRAINT [PK_CallLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[Cat_Req_Menu](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[CatererMenuId] [bigint] NULL,
	[Cat_ReqId] [bigint] NULL,
 CONSTRAINT [PK_Cat_Req_Menu] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[CatererMenu](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[CatererId] [bigint] NOT NULL,
	[MenuId] [bigint] NOT NULL,
	[Cost] [decimal](18, 0) NOT NULL,
 CONSTRAINT [PK_CatererMenu] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Catering_Details](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[code] [nvarchar](max) NULL,
	[Title] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[CatererName] [nvarchar](max) NULL,
	[ContactName] [nvarchar](max) NULL,
	[Telephone] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[FaxNumber] [nvarchar](max) NULL,
	[Address] [nvarchar](max) NULL,
	[PostCode] [nvarchar](max) NULL,
	[BranchId] [bigint] NOT NULL,
 CONSTRAINT [PK_Catering_Details] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[Catering_Requirements](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[BookingDetailId] [bigint] NULL,
	[CatererId] [bigint] NULL,
	[Notes] [nvarchar](max) NULL,
	[TimeFor] [nvarchar](50) NULL,
	[TimeCollected] [nvarchar](50) NULL,
	[NumberOfPeople] [int] NOT NULL,
	[Cost] [decimal](20, 2) NOT NULL,
 CONSTRAINT [PK_Catering_Requirements] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[ContactDetails](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[BookingDetailId] [bigint] NULL,
	[Name] [nvarchar](50) NULL,
	[Department] [nvarchar](max) NULL,
	[Mobile] [nvarchar](50) NULL,
	[Email] [nvarchar](max) NULL,
 CONSTRAINT [PK_ContactDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[Entry_Types](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[code] [nvarchar](max) NULL,
	[Title] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[BranchId] [bigint] NOT NULL,
 CONSTRAINT [PK_Entry_Types] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[Equipment](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[code] [nvarchar](max) NULL,
	[Title] [nvarchar](max) NULL,
	[EquipId] [bigint] NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_Equipment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[EquipmentRequiredForBooking](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[BookingDetailId] [bigint] NOT NULL,
	[EquipmentRequiredId] [bigint] NOT NULL,
	[NoofItem] [int] NOT NULL,
 CONSTRAINT [PK_EquipmentRequiredForBooking] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[ExternalBookingRequest](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ExternalUserId] [bigint] NOT NULL,
	[BookingId] [bigint] NOT NULL,
	[IsAccepted] [bit] NOT NULL
) ON [PRIMARY]

CREATE TABLE [dbo].[InvoiceDetail](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[BookingDetailId] [bigint] NULL,
	[InvoiceAddress] [nvarchar](max) NOT NULL,
	[InvoicePostCode] [nvarchar](50) NULL,
	[ContactName] [nvarchar](50) NULL,
	[InvoiceTo] [nvarchar](50) NOT NULL,
	[Mobile] [nvarchar](50) NOT NULL,
	[Fax] [nvarchar](50) NULL,
	[Email] [nvarchar](max) NOT NULL,
	[HireofFacilities] [decimal](18, 0) NULL,
	[CostCentreCode] [nvarchar](50) NULL,
	[BudgetCode] [nvarchar](50) NULL,
	[VatRate] [decimal](18, 0) NULL,
	[ProfitValue] [decimal](18, 0) NULL,
	[InvoiceNotes] [nvarchar](max) NULL,
	[InvoiceAmount] [decimal](18, 0) NOT NULL,
	[VatAmount] [decimal](18, 0) NOT NULL,
	[GrossAmount] [decimal](18, 0) NOT NULL,
	[InvoiceRequestDate] [nvarchar](max) NULL,
 CONSTRAINT [PK_InvoiceDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[Invoices](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[code] [nvarchar](max) NULL,
	[Title] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Vate] [decimal](20, 2) NOT NULL,
	[IteamCost] [decimal](20, 2) NOT NULL,
	[BudgetRate] [nvarchar](50) NULL,
	[IsIteamVatable] [bit] NOT NULL,
	[BranchId] [bigint] NOT NULL,
 CONSTRAINT [PK_Invoices] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[ItemsToInvoice](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[InvoiceDetailsId] [bigint] NULL,
	[InvoiceMasterId] [bigint] NULL,
	[BookingDetailId] [bigint] NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_ItemsToInvoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[MeetingTypes](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[code] [nvarchar](max) NULL,
	[Title] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[BranchId] [bigint] NOT NULL,
 CONSTRAINT [PK_MeetingTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[Menus](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[Notes] [nvarchar](max) NULL,
	[DescriptionOFFood] [nvarchar](max) NULL,
	[BranchId] [bigint] NOT NULL,
 CONSTRAINT [PK_Menus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[Notes](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[BookingDetailId] [bigint] NULL,
	[Note] [nvarchar](max) NULL,
 CONSTRAINT [PK_Notes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[RecurringBookings](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[BookingDetailId] [bigint] NOT NULL,
 CONSTRAINT [PK_RecurringBookings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Roles](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RoleName] [varchar](100) NOT NULL,
 CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Room_Types](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[code] [nvarchar](max) NULL,
	[Title] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Notes] [nvarchar](max) NULL,
	[Floor] [int] NOT NULL,
	[HourlyRate] [decimal](18, 0) NOT NULL,
	[SundayRate] [decimal](18, 0) NOT NULL,
	[SaturdayRate] [decimal](18, 0) NOT NULL,
	[Maxperson] [int] NOT NULL,
	[BranchId] [bigint] NOT NULL,
	[RoomOrder] [int] NULL,
 CONSTRAINT [PK_Room_Types] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[Security](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[BookingDetailId] [bigint] NULL,
	[DateCollected] [varchar](50) NULL,
	[DateReturned] [varchar](50) NULL,
	[TimeCollected] [varchar](50) NULL,
	[TimeReturned] [varchar](50) NULL,
	[CollectedBy] [varchar](max) NULL,
	[ReturnedBy] [varchar](max) NULL,
	[SecurityNotes] [varchar](max) NULL,
 CONSTRAINT [PK_Security] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[StandardEquipment](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[code] [nvarchar](max) NULL,
	[Title] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[BranchId] [bigint] NOT NULL,
 CONSTRAINT [PK_StandardEquipment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[UserAccess](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsDelete] [bit] NOT NULL,
	[UserId] [bigint] NULL,
	[IsActive] [bit] NULL,
	[MenuId] [nvarchar](max) NULL,
 CONSTRAINT [PK_UserAccess] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[UserGroups](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[code] [nvarchar](max) NULL,
	[Title] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[BranchId] [bigint] NOT NULL,
 CONSTRAINT [PK_UserGroups] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[UserRoles](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[RoleId] [bigint] NOT NULL,
 CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Users](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsDelete] [bit] NOT NULL,
	[UserName] [nvarchar](max) NULL,
	[Password] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[Frgt_Code] [nvarchar](max) NULL,
	[IsAdmin] [bit] NULL,
	[FirstName] [nvarchar](50) NULL,
	[MiddleName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[MobileNumber] [nvarchar](20) NOT NULL,
	[TelephoneNumber] [nvarchar](20) NULL,
	[Notes] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[AddressLine1] [nvarchar](max) NULL,
	[AddressLine2] [nvarchar](max) NULL,
	[UserImage] [nvarchar](max) NULL,
	[BranchId] [bigint] NULL,
	[AdminBranchId] [varchar](max) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[VisitorBooking](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[BookingDetailId] [bigint] NOT NULL,
	[VisitorId] [bigint] NOT NULL,
 CONSTRAINT [PK_VisitorBooking] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Visitors](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [bigint] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [bigint] NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDelete] [bit] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Name] [nvarchar](max) NOT NULL,
	[SurName] [nvarchar](max) NULL,
	[Address] [nvarchar](max) NULL,
	[PostCode] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NOT NULL,
	[Telephone] [nvarchar](max) NULL,
	[Mobile] [nvarchar](max) NULL,
	[Notes] [nvarchar](max) NULL,
	[BranchId] [bigint] NOT NULL,
 CONSTRAINT [PK_Visitors] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


CREATE TABLE [dbo].[RoomImageMaster](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RoomId] [bigint] NOT NULL,
	[RoomImage] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_RoomImage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[RoomImageMaster]  WITH CHECK ADD  CONSTRAINT [FK_RoomImage_RoomType] FOREIGN KEY([RoomId])
REFERENCES [dbo].[Room_Types] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[RoomImageMaster] CHECK CONSTRAINT [FK_RoomImage_RoomType]


ALTER TABLE [dbo].[BookingDetails]  WITH CHECK ADD  CONSTRAINT [FK_BookingDetails_BranchMaster] FOREIGN KEY([BranchId])
REFERENCES [dbo].[BranchMaster] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[BookingDetails] CHECK CONSTRAINT [FK_BookingDetails_BranchMaster]

ALTER TABLE [dbo].[BookingDetails]  WITH CHECK ADD  CONSTRAINT [FK_BookingDetails_Users] FOREIGN KEY([ExternalBookingClientId])
REFERENCES [dbo].[Users] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[BookingDetails] CHECK CONSTRAINT [FK_BookingDetails_Users]

ALTER TABLE [dbo].[CallLogs]  WITH CHECK ADD  CONSTRAINT [FK_CallLogs_BookingDetails] FOREIGN KEY([BookingDetailId])
REFERENCES [dbo].[BookingDetails] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[CallLogs] CHECK CONSTRAINT [FK_CallLogs_BookingDetails]

ALTER TABLE [dbo].[Cat_Req_Menu]  WITH CHECK ADD  CONSTRAINT [FK_Cat_Req_Menu_Catering_Requirements] FOREIGN KEY([Cat_ReqId])
REFERENCES [dbo].[Catering_Requirements] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[Cat_Req_Menu] CHECK CONSTRAINT [FK_Cat_Req_Menu_Catering_Requirements]

ALTER TABLE [dbo].[CatererMenu]  WITH CHECK ADD  CONSTRAINT [FK_CatererMenu_Catering_Details] FOREIGN KEY([CatererId])
REFERENCES [dbo].[Catering_Details] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[CatererMenu] CHECK CONSTRAINT [FK_CatererMenu_Catering_Details]

ALTER TABLE [dbo].[Catering_Details]  WITH CHECK ADD  CONSTRAINT [FK_Catering_Details_BranchMaster] FOREIGN KEY([BranchId])
REFERENCES [dbo].[BranchMaster] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[Catering_Details] CHECK CONSTRAINT [FK_Catering_Details_BranchMaster]

ALTER TABLE [dbo].[Catering_Requirements]  WITH CHECK ADD  CONSTRAINT [FK_Catering_Requirements_BookingDetails] FOREIGN KEY([BookingDetailId])
REFERENCES [dbo].[BookingDetails] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[Catering_Requirements] CHECK CONSTRAINT [FK_Catering_Requirements_BookingDetails]

ALTER TABLE [dbo].[ContactDetails]  WITH CHECK ADD  CONSTRAINT [FK_ContactDetails_BookingDetails] FOREIGN KEY([BookingDetailId])
REFERENCES [dbo].[BookingDetails] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[ContactDetails] CHECK CONSTRAINT [FK_ContactDetails_BookingDetails]

ALTER TABLE [dbo].[Entry_Types]  WITH CHECK ADD  CONSTRAINT [FK_Entry_Types_BranchMaster] FOREIGN KEY([BranchId])
REFERENCES [dbo].[BranchMaster] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[Entry_Types] CHECK CONSTRAINT [FK_Entry_Types_BranchMaster]

ALTER TABLE [dbo].[EquipmentRequiredForBooking]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentRequiredForBooking_BookingDetails] FOREIGN KEY([BookingDetailId])
REFERENCES [dbo].[BookingDetails] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[EquipmentRequiredForBooking] CHECK CONSTRAINT [FK_EquipmentRequiredForBooking_BookingDetails]

ALTER TABLE [dbo].[EquipmentRequiredForBooking]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentRequiredForBooking_Equipment] FOREIGN KEY([EquipmentRequiredId])
REFERENCES [dbo].[Equipment] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[EquipmentRequiredForBooking] CHECK CONSTRAINT [FK_EquipmentRequiredForBooking_Equipment]

ALTER TABLE [dbo].[ExternalBookingRequest]  WITH CHECK ADD  CONSTRAINT [FK_ExternalBookingRequest_Booking] FOREIGN KEY([BookingId])
REFERENCES [dbo].[BookingDetails] ([Id])

ALTER TABLE [dbo].[ExternalBookingRequest] CHECK CONSTRAINT [FK_ExternalBookingRequest_Booking]

ALTER TABLE [dbo].[ExternalBookingRequest]  WITH CHECK ADD  CONSTRAINT [FK_ExternalBookingRequest_User] FOREIGN KEY([ExternalUserId])
REFERENCES [dbo].[Users] ([Id])

ALTER TABLE [dbo].[ExternalBookingRequest] CHECK CONSTRAINT [FK_ExternalBookingRequest_User]

ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD  CONSTRAINT [FK_Invoices_BranchMaster] FOREIGN KEY([BranchId])
REFERENCES [dbo].[BranchMaster] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[Invoices] CHECK CONSTRAINT [FK_Invoices_BranchMaster]

ALTER TABLE [dbo].[ItemsToInvoice]  WITH CHECK ADD  CONSTRAINT [FK_ItemsToInvoice_BookingDetails] FOREIGN KEY([BookingDetailId])
REFERENCES [dbo].[BookingDetails] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[ItemsToInvoice] CHECK CONSTRAINT [FK_ItemsToInvoice_BookingDetails]

ALTER TABLE [dbo].[ItemsToInvoice]  WITH CHECK ADD  CONSTRAINT [FK_ItemsToInvoice_InvoiceDetail] FOREIGN KEY([InvoiceDetailsId])
REFERENCES [dbo].[InvoiceDetail] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[ItemsToInvoice] CHECK CONSTRAINT [FK_ItemsToInvoice_InvoiceDetail]

ALTER TABLE [dbo].[MeetingTypes]  WITH CHECK ADD  CONSTRAINT [FK_MeetingTypes_BranchMaster] FOREIGN KEY([BranchId])
REFERENCES [dbo].[BranchMaster] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[MeetingTypes] CHECK CONSTRAINT [FK_MeetingTypes_BranchMaster]

ALTER TABLE [dbo].[Menus]  WITH CHECK ADD  CONSTRAINT [FK_Menus_BranchMaster] FOREIGN KEY([BranchId])
REFERENCES [dbo].[BranchMaster] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[Menus] CHECK CONSTRAINT [FK_Menus_BranchMaster]

ALTER TABLE [dbo].[Notes]  WITH CHECK ADD  CONSTRAINT [FK_Notes_BookingDetails] FOREIGN KEY([BookingDetailId])
REFERENCES [dbo].[BookingDetails] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[Notes] CHECK CONSTRAINT [FK_Notes_BookingDetails]

ALTER TABLE [dbo].[RecurringBookings]  WITH CHECK ADD  CONSTRAINT [FK_RecurringBookings_BookingDetails] FOREIGN KEY([BookingDetailId])
REFERENCES [dbo].[BookingDetails] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[RecurringBookings] CHECK CONSTRAINT [FK_RecurringBookings_BookingDetails]

ALTER TABLE [dbo].[Room_Types]  WITH CHECK ADD  CONSTRAINT [FK_Room_Types_BranchMaster] FOREIGN KEY([BranchId])
REFERENCES [dbo].[BranchMaster] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[Room_Types] CHECK CONSTRAINT [FK_Room_Types_BranchMaster]



ALTER TABLE [dbo].[Security]  WITH CHECK ADD  CONSTRAINT [FK_Security_BookingDetails] FOREIGN KEY([BookingDetailId])
REFERENCES [dbo].[BookingDetails] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[Security] CHECK CONSTRAINT [FK_Security_BookingDetails]

ALTER TABLE [dbo].[StandardEquipment]  WITH CHECK ADD  CONSTRAINT [FK_StandardEquipment_BranchMaster] FOREIGN KEY([BranchId])
REFERENCES [dbo].[BranchMaster] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[StandardEquipment] CHECK CONSTRAINT [FK_StandardEquipment_BranchMaster]

ALTER TABLE [dbo].[UserGroups]  WITH CHECK ADD  CONSTRAINT [FK_UserGroups_BranchMaster] FOREIGN KEY([BranchId])
REFERENCES [dbo].[BranchMaster] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[UserGroups] CHECK CONSTRAINT [FK_UserGroups_BranchMaster]

ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_Roles]

ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_Users]

ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_Branch] FOREIGN KEY([BranchId])
REFERENCES [dbo].[BranchMaster] ([Id])

ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_Branch]

ALTER TABLE [dbo].[VisitorBooking]  WITH CHECK ADD  CONSTRAINT [FK_VisitorBooking_BookingDetails] FOREIGN KEY([BookingDetailId])
REFERENCES [dbo].[BookingDetails] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[VisitorBooking] CHECK CONSTRAINT [FK_VisitorBooking_BookingDetails]

ALTER TABLE [dbo].[Visitors]  WITH CHECK ADD  CONSTRAINT [FK_Visitors_BranchMaster] FOREIGN KEY([BranchId])
REFERENCES [dbo].[BranchMaster] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[Visitors] CHECK CONSTRAINT [FK_Visitors_BranchMaster]

SET IDENTITY_INSERT [dbo].[BookingStatus] ON 

INSERT [dbo].[BookingStatus] ([Id], [Status],[ColorCode],[IsActive],[IsDelete],[CreatedDate],[ModifiedDate]) VALUES (1, N'Confirmed',N'#979abf',1,0,'2022-01-01','2022-01-01')
INSERT [dbo].[BookingStatus] ([Id], [Status],[ColorCode],[IsActive],[IsDelete],[CreatedDate],[ModifiedDate]) VALUES (2, N'Cancelled',N'#d25160',1,0,'2022-01-01','2022-01-01')
INSERT [dbo].[BookingStatus] ([Id], [Status],[ColorCode],[IsActive],[IsDelete],[CreatedDate],[ModifiedDate]) VALUES (3, N'Pending',N'#c0bb52',1,0,'2022-01-01','2022-01-01')




SET IDENTITY_INSERT [dbo].[BookingStatus] OFF

SET IDENTITY_INSERT [dbo].[Roles] ON 

INSERT [dbo].[Roles] ([Id], [RoleName]) VALUES (1, N'ClientAdmin')
INSERT [dbo].[Roles] ([Id], [RoleName]) VALUES (2, N'BranchAdmin')
INSERT [dbo].[Roles] ([Id], [RoleName]) VALUES (3, N'User')
INSERT [dbo].[Roles] ([Id], [RoleName]) VALUES (4, N'ExternalUser')

SET IDENTITY_INSERT [dbo].[Roles] OFF

create user user1 for login [{dynamicDatanase}]
