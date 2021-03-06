/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 6/9/2019 9:02:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[RoleGroup] [nvarchar](50) NULL,
	[OrganizationId] [uniqueidentifier] NULL,
	[Discriminator] [nvarchar](128) NULL,
 CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 6/9/2019 9:02:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 6/9/2019 9:02:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 6/9/2019 9:02:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 6/9/2019 9:02:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
	[IsEmailVerified] [bit] NULL,
	[Discriminator] [nvarchar](128) NULL,
	[FirstName] [nvarchar](150) NULL,
	[LastName] [nvarchar](150) NULL,
	[OrganizationId] [uniqueidentifier] NULL,
	[StaffId] [int] NULL,
 CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TBL_Contacts]    Script Date: 6/9/2019 9:02:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TBL_Contacts](
	[ContactId] [int] IDENTITY(1,1) NOT NULL,
	[StaffId] [nvarchar](100) NOT NULL,
	[Phone] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_TBL_Contacts] PRIMARY KEY CLUSTERED 
(
	[ContactId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TBL_Staffs]    Script Date: 6/9/2019 9:02:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TBL_Staffs](
	[StaffId] [int] IDENTITY(1,1) NOT NULL,
	[first_name] [nvarchar](100) NOT NULL,
	[last_name] [nvarchar](100) NOT NULL,
	[position] [nvarchar](50) NULL,
	[salary] [money] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime] NULL,
	[DeletedBy] [nvarchar](256) NULL,
	[DeletedDate] [datetime] NULL,
 CONSTRAINT [PK_TBL_Staffs] PRIMARY KEY CLUSTERED 
(
	[StaffId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [RoleGroup], [OrganizationId], [Discriminator]) VALUES (N'FAD284FE-043F-4B6E-83B4-76B162739A3B', N'ADMIN', N'ALL', N'3ec0cbce-7d8b-40e8-b6b7-7ab0fc48666a', N'ApplicationRole')
GO
SET IDENTITY_INSERT [dbo].[AspNetUserClaims] ON 
GO
INSERT [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (1020, N'7975c8c4-e125-4cd8-aa85-41d5ab8cc252', N'OrgId', N'3ec0cbce-7d8b-40e8-b6b7-7ab0fc48666a')
GO
INSERT [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (1021, N'7975c8c4-e125-4cd8-aa85-41d5ab8cc252', N'OrgId', N'3ec0cbce-7d8b-40e8-b6b7-7ab0fc48666a')
GO
INSERT [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (1022, N'7975c8c4-e125-4cd8-aa85-41d5ab8cc252', N'OrgId', N'3ec0cbce-7d8b-40e8-b6b7-7ab0fc48666a')
GO
SET IDENTITY_INSERT [dbo].[AspNetUserClaims] OFF
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'7975c8c4-e125-4cd8-aa85-41d5ab8cc252', N'FAD284FE-043F-4B6E-83B4-76B162739A3B')
GO
INSERT [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [IsEmailVerified], [Discriminator], [FirstName], [LastName], [OrganizationId], [StaffId]) VALUES (N'7975c8c4-e125-4cd8-aa85-41d5ab8cc252', N'david@arise.com', 1, N'AG9Dy2FdpwA0zoRkS1gqRr3I5NDltBQ/HmqN4ottDI9CQavph8WB1qviGo71y/hfWw==', N'0880ec3c-5760-427b-aed2-84496603d64e', NULL, 0, 0, NULL, 0, 0, N'david@arise.com', NULL, NULL, N'David', N'Arise', N'3ec0cbce-7d8b-40e8-b6b7-7ab0fc48666a', 3)
GO
SET IDENTITY_INSERT [dbo].[TBL_Contacts] ON 
GO
INSERT [dbo].[TBL_Contacts] ([ContactId], [StaffId], [Phone]) VALUES (1, N'1', N'111-22-33')
GO
INSERT [dbo].[TBL_Contacts] ([ContactId], [StaffId], [Phone]) VALUES (2, N'1', N'222-333-444')
GO
SET IDENTITY_INSERT [dbo].[TBL_Contacts] OFF
GO
SET IDENTITY_INSERT [dbo].[TBL_Staffs] ON 
GO
INSERT [dbo].[TBL_Staffs] ([StaffId], [first_name], [last_name], [position], [salary], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [DeletedBy], [DeletedDate]) VALUES (1, N'FirstName', N'LastName', N'Developer', 111.0000, NULL, NULL, N'7975c8c4-e125-4cd8-aa85-41d5ab8cc252', CAST(N'2019-06-09T14:53:17.120' AS DateTime), NULL, NULL)
GO
INSERT [dbo].[TBL_Staffs] ([StaffId], [first_name], [last_name], [position], [salary], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [DeletedBy], [DeletedDate]) VALUES (2, N'Prashant', N'Chalise', N'Another', 1111.0000, NULL, NULL, N'7975c8c4-e125-4cd8-aa85-41d5ab8cc252', CAST(N'2019-06-09T13:47:15.073' AS DateTime), NULL, NULL)
GO
INSERT [dbo].[TBL_Staffs] ([StaffId], [first_name], [last_name], [position], [salary], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [DeletedBy], [DeletedDate]) VALUES (3, N'David', N'David', N'test', 123.0000, N'7975c8c4-e125-4cd8-aa85-41d5ab8cc252', CAST(N'2019-06-09T13:46:42.657' AS DateTime), N'7975c8c4-e125-4cd8-aa85-41d5ab8cc252', CAST(N'2019-06-09T14:47:49.447' AS DateTime), NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[TBL_Staffs] OFF
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]
GO
/****** Object:  StoredProcedure [dbo].[csp.Staffs.Get]    Script Date: 6/9/2019 9:02:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
Created by: Prashant 
Created on: 6/9/2019 6:05:52 PM 
DESC: GET DATA FROM TABLE [dbo].[Staffs]


EXEC dbo.[csp.Staffs.Get]
@StaffId = NULL 
,@first_name = NULL  
,@UserId = NULL 
,@PageNumber = 1 
,@PageSize	= 20 
,@ShowAll	= 0 

*/

CREATE PROCEDURE [dbo].[csp.Staffs.Get]
(
	@StaffId  			INT = NULL 
	,@first_name    	NVARCHAR(100) = NULL 
	,@UserId			NVARCHAR(256) = NULL 
	,@PageNumber		INT = 1 
	,@PageSize 			INT = 20 
	,@ShowAll			INT = 0 

)
AS 
	BEGIN 
		WITH TMP_TBL AS 
		( 
			SELECT 
				(ROW_NUMBER() OVER (ORDER BY TB.StaffId)) AS RowNumber 
				,TB.[StaffId] AS [StaffId] 
				,TB.[first_name] AS [first_name]
				,TB.[last_name] AS [last_name]
				,ISNULL(TB.[position],'') AS [position]
				,ISNULL(TB.[salary],0.0) AS [salary]
				,COUNT(TB.[StaffId]) OVER () AS TotalCount 
			FROM 
				[dbo].[TBL_Staffs] TB WITH(NOLOCK) 
			WHERE 
				(ISNULL(@StaffId,0) = 0 OR TB.[StaffId] = @StaffId )
				AND (ISNULL(@first_name,'') = '' OR TB.[first_name] = @first_name )
				AND TB.DeletedDate IS NULL 
		) 
		 
		SELECT 
			RowNumber 
			,[StaffId]
			,[first_name]
			,[last_name]
			,[position]
			,[salary]
			,'[' + ISNULL( SUBSTRING(
				(
					SELECT ','
							+'{' 
							+ '"Phone": "'+ISNULL(C.Phone,'') + '"'
							+ '}' 
							AS [text()]
					FROM 
						[dbo].[TBL_Contacts] C WITH(NOLOCK)
 					WHERE 
						C.StaffId   = T.StaffId 
					For XML PATH ('')
					), 2, 10000
				),'') + ']' 
			AS ContactsJSON
			,[TotalCount]
		FROM TMP_TBL T
		WHERE 
			(@ShowAll = 1 OR (RowNumber > ((@PageNumber-1) * @PageSize))) 
			AND ((@ShowAll = 1 OR RowNumber <= ((@PageNumber-1) * @PageSize) + @PageSize))


	END 
GO
/****** Object:  StoredProcedure [dbo].[csp.Staffs.Update]    Script Date: 6/9/2019 9:02:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
Created by: Prashant 
Created on: 09/06/2019 
DESC: UPDATE DATA TO TABLE [dbo].[Staffs]


DECLARE @MsgType VARCHAR(20) = '' 
DECLARE @MsgText VARCHAR(200) = '' 
EXEC dbo.[csp.Staffs.Update]
	@ActionType = 'ADD' 
	,@StaffId = NULL 
	,@OrganizationId = NULL 
	,@first_name = NULL 
	,@last_name = NULL 
	,@position = NULL 
	,@salary = NULL 
	,@UserId = '' 
	,@MsgType	= @MsgType OUTPUT 
	,@MsgText	= @MsgText OUTPUT 

	SELECT @MsgType,@MsgText 
	SELECT * FROM [dbo].[TBL_Staffs] 


*/

CREATE PROCEDURE [dbo].[csp.Staffs.Update]
(
	@ActionType			VARCHAR(10) 
	,@StaffId  			INT = NULL 
	,@first_name    	NVARCHAR(100) = NULL 
	,@last_name			NVARCHAR(100) = NULL 
	,@position 			NVARCHAR(50) = NULL 
	,@salary   			MONEY = NULL 
	,@UserId	 		NVARCHAR(256) = NULL 
	,@MsgType			VARCHAR(10) =	NULL OUTPUT 
	,@MsgText			VARCHAR(100) =	NULL OUTPUT 
	,@ReturnStaffsId 	INT =	0 OUTPUT 
)
AS 

SET @ReturnStaffsId = @StaffId; 

-- CHECK IF THE USER'S ORGANIZATION MATCHES WITH THE ORGANIZATION PASSED INSIDE.
-- PERFORM OTHER VALIDATION RELATED WITH USER & ROLES HERE.... SHOW ERROR IF NOT MATCHES..
IF (
	@ActionType <> 'ADD'  
	AND NOT EXISTS (SELECT 1 FROM [dbo].[TBL_Staffs] WHERE StaffId = @StaffId) 
)
BEGIN 
	SET @MsgType='ERROR' 
	SET @MsgText='Record does not exist.' 
	RETURN; 

END 
DECLARE @CurrentDate DATETIME = GETUTCDATE(); 
IF (@ActionType <> 'DELETE') 
	BEGIN 
		IF (@ActionType = 'ADD') 
			BEGIN 
				INSERT INTO [dbo].[TBL_Staffs] 
				( 
					[first_name]  
					,[last_name]  
					,[position]  
					,[salary]  
					,[CreatedDate]  
					,[CreatedBy]  
				)  
				VALUES  
				(  
					@first_name  
					,@last_name  
					,@position  
					,@salary  
					,@CurrentDate  
					,@UserId 
				)  


				SET @MsgType='OK'  
				SET @MsgText='Row added successfully.'  

				SET @ReturnStaffsId = @@IDENTITY;  

			END  
		ELSE IF (@ActionType = 'UPDATE')  
			BEGIN 
				UPDATE [dbo].[TBL_Staffs] 
				SET 
					[first_name]    			 = @first_name  
					,[last_name]			 = @last_name  
					,[position] 				 = @position  
					,[salary]   				 = @salary  
					,[UpdatedDate] 	= @CurrentDate  
					,[UpdatedBy] 	= ISNULL(@UserId,[UpdatedBy])  
				WHERE StaffId = @StaffId


				SET @MsgType='OK' 
				SET @MsgText='Data edited successfully.' 

			END 
	END 
ELSE 
	BEGIN 
		UPDATE [dbo].[TBL_Staffs] 
		SET 
			[DeletedDate] 	= @CurrentDate 
			,[DeletedBy] 	= ISNULL(@UserId,[DeletedBy]) 
		WHERE StaffId = @StaffId 


		SET @MsgType='OK' 
		SET @MsgText='Data deleted successfully.' 

	END 
GO
