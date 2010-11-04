IF schema_id('MelaveMalka') IS NULL
	EXECUTE('create schema MelaveMalka');

--This table has the same format as a ListMaker list.
CREATE TABLE MelaveMalka.Invitees (
	RowId			UNIQUEIDENTIFIER	NOT NULL	ROWGUIDCOL	PRIMARY KEY DEFAULT(newid()),
	PersonId		UNIQUEIDENTIFIER	NOT NULL	REFERENCES Data.MasterDirectory(Id),
	[RowVersion]	RowVersion,
	DateAdded		DATETIME			NOT NULL	DEFAULT getdate(),
	--Custom fields:
	[Year]			INTEGER				NOT NULL	DEFAULT year(getdate()),
	[Source]		NVARCHAR(64)		NOT NULL
);

CREATE TABLE MelaveMalka.Ads (
	AdId			UNIQUEIDENTIFIER	NOT NULL	ROWGUIDCOL	PRIMARY KEY DEFAULT(newid()),
	[Year]			INTEGER				NOT NULL	DEFAULT year(getdate()),
	DateAdded		DATETIME			NOT NULL	DEFAULT getdate(),
	AdType			NVARCHAR(64)		NOT NULL,
	ExternalId		INTEGER				NULL,
	Comments		NVARCHAR(512)		NULL,

	[RowVersion]	RowVersion
);
--This table also has the same format as a ListMaker list.
CREATE TABLE MelaveMalka.SeatingReservations (
	RowId			UNIQUEIDENTIFIER	NOT NULL	ROWGUIDCOL	PRIMARY KEY DEFAULT(newid()),
	PersonId		UNIQUEIDENTIFIER	NOT NULL	REFERENCES Data.MasterDirectory(Id),
	[RowVersion]	RowVersion,
	DateAdded		DATETIME			NOT NULL	DEFAULT getdate(),
	--Custom fields:
	[Year]			INTEGER				NOT NULL	DEFAULT year(getdate()),
	--The Seats fields can be null to indicate Not Coming
	MensSeats		INTEGER				NULL,
	WomensSeats		INTEGER				NULL
);
