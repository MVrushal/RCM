ALTER procedure [dbo].[createsp](
@dbName varchar(max)
)
AS
BEGIN

DECLARE @UseAndExecStatment NVARCHAR(MAX),@SQLString NVARCHAR(MAX);

SET @SQLString='CREATE PROCEDURE [dbo].[BindDiary]
 @Input_Date varchar(max),
 @BranchId Bigint,
 @BookingStatusId int,
 @IsCan bit
AS
BEGIN
select 
ISNULL(rm.Title,'''') as Title,
ISNULL(bd.RoomTypeId,0)as RoomTypeId,
ISNULL(convert(varchar,bd.BookingDate ,111),'''')as BookingDate,
ISNULL(bd.BookingStatus,0) as BookingStatus,
ISNULL(bd.StartTime,'''')as StartTime,
ISNULL(bd.FinishTime,'''')as FinishTime,
bd.Id as BookingId,
ISNULL(u.Title,'''') as UserGroupName,
ISNULL(bd.TitleOfMeeting,'''') as MeetingTitle,
ISNULL(bd.BookingContact,'''')as Contact,
ISNULL(bs.ColorCode,'''') as ColorCode
from bookingdetails bd

join Room_Types rm on bd.RoomTypeId=rm.Id
join UserGroups u on bd.UserGroupId=u.Id
join BookingStatus bs on bd.BookingStatus = bs.Id

where (convert(date,bd.bookingDate)=convert(date,@Input_Date) and bd.branchId=@BranchId and @IsCan=1 and  BookingStatus!=2 and bd.IsDelete = 0) or (convert(date,bd.bookingDate)=convert(date,@Input_Date) and bd.branchId=@BranchId and @IsCan=0 and BookingStatus=IIF(@BookingStatusId=0,bd.BookingStatus,@BookingStatusId)) and bd.IsDelete = 0
END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[BookingStatusCheckIsEquipmentAvalable]
@BookingDetailId int 
AS
BEGIN

declare @noofItem int = 0 ;
declare @EquipmentRequiredId int = 0 

		Create table ##EquipmentRequiredListBookingWise
			(EquipmentID  int,Noofitem int , EquipmentName varchar(max) )
        create table ##tblCheckEquipmetIsavailable
				(EquipmetName  varchar(max),NoOfItemINBooking int  , NoOfItemAvalable int  )

			insert into ##EquipmentRequiredListBookingWise
			select  erb.EquipmentRequiredId , erb.Noofitem , se.Title  from EquipmentRequiredForBooking ERB 
			inner join  BookingDetails BD  on bd.id = ERB.BookingDetailId  
			inner join StandardEquipment  se on erb.EquipmentRequiredId = se.id 
			where bd.Id =  @BookingDetailId 
			
DECLARE cursor_product CURSOR
FOR SELECT  EquipmentID from ##EquipmentRequiredListBookingWise 
 
OPEN cursor_product;
 
FETCH NEXT FROM cursor_product into @EquipmentRequiredId
 
WHILE @@FETCH_STATUS = 0
BEGIN

select  @noofItem = sum(NoofItem) from EquipmentRequiredForBooking ERB inner join  BookingDetails BD  on bd.id = ERB.BookingDetailId  
where bd.BookingStatus =  1 and ERB.EquipmentRequiredId =  @EquipmentRequiredId
and BD.BookingDate = (select BookingDate from BookingDetails where id = @BookingDetailId) 
and 

((bd.StartTime between (select StartTime from BookingDetails where id = @BookingDetailId) and  (select FinishTime from BookingDetails where id = @BookingDetailId) )or 
( bd.FinishTime between (select StartTime from BookingDetails where id = @BookingDetailId) and  (select FinishTime from BookingDetails where id = @BookingDetailId)))

INSERT INTO ##tblCheckEquipmetIsavailable VALUES(
  (select  Title from StandardEquipment where Id = @EquipmentRequiredId), (select Noofitem from ##EquipmentRequiredListBookingWise where EquipmentID=  @EquipmentRequiredId) ,
  ( select  case when  Title - ISNULL(@noofItem , 0 ) < 0 then 0 else  Title - ISNULL(@noofItem , 0 ) end  from Equipment where EquipId = @EquipmentRequiredId))

 FETCH NEXT FROM cursor_product into @EquipmentRequiredId;
END;
CLOSE cursor_product;
 SELECT * FROM ##tblCheckEquipmetIsavailable
DEALLOCATE cursor_product;

 drop table ##EquipmentRequiredListBookingWise
 drop table ##tblCheckEquipmetIsavailable
End 

';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString

Set @SQLString='CREATE procedure [dbo].[CalculateBookingCostByDate]
@BookingDate VARCHAR(MAX),
@StartTime VARCHAR(MAX),
@FinishTime VARCHAR(MAX),
@RoomTypeId BIGINT

AS
BEGIN

SELECT
CASE
WHEN (SELECT DATENAME(WEEKDAY, Convert(date,@BookingDate))) = ''Saturday'' THEN (select SaturdayRate * DATEDIFF(HOUR, @StartTime , @FinishTime) from Room_Types where Id = @RoomTypeId)
WHEN (SELECT DATENAME(WEEKDAY, Convert(date,@BookingDate))) = ''Sunday'' THEN (select SundayRate * DATEDIFF(HOUR, @StartTime , @FinishTime) from Room_Types where Id = @RoomTypeId)
ELSE (select HourlyRate * DATEDIFF(HOUR, @StartTime , @FinishTime) from Room_Types where Id = @RoomTypeId)
END

End
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[CalculateInvoiceAmountAndGrossAmount]
@BookingId BIGINT
AS
BEGIN
with Invoice as(
SELECT itemToInv.Id, SUM(inv.IteamCost*itemToInv.Quantity) as TotalCost, AVG((inv.IteamCost * inv.Vate) / inv.IteamCost) as CountVat from ItemsToInvoice itemToInv
inner join Invoices inv on itemToInv.InvoiceMasterId = inv.Id

WHERE itemToInv.IsDelete=0 AND itemToInv.BookingDetailId = @BookingId group by itemToInv.Id)
select * ,(select SUM(TotalCost) from Invoice)as InvoiceAmount, (select SUM(TotalCost+CountVat) from Invoice) as GrossAmount, (select SUM(CountVat) from Invoice) as VatAmount from Invoice

End

';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[CheckBooking]
 @Input_Date varchar(max),
 @RoomId  bigint

AS
BEGIN
select rm.Title,
bd.RoomTypeId,
convert(varchar,bd.BookingDate ,111)as BookingDate,
bd.BookingStatus ,
bd.StartTime,
bd.FinishTime,
bd.Id as BookingId,
u.Title as UserGroupName,
bd.TitleOfMeeting as MeetingTitle
from bookingdetails bd

JOIN Room_Types rm on bd.RoomTypeId=rm.Id
JOIN UserGroups u on bd.UserGroupId=u.Id

WHERE format(convert(date,bd.bookingDate),''yyyy/MM/dd'')=format(convert(date,@Input_Date),''yyyy/MM/dd'') and rm.Id=@RoomId
END

';

print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[CheckIsBookingAvailableForRecurringBooking]
@EventTitle VARCHAR(MAX) = null,
@RoomTypeId BIGINT = 0,
@TimeEnd VARCHAR(MAX) = null,
@TimeStart VARCHAR(MAX) = null,
@DateTo VARCHAR(MAX) = null,
@DateFrom VARCHAR(MAX) = null

AS
BEGIN
DECLARE @Count NVARCHAR(MAX);

SET @Count = (SELECT COUNT(*) FROM BookingDetails 
	where (RoomTypeId = @RoomTypeId)  
	AND BookingStatus = 1 AND IsDelete = 0 AND (BookingDate BETWEEN @DateTo AND @DateFrom) 
	AND (StartTime BETWEEN @TimeStart AND @TimeEnd)
	AND (FinishTime BETWEEN @TimeStart AND @TimeEnd));

SELECT
CASE WHEN @Count = 0
	THEN '''' 
	ELSE ''Room is not available on this date'' 

	END
 
End
';

print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[CheckIsBookingDetailAvailable]
@BookingDetailId BIGINT = 0,
@BookingDate VARCHAR(MAX) = null,
@StartTime VARCHAR(MAX) = null,
@FinishTime VARCHAR(MAX) = null,
@RoomTypeId BIGINT = 0,
@UserGroupId BIGINT = 0,
@MeetingTypeId BIGINT = 0

AS
BEGIN

SELECT Id, ISNULL(TitleOfMeeting, '''') AS TitleOfMeeting,StartTime,FinishTime,BookingStatus FROM BookingDetails WHERE (RoomTypeId = @RoomTypeId OR MeetingTypeId = @MeetingTypeId OR UserGroupId = @UserGroupId ) AND Id != @BookingDetailId 
								    AND BookingStatus != 2 AND IsDelete = 0 AND (FORMAT(CONVERT(DATE,BookingDate),''yyyy/MM/dd'') = FORMAT(CONVERT(DATE,@BookingDate),''yyyy/MM/dd'') 
									AND (((@StartTime BETWEEN StartTime AND DATEADD(MINUTE, -1, FinishTime))) OR ((@FinishTime BETWEEN DATEADD(MINUTE, 1, StartTime) AND FinishTime) )));

End



';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[CheckIsEquipmentAvalable]
@BookingDetailId int , 
@EquipmentRequiredId int


AS
BEGIN

declare @noofItem int = 0 ;
DECLARE @Num INT;  

select  @noofItem = sum(NoofItem) from EquipmentRequiredForBooking ERB inner join  BookingDetails BD  on bd.id = ERB.BookingDetailId  
where bd.BookingStatus =  1 and ERB.EquipmentRequiredId =  @EquipmentRequiredId
and BD.BookingDate = (select BookingDate from BookingDetails where id = @BookingDetailId) 
and 

((bd.StartTime between (select StartTime from BookingDetails where id = @BookingDetailId) and  (select FinishTime from BookingDetails where id = @BookingDetailId) )or 
( bd.FinishTime between (select StartTime from BookingDetails where id = @BookingDetailId) and  (select FinishTime from BookingDetails where id = @BookingDetailId)))


select  ISNULL(Title - ISNULL(@noofItem , 0 ), 0)  from Equipment where EquipId = @EquipmentRequiredId


 
End

';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[CheckRecurringbookingAvailable]
@BookingFrom varchar(max),
@BookingTo varchar(max),
@StartTime varchar(max),
@FinishTime varchar(max),
@Day varchar(max),
@RoomTypeId bigint=3

AS
BEGIN
select Id,RoomTypeId from BookingDetails where CONVERT(date,BookingDate)>=convert(date,@BookingFrom) and convert(date,BookingDate) <=convert(date,@BookingTo) 
and  DATENAME (WEEKDAY ,convert(date,BookingDate)) in  (select part from dbo.SplitString(@Day,'',''))
 
 AND BookingStatus = 1 AND IsDelete = 0 
									AND ( 
										(
										(convert(time,@StartTime) > convert(time,StartTime) and convert(time,@StartTime)< convert(time,FinishTime))   OR
										(convert(time,@FinishTime) > convert(time,StartTime) and convert(time,@FinishTime) < convert(time,FinishTime)
										)OR
										(
										(convert(time,StartTime) > convert(time,@StartTime) and convert(time,FinishTime)< convert(time,@FinishTime))   OR
										(convert(time,StartTime) > convert(time,@FinishTime) and convert(time,FinishTime) < convert(time,@FinishTime)
										)
										)
									)
									)
END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[DashboardDetailList]
@BranchId bigint,
@startDate varchar(max),
@enddate varchar(max),
@bookingType bigint = 1
AS
BEGIN
declare @Pstartdate date = null ,

@PEnddate date = null;
set @Pstartdate = convert(date, @startdate)
set @PEnddate = convert(date, DateAdd(dd,1,@Enddate))
print @Pstartdate
print @PEnddate


--select count(Id) from BookingDetails where isdelete = 0 and convert(date  , BookingDate) between @Pstartdate and @PEnddate



Create Table ##Month(ID int ,Names varchar (max))



insert into ##MOnth(ID, Names) values
(1, ''January'') , (2, ''February'') , (3, ''March''),
(4, ''April''),(5, ''May''),(6, ''June''),(7, ''July''),(8, ''August''),(9, ''September''),(10, ''October''),(11, ''November''),(12, ''December'')

-- and BookingDate between @Pstartdate and @PEnddate
create table ##DashboardCount (BookingCount Int,CateringCount int , Visitors int,TotalEarning int);
create table ##RoomTypes (RoomName varchar(max), test varchar , jan int, feb int, march int , april int , may int , june int , july int , augest int , sep int, oct int, nov int, dece int );

IF(@bookingType = 1 )
BEGIN 


insert into ##DashboardCount values(
(select count(Id) from BookingDetails where  ExternalBookingClientId is null and  isdelete = 0 and BranchId=@BranchId and (convert(date,BookingDate) >= convert(date,@startDate) and convert(date,BookingDate) <=convert(date,@enddate))), 
(select count(*) from Catering_Details where    isdelete = 0 and BranchId=@BranchId and (convert(date,CreatedDate) >= convert(date,@startDate) and convert(date,CreatedDate) <=convert(date,@enddate))),
(select count(*) from Visitors where isdelete = 0 and (convert(date,CreatedDate) >= convert(date,@startDate) AND BranchId=@BranchId   and convert(date,CreatedDate) <=convert(date,@enddate))),
ISNULL((select SUM(Cost) from BookingDetails where ExternalBookingClientId is null and
isdelete = 0  and BranchId=@BranchId and (convert(date,BookingDate) >= convert(date,@startDate) and convert(date,BookingDate) <=convert(date,@enddate))),0)
);


with exccount as (
select mo.Names as RoomName , bod.ExternalBookingClientId  , mo.ID from ##month mo right join
BookingDetails bod on DATENAME(MONTH,BookingDate) = mo.Names where bod.IsDelete = 0 and bod.ExternalBookingClientId is null  and
bod.BranchId=@BranchId and(convert(date,BookingDate) >= convert(date,@Pstartdate) and convert(date,BookingDate) <= convert(date,@PEnddate))

)
select mo.Names As MonthName ,  count(exccount.ExternalBookingClientId) as ExternalBokingCount , mo.id  from exccount  
right join ##month mo on exccount.id = mo.id  group by  mo.id , mo.Names order by mo.id

select * From ##DashboardCount



select bs.Status as status , count(bds.BookingStatus) as StatusCount from BookingDetails bds
right join BookingStatus bs on bds.BookingStatus = bs.id where bds.ExternalBookingClientId is null and bds.IsDelete = 0 and BranchId=@BranchId and
 (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate))
 group by bs.Status

select Rmt.Title as MeetingName , COUNT(bod.MeetingTypeId) as BokingCount from MeetingTypes Rmt left join BookingDetails bod
on rmt.Id = bod.MeetingTypeId where rmt.IsDelete = 0 and bod.BranchId=@BranchId and bod.ExternalBookingClientId is null and
(convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate))
group by Rmt.Title ; 



with commontable as (

select mo.Names as RoomName , mo.id,  bod.id as asr from ##month mo right join
BookingDetails bod on DATENAME(MONTH,BookingDate) = mo.Names where bod.IsDelete = 0 and  bod.BranchId=@BranchId and bod.ExternalBookingClientId is null and
(convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate))

)
select mo.Names As MonthNames ,  count(commontable.RoomName) as BookingCount , mo.id  from commontable  
right join ##month mo on commontable.id = mo.id  group by  mo.id , mo.Names order by mo.id



DECLARE @roomid INT;
DECLARE cursor_product CURSOR
FOR SELECT id from Room_Types where IsDelete = 0
OPEN cursor_product;
FETCH NEXT FROM cursor_product into @roomid
WHILE @@FETCH_STATUS = 0
BEGIN




insert into ##RoomTypes (RoomName, jan,feb,march,april ,may , june ,july , augest,sep ,oct , nov , dece) values (
(select title from Room_Types where Id = @roomid ),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is null and bod.IsDelete = 0 and bod.RoomTypeId = @roomid and DATENAME(MONTH,bod.BookingDate) = ''January''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is null and bod.IsDelete = 0 and bod.RoomTypeId = @roomid and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''February''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is null and bod.IsDelete = 0 and bod.RoomTypeId = @roomid and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''March''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is null and bod.IsDelete = 0 and bod.RoomTypeId = @roomid and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''April''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is null and bod.IsDelete = 0 and bod.RoomTypeId = @roomid and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''May''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is null and bod.IsDelete = 0 and bod.RoomTypeId = @roomid and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''June''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is null and bod.IsDelete = 0 and bod.RoomTypeId = @roomid and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''July''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is null and bod.IsDelete = 0 and bod.RoomTypeId = @roomid and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''August''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is null and bod.IsDelete = 0 and bod.RoomTypeId = @roomid and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''September''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is null and bod.IsDelete = 0 and bod.RoomTypeId = @roomid and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''October''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is null and bod.IsDelete = 0 and bod.RoomTypeId = @roomid and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''November''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is null and bod.IsDelete = 0 and bod.RoomTypeId = @roomid and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''December'')

)

FETCH NEXT FROM cursor_product into @roomid;
END;
CLOSE cursor_product;
SELECT * FROM ##RoomTypes

DEALLOCATE cursor_product;

END

IF(@bookingType = 2 )
BEGIN
insert into ##DashboardCount values(
(select count(Id) from BookingDetails where ExternalBookingClientId is not null and  isdelete = 0 and BranchId=@BranchId and (convert(date,CreatedDate) >= convert(date,@startDate) and convert(date,CreatedDate) <=convert(date,@enddate))), 
(select count(*) from Catering_Details where isdelete = 0 and BranchId=@BranchId and (convert(date,CreatedDate) >= convert(date,@startDate) and convert(date,CreatedDate) <=convert(date,@enddate))),
(select count(*) from Visitors where isdelete = 0 and (convert(date,CreatedDate) >= convert(date,@startDate) AND BranchId=@BranchId   and convert(date,CreatedDate) <=convert(date,@enddate))),
ISNULL((select SUM(Cost) from BookingDetails where ExternalBookingClientId is not null and
isdelete = 0  and BranchId=@BranchId and (convert(date,CreatedDate) >= convert(date,@startDate) and convert(date,CreatedDate) <=convert(date,@enddate))),0)
);

with exccount as (
select mo.Names as RoomName , bod.ExternalBookingClientId  , mo.ID from ##month mo right join
BookingDetails bod on DATENAME(MONTH,BookingDate) = mo.Names where bod.IsDelete = 0 and bod.ExternalBookingClientId is not null  and
bod.BranchId=@BranchId and(convert(date,BookingDate) >= convert(date,@Pstartdate) and convert(date,BookingDate) <= convert(date,@PEnddate))

)
select mo.Names As MonthName ,  count(exccount.ExternalBookingClientId) as ExternalBokingCount , mo.id  from exccount  
right join ##month mo on exccount.id = mo.id  group by  mo.id , mo.Names order by mo.id

select * From ##DashboardCount



select bs.Status as status , count(bds.BookingStatus) as StatusCount from BookingDetails bds
right join BookingStatus bs on bds.BookingStatus = bs.id where bds.ExternalBookingClientId is not null and bds.IsDelete = 0 and BranchId=@BranchId and
 (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate))
 group by bs.Status

select Rmt.Title as MeetingName , COUNT(bod.MeetingTypeId) as BokingCount from MeetingTypes Rmt left join BookingDetails bod
on rmt.Id = bod.MeetingTypeId where rmt.IsDelete = 0 and bod.BranchId=@BranchId and bod.ExternalBookingClientId is not null and
(convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate))
group by Rmt.Title ; 



with commontable as (

select mo.Names as RoomName , mo.id,  bod.id as asr from ##month mo right join
BookingDetails bod on DATENAME(MONTH,BookingDate) = mo.Names where bod.IsDelete = 0 and  bod.BranchId=@BranchId and bod.ExternalBookingClientId is not null and
(convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate))

)
select mo.Names As MonthNames ,  count(commontable.RoomName) as BookingCount , mo.id  from commontable  
right join ##month mo on commontable.id = mo.id  group by  mo.id , mo.Names order by mo.id





DECLARE @roomidforExternal INT;
DECLARE cursor_product CURSOR
FOR SELECT id from Room_Types where IsDelete = 0
OPEN cursor_product;
FETCH NEXT FROM cursor_product into @roomidforExternal
WHILE @@FETCH_STATUS = 0
BEGIN


insert into ##RoomTypes (RoomName, jan,feb,march,april ,may , june ,july , augest,sep ,oct , nov , dece) values (
(select title from Room_Types where Id = @roomidforExternal ),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is not null and bod.IsDelete = 0 and bod.RoomTypeId = @roomidforExternal and DATENAME(MONTH,bod.BookingDate) = ''January''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is not null and bod.IsDelete = 0 and bod.RoomTypeId = @roomidforExternal and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''February''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is not null and bod.IsDelete = 0 and bod.RoomTypeId = @roomidforExternal and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''March''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is not null and bod.IsDelete = 0 and bod.RoomTypeId = @roomidforExternal and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''April''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is not null and bod.IsDelete = 0 and bod.RoomTypeId = @roomidforExternal and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''May''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is not null and bod.IsDelete = 0 and bod.RoomTypeId = @roomidforExternal and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''June''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is not null and bod.IsDelete = 0 and bod.RoomTypeId = @roomidforExternal and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''July''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is not null and bod.IsDelete = 0 and bod.RoomTypeId = @roomidforExternal and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''August''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is not null and bod.IsDelete = 0 and bod.RoomTypeId = @roomidforExternal and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''September''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is not null and bod.IsDelete = 0 and bod.RoomTypeId = @roomidforExternal and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''October''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is not null and bod.IsDelete = 0 and bod.RoomTypeId = @roomidforExternal and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''November''),
(select count(bod.RoomTypeId) as BookingCount from BookingDetails bod where bod.ExternalBookingClientId is not null and bod.IsDelete = 0 and bod.RoomTypeId = @roomidforExternal and (convert(date,BookingDate) >= convert(date,@Pstartdate) AND convert(date,BookingDate) <= convert(date,@PEnddate)) and DATENAME(MONTH,bod.BookingDate) = ''December'')

)

FETCH NEXT FROM cursor_product into @roomidforExternal;
END;
CLOSE cursor_product;
SELECT * FROM ##RoomTypes

DEALLOCATE cursor_product;


END;

drop table ##RoomTypes
drop table ##DashboardCount
drop table ##Month

END'
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql @UseAndExecStatment,
N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE deleteAccess(
@Id  Bigint 
)
AS 
BEGIN
DELETE FROM UserAccess WHERE UserId=@Id;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetBookingChartDatadetail]
@BranchId   BIGINT,
@flag bigint = 1,
@startDate varchar(max) = null ,
@enddate varchar(max) = null,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX), @QueryCount NVARCHAR(MAX); --, @Pstartdate date = null , @PEnddate date = null;

--SET @Pstartdate = convert(date, @startdate)
--SET @PEnddate = convert(date, DateAdd(dd,1,@Enddate))

IF(@flag = 1 )
BEGIN
SET @Finalquery = ''SELECT bd.Id, ISNULL(ug.Title, '''''''') as UserGroupName ,
ISNULL(bd.TitleOfMeeting, '''''''') as MeetingTitle, ISNULL(mt.Title, '''''''') as MeetingType,
ISNULL(rt.Title, '''''''') as RoomTitle, ISNULL(bd.StartTime, '''''''') as StartTime, ISNULL(bd.FinishTime, '''''''') as FinishTime,
ISNULL(bd.NumberOfAttending, 0) as NumberOfAttending, ISNULL(bd.CarSpaceRequired, 0) as CarSpaceRequired,
ISNULL(bd.HouseKeepingRequired, 0) as HouseKeepingRequired, ISNULL(bd.SecurityRequired, 0) as SecurityRequired,
ISNULL(bd.Cost, 0) as Cost, ISNULL(bd.LayoutInformation, '''''''') as LayoutInformation,
ISNULL(bd.TobeInvoiced, 0) as TobeInvoiced, ISNULL(bd.TechnicianOnSite, 0) as TechnicianOnSite,
ISNULL(bd.DisabledAccess, 0) as DisabledAccess,
ISNULL(bd.ReturnedBookingForm, 0) as ReturnedBookingForm, ISNULL(bd.Mobile, '''''''') as Mobile,
ISNULL(bd.DateFromSent,'''''''') as DateFromSent, CAST(CONVERT(date, bd.BookingDate , 103) as varchar) as BookingDateForDisplay,
ISNULL(bd.BookingStatus, 0) as BookingStatus,
ISNULL(bd.AdditionalInformation, '''''''') as AdditionalInformation, ISNULL(bd.CancellationDetail, '''''''') as CancellationDetail , ISNULL(bd.BookingContact, '''''''') as BookingContact,TotalRecords = COUNT(1) OVER()
, bs.Status as BookingStatusName
from BookingDetails bd
inner join Room_Types rt on bd.RoomTypeId = rt.Id
inner join UserGroups ug on bd.UserGroupId = ug.Id
inner join BookingStatus bs on bs.Id = bd.BookingStatus
left join MeetingTypes mt on bd.MeetingTypeId = mt.Id where bd.IsDelete = 0 
 and bd.BranchId=@BranchId and bd.ExternalBookingClientId is null
AND (bd.BookingDate >= @startdate AND bd.BookingDate <= @Enddate)''
END

IF(@flag = 2 )
BEGIN
SET @Finalquery = ''SELECT bd.Id, ISNULL(ug.Title, '''''''') as UserGroupName ,
ISNULL(bd.TitleOfMeeting, '''''''') as MeetingTitle, ISNULL(mt.Title, '''''''') as MeetingType,
ISNULL(rt.Title, '''''''') as RoomTitle, ISNULL(bd.StartTime, '''''''') as StartTime, ISNULL(bd.FinishTime, '''''''') as FinishTime,
ISNULL(bd.NumberOfAttending, 0) as NumberOfAttending, ISNULL(bd.CarSpaceRequired, 0) as CarSpaceRequired,
ISNULL(bd.HouseKeepingRequired, 0) as HouseKeepingRequired, ISNULL(bd.SecurityRequired, 0) as SecurityRequired,
ISNULL(bd.Cost, 0) as Cost, ISNULL(bd.LayoutInformation, '''''''') as LayoutInformation,
ISNULL(bd.TobeInvoiced, 0) as TobeInvoiced, ISNULL(bd.TechnicianOnSite, 0) as TechnicianOnSite,
ISNULL(bd.DisabledAccess, 0) as DisabledAccess,
ISNULL(bd.ReturnedBookingForm, 0) as ReturnedBookingForm, ISNULL(bd.Mobile, '''''''') as Mobile,
ISNULL(bd.DateFromSent, '''''''') as DateFromSent, CAST(CONVERT(date, bd.BookingDate , 103) as varchar) as BookingDateForDisplay,
ISNULL(bd.BookingStatus, 0) as BookingStatus,
ISNULL(bd.AdditionalInformation, '''''''') as AdditionalInformation, ISNULL(bd.CancellationDetail, '''''''') as CancellationDetail , ISNULL(bd.BookingContact, '''''''') as BookingContact,TotalRecords = COUNT(1) OVER()
, bs.Status as BookingStatusName
from BookingDetails bd
inner join Room_Types rt on bd.RoomTypeId = rt.Id
inner join UserGroups ug on bd.UserGroupId = ug.Id
inner join BookingStatus bs on bs.Id = bd.BookingStatus
left join MeetingTypes mt on bd.MeetingTypeId = mt.Id where bd.IsDelete = 0 AND
bd.BranchId=@BranchId and
 bd.ExternalBookingClientId is not null AND (bd.BookingDate >= @startdate AND bd.BookingDate <= @Enddate)''
END

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND (ug.Title LIKE ''''%''''+@Search+''''%'''' OR bd.TitleOfMeeting LIKE ''''%''''+@Search+''''%'''' OR mt.Title LIKE ''''%''''+@Search+''''%'''' OR rt.Title LIKE ''''%''''+@Search+''''%''''
OR bd.StartTime LIKE ''''%''''+@Search+''''%'''' OR bd.FinishTime LIKE ''''%''''+@Search+''''%'''' OR bd.NumberOfAttending LIKE ''''%''''+@Search+''''%'''' OR bd.CarSpaceRequired LIKE ''''%''''+@Search+''''%'''' OR bd.Cost LIKE ''''%''''+@Search+''''%'''' 
OR bd.Mobile LIKE ''''%''''+@Search+''''%'''' OR bd.DateFromSent LIKE ''''%''''+@Search+''''%'''' OR bd.BookingDate LIKE ''''%''''+@Search+''''%'''' OR bd.BookingStatus LIKE ''''%''''+@Search+''''%'''' OR bd.BookingContact LIKE ''''%''''+@Search+''''%''''
OR bs.Status LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId   BIGINT,@flag bigint =0 , @startDate varchar(max) = null , @enddate varchar(max) = null, @Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@flag, @startDate, @enddate, @Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END'
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[GetBookingDetailList]
 @sID  BIGINT = 0,
 @BranchId  BIGINT,
 @RoomTypeId BIGINT = 0,
 @BookingStatus BIGINT = 0,
 @StartDate nvarchar(max) =null,
 @EndDate nvarchar(max) = null,
 @UserName BIGINT = 0,
 @iDisplayStart INT,
 @iDisplayLength INT,
 @SortColumn VARCHAR(MAX),
 @SortDir VARCHAR(MAX),
 @Search VARCHAR(MAX),
@SearchRecords INT OUT
 

  AS
  BEGIN

  DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);
	BEGIN
	
	SET @Finalquery =''
	SELECT 
	
		bd.Id, ISNULL(ug.Title, '''''''') as UserGroupName ,
		ISNULL(bd.TitleOfMeeting, '''''''') as MeetingTitle, 
		ISNULL(mt.Title, '''''''') as  MeetingType,
		ISNULL(rt.Title, '''''''') as RoomTitle,
		CAST(Convert(char(5),bd.StartTime,108)as varchar) as StartTime, 
		CAST(Convert(char(5),bd.FinishTime,108)as varchar) as FinishTime,
		ISNULL(bd.NumberOfAttending, 0) as NumberOfAttending, 
		ISNULL(bd.CarSpaceRequired, 0) as CarSpaceRequired, 
		ISNULL(bd.HouseKeepingRequired, 0) as HouseKeepingRequired,
		ISNULL(bd.SecurityRequired, 0) as SecurityRequired,
		CAST(CONVERT(date,bd.CreatedDate,103) as varchar) as BookingCreatedDate,
		CAST(CONVERT(date, bd.BookingDate , 103) as varchar) as BookingDateForDisplay,
		ISNULL(bd.Cost, 0) as Cost, 
		ISNULL(bd.LayoutInformation, '''''''') as LayoutInformation,
		ISNULL(bd.TobeInvoiced, 0) as TobeInvoiced,
		ISNULL(bd.TechnicianOnSite, 0) as TechnicianOnSite,
		ISNULL(bd.DisabledAccess, 0) as DisabledAccess, 
		ISNULL(bd.ExternalBookingClientId, 0) as ExternalBookingClientId,
		ISNULL(bd.ReturnedBookingForm, 0) as ReturnedBookingForm,
		ISNULL(bd.Mobile, '''''''') as Mobile,
		ISNULL(bd.DateFromSent, '''''''') as DateFromSent,
		CAST(CONVERT(date, bd.BookingDate ,103) as varchar) as BookingDateForDisplay,
		ISNULL(bd.BookingStatus, 0) as BookingStatus,
		ISNULL(bd.AdditionalInformation, '''''''') as AdditionalInformation,
		ISNULL(bd.CancellationDetail, '''''''') as CancellationDetail,
		ISNULL(bd.BookingContact,'''''''') as BookingContact, 
		ISNULL(usr.FirstName,'''''''') as FirstName,
		ISNULL(usr.LastName,'''''''') as LastName,
		ISNULL(usr.FirstName + '''' '''' + usr.LastName,'''''''') as CreatedUserName,
		TotalRecords = COUNT(1) OVER()

		FROM BookingDetails bd 
		INNER JOIN Room_Types rt ON bd.RoomTypeId = rt.Id
		INNER JOIN UserGroups ug ON bd.UserGroupId = ug.Id
		LEFT JOIN MeetingTypes mt ON bd.MeetingTypeId = mt.Id 
		LEFT JOIN Users usr ON usr.Id = bd.CreatedBy 
		LEFT JOIN BookingStatus bs ON bs.Id = bd.BookingStatus 

		WHERE (@sID = 0 AND (bd.IsDelete = 0 AND 
		bd.Id = IIF(@sID!=0,@sID,bd.Id) AND 
		bd.BranchId=@BranchId AND
		bd.RoomTypeId = IIF(@RoomTypeId=0,rt.Id,@RoomTypeId) AND
		bd.BookingStatus = IIF(@BookingStatus=0,bd.BookingStatus,@BookingStatus) AND 
		CONVERT(date,bd.BookingDate) >= CONVERT(date,@Startdate) AND 
		CONVERT(date,bd.BookingDate) <= CONVERT(date,@EndDate) AND 
		bd.CreatedBy = IIF(@UserName=0,bd.CreatedBy,@UserName) )
		) OR
		(@sID != 0 AND (bd.IsDelete = 0 AND 
		bd.Id = IIF(@sID!=0,@sID,bd.Id) AND 
		bd.BranchId=@BranchId AND
		bd.RoomTypeId = IIF(@RoomTypeId=0,rt.Id,@RoomTypeId) AND
		bd.BookingStatus = IIF(@BookingStatus=0,bd.BookingStatus,@BookingStatus) AND 
		bd.CreatedBy = IIF(@UserName=0,bd.CreatedBy,@UserName) ) 
		)''; 
	END

		

		IF @Search != ''''
		BEGIN
		SET @Finalquery = @Finalquery +'' AND (bd.BookingContact LIKE ''''%''''+@Search+''''%'''' OR rt.Title LIKE ''''%''''+@Search+''''%'''' OR bd.Id LIKE ''''%''''+@Search+''''%'''' OR bs	.Status LIKE ''''%''''+@Search+''''%'''')'';
		END;

		IF ISNULL(@SortColumn,'''') =''''
		BEGIN
			SET @Finalquery = @Finalquery+'' ORDER BY bd.BookingDate ASC'';
		END;
		ELSE
		BEGIN
			SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');
			IF @SortDir = ''desc''
			BEGIN
			SET @Finalquery = @Finalquery+'' DESC'';
			END;
		END;

		

		SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
		SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

		PRINT @Finalquery

		EXEC sp_executesql @Finalquery, N'' 
 @sID BIGINT,		
 @RoomTypeId BIGINT,
 @BookingStatus BIGINT,
 @UserName BIGINT,
 @StartDate nvarchar(max),
 @EndDate nvarchar(max),
@BranchId BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@sID,
 @RoomTypeId ,
 @BookingStatus ,
 @UserName ,
 @StartDate ,
 @EndDate ,@BranchId,@Search, @iDisplayStart ,@iDisplayLength, @SortColumn,@SearchRecords OUTPUT;
END'
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetBookingReportTypes]
@roomTypeId BIGINT=0,
@BookingStatus bigint=0,
@StartDate VARCHAR(MAX),
@EndDate VARCHAR(MAX),
@StartTime VARCHAR(MAX),
@EndTime VARCHAR(MAX),
@UserGroupId BIGINT=0,
@BranchId   BIGINT,	
@iDisplayStart  INT,
@iDisplayLength INT,
@SortColumn     VARCHAR(MAX),
@SortDir        VARCHAR(MAX),
@Search         VARCHAR(MAX),
@SearchRecords  INT OUT
AS
BEGIN
	
DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);
	IF @EndTime=''24:00''
  BEGIN
		SET @EndTime=''23:59''
  END

  IF OBJECT_ID(''tempdb..##TotalDates'') IS NOT NULL
  BEGIN 
    TRUNCATE TABLE ##TotalDates
	END
ELSE
BEGIN
	CREATE TABLE ##TotalDates(startDate varchar(max));
	END

	DECLARE @TEMP_S_DATE varchar(max)=@StartDate,@TEMP_E_DATE varchar(max)=@EndDate;
	WHILE(Convert(date,@TEMP_S_DATE) <= Convert(date,@TEMP_E_DATE))
   BEGIN
		INSERT INTO ##TotalDates values(FORMAT (Convert(Date,@TEMP_S_DATE), ''yyyy/MM/dd''))
		SET @TEMP_S_DATE=DATEADD(DAY,1,@TEMP_S_DATE);
   END;

SET @Finalquery = ''SELECT 
	bd.Id,
	bd.Id as BookingNumber,
	Convert(varchar,bd.BookingDate,103)as StringBookingDate,
	bd.TitleOfMeeting as MeetingTitle,
	ug.Title as UserGroupName,
	rt.Title as RoomType,
	bd.StartTime,
	bd.FinishTime ,
	bs.Status as BookingStatus,
	TotalRecords =Count(1) over()
	FROM bookingdetails bd
	JOIN BookingStatus bs on bd.BookingStatus=bs.Id
	JOIN Room_Types rt on bd.RoomTypeId=rt.Id
	JOIN UserGroups ug on bd.UserGroupId=ug.Id
	WHERE
	bd.IsDelete=0 AND bd.IsActive=1
	AND bd.BranchId=@BranchId
	AND (bd.RoomTypeId=IIF(ISNULL(@RoomTypeId,0)=0,bd.RoomTypeId,@RoomTypeId))
	AND (bd.BookingStatus=IIF(ISNULL(@BookingStatus,0)=0,bd.BookingStatus,@BookingStatus))
	AND (bd.UserGroupId=IIF(ISNULL(@UserGroupId,0)=0,bd.UserGroupId,@UserGroupId))
	AND
	 (FORMAT(Convert(date,bd.BookingDate),''''yyyy/MM/dd'''') IN (select startDate from ##TotalDates))
	AND
		(
		  	Convert(time,@endTime) <= Convert(time,bd.StartTime) 
		OR
		 	Convert(time,@startTime) >= Convert(time,bd.FinishTime)
		)'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND (bd.Id  LIKE ''''%''''+@Search+''''%'''' OR Convert(varchar,bd.BookingDate,103)  LIKE ''''%''''+@Search+''''%''''  OR 
bd.TitleOfMeeting  LIKE ''''%''''+@Search+''''%'''' OR ug.Title  LIKE ''''%''''+@Search+''''%'''')'';
END;

SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

print(@Finalquery);

EXEC sp_executesql @Finalquery, N''@roomTypeId BIGINT,@BookingStatus BIGINT,@StartDate varchar(max),@EndDate varchar(max),@StartTime varchar(max),
@EndTime varchar(max),@userGroupId BIGINT,@BranchId   BIGINT,@iDisplayStart INT,@iDisplayLength INT, @SortColumn VARCHAR(1000),@SortDir VARCHAR(MAX),@Search VARCHAR(MAX), @SearchRecords INT OUTPUT'',
@roomTypeId,@BookingStatus,@StartDate,@EndDate,@StartTime,@EndTime,@userGroupId,@BranchId,@iDisplayStart, @iDisplayLength,@SortColumn,@SortDir,@Search,@SearchRecords OUTPUT;

DROP TABLE ##TotalDates;
END'
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetBookingStatusList]
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @QueryCount NVARCHAR(MAX);

SELECT Id,ISNULL(Status,'''')as Status,ISNULL(ColorCode,'''') as ColorCode,TotalRecords = COUNT(1) OVER() FROM BookingStatus
WHERE IsDelete=0 

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetbookingTableList]
@BookingDate varchar(max),
@roomTypeId bigint=0,
@clientId bigint=0,
@numberOfAttending int=0,
@bookingStatus int=0,
@userGroupId bigint=0,
@MaxPerson int=0

AS
BEGIN

Create table #RoomAvl(RoomTitle varchar(max),StartTime varchar(max),EndTime varchar(max))
INSERT INTO #RoomAvl 
SELECT r.Title,
ISNULL((SELECT TOP 1 StartTime from bookingdetails where  convert(date,bookingdate)=convert(date,@BookingDate) and roomtypeId=r.Id),'''')as StartTime,
ISNULL((SELECT TOP 1 FinishTime from bookingdetails where convert(date,bookingdate)=convert(date,@BookingDate) and roomtypeId=r.Id),'''')as FinishTime
FROM room_types r 
left join bookingdetails b on r.Id=b.RoomTypeId 
where
r.Id=iif(@roomTypeId=0,r.Id,@roomTypeId) 

AND r.Maxperson >=iif(@MaxPerson=0,r.Maxperson,@MaxPerson)

 and b.UserGroupId=iif(@userGroupId=0,b.UserGroupId,@userGroupId)
and b.BookingStatus=iif(@bookingStatus=0,b.BookingStatus,@bookingStatus)
and b.NumberOfAttending =iif(@numberOfAttending=0,b.NumberOfAttending,@numberOfAttending)
AND b.ExternalBookingClientId=iif(@clientId=0,b.ExternalBookingClientId,@clientId)

select * from #RoomAvl group by RoomTitle,StartTime,EndTime
drop table #RoomAvl

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


SET @SQLString='CREATE PROCEDURE [dbo].[GetBranchList]
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX) = null,
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT
ISNULL(Id,0)as Id,
IsActive,
ISNULL(BranchName,'''''''')as BranchName,
TotalRecords = COUNT(1) OVER() FROM
BranchMaster
WHERE IsDelete=0
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND (  BranchName LIKE ''''%''''+@Search+''''%'''')'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


SET @SQLString='CREATE PROCEDURE [dbo].[GetCallLogsList]
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT CallLogs.Id,ISNULL(Subject,'''''''')as Subject,ISNULL(Entry_Types.Title,0) as EntryTypeTitle,ISNULL(convert(varchar,DateOfentry),'''''''') as EntryDate,
ISNULL(Time,'''''''')as Time,ISNULL(Contact,'''''''') as Contact,ISNULL(Address,'''''''') as Address,
ISNULL(Comments,'''''''')as Comments,ISNULL(PostCode,'''''''') as PostCode,ISNULL(Users.FirstName + Users.LastName ,'''''''') as TakenBy,
ISNULL(EmpCode,'''''''')as EmpCode,ISNULL(convert(varchar,NextContactDate),'''''''') as NextconDate,ISNULL(ISCompleted,0) as ISCompleted
,TotalRecords = COUNT(1) OVER() FROM CallLogs inner join Entry_Types on CallLogs.EntryType = Entry_Types.Id
inner join Users on CallLogs.TakenBy = Users.Id
WHERE CallLogs.IsDelete=0
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND (Users.FirstName+''''''''+Users.LastName  LIKE ''''%''''+@Search+''''%'''' OR   Entry_Types.Title  LIKE ''''%''''+@Search+''''%'''' OR Subject LIKE ''''%''''+@Search+''''%'''' OR DateOfentry LIKE ''''%''''+@Search+''''%''''
OR Time LIKE ''''%''''+@Search+''''%'''' OR Contact LIKE ''''%''''+@Search+''''%'''' OR Address LIKE ''''%''''+@Search+''''%'''' OR Comments LIKE ''''%''''+@Search+''''%'''' OR PostCode LIKE ''''%''''+@Search+''''%'''' 
OR TakenBy LIKE ''''%''''+@Search+''''%'''' OR EmpCode LIKE ''''%''''+@Search+''''%'''' OR NextContactDate LIKE ''''%''''+@Search+''''%'''' OR ISCompleted LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


SET @SQLString='CREATE procedure [dbo].[GetCallLogsListForBookingDetail]
@BookingDetailId BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT CallLogs.Id,ISNULL(Subject,'''''''')as Subject,ISNULL(Entry_Types.Title,0) as EntryTypeTitle,ISNULL(convert(varchar,DateOfentry),'''''''') as EntryDate,
ISNULL(Time,'''''''')as Time,ISNULL(Contact,'''''''') as Contact,ISNULL(Address,'''''''') as Address,
ISNULL(Comments,'''''''')as Comments,ISNULL(PostCode,'''''''') as PostCode,ISNULL(Users.FirstName + Users.LastName ,'''''''') as TakenBy,
ISNULL((SELECT FirstName + LastName from Users where Id = TakenFor),'''''''')as TakenFor,
ISNULL(convert(varchar,NextContactDate),'''''''') as NextconDate,ISNULL(ISCompleted,0) as ISCompleted
,TotalRecords = COUNT(1) OVER() FROM CallLogs inner join Entry_Types on CallLogs.EntryType = Entry_Types.Id
inner join Users on CallLogs.TakenBy = Users.Id
WHERE CallLogs.IsDelete=0 AND CallLogs.Associated = 1 AND CallLogs.BookingDetailId = @BookingDetailId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND (Users.FirstName+''''+Users.LastName  LIKE ''''%''''+@Search+''''%'''' OR   Entry_Types.Title  LIKE ''''%''''+@Search+''''%'''' OR Subject LIKE ''''%''''+@Search+''''%'''' OR DateOfentry LIKE ''''%''''+@Search+''''%''''
OR Time LIKE ''''%''''+@Search+''''%'''' OR Contact LIKE ''''%''''+@Search+''''%'''' OR Address LIKE ''''%''''+@Search+''''%'''' OR Comments LIKE ''''%''''+@Search+''''%'''' OR PostCode LIKE ''''%''''+@Search+''''%'''' 
OR TakenBy LIKE ''''%''''+@Search+''''%'''' OR TakenFor LIKE ''''%''''+@Search+''''%'''' OR NextContactDate LIKE ''''%''''+@Search+''''%'''' OR ISCompleted LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''','''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BookingDetailId BIGINT, @Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BookingDetailId, @Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;
END

';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[GetCallLogsListForCatereringDetail]
@BranchId  BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT CallLogs.Id,ISNULL(Subject,'''''''')as Subject,ISNULL(Entry_Types.Title,0) as EntryTypeTitle,ISNULL(convert(varchar,DateOfentry),'''''''') as EntryDate,
ISNULL(Time,'''''''')as Time,ISNULL(Contact,'''''''') as Contact,ISNULL(Address,'''''''') as Address,
ISNULL(Comments,'''''''')as Comments,ISNULL(PostCode,'''''''') as PostCode,ISNULL(Users.FirstName + Users.LastName ,'''''''') as TakenBy,
ISNULL((SELECT FirstName + LastName from Users where Id = TakenFor),'''''''')as TakenFor,
ISNULL(convert(varchar,NextContactDate),'''''''') as NextconDate,ISNULL(ISCompleted,0) as ISCompleted
,TotalRecords = COUNT(1) OVER() FROM CallLogs inner join Entry_Types on CallLogs.EntryType = Entry_Types.Id
inner join Users on CallLogs.TakenBy = Users.Id
WHERE CallLogs.IsDelete=0 AND CallLogs.Associated = 2 and Entry_Types.BranchId=@BranchId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND (Users.FirstName+''''+Users.LastName  LIKE ''''%''''+@Search+''''%'''' OR   Entry_Types.Title  LIKE ''''%''''+@Search+''''%'''' OR Subject LIKE ''''%''''+@Search+''''%'''' OR DateOfentry LIKE ''''%''''+@Search+''''%''''
OR Time LIKE ''''%''''+@Search+''''%'''' OR Contact LIKE ''''%''''+@Search+''''%'''' OR Address LIKE ''''%''''+@Search+''''%'''' OR Comments LIKE ''''%''''+@Search+''''%'''' OR PostCode LIKE ''''%''''+@Search+''''%'''' 
OR TakenBy LIKE ''''%''''+@Search+''''%'''' OR TakenFor LIKE ''''%''''+@Search+''''%'''' OR NextContactDate LIKE ''''%''''+@Search+''''%'''' OR ISCompleted LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''','''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId  BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[GetCallLogsListForReminder]
@BranchId   BIGINT,
@UserName VARCHAR(MAX), 
@isDashboard bit=0,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

IF @isDashboard=1
		begin
		SET @Finalquery = ''Select * From (
SELECT CallLogs.Id,ISNULL(Subject,'''''''')as Subject,ISNULL(Entry_Types.Title,0) as EntryTypeTitle,ISNULL(convert(varchar,DateOfentry),'''''''') as EntryDate,
ISNULL(Time,'''''''')as Time,ISNULL(Contact,'''''''') as Contact,ISNULL(Address,'''''''') as Address,
ISNULL(Comments,'''''''')as Comments,ISNULL(PostCode,'''''''') as PostCode,ISNULL(Users.FirstName + Users.LastName ,'''''''') as TakenBy,
ISNULL((SELECT FirstName + LastName from Users where Id = TakenFor),'''''''')as TakenFor,
ISNULL(convert(varchar,NextContactDate),'''''''') as NextconDate,ISNULL(ISCompleted,0) as ISCompleted
,TotalRecords = COUNT(1) OVER() FROM CallLogs inner join Entry_Types on CallLogs.EntryType = Entry_Types.Id
inner join Users on CallLogs.TakenBy = Users.Id
WHERE CallLogs.IsDelete=0 AND users.branchId=@BranchId AND CallLogs.ISCompleted=0
) as Level1
Where 1=1 
'';
		end;
else
	begin
			SET @Finalquery = ''Select * From (
SELECT CallLogs.Id,ISNULL(Subject,'''''''')as Subject,ISNULL(Entry_Types.Title,0) as EntryTypeTitle,ISNULL(convert(varchar,DateOfentry),'''''''') as EntryDate,
ISNULL(Time,'''''''')as Time,ISNULL(Contact,'''''''') as Contact,ISNULL(Address,'''''''') as Address,
ISNULL(Comments,'''''''')as Comments,ISNULL(PostCode,'''') as PostCode,ISNULL(Users.FirstName + Users.LastName ,'''''''') as TakenBy,
ISNULL((SELECT FirstName + LastName from Users where Id = TakenFor),'''''''')as TakenFor,
ISNULL(convert(varchar,NextContactDate),'''') as NextconDate,ISNULL(ISCompleted,0) as ISCompleted
,TotalRecords = COUNT(1) OVER() FROM CallLogs inner join Entry_Types on CallLogs.EntryType = Entry_Types.Id
inner join Users on CallLogs.TakenBy = Users.Id
WHERE CallLogs.IsDelete=0 AND users.branchId=@BranchId
) as Level1
Where 1=1 
'';
END;


IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +''AND (Subject  LIKE ''''%''''+@Search+''''%'''' OR EntryDate  LIKE ''''%''''+@Search+''''%'''' OR Contact  LIKE ''''%''''+@Search+''''%'''' OR TakenFor  LIKE ''''%''''+@Search+''''%'''' OR TakenBy  LIKE ''''%''''+@Search+''''%'''' OR  NextconDate  LIKE ''''%''''+@Search+''''%'''')'';
END;
print @Finalquery

IF @isDashboard=1
		begin
			SET @Finalquery = @Finalquery+'' ORDER BY ISCompleted ASC'';
		end;
else
	begin
			SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');
	
	IF @SortDir = ''desc''
	BEGIN
	SET @Finalquery = @Finalquery+'' DESC'';
	END;
END;


SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId   BIGINT,@UserName VARCHAR(MAX), @Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@UserName, @Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END'
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetCateringDetailList]
@BranchId   BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT Id,ISNULL(Title,'''''''')as Title,ISNULL(Description,'''''''') as Description,ISNULL(CatererName,'''''''') as CatererName,
ISNULL(ContactName,'''''''')as ContactName,ISNULL(Telephone,'''''''') as Telephone,ISNULL(Email,'''''''') as Email,
ISNULL(FaxNumber,'''''''')as FaxNumber,ISNULL(Address,'''''''') as Address,ISNULL(PostCode,'''''''') as PostCode
,TotalRecords = COUNT(1) OVER() FROM Catering_Details
WHERE IsDelete=0 and BranchId = @BranchId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( code LIKE ''''%''''+@Search+''''%'''' OR Title LIKE ''''%''''+@Search+''''%'''' OR Description LIKE ''''%''''+@Search+''''%''''
OR CatererName LIKE ''''%''''+@Search+''''%'''' OR ContactName LIKE ''''%''''+@Search+''''%'''' OR Email LIKE ''''%''''+@Search+''''%'''' OR PostCode LIKE ''''%''''+@Search+''''%'''' OR Telephone LIKE ''''%''''+@Search+''''%'''' OR Address LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId   BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetCateringRequirementsList]
@BookingDetailId BIGINT,
@BranchId  BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

declare @TotalCateringCost decimal;

set @Finalquery = ''select car.Id  , cad.CatererName  as CatererName , car.TimeFor as TimeFor  , car.TimeCollected as  TimeCollected  , car.NumberOfPeople as NumberOfPeople , 
(
  select sum(cam.Cost * cr.NumberOfPeople) from 
  Cat_Req_Menu cm inner join Catering_Requirements cr on cm.Cat_ReqId = cr.Id
  inner join CatererMenu cam on cam.Id = cm.CatererMenuId
   where cr.BookingDetailId = @BookingDetailId and cr.IsDelete = 0) as TotalCateringCost, TotalRecords = COUNT(1) OVER( )
  from Catering_Requirements car
inner join Catering_Details cad on cad.id = car.CatererId where car.IsDelete = 0 AND car.BookingDetailId = @BookingDetailId AND cad.BranchId=@BranchId''

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( CatererName LIKE ''''%''''+@Search+''''%'''' 
OR TimeFor LIKE ''''%''''+@Search+''''%'''' OR TimeCollected LIKE ''''%''''+@Search+''''%'''' OR NumberOfPeople LIKE ''''%''''+@Search+''''%'''')'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BookingDetailId BIGINT,@BranchId  BIGINT,@Search VARCHAR(MAX),@iDisplayStart INT,@iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BookingDetailId,@BranchId, @Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetCatMenuList]
@BranchId BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''with CatMenuList as(
select cm.CatererId ,c.CatererName,TotalRecords = COUNT(1) OVER()from  CatererMenu cm
join Catering_Details c on cm.CatererId=c.Id
WHERE c.IsDelete=0 AND c.BranchId=@BranchId
group by cm.CatererId,c.CatererName
)

select * from CatMenuList 
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +''WHERE CatererName  LIKE ''''%''''+@Search+''''%'''' '';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE  [dbo].[GetCompanyName]
@Companycode  varchar(100)
AS
BEGIN
 DECLARE @Prefix varchar(MAX)=''select OrganisationName from [ClientAdmin_501].[dbo].[companies] where CompanyCode=''
 EXEC(@Prefix+@Companycode)
END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE   PROCEDURE [dbo].[GetCompanyUserList]
@BranchId  BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX) = null,
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT
Id as ID,
ISNULL(FirstName,'''''''') as FirstName,
ISNULL(LastName,'''''''') as LastName,
ISNULL(Email,'''''''')as Email,
IsAdmin,
IsActive,
TotalRecords = COUNT(1) OVER() FROM
Users
WHERE IsDelete=0 and BranchId=@BranchId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( FirstName LIKE ''''%''''+@Search+''''%'''' OR LastName LIKE ''''%''''+@Search+''''%'''' OR Email LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId  BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[GetContactDetailsList]
@BookingDetailId BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT Id, ISNULL(Name, '''''''') as Name, ISNULL(Department, '''''''') as Department, ISNULL(Mobile, '''''''') as Mobile, ISNULL(Email, '''''''') as Email, TotalRecords = COUNT(1) OVER() from ContactDetails
 WHERE IsDelete=0 AND BookingDetailId = @BookingDetailId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( Name LIKE ''''%''''+@Search+''''%'''' OR Department LIKE ''''%''''+@Search+''''%''''
OR Mobile LIKE ''''%''''+@Search+''''%'''' OR Email LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BookingDetailId BIGINT, @Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BookingDetailId, @Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetDayWiseDiaryReportPDF]
@SelectedDate varchar(max),
@RoomTypeId BIGINT = 0,
@BranchId bigint,
@BookingStatusId int,
@IsCan bit

AS
BEGIN

DECLARE  @tbl as Table (BookingDate varchar(max),Slot varchar(max),StartTime varchar(max),FinishTime varchar(max),Contact varchar(max),BookingYear varchar(max),MeetingTitle varchar(max),NumberOfpeople varchar(max),UserGroup varchar(max),BookingStatus varchar(max),RoomType varchar(max));

BEGIN
  
	INSERT INTO @tbl values(FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy''),
	ISNULL((
	  SELECT (StartTime +'' - ''+FinishTime)  + '','' AS [text()] from BookingDetails
	  WHERE (BranchId=@BranchId and BookingStatus!=2  and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and FORMAT(CONVERT(date,bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'') and @IsCan = 1) OR 
	  (BranchId=@BranchId and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId )  and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and FORMAT(CONVERT(date,bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'') and @IsCan = 0) ORDER BY StartTime  FOR XML PATH ('''')) ,'''') 
	  ,
	  ISNULL((
	  SELECT FORMAT(CAST(StartTime as datetime),''HH:mm'') + '',''  AS [text()] from BookingDetails
	  WHERE (BranchId=@BranchId and BookingStatus!=2  and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and FORMAT(CONVERT(date,bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'') and @IsCan = 1) OR 
	  (BranchId=@BranchId and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId )  and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and FORMAT(CONVERT(date,bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'') and @IsCan = 0) ORDER BY StartTime  FOR XML PATH ('''')) ,'''') 
	  ,
	   ISNULL((
	  SELECT FORMAT(CAST(FinishTime as datetime),''HH:mm'')+ '',''  AS [text()] from BookingDetails
	  WHERE (BranchId=@BranchId and BookingStatus!=2  and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and FORMAT(CONVERT(date,bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'') and @IsCan = 1) OR 
	  (BranchId=@BranchId and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId )  and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and FORMAT(CONVERT(date,bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'') and @IsCan = 0) ORDER BY StartTime  FOR XML PATH ('''')) ,'''') 
	  ,
	   ISNULL((
	  SELECT ISNULL(CONVERT(varchar,BookingContact ),''-'')+ '',''  AS [text()] from BookingDetails
	  WHERE (BranchId=@BranchId and BookingStatus!=2  and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and FORMAT(CONVERT(date,bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'') and @IsCan = 1) OR 
	  (BranchId=@BranchId and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId )  and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and FORMAT(CONVERT(date,bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'') and @IsCan = 0) ORDER BY StartTime  FOR XML PATH ('''')) ,'''') 
	  ,
	 (SELECT YEAR(CONVERT(date,@SelectedDate))),
	  ISNULL((
	  SELECT ISNULL(TitleOfMeeting,''-'') + '',%'' from BookingDetails
	  WHERE (BranchId=@BranchId and BookingStatus!=2  and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and FORMAT(CONVERT(date,bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'') and @IsCan = 1) OR 
	  (BranchId=@BranchId and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId )  and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and FORMAT(CONVERT(date,bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'') and @IsCan = 0) ORDER BY StartTime  FOR XML PATH ('''')) ,'''') 
	  ,
	   ISNULL((
	  SELECT ISNULL(CONVERT(varchar,NumberOfAttending ),''-'')+ '',''  AS [text()] from BookingDetails
	  WHERE (BranchId=@BranchId and BookingStatus!=2  and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and FORMAT(CONVERT(date,bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'') and @IsCan = 1) OR 
	  (BranchId=@BranchId and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId )  and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and FORMAT(CONVERT(date,bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'') and @IsCan = 0) ORDER BY StartTime  FOR XML PATH ('''')) ,'''') 
	  ,
	   ISNULL((
	  SELECT ISNULL(CONVERT(varchar,u.Title ),''-'')+ '',%''  AS [text()] from BookingDetails b
	  JOIN UserGroups u on b.UserGroupId=u.Id
	  WHERE (b.BranchId=@BranchId and BookingStatus !=2  and RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and   FORMAT(CONVERT(date,b.bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'') and @IsCan = 1) OR
	  (b.BranchId=@BranchId and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId )  and RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and   FORMAT(CONVERT(date,b.bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'') and @IsCan = 0) ORDER BY StartTime FOR XML PATH ('''')),'''')
	   ,
	   ISNULL((
	  SELECT ISNULL(CONVERT(varchar,s.Status ),''-'')+ '',''  AS [text()] from BookingDetails b
	  JOIN BookingStatus s on b.BookingStatus=s.Id
	  WHERE (b.BranchId=@BranchId and BookingStatus!=2  and RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and   FORMAT(CONVERT(date,b.bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'')and @IsCan = 1) OR
	  (b.BranchId=@BranchId and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId )  and RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and   FORMAT(CONVERT(date,b.bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'')and @IsCan = 0) ORDER BY StartTime FOR XML PATH ('''')),'''')
	   ,
	   ISNULL((
	  SELECT ISNULL(CONVERT(varchar,r.Title ),''-'')+ '',''  AS [text()] FROM BookingDetails b
	  JOIN Room_Types r on b.RoomTypeId=r.Id
	  WHERE (b.BranchId=@BranchId and BookingStatus!=2  and RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and   FORMAT(CONVERT(date,b.bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'')and @IsCan = 1) OR
	  (b.BranchId=@BranchId and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId )  and RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and   FORMAT(CONVERT(date,b.bookingDate),''dd-MM-yyyy'')=FORMAT(CONVERT(date,@SelectedDate),''dd-MM-yyyy'')and @IsCan = 0) ORDER BY StartTime FOR XML PATH ('''')),'''')
	);
	SET @SelectedDate=DATEADD(DAY, 1,CONVERT(date,@SelectedDate));

END
 SELECT BookingDate, Slot, StartTime, FinishTime, Contact, BookingYear, MeetingTitle, NumberOfpeople, UserGroup, BookingStatus, RoomType FROM @tbl ORDER BY BookingDate

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetEntryList]
@BranchId   BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX) = null,
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT
ISNULL(Id,0)as Id,
ISNULL(code,'''''''')as code,
ISNULL(Title,'''''''')as Title,
ISNULL([Description],'''''''')as Description,
TotalRecords = COUNT(1) OVER() FROM
Entry_Types
WHERE IsDelete=0 and BranchId=@BranchId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND (  code LIKE ''''%''''+@Search+''''%'''' OR Title LIKE ''''%''''+@Search+''''%'''' OR Description LIKE ''''%''''+@Search+''''%'''')'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId   BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetEquipList]
@BranchId BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX) = null,
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT
Equipment.Id,
ISNULL(Equipment.code,'''''''') as Code,
ISNULL(Equipment.Title,'''''''') as Title,
ISNULL(StandardEquipment.Title,'''''''') as StandardEquipmentName,
ISNULL(Equipment.Description,'''''''') as Description,

TotalRecords = COUNT(1) OVER() FROM
Equipment  join StandardEquipment   on  Equipment.EquipId=StandardEquipment.Id
WHERE Equipment.IsDelete=0 and StandardEquipment.BranchId=@BranchId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( Equipment.code LIKE ''''%''''+@Search+''''%'''' OR Equipment.Title LIKE ''''%''''+@Search+''''%''''
OR Equipment.Description LIKE ''''%''''+@Search+''''%'''' OR StandardEquipment.Title LIKE ''''%''''+@Search+''''%'''' )'';
END;
print @Finalquery
if(@SortColumn = ''title'' )
begin 
SET @Finalquery = @Finalquery+'' ORDER BY    cast(Equipment.title  as INT )'';

end
else
begin

SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');
end
IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetEquipmentList]
@BranchId BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT
Id,
ISNULL(code,'''''''') as code,
ISNULL(Title,'''''''') as Title,
ISNULL(Description,'''''''') as Description,

TotalRecords = COUNT(1) OVER() FROM
StandardEquipment
WHERE IsDelete=0 and  BranchId=@BranchId 
''

IF @Search != ''''
BEGIN
SET @Finalquery =  @Finalquery +'' AND (  code LIKE ''''%''''+@Search+''''%'''' OR Title LIKE ''''%''''+@Search+''''%'''' OR Description LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId ,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[GetEquipmentRequiredForBookingList]
@BookingDetailId BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT 
ISNULL(erb.Id, 0) as Id, 
ISNULL(erb.BookingDetailId, 0) as BookingDetailId,
ISNULL(erb.NoofItem, 0) as NoofItem,
ISNULL(seq.Title, '''''''') as EqupTitle,
TotalRecords = COUNT(1) OVER()
 from EquipmentRequiredForBooking erb
 inner join Equipment eq on erb.EquipmentRequiredId = eq.Id
 inner join StandardEquipment seq on eq.EquipId = seq.Id
 where erb.IsDelete = 0  AND erb.BookingDetailId = @BookingDetailId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( erb.NoofItem LIKE ''''%''''+@Search+''''%'''' OR seq.Title LIKE ''''%''''+@Search+''''%'''')'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BookingDetailId BIGINT, @Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BookingDetailId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[GetInvoiceDetailList]
@BookingDetailId BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT Id,ISNULL(ContactName,'''''''')as ContactName,ISNULL(InvoiceTo,'''''''')as InvoiceTo,ISNULL(Email,'''''''')as Email,ISNULL(Mobile,'''''''')as Mobile,ISNULL(InvoiceRequestDate,'''''''')as InvoiceRequestDate
,ISNULL(InvoiceAddress,'''''''')as InvoiceAddress,ISNULL(InvoicePostCode,'''''''')as InvoicePostCode
,TotalRecords = COUNT(1) OVER() FROM InvoiceDetail
WHERE IsDelete=0 AND BookingDetailId = @BookingDetailId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( ContactName LIKE ''''%''''+@Search+''''%'''' OR InvoiceTo LIKE ''''%''''+@Search+''''%'''' OR Email LIKE ''''%''''+@Search+''''%'''' OR Mobile LIKE ''''%''''+@Search+''''%'''' OR InvoiceRequestDate LIKE ''''%''''+@Search+''''%'''' OR InvoiceAddress LIKE ''''%''''+@Search+''''%'''' OR InvoicePostCode LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BookingDetailId BIGINT, @Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BookingDetailId, @Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetInvoiceList]
@BranchId    BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT


AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);



SET @Finalquery = ''SELECT Id,ISNULL(code,'''''''') as code,ISNULL(Title,'''''''')as Title,ISNULL(Description,'''''''') as Description,ISNULL(Vate,0) as Vate,
 ISNULL(IteamCost,0) as IteamCost,ISNULL(BudgetRate,0) as BudgetRate,ISNULL(IsIteamVatable, 0) as IsIteamVatable,
TotalRecords = COUNT(1) OVER() FROM Invoices
WHERE IsDelete=0 and BranchId=@BranchId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( Id LIKE ''''%''''+@Search+''''%'''' OR code LIKE ''''%''''+@Search+''''%'''' OR Title LIKE ''''%''''+@Search+''''%'''' OR Description LIKE ''''%''''+@Search+''''%''''  OR Vate LIKE ''''%''''+@Search+''''%'''' 
 or IteamCost LIKE ''''%''''+@Search+''''%''''  or BudgetRate LIKE ''''%''''+@Search+''''%''''  or IsIteamVatable LIKE ''''%''''+@Search+''''%'''')'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[GetItemsToInvoiceList]
@InvoiceDetaillId BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT


AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);



SET @Finalquery = ''SELECT itemInv.Id,ISNULL(inv.code,'''''''') as code,ISNULL(inv.Title,'''''''')as Title,ISNULL(inv.Description,'''''''') as Description,ISNULL(inv.Vate,0) as Vate,
 ISNULL(inv.IteamCost,0) as IteamCost,ISNULL(inv.BudgetRate,0) as BudgetRate,ISNULL(inv.IsIteamVatable, 0) as IsIteamVatable, ISNULL(itemInv.Quantity, 0) as Quantity,
TotalRecords = COUNT(1) OVER() FROM Invoices inv inner join ItemsToInvoice itemInv on  inv.Id = itemInv.InvoiceMasterId
WHERE itemInv.IsDelete=0 AND itemInv.InvoiceDetailsId = @InvoiceDetaillId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( Title LIKE ''''%''''+@Search+''''%'''' OR Description LIKE ''''%''''+@Search+''''%'''' OR Vate LIKE ''''%''''+@Search+''''%'''' OR  IteamCost LIKE ''''%''''+@Search+''''%'''' OR BudgetRate LIKE ''''%''''+@Search+''''%'''' 
OR  IsIteamVatable LIKE ''''%''''+@Search+''''%'''' OR Quantity LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@InvoiceDetaillId BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@InvoiceDetaillId ,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END

';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetMeetingList]
@BranchId  BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX) = null,
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT
ISNULL(Id,0)as Id,
ISNULL(code,'''''''')as code,
ISNULL(Title,'''''''')as Title,
ISNULL(Description,'''''''')as Description,
TotalRecords = COUNT(1) OVER() FROM
MeetingTypes
WHERE IsDelete=0 and  BranchId=@BranchId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND (  code LIKE ''''%''''+@Search+''''%'''' OR Title LIKE ''''%''''+@Search+''''%'''' OR Description LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId  BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetMenuList]
@BranchId    BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT Id,ISNULL(Notes,'''''''') as Notes,ISNULL(DescriptionOFFood,'''''''')as DescriptionOFFood,
TotalRecords = COUNT(1) OVER() FROM Menus
WHERE IsDelete=0  and BranchId = @BranchId'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( Id LIKE ''''%''''+@Search+''''%'''' OR Notes LIKE ''''%''''+@Search+''''%'''' OR DescriptionOFFood LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId    BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetMonthlyDiaryList]
 @Input_Date varchar(max),
 @BranchId Bigint,
 @RoomTypeId BIGINT,
 @BookingStatusId bigint,
 @IsCan bigint

AS
BEGIN

declare @firstDay date;
declare @lastDay date;
declare @rowdate date;
SET @firstDay=(select convert(datetime,@Input_Date)-DAY(convert(date,@Input_Date))+1)
SET @lastDay=(select EOMONTH(convert(datetime,@Input_Date)))
DECLARE  @tbl as Table (BookingDate varchar(max),startTime varchar(max),finishTime varchar(max));
CREATE TABLE #monthdata
(
	bookingDate  date,
	slot varchar(max)
)
while @firstDay<=@lastDay
  begin
        
		insert into #monthdata values(@firstDay,
    (SELECT (''<h5 class="monthViewClass" data-id=" "+convert(varchar(max),b.Id) +" ">''+(FORMAT(CAST(b.StartTime as datetime),''HH:mm'')) +'' to ''+(FORMAT(CAST(b.FinishTime as datetime),''HH:mm''))+''</h5>'') + '',''
     FROM BookingDetails b where (convert(date,b.BookingDate)=@firstDay and b.BranchId=@BranchId and RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus != 2 and @IsCan = 1) OR (convert(date,b.BookingDate)=@firstDay and b.BranchId=@BranchId and RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm''))
     FOR XML PATH('''')
    ))
        set @firstDay=(select DATEADD(day, 1,@firstDay))
  end

  select convert(varchar,bookingDate)as BookingDate,ISNULL(slot,'''') as Slot from #monthdata order by BookingDate
  drop table #monthdata
END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString

Set @SQLString='CREATE PROCEDURE [dbo].[GetMonthlyDiaryListPDF]
 @SelectedDate varchar(max),
@RoomTypeId BIGINT,
@BranchId bigint,
@BookingStatusId BIGINT,
@IsCan bigint
AS
BEGIN

declare @firstDay date;
declare @lastDay date;
declare @rowdate date;
SET @firstDay=(select convert(datetime,@SelectedDate)-DAY(convert(date,@SelectedDate))+1)
SET @lastDay=(select EOMONTH(convert(datetime,@SelectedDate)))

CREATE TABLE #monthdata
(
	BookingDate varchar(max),Slot varchar(max),StartTime varchar(max),FinishTime varchar(max),Contact varchar(max),BookingYear varchar(max),MeetingTitle varchar(max),NumberOfpeople varchar(max),UserGroup varchar(max),BookingStatus varchar(max),RoomType varchar(max)

)
while @firstDay<=@lastDay
  begin
        
		insert into #monthdata values(format(convert(date,@firstDay),''dd-MM-yyyy''),
	ISNULL((
	  select (FORMAT(CAST(StartTime as datetime),''HH:mm'') +'' - ''+FORMAT(CAST(FinishTime as datetime),''HH:mm''))  + '','' AS [text()] from BookingDetails
	  where (convert(date,BookingDate)=@firstDay and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus !=2 and @IsCan = 1) OR
	  (convert(date,BookingDate)=@firstDay and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''') 
	  ,
	 
	  ISNULL((
	  select FORMAT(CAST(StartTime as datetime),''HH:mm'')  + '','' AS [text()] from BookingDetails
	  where (convert(date,BookingDate)=@firstDay and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus !=2 and @IsCan = 1) OR
	  (convert(date,BookingDate)=@firstDay and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''') 
	  ,
	  ISNULL((
	  select FORMAT(CAST(FinishTime as datetime),''HH:mm'')  + '','' AS [text()] from BookingDetails
	  where (convert(date,BookingDate)=@firstDay and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus !=2 and @IsCan = 1) OR
	  (convert(date,BookingDate)=@firstDay and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''') 
	  ,
	   ISNULL((
	  select ISNULL(convert(varchar,BookingContact ),''-'')+ '',''  AS [text()] from BookingDetails
	  where (convert(date,BookingDate)=@firstDay and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus !=2 and @IsCan = 1) OR
	  (convert(date,BookingDate)=@firstDay and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''') 
	  ,
	 (select YEAR(convert(date,@SelectedDate))),
	  ISNULL((
	  select ISNULL(TitleOfMeeting,''-'') + '',%''  from BookingDetails
	  where (convert(date,BookingDate)=@firstDay and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus !=2 and @IsCan = 1) OR
	  (convert(date,BookingDate)=@firstDay and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''') 
	  ,
	   ISNULL((
	  select convert(varchar,NumberOfAttending )+ '',''  AS [text()] from BookingDetails
	  where (convert(date,BookingDate)=@firstDay and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus !=2 and @IsCan = 1) OR
	  (convert(date,BookingDate)=@firstDay and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''') 
	  ,
	   ISNULL((
	  select ISNULL(convert(varchar,u.Title ),''-'')+ '',%''  AS [text()] from BookingDetails b
	  join UserGroups u on b.UserGroupId=u.Id
	  where (convert(date,BookingDate)=@firstDay and b.BranchId=@BranchId and RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus !=2 and @IsCan = 1) OR
	  ( convert(date,BookingDate)=@firstDay and b.BranchId=@BranchId and RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and  @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm''))  FOR XML PATH('''')),'''')
	   ,
	    ISNULL((
	  select ISNULL(convert(varchar,s.Status ),''-'')+ '',''  AS [text()] from BookingDetails b
	  join BookingStatus s on b.BookingStatus=s.Id
	  where (convert(date,BookingDate)=@firstDay and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus !=2 and @IsCan = 1) OR
	  (convert(date,BookingDate)=@firstDay and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''') 
	  ,
	   ISNULL((
	  select  ISNULL(convert(varchar,r.Title ),''-'')+ '',''  AS [text()] from BookingDetails b
	  join Room_Types r on b.RoomTypeId=r.Id
	  where (convert(date,BookingDate)=@firstDay and b.BranchId=@BranchId and RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus !=2 and @IsCan = 1) OR
	  (convert(date,BookingDate)=@firstDay and b.BranchId=@BranchId and RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH('''')),'''')

	);
        set @firstDay=(select DATEADD(day, 1,@firstDay))
  end

  select BookingDate ,ISNULL(Slot,'''') as Slot, StartTime, FinishTime, Contact, BookingYear, MeetingTitle, NumberOfpeople, UserGroup, BookingStatus, RoomType from #monthdata order by BookingDate
  drop table #monthdata
END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[GetNotesList]
@BookingDetailId BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT Id, ISNULL(Note, '''''''') as Note, TotalRecords = COUNT(1) OVER() from Notes  
 WHERE IsDelete=0 AND BookingDetailId = @BookingDetailId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( Note LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BookingDetailId BIGINT, @Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BookingDetailId, @Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[GetRecurringBookingDetailList]
@BranchId    BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''
SELECT bd.Id, ISNULL(bd.TitleOfMeeting, '''''''') as MeetingTitle, ISNULL(rt.Title, '''''''') as RoomTitle, ISNULL(bd.StartTime, '''''''') as StartTime, ISNULL(bd.FinishTime, '''''''') as FinishTime,
 CAST(CONVERT(date, bd.BookingDate , 103) as varchar) as BookingDateForDisplay, ISNULL(bd.ExternalBookingClientId, 0) as ExternalBookingClientId, ISNULL(us.FirstName,'''''''') as FirstName, ISNULL(us.LastName,'''''''') as LastName, TotalRecords = COUNT(1) OVER()

 from BookingDetails bd 
 inner join RecurringBookings rb on rb.BookingDetailId = bd.Id
 left join Users us on us.Id = bd.ExternalBookingClientId
 inner join Room_Types rt on bd.RoomTypeId = rt.Id where bd.IsDelete = 0 and bd.BranchId=@BranchId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( bd.Id LIKE ''''%''''+@Search+''''%'''' OR bd.TitleOfMeeting  LIKE ''''%''''+@Search+''''%'''' OR bd.StartTime LIKE ''''%''''+@Search+''''%'''' OR bd.FinishTime LIKE ''''%''''+@Search+''''%'''' 
OR BookingDate LIKE ''''%''''+@Search+''''%'''' OR rt.Title LIKE ''''%''''+@Search+''''%'''' OR CAST(CONVERT(date, BookingDate , 103) as varchar) LIKE ''''%''''+@Search+''''%''''
OR us.FirstName LIKE ''''%''''+@Search+''''%'''' OR us.LastName LIKE ''''%''''+@Search+''''%'''')'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId    BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString

Set @SQLString='CREATE PROCEDURE [dbo].[GetReportExcel]
@roomTypeId BIGINT=0,
@BookingStatusId INT=0,
@UserGroupId BIGINT=0,
@StartDate VARCHAR(MAX)='''',
@EndDate VARCHAR(MAX)='''',
@StartTime VARCHAR(MAX)='''',
@EndTime VARCHAR(MAX)='''',
@BranchId   BIGINT
AS
BEGIN
	
	IF @EndTime=''24:00''
  BEGIN
		SET @EndTime=''23:59''
  END

  IF OBJECT_ID(''tempdb..##TotalDates'') IS NOT NULL
  BEGIN 
    Truncate TABLE ##TotalDates
	END
ELSE
BEGIN
	CREATE TABLE ##TotalDates(startDate varchar(max));
	END

	DECLARE @TEMP_S_DATE varchar(max)=ISNULL(@StartDate,''2021-02-22''),@TEMP_E_DATE varchar(max)=ISNULL(@EndDate,''2021-02-28'');
	WHILE(Convert(date,@TEMP_S_DATE) <= Convert(date,@TEMP_E_DATE))
   BEGIN
		INSERT INTO ##TotalDates values(FORMAT (Convert(Date,@TEMP_S_DATE), ''yyyy/MM/dd''))
		SET @TEMP_S_DATE=DATEADD(DAY,1,@TEMP_S_DATE);
   END;


   

	SELECT 
	bd.Id as BookingNumber,
	bd.BookingDate,
	bd.TitleOfMeeting as MeetingTitle,
	ug.Title as UserGroupName,
	rt.Title as RoomType,
	bd.StartTime,
	bd.FinishTime 
	FROM bookingdetails bd
	JOIN BookingStatus bs on bd.BookingStatus=bs.Id
	JOIN Room_Types rt on bd.RoomTypeId=rt.Id
	JOIN UserGroups ug on bd.UserGroupId=ug.Id
	WHERE
	bd.IsDelete=0 AND bd.IsActive=1
	AND bd.BranchId=@BranchId
	AND (bd.RoomTypeId=IIF(ISNULL(@RoomTypeId,0)=0,bd.RoomTypeId,@RoomTypeId))
	AND (bd.BookingStatus=IIF(ISNULL(@BookingStatusId,0)=0,bd.BookingStatus,@BookingStatusId))
	AND (bd.UserGroupId=IIF(ISNULL(@UserGroupId,0)=0,bd.UserGroupId,@UserGroupId))
	AND
	 (FORMAT(Convert(date,bd.BookingDate),''yyyy/MM/dd'') IN (select startDate from ##TotalDates))
	 AND
		(
			Convert(time,@endTime) >= Convert(time,bd.StartTime) 
		AND
			Convert(time,@startTime) <= Convert(time,bd.FinishTime)
		
		)
	DROP TABLE ##TotalDates	
END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetRequestList]
@BranchId   BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX) = null,
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''
Select br.Id  as RequestId,br.BookingStatus, u.Email, r.Title as RoomType,(br.startTime+'''' to ''''+br.finishTime) as Slot ,
TotalRecords = COUNT(1) OVER() 
from bookingdetails br

join Users u on br.ExternalBookingClientId=u.Id
join UserRoles ur on u.Id=ur.UserId
JOIN Room_types r on br.RoomTypeId=r.Id
where ur.RoleId=4 and br.BookingStatus=3  and
br.BranchId=@BranchId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND (u.Email LIKE ''''%''''+@Search+''''%'''')'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId   BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[GetRoomAvailiblity]
@roomTypeId bigint=0,
@startDate varchar(max)='''',
@endDate varchar(max)='''',
@maxPerson int=0,
@userGroup bigint=0,
@BookingStatus int=0,
@NumberOfAttending int=0,
@ClientId bigint=0,
@SUNDAY bit=0,
@MONDAY bit=0,
@TUESDAY bit=0,
@WEDNESDAY bit=0,
@THURSDAY bit=0,
@FRIDAY bit=0,
@SATURDAY bit=0,
@startTime varchar(max)= NULL,
@endTime varchar(max)=NULL,
@BranchId BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

IF @endTime=''24:00''
  BEGIN
		SET @endTime=''23:59''
  END

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

CREATE TABLE ##test(startDate varchar(max));
CREATE TABLE ##rmAvailable(Id bigint,Title varchar(max));
CREATE TABLE ##Final(Id bigint,Title varchar(max),BookingDate varchar(max),[Day] varchar(max),StartTime varchar(max),EndTime varchar(max));

DECLARE @TEMP_S_DATE varchar(max)=@startDate,@TEMP_E_DATE varchar(max)=@endDate
   WHILE(Convert(date,@TEMP_S_DATE) <= Convert(date,@TEMP_E_DATE))
   BEGIN
		INSERT INTO ##test values(FORMAT (Convert(Date,@TEMP_S_DATE), ''dd/MM/yyyy''))
		SET @TEMP_S_DATE=DATEADD(DAY,1,@TEMP_S_DATE);
   END;

   WITH bookingExist as(

   SELECT *  from BookingDetails bd where 
		(FORMAT(Convert(date,bd.BookingDate),''dd/MM/yyyy'') IN (select startDate from ##test))
		AND
		(
			Convert(time,@endTime) <= Convert(time,bd.StartTime) 
		OR
			Convert(time,@startTime) >= Convert(time,bd.FinishTime)
		)
		)
	    		
   INSERT INTO ##rmAvailable
   SELECT Id,Title FROM Room_Types WHERE  BranchId=@BranchId  AND   Id NOT  IN (SELECT roomTypeId FROM bookingExist) AND Id=IIF(@roomTypeId=0,Id,@roomTypeId) AND Maxperson >= IIF(@maxPerson=0,Id,@maxPerson);
   
   SET @TEMP_S_DATE = @startDate;
   SET @TEMP_E_DATE = @endDate;

   WHILE(Convert(date,@TEMP_S_DATE) <= Convert(date,@TEMP_E_DATE))
   BEGIN
		 print @TEMP_S_DATE;
		
		INSERT INTO ##Final
		  SELECT Id,Title,Convert(date,@TEMP_S_DATE) as BookingDate ,DATENAME(dw,CONVERT(DATE,@TEMP_S_DATE)) as [day],
		  @startTime as StartTime,@endTime as  EndTime
		   from ##rmAvailable;
		 
	     SET @TEMP_S_DATE=DATEADD(DAY,1,@TEMP_S_DATE);
   END;
SET @Finalquery = ''SELECT Id,Title,StartTime,EndTime,BookingDate as BookingDate,[Day] ,TotalRecords=Count(1) Over()from ##Final 
	WHERE
	    DATENAME(dw,CONVERT(DATE,BookingDate))<>IIF(@SUNDAY=1,''''Sunday'''',DATENAME(dw, CONVERT(DATE, DateAdd(dd,1,BookingDate))))
	AND DATENAME(dw,CONVERT(DATE,BookingDate))<>IIF(@MONDAY=1,''''Monday'''',DATENAME(dw, CONVERT(DATE, DateAdd(dd,1,BookingDate))))
	AND DATENAME(dw,CONVERT(DATE,BookingDate))<>IIF(@TUESDAY=1,''''Tuesday'''',DATENAME(dw, CONVERT(DATE , DateAdd(dd,1,BookingDate))))
	AND DATENAME(dw,CONVERT(DATE,BookingDate))<>IIF(@WEDNESDAY=1,''''Wednesday'''',DATENAME(dw, CONVERT(DATE ,DateAdd(dd,1,BookingDate))))
	AND DATENAME(dw,CONVERT(DATE,BookingDate))<>IIF(@THURSDAY=1,''''Thursday'''',DATENAME(dw, CONVERT(DATE, DateAdd(dd,1,BookingDate))))
	AND DATENAME(dw,CONVERT(DATE,BookingDate))<>IIF(@FRIDAY=1,''''Friday'''',DATENAME(dw, CONVERT(DATE, DateAdd(dd,1,BookingDate))))
	AND DATENAME(dw,CONVERT(DATE,BookingDate))<>IIF(@SATURDAY=1,''''Saturday'''',DATENAME(dw, CONVERT(DATE, DateAdd(dd,1,BookingDate))))
'';

IF @Search != ''''
BEGIN
 SET @Finalquery = @Finalquery +'' AND ( Title LIKE ''''%''''+@Search+''''%'''' OR  StartTime LIKE ''''%''''+@Search+''''%'''' OR EndTime LIKE ''''%''''+@Search+''''%'''')'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

print @Finalquery
IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';


EXEC sp_executesql @Finalquery, N''@roomTypeId bigint,@startDate varchar(max),@endDate varchar(max),@maxPerson int,@userGroup bigint,
@BookingStatus int,@NumberOfAttending int,@ClientId bigint,@SUNDAY bit,@MONDAY bit,@TUESDAY bit,@WEDNESDAY bit,@THURSDAY bit,@FRIDAY bit,@SATURDAY bit,
@startTime varchar(max),@endTime varchar(max),@BranchId BIGINT,
@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@roomTypeId,@startDate,@endDate,@maxPerson,@userGroup,@BookingStatus,@NumberOfAttending,@ClientId,@SUNDAY,@MONDAY,@TUESDAY,@WEDNESDAY,@THURSDAY,@FRIDAY,@SATURDAY,
@startTime,@endTime,@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

    DROP TABLE ##test;
   	DROP TABLE ##rmAvailable;
	DROP TABLE ##Final;
END'
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql @UseAndExecStatment,
N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetRoomTypesList]
@BranchId BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT Id,ISNULL(code,'''''''') as code,ISNULL(Title,'''''''')as Title,ISNULL(Description,'''''''') as Description,ISNULL(Notes,'''''''') as Notes,
ISNULL(Floor,0) as Floor ,ISNULL(HourlyRate,0) as HourlyRate,ISNULL(SundayRate,0) as SundayRate ,ISNULL(SaturdayRate,0) as SaturdayRate,
ISNULL(Maxperson,0) as Maxperson ,TotalRecords = COUNT(1) OVER() FROM Room_Types
WHERE IsDelete=0 and BranchId=@BranchId 
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( code LIKE ''''%''''+@Search+''''%'''' OR Title LIKE ''''%''''+@Search+''''%'''' OR Notes LIKE ''''%''''+@Search+''''%'''' OR Floor LIKE ''''%''''+@Search+''''%'''' OR HourlyRate LIKE ''''%''''+@Search+''''%'''' OR SundayRate LIKE ''''%''''+@Search+''''%'''' OR SaturdayRate LIKE ''''%''''+@Search+''''%'''' OR Maxperson LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[GetSecurityList]
@BookingDetailId BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT sec.Id, ISNULL(sec.DateCollected, '''''''') as DateCollected, ISNULL(sec.DateReturned, '''''''') as DateReturned, ISNULL(sec.TimeCollected, '''''''') as TimeCollected, ISNULL(sec.TimeReturned, '''''''') as TimeReturned,
 ISNULL(sec.CollectedBy, '''''''') as CollectedBy, ISNULL(sec.ReturnedBy, '''''''') as ReturnedBy, TotalRecords = COUNT(1) OVER() from Security sec 
 WHERE sec.IsDelete=0 AND sec.BookingDetailId = @BookingDetailId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( DateCollected LIKE ''''%''''+@Search+''''%'''' OR DateReturned LIKE ''''%''''+@Search+''''%''''
OR TimeCollected LIKE ''''%''''+@Search+''''%'''' OR TimeReturned LIKE ''''%''''+@Search+''''%'''' OR CollectedBy LIKE ''''%''''+@Search+''''%'''' OR ReturnedBy LIKE ''''%''''+@Search+''''%'''')'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BookingDetailId BIGINT, @Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BookingDetailId, @Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE   PROCEDURE [dbo].[GetUserAccess]
AS
BEGIN
select 
Cast(ISNULL(UserId,0)as varchar(max))as UserId,
cast(ISNULL(MenuId,0)as varchar(max))as MenuId
   from UserAccess;
END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetUserGroupList]
@BranchId  BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT Id,ISNULL(code,'''''''') as code,ISNULL(Title,'''''''')as Title,ISNULL(Description,'''''''') as Description, TotalRecords = COUNT(1) OVER() FROM UserGroups
WHERE IsDelete=0 and BranchId=@BranchId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( code LIKE ''''%''''+@Search+''''%'''' OR Title LIKE ''''%''''+@Search+''''%'''' OR Description LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE procedure [dbo].[GetVisitorBookingList]
@BookingDetailId BIGINT,
@CustomSearch VARCHAR(MAX) = null,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX) = null,
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX), @CustomQuery NVARCHAR(MAX);

SET @Finalquery = ''SELECT
ISNULL( Id, 0) as VisitorId,
ISNULL(Description, '''''''') as Description,
ISNULL(Name,'''''''')as Name,
ISNULL(SurName,'''''''')as SurName,
ISNULL(Address,'''''''')as Address,
ISNULL(PostCode,'''''''') as PostCode,
ISNULL(Email,'''''''') as Email,
ISNULL(Telephone,'''''''') as Telephone,
ISNULL(Mobile,'''''''') as Mobile,
ISNULL(Notes,'''''''') as Notes,
TotalRecords = COUNT(1) OVER() FROM Visitors
WHERE IsDelete=0 AND Id NOT IN (SELECT VisitorId from VisitorBooking  where IsDelete = 0)
'';

SET @CustomQuery = ''
SELECT
ISNULL(vb.Id,0)as Id,
ISNULL(vb.BookingDetailId, 0) as BookingDetailsId,
ISNULL(vi. Id, 0) as VisitorId,
ISNULL(vi.Description, '''''''') as Description,
ISNULL(vi.Name,'''''''')as Name,
ISNULL(vi.SurName,'''''''')as SurName,
ISNULL(vi.Address,'''''''')as Address,
ISNULL(vi.PostCode,'''''''') as PostCode,
ISNULL(vi.Email,'''''''') as Email,
ISNULL(vi.Telephone,'''''''') as Telephone,
ISNULL(vi.Mobile,'''''''') as Mobile,
ISNULL(vi.Notes,'''''''') as Notes,
TotalRecords = COUNT(1) OVER() FROM
VisitorBooking vb inner join Visitors vi on vb.VisitorId = vi.Id
WHERE vb.IsDelete=0 AND vb.BookingDetailId = @BookingDetailId
'';

IF @Search = '''' AND @CustomSearch = ''''
BEGIN
SET @Finalquery = @CustomQuery;
END;

ELSE IF @CustomSearch != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( Description LIKE ''''%''''+@Search+''''%'''' OR Name LIKE ''''%''''+@Search+''''%'''' OR SurName LIKE ''''%''''+@Search+''''%''''
OR Address LIKE ''''%''''+@Search+''''%'''' OR PostCode LIKE ''''%''''+@Search+''''%'''' OR Email LIKE ''''%''''+@Search+''''%'''' OR Telephone LIKE ''''%''''+@Search+''''%''''
OR Mobile LIKE ''''%''''+@Search+''''%'''' OR Notes LIKE ''''%''''+@Search+''''%'''')'';
END;

ELSE
BEGIN
SET @Finalquery = @CustomQuery +'' AND ( Description LIKE ''''%''''+@Search+''''%'''' OR Name LIKE ''''%''''+@Search+''''%'''' OR SurName LIKE ''''%''''+@Search+''''%''''
OR Address LIKE ''''%''''+@Search+''''%'''' OR PostCode LIKE ''''%''''+@Search+''''%'''' OR Email LIKE ''''%''''+@Search+''''%'''' OR Telephone LIKE ''''%''''+@Search+''''%''''
OR Mobile LIKE ''''%''''+@Search+''''%'''' OR Notes LIKE ''''%''''+@Search+''''%'''')'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BookingDetailId BIGINT, @CustomSearch VARCHAR(MAX), @Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BookingDetailId, @CustomSearch, @Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetVisitorList]
@BranchId       BIGINT,
@iDisplayStart INT,
@iDisplayLength INT,
@SortColumn VARCHAR(MAX),
@SortDir VARCHAR(MAX),
@Search VARCHAR(MAX),
@SearchRecords INT OUT

AS
BEGIN

DECLARE @Finalquery NVARCHAR(MAX),@QueryCount NVARCHAR(MAX);

SET @Finalquery = ''SELECT Id,ISNULL(Description,'''''''') as Description,ISNULL(Name,'''''''') as Name,ISNULL(SurName,'''''''') as SurName,ISNULL(Address,'''''''') as Address,
ISNULL(PostCode,'''''''') as PostCode,ISNULL(Email,'''''''') as Email,ISNULL(Telephone,'''''''') as Telephone,ISNULL(Mobile,'''''''') as Mobile,ISNULL(Notes,'''''''') as Notes,
TotalRecords = COUNT(1) OVER() FROM Visitors
WHERE IsDelete=0  and BranchId=@BranchId
'';

IF @Search != ''''
BEGIN
SET @Finalquery = @Finalquery +'' AND ( Description LIKE ''''%''''+@Search+''''%'''' OR Name LIKE ''''%''''+@Search+''''%'''' OR SurName LIKE ''''%''''+@Search+''''%'''' 
OR Address LIKE ''''%''''+@Search+''''%'''' OR PostCode LIKE ''''%''''+@Search+''''%'''' OR Email LIKE ''''%''''+@Search+''''%'''' OR Telephone LIKE ''''%''''+@Search+''''%'''' OR Mobile LIKE ''''%''''+@Search+''''%'''' 
OR Notes LIKE ''''%''''+@Search+''''%'''' )'';
END;
SET @Finalquery = @Finalquery+'' ORDER BY ''+REPLACE(@SortColumn, '''', '''');

IF @SortDir = ''desc''
BEGIN
SET @Finalquery = @Finalquery+'' DESC'';
END;

SET @Finalquery = @Finalquery+'' OFFSET @iDisplayStart ROWS FETCH NEXT @iDisplayLength ROWS ONLY;'';
SET @Finalquery = @Finalquery+'' SELECT @SearchRecords = @@ROWCOUNT;'';

EXEC sp_executesql @Finalquery, N''@BranchId   BIGINT,@Search VARCHAR(MAX), @iDisplayStart INT, @iDisplayLength INT, @SortColumn VARCHAR(1000), @SearchRecords INT OUTPUT'',
@BranchId,@Search, @iDisplayStart, @iDisplayLength, @SortColumn, @SearchRecords OUTPUT;

END';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


SET @SQLString='CREATE PROCEDURE [dbo].[GetWeeklyDiaryReport] 
@SelectedDate varchar(max),
@RoomTypeId BIGINT,
@BranchId bigint,
@BookingStatusId bigint,
@IsCan bigint

AS
BEGIN

 DECLARE @Counter INT 
SET @Counter=1
DECLARE  @tbl as Table (BookingDate varchar(max),startTime varchar(max),finishTime varchar(max),BookingId bigint,MeetingTitle varchar(max),BookingStatus bigint);
WHILE ( @Counter <= 7)
BEGIN
  
	insert into @tbl 
	 select 
	 format(convert(date,@SelectedDate),''dd-MM-yyyy''),
	 (FORMAT(CAST(startTime as datetime),''HH:mm'')) ,
	 (FORMAT(CAST(finishTime as datetime),''HH:mm'')),
	 Id,
	 TitleOfMeeting,
	 BookingStatus
	 from BookingDetails where  (convert(date,BookingDate)=Convert(Date,@SelectedDate) and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus != 2 and @IsCan = 1) OR
	 (convert(date,BookingDate)=Convert(Date,@SelectedDate) and BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId) and @IsCan = 0)
	 
	set @SelectedDate=DATEADD(DAY, 1,Convert(date,@SelectedDate));
    SET @Counter = @Counter  + 1
END
select * from @tbl Order By BookingDate,StartTime

END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE PROCEDURE [dbo].[GetWeeklyDiaryReportPDF]
@SelectedDate varchar(max),
@RoomTypeId BIGINT,
@BranchId bigint,
@BookingStatusId bigint,
@IsCan bigint
AS
BEGIN

 DECLARE @Counter INT 
SET @Counter=1
DECLARE  @tbl as Table (BookingDate varchar(max),Slot varchar(max),StartTime varchar(max),FinishTime varchar(max),Contact varchar(max),BookingYear varchar(max),MeetingTitle varchar(max),NumberOfpeople varchar(max),UserGroup varchar(max),BookingStatus varchar(max),RoomType varchar(max));
WHILE ( @Counter <= 7)
BEGIN
  
	insert into @tbl values(format(convert(date,@SelectedDate),''dd-MM-yyyy''),
	ISNULL((
	  select (FORMAT(CAST(StartTime as datetime),''HH:mm'') +'' - ''+FORMAT(CAST(FinishTime as datetime),''HH:mm''))  + '','' AS [text()] from BookingDetails
	  where (BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and format(convert(date,bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus !=2 and @IsCan = 1) OR
	  (BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and format(convert(date,bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''') 
	  ,
	  ISNULL((
	  select FORMAT(CAST(StartTime as datetime),''HH:mm'') + '','' AS [text()] from BookingDetails
	   where (BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and format(convert(date,bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus !=2 and @IsCan = 1) OR
	  (BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and format(convert(date,bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''') 
	   ,
	   ISNULL((
	  select FORMAT(CAST(FinishTime as datetime),''HH:mm'')+ '',''  AS [text()] from BookingDetails
	   where (BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and format(convert(date,bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus !=2 and @IsCan = 1) OR
	  (BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and format(convert(date,bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''') 
	   ,
	   ISNULL((
	  select ISNULL(convert(varchar,BookingContact ),''-'')+ '',''  AS [text()] from BookingDetails
	   where (BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and format(convert(date,bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus !=2 and @IsCan = 1) OR
	  (BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and format(convert(date,bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''') 
	   ,
	 (select YEAR(convert(date,@SelectedDate))),
	  ISNULL((
	  select ISNULL(TitleOfMeeting,''-'') + '',%'' from BookingDetails
	   where (BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and format(convert(date,bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus !=2 and @IsCan = 1) OR
	  (BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and format(convert(date,bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''') 
	  ,
	   ISNULL((
	  select ISNULL(convert(varchar,NumberOfAttending ),''-'')+ '',''  AS [text()] from BookingDetails
	   where (BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and format(convert(date,bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus !=2 and @IsCan = 1) OR
	  (BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId )and format(convert(date,bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''') 
	   ,
	   ISNULL((
	  select ISNULL(convert(varchar,u.Title ),''-'')+ '',%''  AS [text()] from BookingDetails b
	  join UserGroups u on b.UserGroupId=u.Id
	  where (b.BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and   format(convert(date,b.bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus!= 2 and @IsCan = 1)  OR (b.BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and   format(convert(date,b.bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''')
	   ,
	   ISNULL((
	  select ISNULL(convert(varchar,s.Status ),''-'')+ '',''  AS [text()] from BookingDetails b
	  join BookingStatus s on b.BookingStatus=s.Id
	  where (b.BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and   format(convert(date,b.bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus!= 2 and @IsCan = 1)  OR (b.BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and   format(convert(date,b.bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''')
	   ,
	   ISNULL((
	  select ISNULL(convert(varchar,r.Title ),''-'')+ '',''  AS [text()] from BookingDetails b
	  join Room_Types r on b.RoomTypeId=r.Id
	   where (b.BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and   format(convert(date,b.bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus!= 2 and @IsCan = 1)  OR (b.BranchId=@BranchId and  RoomTypeId=iif(@RoomTypeId =0,RoomTypeId,@RoomTypeId ) and   format(convert(date,b.bookingDate),''dd-MM-yyyy'')=format(convert(date,@SelectedDate),''dd-MM-yyyy'') and BookingStatus=iif(@BookingStatusId =0,BookingStatus,@BookingStatusId ) and @IsCan = 0) ORDER BY (FORMAT(CAST(StartTime as datetime),''HH:mm'')) FOR XML PATH ('''')),'''')
	);
	set @SelectedDate=DATEADD(DAY, 1,Convert(date,@SelectedDate));
    SET @Counter  = @Counter  + 1	
END
 select BookingDate, Slot, StartTime, FinishTime, Contact, BookingYear, MeetingTitle, NumberOfpeople, UserGroup, BookingStatus, RoomType from @tbl order by BookingDate

END


';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString


Set @SQLString='CREATE FUNCTION SplitString 
(
    -- Add the parameters for the function here
    @myString varchar(500),
    @deliminator varchar(10)
)
RETURNS 
@ReturnTable TABLE 
(
    -- Add the column definitions for the TABLE variable here
    [id] [int] IDENTITY(1,1) NOT NULL,
    [part] [varchar](50) NULL
)
AS
BEGIN
        Declare @iSpaces int
        Declare @part varchar(50)

        --initialize spaces
        Select @iSpaces = charindex(@deliminator,@myString,0)
        While @iSpaces > 0

        Begin
            Select @part = substring(@myString,0,charindex(@deliminator,@myString,0))

            Insert Into @ReturnTable(part)
            Select @part

    Select @myString = substring(@mystring,charindex(@deliminator,@myString,0)+ len(@deliminator),len(@myString) - charindex('' '',@myString,0))


            Select @iSpaces = charindex(@deliminator,@myString,0)
        end

        If len(@myString) > 0
            Insert Into @ReturnTable
            Select @myString

    RETURN 
END
';
print(@SQLString);
SET @UseAndExecStatment = 'use ' + @dbName +' exec sp_executesql @SQLString'

EXEC sp_executesql  @UseAndExecStatment,
            N'@SQLString nvarchar(max)', @SQLString=@SQLString

DECLARE @SQL NVARCHAR(MAX)
SET @SQL = 'use ' + @dbName + ' ALTER ROLE db_owner ADD MEMBER user1;'
EXEC sp_executesql @sql
END 