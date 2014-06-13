use CCP_DATA
IF  EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[ShipTypesView]'))
DROP TABLE [dbo].ShipTypesView
GO

----create table -- do not use -- use select into below
create table dbo.ShipTypesView
(
TypeID integer,
ShipName varchar(100),
ShipType varchar(100),
[description] nvarchar(3000),
difficulty integer
);
go


select typeID, typeName as ShipName, groupName as ShipType, t.description, 1 as difficulty
into ShipTypesView
from invTypes t
inner join invGroups g
on g.groupID = t.groupID
where g.categoryID = 6
and t.published = 1
GO



/*
difficulty 
0=trivial/exclude(duplicates such as navy issue), 
1=easy (baseline) 
2=medium (include capital/dread/super/hookbill/slicer/comet) 
3=hard (include pirate/rare)

*/

update dbo.ShipTypesView
set difficulty = 0
where typeID in (11936,11938,13202,17634,17636,17709,17713,17726,17728,17732,17843,26840,26842,29336,29337,29340,29344,32305,32307,
32309,32311,33151,33153,33155,33157,32840,32842,32844,32846,32848,33099,4363,4388,32811,4005,32983,32985,32989,33190,32987,21097,
672,11129,11132,11134,21097,21628,30842,33553,33553,33623,33625,33627,33629,33631,33633,33635,33637,33639,33641,33643,33645,33647,
33649,33651,33653,33655,33657,33659,33661,33663,33665,33667,33669,33677,33683,33685,33687,33689,33691,33693,33695,33869,33871,
33873,33875,33877,33879,33881,33883)
--variants: navy issue, ishukone watch, shuttles etc. difficulty 0 (exclude)


update dbo.ShipTypesView
set difficulty = 2
where typeID in (671,3764,11567,19720,19722,19724,22852,23773,23911,23913,23915,24483,28352,23757,23917,23919,19726)
--difficulty 2: capitals 

update dbo.ShipTypesView
set difficulty = 3
where typeID in (615,617,635,2078,2834,2836,2863,3514,3516,3518,3532,11011,11940,
11942,17715,17718,17720,17722,17736,17738,17740,17918,17920,17922,17924,17928,17930,17932,
29266,32207,32209,32788,32790,33081,33083,33395,33397,33468,33470,33472,33513,33553,33816,33818,33820,33079)
--difficulty 3: pirate/rare
--add Garmur etc


--------------------REMOVE <font etc TAGS FROM DATA--------------------------
  select [typeID],
  [description], 
  RIGHT([description],LEN([description])-41) as [updateddesc] 
  into #shiptypestemp 
  FROM [CCP_Data].[dbo].[ShipTypesView]
  where [description] like '<font%'
  
  insert into #shiptypestemp
  ( [typeID],  [description],[updateddesc] )
  select [typeID],[description],  RIGHT([description],LEN([description])-45) FROM [CCP_Data].[dbo].[ShipTypesView]
  where [description] like '<br><font%'
  
    
  --select * from #shiptypestemp
  
  update
  [CCP_Data].[dbo].[ShipTypesView]
  set
  [description] = [updateddesc]
  from
  [CCP_Data].[dbo].[ShipTypesView] s
  inner join
  #shiptypestemp t
  on s.typeID = t.typeID
    
  drop table #shiptypestemp

  ----------------------

  
select typeID, ShipName, difficulty from ShipTypesView 
order by difficulty

-----------upload data from local to server-----------------------
use [fenjaylabs.com].[CCP_Data]
IF  EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[ShipTypesView]'))
DROP TABLE [dbo].ShipTypesView
GO

insert [fenjaylabs.com].CCP_Data.dbo.ShipTypesView
([typeID],[ShipName],[ShipType],[description],[difficulty])
SELECT [typeID]
      ,[ShipName]
      ,[ShipType]
      ,[description]
      ,[difficulty]
  FROM CCP_Data.[dbo].[ShipTypesView]




create table CCP_Data.dbo.ShipTypesView
(
TypeID integer,
ShipName varchar(100),
ShipType varchar(100),
[description] nvarchar(3000),
difficulty integer
);
go