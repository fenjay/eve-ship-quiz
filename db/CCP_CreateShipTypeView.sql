use CCP_DATA
IF  EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[ShipTypesView]'))
DROP TABLE [dbo].ShipTypesView
GO


--create table dbo.ShipTypesView
--(
--typeID integer,
--ShipName varchar(50),
--ShipType varchar(50),
--description varchar(200),
--difficulty integer
--);
--GO


select typeID, typeName as ShipName, groupName as ShipType, t.description, 1 as difficulty
into ShipTypesView
from invTypes t
inner join invGroups g
on g.groupID = t.groupID
where g.categoryID = 6
and t.published = 1
GO

update dbo.ShipTypesView
set difficulty = 0
where typeID in (11936,11938,13202,17634,17636,17709,17713,17726,17728,17732,17843,26840,26842,29336,29337,29340,29344,32305,32307,
32309,32311,33151,33153,33155,33157,32840,32842,32844,32846,32848,33099,4363,4388,32811,4005,32983,32985,32989,33190,32987,21097)
--variants: navy issue, ishukone watch etc

update dbo.ShipTypesView
set difficulty = 2
where typeID in (671,3764,11567,19720,19722,19724,22852,23773,23911,23913,23915,24483,28352,23757,23917,23919)
--difficulty 2: capitals 

update dbo.ShipTypesView
set difficulty = 2
where typeID in (615,617,635,2078,2834,2836,2863,3514,3516,3518,3532,11011,11940,
11942,17703,17715,17718,17720,17722,17736,17738,17740,17812,17841,17918,17920,17922,17924,17928,17930,17932,
29266,32207,32209,32788,32790,33081,33083,33395,33397,33468,33470,33472,33513,33553)
--difficulty 2: pirate/rare



select typeID, ShipName, difficulty from ShipTypesView 

/*
difficulty 
0=trivial/exclude(duplicates such as navy issue), 
1=easy (baseline) 
2=medium (include capital/dread/super/hookbill/slicer/comet) 
3=hard (include pirate/rare)


*/

