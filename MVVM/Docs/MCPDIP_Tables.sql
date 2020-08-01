if not exists(select * from sys.schemas where name='Staging') 
begin
exec ('create schema Staging');
end
go
if not exists(select * from sys.schemas where name='History') 
begin
exec ('create schema History');
end
go
if not exists(select * from sys.schemas where name='Archive') 
begin
exec ('create schema Archive');
end
go
if not exists(select * from sys.schemas where name='Error') 
begin
exec ('create schema Error');
end
go
if not exists(select * from sys.schemas where name='Response')
begin
exec ('create schema Response');
end
go

if object_id('Staging.McpdAppeal') is not null drop table Staging.McpdAppeal;
if object_id('Staging.McpdContinuityOfCare') is not null drop table Staging.McpdContinuityOfCare;
if object_id('Staging.McpdGrievanceType') is not null drop table Staging.McpdGrievanceType;
if object_id('Staging.McpdGrievance') is not null drop table Staging.McpdGrievance;
if object_id('Staging.McpdOutOfNetwork') is not null drop table Staging.McpdOutOfNetwork;
if object_id('Staging.McpdHeader') is not null drop table Staging.McpdHeader;
if object_id('Staging.PcpAssignment') is not null drop table Staging.PcpAssignment;
if object_id('Staging.PcpHeader') is not null drop table Staging.PcpHeader;

CREATE TABLE [Staging].[McpdHeader](
                [McpdHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
                [PlanParent] [varchar](30) NOT NULL,
                [SubmissionDate] [datetime2](7) NOT NULL,
                [SchemaVersion] [varchar](20) NOT NULL,
                [ReportingPeriod] [varchar](10) NULL,
                [GrievanceProcessing] [bit] NULL,
                [AppealProcessing] [bit] NULL,
                [CocProcessing] [bit] NULL,
                [OonProcessing] [bit] NULL,
                [JsonProcessing] [bit] NULL,
CONSTRAINT [PK_StagingMcpdHeaderId] PRIMARY KEY CLUSTERED 
(
                [McpdHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [Staging].[McpdGrievance](
                [McpdGrievanceId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderId] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [GrievanceId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentGrievanceId] [varchar](20) NULL,
                [GrievanceReceivedDate] [char](8) NULL,
                [GrievanceType] [varchar](2000) NOT NULL,
                [BenefitType] [varchar](100) NOT NULL,
                [ExemptIndicator] [varchar](20) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_StagingMcpdGrievanceId] PRIMARY KEY CLUSTERED 
(
                [McpdGrievanceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Staging].[McpdGrievance]  WITH CHECK ADD  CONSTRAINT [FK_StagingMcpdGrievance_McpdHeader] FOREIGN KEY([McpdHeaderId])
REFERENCES [Staging].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [Staging].[McpdGrievance] CHECK CONSTRAINT [FK_StagingMcpdGrievance_McpdHeader]
GO


CREATE TABLE [Staging].[McpdGrievanceType](
                [McpdGrievanceId] [bigint] NOT NULL,
                [GrievanceType] [varchar](100) NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [Staging].[McpdGrievanceType]  WITH CHECK ADD  CONSTRAINT [FK_StagingMcpdGrievanceType_McpdGrievance] FOREIGN KEY([McpdGrievanceId])
REFERENCES [Staging].[McpdGrievance] ([McpdGrievanceId])
GO

ALTER TABLE [Staging].[McpdGrievanceType] CHECK CONSTRAINT [FK_StagingMcpdGrievanceType_McpdGrievance]
GO

CREATE TABLE [Staging].[McpdAppeal](
                [McpdAppealId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderId] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [AppealId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentGrievanceId] [varchar](20) NULL,
                [ParentAppealId] [varchar](20) NULL,
                [AppealReceivedDate] [char](8) NOT NULL,
                [NoticeOfActionDate] [char](8) NOT NULL,
                [AppealType] [varchar](20) NOT NULL,
                [BenefitType] [varchar](250) NOT NULL,
                [AppealResolutionStatusIndicator] [varchar](250) NOT NULL,
                [AppealResolutionDate] [char](8) NULL,
                [PartiallyOverturnIndicator] [varchar](250) NOT NULL,
                [ExpeditedIndicator] [varchar](50) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_StagingMcpdAppealId] PRIMARY KEY CLUSTERED 
(
                [McpdAppealId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Staging].[McpdAppeal]  WITH CHECK ADD  CONSTRAINT [FK_StagingMcpdAppeal_McpdHeader] FOREIGN KEY([McpdHeaderId])
REFERENCES [Staging].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [Staging].[McpdAppeal] CHECK CONSTRAINT [FK_StagingMcpdAppeal_McpdHeader]
GO

CREATE TABLE [Staging].[McpdContinuityOfCare](
                [McpdContinuityOfCareId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderid] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [CocId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentCocId] [varchar](20) NULL,
                [CocReceivedDate] [char](8) NOT NULL,
                [CocType] [varchar](20) NOT NULL,
                [BenefitType] [varchar](100) NOT NULL,
                [CocDispositionIndicator] [varchar](20) NOT NULL,
                [CocExpirationDate] [char](8) NULL,
                [CocDenialReasonIndicator] [varchar](100) NULL,
                [SubmittingProviderNpi] [varchar](10) NULL,
                [CocProviderNpi] [varchar](10) NULL,
                [ProviderTaxonomy] [varchar](10) NULL,
                [MerExemptionId] [varchar](6) NULL,
                [ExemptionToEnrollmentDenialCode] [varchar](10) NULL,
                [ExemptionToEnrollmentDenialDate] [char](8) NULL,
                [MerCocDispositionIndicator] [varchar](250) NULL,
                [MerCocDispositionDate] [char](8) NULL,
                [ReasonMerCocNotMetIndicator] [varchar](250) NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_StagingMcpdContinuityOfCareId] PRIMARY KEY CLUSTERED 
(
                [McpdContinuityOfCareId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Staging].[McpdContinuityOfCare]  WITH CHECK ADD  CONSTRAINT [FK_StagingMcpdContinuityOfCare_McpdHeader] FOREIGN KEY([McpdHeaderid])
REFERENCES [Staging].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [Staging].[McpdContinuityOfCare] CHECK CONSTRAINT [FK_StagingMcpdContinuityOfCare_McpdHeader]
GO

CREATE TABLE [Staging].[McpdOutOfNetwork](
                [McpdOutOfNetworkId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderId] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [OonId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentOonId] [varchar](20) NULL,
                [OonRequestReceivedDate] [char](8) NOT NULL,
                [ReferralRequestReasonIndicator] [varchar](200) NOT NULL,
                [OonResolutionStatusIndicator] [varchar](200) NOT NULL,
                [OonRequestResolvedDate] [char](8) NULL,
                [PartialApprovalExplanation] [varchar](500) NULL,
                [SpecialistProviderNpi] [varchar](10) NOT NULL,
                [ProviderTaxonomy] [varchar](10) NOT NULL,
                [ServiceLocationAddressLine1] [varchar](50) NOT NULL,
                [ServiceLocationAddressLine2] [varchar](50) NULL,
                [ServiceLocationCity] [varchar](50) NOT NULL,
                [ServiceLocationState] [varchar](2) NOT NULL,
                [ServiceLocationZip] [varchar](5) NOT NULL,
                [ServiceLocationCountry] [varchar](3) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_StagingMcpdOutOfNetworkId] PRIMARY KEY CLUSTERED 
(
                [McpdOutOfNetworkId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Staging].[McpdOutOfNetwork]  WITH CHECK ADD  CONSTRAINT [FK_StagingMcpdOutOfNetwork_McpdHeader] FOREIGN KEY([McpdHeaderId])
REFERENCES [Staging].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [Staging].[McpdOutOfNetwork] CHECK CONSTRAINT [FK_StagingMcpdOutOfNetwork_McpdHeader]
GO

CREATE TABLE [Staging].[PcpHeader](
                [PcpHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
                [PlanParent] [varchar](30) NOT NULL,
                [SubmissionDate] [char](8) NOT NULL,
                [ReportingPeriod] [char](8) NOT NULL,
                [SubmissionType] [varchar](30) NOT NULL,
                [SubmissionVersion] [char](3) NOT NULL,
                [SchemaVersion] [varchar](10) NULL,
                [IEHPProcessing] [bit] NULL,
                [JsonProcessing] [bit] NULL,
CONSTRAINT [PK_StagingPcpHeaderId] PRIMARY KEY CLUSTERED 
(
                [PcpHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [Staging].[PcpAssignment](
                [PcpAssignmentId] [bigint] IDENTITY(1,1) NOT NULL,
                [PcpHeaderId] [bigint] NOT NULL,
                [PlanCode] [varchar](3) NOT NULL,
                [Cin] [varchar](9) NOT NULL,
                [Npi] [varchar](10) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_StagingPcpAssignmentId] PRIMARY KEY CLUSTERED 
(
                [PcpAssignmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Staging].[PcpAssignment]  WITH CHECK ADD  CONSTRAINT [FK_StagingPcpAssignment_PcpHeader] FOREIGN KEY([PcpHeaderId])
REFERENCES [Staging].[PcpHeader] ([PcpHeaderId])
GO

ALTER TABLE [Staging].[PcpAssignment] CHECK CONSTRAINT [FK_StagingPcpAssignment_PcpHeader]
GO

if object_id('History.McpdAppeal') is not null drop table History.McpdAppeal;
if object_id('History.McpdContinuityOfCare') is not null drop table History.McpdContinuityOfCare;
if object_id('History.McpdGrievanceType') is not null drop table History.McpdGrievanceType;
if object_id('History.McpdGrievance') is not null drop table History.McpdGrievance;
if object_id('History.McpdOutOfNetwork') is not null drop table History.McpdOutOfNetwork;
if object_id('History.McpdHeader') is not null drop table History.McpdHeader;
if object_id('History.PcpAssignment') is not null drop table History.PcpAssignment;
if object_id('History.PcpHeader') is not null drop table History.PcpHeader;

CREATE TABLE [History].[McpdHeader](
                [McpdHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
                [PlanParent] [varchar](30) NOT NULL,
                [SubmissionDate] [datetime2](7) NOT NULL,
                [SchemaVersion] [varchar](20) NOT NULL,
                [ReportingPeriod] [varchar](10) NULL,
                [GrievanceProcessing] [bit] NULL,
                [AppealProcessing] [bit] NULL,
                [CocProcessing] [bit] NULL,
                [OonProcessing] [bit] NULL,
                [JsonProcessing] [bit] NULL,
CONSTRAINT [PK_HistoryMcpdHeaderId] PRIMARY KEY CLUSTERED 
(
                [McpdHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [History].[McpdGrievance](
                [McpdGrievanceId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderId] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [GrievanceId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentGrievanceId] [varchar](20) NULL,
                [GrievanceReceivedDate] [char](8) NULL,
                [GrievanceType] [varchar](2000) NOT NULL,
                [BenefitType] [varchar](100) NOT NULL,
                [ExemptIndicator] [varchar](20) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_HistoryMcpdGrievanceId] PRIMARY KEY CLUSTERED 
(
                [McpdGrievanceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [History].[McpdGrievance]  WITH CHECK ADD  CONSTRAINT [FK_HistoryMcpdGrievance_McpdHeader] FOREIGN KEY([McpdHeaderId])
REFERENCES [History].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [History].[McpdGrievance] CHECK CONSTRAINT [FK_HistoryMcpdGrievance_McpdHeader]
GO

CREATE TABLE [History].[McpdGrievanceType](
                [McpdGrievanceId] [bigint] NOT NULL,
                [GrievanceType] [varchar](100) NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [History].[McpdGrievanceType]  WITH CHECK ADD  CONSTRAINT [FK_HistoryMcpdGrievanceType_McpdGrievance] FOREIGN KEY([McpdGrievanceId])
REFERENCES [History].[McpdGrievance] ([McpdGrievanceId])
GO

ALTER TABLE [History].[McpdGrievanceType] CHECK CONSTRAINT [FK_HistoryMcpdGrievanceType_McpdGrievance]
GO

CREATE TABLE [History].[McpdAppeal](
                [McpdAppealId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderId] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [AppealId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentGrievanceId] [varchar](20) NULL,
                [ParentAppealId] [varchar](20) NULL,
                [AppealReceivedDate] [char](8) NOT NULL,
                [NoticeOfActionDate] [char](8) NOT NULL,
                [AppealType] [varchar](20) NOT NULL,
                [BenefitType] [varchar](250) NOT NULL,
                [AppealResolutionStatusIndicator] [varchar](250) NOT NULL,
                [AppealResolutionDate] [char](8) NULL,
                [PartiallyOverturnIndicator] [varchar](250) NOT NULL,
                [ExpeditedIndicator] [varchar](50) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_HistoryMcpdAppealId] PRIMARY KEY CLUSTERED 
(
                [McpdAppealId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [History].[McpdAppeal]  WITH CHECK ADD  CONSTRAINT [FK_HistoryMcpdAppeal_McpdHeader] FOREIGN KEY([McpdHeaderId])
REFERENCES [History].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [History].[McpdAppeal] CHECK CONSTRAINT [FK_HistoryMcpdAppeal_McpdHeader]
GO

CREATE TABLE [History].[McpdContinuityOfCare](
                [McpdContinuityOfCareId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderid] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [CocId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentCocId] [varchar](20) NULL,
                [CocReceivedDate] [char](8) NOT NULL,
                [CocType] [varchar](20) NOT NULL,
                [BenefitType] [varchar](100) NOT NULL,
                [CocDispositionIndicator] [varchar](20) NOT NULL,
                [CocExpirationDate] [char](8) NULL,
                [CocDenialReasonIndicator] [varchar](100) NULL,
                [SubmittingProviderNpi] [varchar](10) NULL,
                [CocProviderNpi] [varchar](10) NULL,
                [ProviderTaxonomy] [varchar](10) NULL,
                [MerExemptionId] [varchar](6) NULL,
                [ExemptionToEnrollmentDenialCode] [varchar](10) NULL,
                [ExemptionToEnrollmentDenialDate] [char](8) NULL,
                [MerCocDispositionIndicator] [varchar](250) NULL,
                [MerCocDispositionDate] [char](8) NULL,
                [ReasonMerCocNotMetIndicator] [varchar](250) NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_HistoryMcpdContinuityOfCareId] PRIMARY KEY CLUSTERED 
(
                [McpdContinuityOfCareId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [History].[McpdContinuityOfCare]  WITH CHECK ADD  CONSTRAINT [FK_HistoryMcpdContinuityOfCare_McpdHeader] FOREIGN KEY([McpdHeaderid])
REFERENCES [History].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [History].[McpdContinuityOfCare] CHECK CONSTRAINT [FK_HistoryMcpdContinuityOfCare_McpdHeader]
GO

CREATE TABLE [History].[McpdOutOfNetwork](
                [McpdOutOfNetworkId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderId] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [OonId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentOonId] [varchar](20) NULL,
                [OonRequestReceivedDate] [char](8) NOT NULL,
                [ReferralRequestReasonIndicator] [varchar](200) NOT NULL,
                [OonResolutionStatusIndicator] [varchar](200) NOT NULL,
                [OonRequestResolvedDate] [char](8) NULL,
                [PartialApprovalExplanation] [varchar](500) NULL,
                [SpecialistProviderNpi] [varchar](10) NOT NULL,
                [ProviderTaxonomy] [varchar](10) NOT NULL,
                [ServiceLocationAddressLine1] [varchar](50) NOT NULL,
                [ServiceLocationAddressLine2] [varchar](50) NULL,
                [ServiceLocationCity] [varchar](50) NOT NULL,
                [ServiceLocationState] [varchar](2) NOT NULL,
                [ServiceLocationZip] [varchar](5) NOT NULL,
                [ServiceLocationCountry] [varchar](3) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_HistoryMcpdOutOfNetworkId] PRIMARY KEY CLUSTERED 
(
                [McpdOutOfNetworkId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [History].[McpdOutOfNetwork]  WITH CHECK ADD  CONSTRAINT [FK_HistoryMcpdOutOfNetwork_McpdHeader] FOREIGN KEY([McpdHeaderId])
REFERENCES [History].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [History].[McpdOutOfNetwork] CHECK CONSTRAINT [FK_HistoryMcpdOutOfNetwork_McpdHeader]
GO

CREATE TABLE [History].[PcpHeader](
                [PcpHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
                [PlanParent] [varchar](30) NOT NULL,
                [SubmissionDate] [char](8) NOT NULL,
                [ReportingPeriod] [char](8) NOT NULL,
                [SubmissionType] [varchar](30) NOT NULL,
                [SubmissionVersion] [char](3) NOT NULL,
                [SchemaVersion] [varchar](10) NULL,
                [IEHPProcessing] [bit] NULL,
                [JsonProcessing] [bit] NULL,
CONSTRAINT [PK_HistoryPcpHeaderId] PRIMARY KEY CLUSTERED 
(
                [PcpHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [History].[PcpAssignment](
                [PcpAssignmentId] [bigint] IDENTITY(1,1) NOT NULL,
                [PcpHeaderId] [bigint] NOT NULL,
                [PlanCode] [varchar](3) NOT NULL,
                [Cin] [varchar](9) NOT NULL,
                [Npi] [varchar](10) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_HistoryPcpAssignmentId] PRIMARY KEY CLUSTERED 
(
                [PcpAssignmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [History].[PcpAssignment]  WITH CHECK ADD  CONSTRAINT [FK_HistoryPcpAssignment_PcpHeader] FOREIGN KEY([PcpHeaderId])
REFERENCES [History].[PcpHeader] ([PcpHeaderId])
GO

ALTER TABLE [History].[PcpAssignment] CHECK CONSTRAINT [FK_HistoryPcpAssignment_PcpHeader]
GO

if object_id('Archive.McpdAppeal') is not null drop table Archive.McpdAppeal;
if object_id('Archive.McpdContinuityOfCare') is not null drop table Archive.McpdContinuityOfCare;
if object_id('Archive.McpdGrievanceType') is not null drop table Archive.McpdGrievanceType;
if object_id('Archive.McpdGrievance') is not null drop table Archive.McpdGrievance;
if object_id('Archive.McpdOutOfNetwork') is not null drop table Archive.McpdOutOfNetwork;
if object_id('Archive.McpdHeader') is not null drop table Archive.McpdHeader;
if object_id('Archive.PcpAssignment') is not null drop table Archive.PcpAssignment;
if object_id('Archive.PcpHeader') is not null drop table Archive.PcpHeader;

CREATE TABLE [Archive].[McpdHeader](
                [McpdHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
                [PlanParent] [varchar](30) NOT NULL,
                [SubmissionDate] [datetime2](7) NOT NULL,
                [SchemaVersion] [varchar](20) NOT NULL,
                [ReportingPeriod] [varchar](10) NULL,
                [GrievanceProcessing] [bit] NULL,
                [AppealProcessing] [bit] NULL,
                [CocProcessing] [bit] NULL,
                [OonProcessing] [bit] NULL,
                [JsonProcessing] [bit] NULL,
CONSTRAINT [PK_ArchiveMcpdHeaderId] PRIMARY KEY CLUSTERED 
(
                [McpdHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [Archive].[McpdGrievance](
                [McpdGrievanceId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderId] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [GrievanceId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentGrievanceId] [varchar](20) NULL,
                [GrievanceReceivedDate] [char](8) NULL,
                [GrievanceType] [varchar](2000) NOT NULL,
                [BenefitType] [varchar](100) NOT NULL,
                [ExemptIndicator] [varchar](20) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_ArchiveMcpdGrievanceId] PRIMARY KEY CLUSTERED 
(
                [McpdGrievanceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Archive].[McpdGrievance]  WITH CHECK ADD  CONSTRAINT [FK_ArchiveMcpdGrievance_McpdHeader] FOREIGN KEY([McpdHeaderId])
REFERENCES [Archive].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [Archive].[McpdGrievance] CHECK CONSTRAINT [FK_ArchiveMcpdGrievance_McpdHeader]
GO

CREATE TABLE [Archive].[McpdGrievanceType](
                [McpdGrievanceId] [bigint] NOT NULL,
                [GrievanceType] [varchar](100) NOT NULL
) ON [PRIMARY]
GO


ALTER TABLE [Archive].[McpdGrievanceType]  WITH CHECK ADD  CONSTRAINT [FK_ArchiveMcpdGrievanceType_McpdGrievance] FOREIGN KEY([McpdGrievanceId])
REFERENCES [Archive].[McpdGrievance] ([McpdGrievanceId])
GO

ALTER TABLE [Archive].[McpdGrievanceType] CHECK CONSTRAINT [FK_ArchiveMcpdGrievanceType_McpdGrievance]
GO

CREATE TABLE [Archive].[McpdAppeal](
                [McpdAppealId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderId] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [AppealId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentGrievanceId] [varchar](20) NULL,
                [ParentAppealId] [varchar](20) NULL,
                [AppealReceivedDate] [char](8) NOT NULL,
                [NoticeOfActionDate] [char](8) NOT NULL,
                [AppealType] [varchar](20) NOT NULL,
                [BenefitType] [varchar](250) NOT NULL,
                [AppealResolutionStatusIndicator] [varchar](250) NOT NULL,
                [AppealResolutionDate] [char](8) NULL,
                [PartiallyOverturnIndicator] [varchar](250) NOT NULL,
                [ExpeditedIndicator] [varchar](50) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_ArchiveMcpdAppealId] PRIMARY KEY CLUSTERED 
(
                [McpdAppealId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Archive].[McpdAppeal]  WITH CHECK ADD  CONSTRAINT [FK_ArchiveMcpdAppeal_McpdHeader] FOREIGN KEY([McpdHeaderId])
REFERENCES [Archive].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [Archive].[McpdAppeal] CHECK CONSTRAINT [FK_ArchiveMcpdAppeal_McpdHeader]
GO

CREATE TABLE [Archive].[McpdContinuityOfCare](
                [McpdContinuityOfCareId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderid] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [CocId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentCocId] [varchar](20) NULL,
                [CocReceivedDate] [char](8) NOT NULL,
                [CocType] [varchar](20) NOT NULL,
                [BenefitType] [varchar](100) NOT NULL,
                [CocDispositionIndicator] [varchar](20) NOT NULL,
                [CocExpirationDate] [char](8) NULL,
                [CocDenialReasonIndicator] [varchar](100) NULL,
                [SubmittingProviderNpi] [varchar](10) NULL,
                [CocProviderNpi] [varchar](10) NULL,
                [ProviderTaxonomy] [varchar](10) NULL,
                [MerExemptionId] [varchar](6) NULL,
                [ExemptionToEnrollmentDenialCode] [varchar](10) NULL,
                [ExemptionToEnrollmentDenialDate] [char](8) NULL,
                [MerCocDispositionIndicator] [varchar](250) NULL,
                [MerCocDispositionDate] [char](8) NULL,
                [ReasonMerCocNotMetIndicator] [varchar](250) NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_ArchiveMcpdContinuityOfCareId] PRIMARY KEY CLUSTERED 
(
                [McpdContinuityOfCareId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Archive].[McpdContinuityOfCare]  WITH CHECK ADD  CONSTRAINT [FK_ArchiveMcpdContinuityOfCare_McpdHeader] FOREIGN KEY([McpdHeaderid])
REFERENCES [Archive].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [Archive].[McpdContinuityOfCare] CHECK CONSTRAINT [FK_ArchiveMcpdContinuityOfCare_McpdHeader]
GO

CREATE TABLE [Archive].[McpdOutOfNetwork](
                [McpdOutOfNetworkId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderId] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [OonId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentOonId] [varchar](20) NULL,
                [OonRequestReceivedDate] [char](8) NOT NULL,
                [ReferralRequestReasonIndicator] [varchar](200) NOT NULL,
                [OonResolutionStatusIndicator] [varchar](200) NOT NULL,
                [OonRequestResolvedDate] [char](8) NULL,
                [PartialApprovalExplanation] [varchar](500) NULL,
                [SpecialistProviderNpi] [varchar](10) NOT NULL,
                [ProviderTaxonomy] [varchar](10) NOT NULL,
                [ServiceLocationAddressLine1] [varchar](50) NOT NULL,
                [ServiceLocationAddressLine2] [varchar](50) NULL,
                [ServiceLocationCity] [varchar](50) NOT NULL,
                [ServiceLocationState] [varchar](2) NOT NULL,
                [ServiceLocationZip] [varchar](5) NOT NULL,
                [ServiceLocationCountry] [varchar](3) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_ArchiveMcpdOutOfNetworkId] PRIMARY KEY CLUSTERED 
(
                [McpdOutOfNetworkId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Archive].[McpdOutOfNetwork]  WITH CHECK ADD  CONSTRAINT [FK_ArchiveMcpdOutOfNetwork_McpdHeader] FOREIGN KEY([McpdHeaderId])
REFERENCES [Archive].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [Archive].[McpdOutOfNetwork] CHECK CONSTRAINT [FK_ArchiveMcpdOutOfNetwork_McpdHeader]
GO

CREATE TABLE [Archive].[PcpHeader](
                [PcpHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
                [PlanParent] [varchar](30) NOT NULL,
                [SubmissionDate] [char](8) NOT NULL,
                [ReportingPeriod] [char](8) NOT NULL,
                [SubmissionType] [varchar](30) NOT NULL,
                [SubmissionVersion] [char](3) NOT NULL,
                [SchemaVersion] [varchar](10) NULL,
                [IEHPProcessing] [bit] NULL,
                [JsonProcessing] [bit] NULL,
CONSTRAINT [PK_ArchivePcpHeaderId] PRIMARY KEY CLUSTERED 
(
                [PcpHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [Archive].[PcpAssignment](
                [PcpAssignmentId] [bigint] IDENTITY(1,1) NOT NULL,
                [PcpHeaderId] [bigint] NOT NULL,
                [PlanCode] [varchar](3) NOT NULL,
                [Cin] [varchar](9) NOT NULL,
                [Npi] [varchar](10) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_ArchivePcpAssignmentId] PRIMARY KEY CLUSTERED 
(
                [PcpAssignmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Archive].[PcpAssignment]  WITH CHECK ADD  CONSTRAINT [FK_ArchivePcpAssignment_PcpHeader] FOREIGN KEY([PcpHeaderId])
REFERENCES [Archive].[PcpHeader] ([PcpHeaderId])
GO

ALTER TABLE [Archive].[PcpAssignment] CHECK CONSTRAINT [FK_ArchivePcpAssignment_PcpHeader]
GO

if object_id('Error.McpdAppeal') is not null drop table Error.McpdAppeal;
if object_id('Error.McpdContinuityOfCare') is not null drop table Error.McpdContinuityOfCare;
if object_id('Error.McpdGrievanceType') is not null drop table Error.McpdGrievanceType;
if object_id('Error.McpdGrievance') is not null drop table Error.McpdGrievance;
if object_id('Error.McpdOutOfNetwork') is not null drop table Error.McpdOutOfNetwork;
if object_id('Error.McpdHeader') is not null drop table Error.McpdHeader;
if object_id('Error.PcpAssignment') is not null drop table Error.PcpAssignment;
if object_id('Error.PcpHeader') is not null drop table Error.PcpHeader;

CREATE TABLE [Error].[McpdHeader](
                [McpdHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
                [PlanParent] [varchar](30) NOT NULL,
                [SubmissionDate] [datetime2](7) NOT NULL,
                [SchemaVersion] [varchar](20) NOT NULL,
                [ReportingPeriod] [varchar](10) NULL,
                [GrievanceProcessing] [bit] NULL,
                [AppealProcessing] [bit] NULL,
                [CocProcessing] [bit] NULL,
                [OonProcessing] [bit] NULL,
                [JsonProcessing] [bit] NULL,
CONSTRAINT [PK_ErrorMcpdHeaderId] PRIMARY KEY CLUSTERED 
(
                [McpdHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [Error].[McpdGrievance](
                [McpdGrievanceId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderId] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [GrievanceId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentGrievanceId] [varchar](20) NULL,
                [GrievanceReceivedDate] [char](8) NULL,
                [GrievanceType] [varchar](2000) NOT NULL,
                [BenefitType] [varchar](100) NOT NULL,
                [ExemptIndicator] [varchar](20) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_ErrorMcpdGrievanceId] PRIMARY KEY CLUSTERED 
(
                [McpdGrievanceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Error].[McpdGrievance]  WITH CHECK ADD  CONSTRAINT [FK_ErrorMcpdGrievance_McpdHeader] FOREIGN KEY([McpdHeaderId])
REFERENCES [Error].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [Error].[McpdGrievance] CHECK CONSTRAINT [FK_ErrorMcpdGrievance_McpdHeader]
GO

CREATE TABLE [Error].[McpdGrievanceType](
                [McpdGrievanceId] [bigint] NOT NULL,
                [GrievanceType] [varchar](100) NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [Error].[McpdGrievanceType]  WITH CHECK ADD  CONSTRAINT [FK_ErrorMcpdGrievanceType_McpdGrievance] FOREIGN KEY([McpdGrievanceId])
REFERENCES [Error].[McpdGrievance] ([McpdGrievanceId])
GO

ALTER TABLE [Error].[McpdGrievanceType] CHECK CONSTRAINT [FK_ErrorMcpdGrievanceType_McpdGrievance]
GO

CREATE TABLE [Error].[McpdAppeal](
                [McpdAppealId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderId] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [AppealId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentGrievanceId] [varchar](20) NULL,
                [ParentAppealId] [varchar](20) NULL,
                [AppealReceivedDate] [char](8) NOT NULL,
                [NoticeOfActionDate] [char](8) NOT NULL,
                [AppealType] [varchar](20) NOT NULL,
                [BenefitType] [varchar](250) NOT NULL,
                [AppealResolutionStatusIndicator] [varchar](250) NOT NULL,
                [AppealResolutionDate] [char](8) NULL,
                [PartiallyOverturnIndicator] [varchar](250) NOT NULL,
                [ExpeditedIndicator] [varchar](50) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_ErrorMcpdAppealId] PRIMARY KEY CLUSTERED 
(
                [McpdAppealId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Error].[McpdAppeal]  WITH CHECK ADD  CONSTRAINT [FK_ErrorMcpdAppeal_McpdHeader] FOREIGN KEY([McpdHeaderId])
REFERENCES [Error].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [Error].[McpdAppeal] CHECK CONSTRAINT [FK_ErrorMcpdAppeal_McpdHeader]
GO

CREATE TABLE [Error].[McpdContinuityOfCare](
                [McpdContinuityOfCareId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderid] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [CocId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentCocId] [varchar](20) NULL,
                [CocReceivedDate] [char](8) NOT NULL,
                [CocType] [varchar](20) NOT NULL,
                [BenefitType] [varchar](100) NOT NULL,
                [CocDispositionIndicator] [varchar](20) NOT NULL,
                [CocExpirationDate] [char](8) NULL,
                [CocDenialReasonIndicator] [varchar](100) NULL,
                [SubmittingProviderNpi] [varchar](10) NULL,
                [CocProviderNpi] [varchar](10) NULL,
                [ProviderTaxonomy] [varchar](10) NULL,
                [MerExemptionId] [varchar](6) NULL,
                [ExemptionToEnrollmentDenialCode] [varchar](10) NULL,
                [ExemptionToEnrollmentDenialDate] [char](8) NULL,
                [MerCocDispositionIndicator] [varchar](250) NULL,
                [MerCocDispositionDate] [char](8) NULL,
                [ReasonMerCocNotMetIndicator] [varchar](250) NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_ErrorMcpdContinuityOfCareId] PRIMARY KEY CLUSTERED 
(
                [McpdContinuityOfCareId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Error].[McpdContinuityOfCare]  WITH CHECK ADD  CONSTRAINT [FK_ErrorMcpdContinuityOfCare_McpdHeader] FOREIGN KEY([McpdHeaderid])
REFERENCES [Error].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [Error].[McpdContinuityOfCare] CHECK CONSTRAINT [FK_ErrorMcpdContinuityOfCare_McpdHeader]
GO

CREATE TABLE [Error].[McpdOutOfNetwork](
                [McpdOutOfNetworkId] [bigint] IDENTITY(1,1) NOT NULL,
                [McpdHeaderId] [bigint] NOT NULL,
                [PlanCode] [char](3) NOT NULL,
                [Cin] [char](9) NOT NULL,
                [OonId] [varchar](20) NOT NULL,
                [RecordType] [varchar](20) NOT NULL,
                [ParentOonId] [varchar](20) NULL,
                [OonRequestReceivedDate] [char](8) NOT NULL,
                [ReferralRequestReasonIndicator] [varchar](200) NOT NULL,
                [OonResolutionStatusIndicator] [varchar](200) NOT NULL,
                [OonRequestResolvedDate] [char](8) NULL,
                [PartialApprovalExplanation] [varchar](500) NULL,
                [SpecialistProviderNpi] [varchar](10) NOT NULL,
                [ProviderTaxonomy] [varchar](10) NOT NULL,
                [ServiceLocationAddressLine1] [varchar](50) NOT NULL,
                [ServiceLocationAddressLine2] [varchar](50) NULL,
                [ServiceLocationCity] [varchar](50) NOT NULL,
                [ServiceLocationState] [varchar](2) NOT NULL,
                [ServiceLocationZip] [varchar](5) NOT NULL,
                [ServiceLocationCountry] [varchar](3) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_ErrorMcpdOutOfNetworkId] PRIMARY KEY CLUSTERED 
(
                [McpdOutOfNetworkId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Error].[McpdOutOfNetwork]  WITH CHECK ADD  CONSTRAINT [FK_ErrorMcpdOutOfNetwork_McpdHeader] FOREIGN KEY([McpdHeaderId])
REFERENCES [Error].[McpdHeader] ([McpdHeaderId])
GO

ALTER TABLE [Error].[McpdOutOfNetwork] CHECK CONSTRAINT [FK_ErrorMcpdOutOfNetwork_McpdHeader]
GO

CREATE TABLE [Error].[PcpHeader](
                [PcpHeaderId] [bigint] IDENTITY(1,1) NOT NULL,
                [PlanParent] [varchar](30) NOT NULL,
                [SubmissionDate] [char](8) NOT NULL,
                [ReportingPeriod] [char](8) NOT NULL,
                [SubmissionType] [varchar](30) NOT NULL,
                [SubmissionVersion] [char](3) NOT NULL,
                [SchemaVersion] [varchar](10) NULL,
                [IEHPProcessing] [bit] NULL,
                [JsonProcessing] [bit] NULL,
CONSTRAINT [PK_ErrorPcpHeaderId] PRIMARY KEY CLUSTERED 
(
                [PcpHeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [Error].[PcpAssignment](
                [PcpAssignmentId] [bigint] IDENTITY(1,1) NOT NULL,
                [PcpHeaderId] [bigint] NOT NULL,
                [PlanCode] [varchar](3) NOT NULL,
                [Cin] [varchar](9) NOT NULL,
                [Npi] [varchar](10) NOT NULL,
                [TradingPartnerCode] [varchar](50) NULL,
                [ErrorMessage] [varchar](255) NULL,
                [DataSource] [varchar](50) NULL,
CONSTRAINT [PK_ErrorPcpAssignmentId] PRIMARY KEY CLUSTERED 
(
                [PcpAssignmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Error].[PcpAssignment]  WITH CHECK ADD  CONSTRAINT [FK_ErrorPcpAssignment_PcpHeader] FOREIGN KEY([PcpHeaderId])
REFERENCES [Error].[PcpHeader] ([PcpHeaderId])
GO

ALTER TABLE [Error].[PcpAssignment] CHECK CONSTRAINT [FK_ErrorPcpAssignment_PcpHeader]
GO

if object_id('Response.McpdipHeader') is not null drop table Response.McpdipHeader;

CREATE TABLE [Response].[McpdipHeader](
                [HeaderId] [int] IDENTITY(1,1) NOT NULL,
                [FileName] [varchar](200) NOT NULL,
                [FileType] [varchar](50) NOT NULL,
                [SubmitterName] [varchar](50) NOT NULL,
                [SubmissionDate] [varchar](50) NOT NULL,
                [ValidationStatus] [varchar](50) NOT NULL,
                [Levels] [varchar](50) NULL,
                [SchemaVersion] [varchar](50) NOT NULL,
                [RecordYear] [varchar](50) NOT NULL,
                [RecordMonth] [varchar](50) NOT NULL,
CONSTRAINT [PK_McpdipHeader] PRIMARY KEY CLUSTERED 
(
                [HeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

if object_id('Response.McpdipHierarchy') is not null drop table Response.McpdipHierarchy

CREATE TABLE [Response].[McpdipHierarchy](
                [HierarchyId] [bigint] IDENTITY(1,1) NOT NULL,
                [HeaderId] [bigint] NOT NULL,
                [LevelIdentifier] [varchar](50) NULL,
                [SectionIdentifier] [varchar](50) NULL,
CONSTRAINT [PK_McpdipHierarchy] PRIMARY KEY CLUSTERED 
(
                [HierarchyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

if object_id('Response.McpdipChildren') is not null drop table Response.McpdipChildren;

CREATE TABLE [Response].[McpdipChildren](
                [ChildrenId] [bigint] IDENTITY(1,1) NOT NULL,
                [HierarchyId] [bigint] NOT NULL,
                [LevelIdentifier] [varchar](50) NULL,
                [SectionIdentifier] [varchar](50) NULL,
CONSTRAINT [PK_McpdipChildren] PRIMARY KEY CLUSTERED 
(
                [ChildrenId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

if object_id('Response.McpdipDetail') is not null drop table Response.McpdipDetail;

CREATE TABLE [Response].[McpdipDetail](
                [DetailId] [bigint] IDENTITY(1,1) NOT NULL,
                [ResponseTarget] [varchar](50) NOT NULL,
                [ChildrenId] [bigint] NOT NULL,
                [ItemReference] [varchar](50) NULL,
                [Id] [varchar](50) NULL,
                [Description] [varchar](1000) NULL,
                [Severity] [varchar](50) NULL,
                [OriginalTable] [varchar](50) NULL,
                [OriginalId] [bigint] NULL,
                [OriginalHeaderId] [bigint] NULL,
                [OriginalCin] [varchar](50) NULL,
                [OriginalItemId] [varchar](50) NULL,
CONSTRAINT [PK_McpdipDetail] PRIMARY KEY CLUSTERED 
(
                [DetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

if object_id('dbo.SubmissionLog') is not null drop table dbo.SubmissionLog;

CREATE TABLE [dbo].[SubmissionLog](
                [SubmissionId] [int] IDENTITY(1,1) NOT NULL,
                [RecordYear] [varchar](50) NOT NULL,
                [RecordMonth] [varchar](50) NOT NULL,
                [FileName] [varchar](50) NOT NULL,
                [FileType] [varchar](50) NOT NULL,
                [SubmitterName] [varchar](50) NOT NULL,
                [SubmissionDate] [varchar](50) NOT NULL,
                [ValidationStatus] [varchar](50) NULL,
                [TotalGrievanceSubmitted] [int] NULL,
                [TotalGrievanceAccepted] [int] NULL,
                [TotalGrievanceRejected] [int] NULL,
                [TotalAppealSubmitted] [int] NULL,
                [TotalAppealAccepted] [int] NULL,
                [TotalAppealRejected] [int] NULL,
                [TotalCOCSubmitted] [int] NULL,
                [TotalCOCAccepted] [int] NULL,
                [TotalCOCRejected] [int] NULL,
                [TotalOONSubmitted] [int] NULL,
                [TotalOONAccepted] [int] NULL,
                [TotalOONRejected] [int] NULL,
                [TotalPCPASubmitted] [int] NULL,
                [TotalPCPAAccepted] [int] NULL,
                [TotalPCPARejected] [int] NULL,
                [ResponseHeaderId] [int] NULL,
                [CreateDate] [datetime] NOT NULL,
                [CreateBy] [varchar](50) NOT NULL,
                [UpdateDate] [datetime] NULL,
                [UpdateBy] [varchar](50) NULL,
CONSTRAINT [PK_SubmissionLog] PRIMARY KEY CLUSTERED 
(
                [SubmissionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

if object_id('dbo.ProcessLog') is not null drop table dbo.ProcessLog;

CREATE TABLE [dbo].[ProcessLog](
                [LogId] [int] IDENTITY(1,1) NOT NULL,
                [TradingPartnerCode] [varchar](50) NOT NULL,
                [RecordYear] [varchar](50) NOT NULL,
                [RecordMonth] [varchar](50) NOT NULL,
                [GrievanceTotal] [int] NULL,
                [GrievanceSubmits] [int] NULL,
                [GrievanceErrors] [int] NULL,
                [AppealTotal] [int] NULL,
                [AppealSubmits] [int] NULL,
                [AppealErrors] [int] NULL,
                [COCTotal] [int] NULL,
                [COCSubmits] [int] NULL,
                [COCErrors] [int] NULL,
                [OONTotal] [int] NULL,
                [OONSubmits] [int] NULL,
                [OONErrors] [int] NULL,
                [PCPATotal] [int] NULL,
                [PCPASubmits] [int] NULL,
                [PCPAErrors] [int] NULL,
                [RunStatus] [char](1) NOT NULL,
                [RunTime] [datetime] NOT NULL,
                [RunBy] [varchar](50) NOT NULL,
CONSTRAINT [PK_ProcessLog] PRIMARY KEY CLUSTERED 
(
                [LogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

if object_id('dbo.OperationLog') is not null drop table dbo.OperationLog;

CREATE TABLE [dbo].[OperationLog](
                [OperationLogId] [bigint] IDENTITY(1,1) NOT NULL,
                [UserId] [varchar](50) NOT NULL,
                [ModuleName] [varchar](50) NOT NULL,
                [Message] [varchar](1000) NULL,
                [OperationTime] [datetime] NOT NULL,
CONSTRAINT [PK_OperationLog] PRIMARY KEY CLUSTERED 
(
                [OperationLogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
