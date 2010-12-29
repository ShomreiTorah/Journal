IF schema_id('MelaveMalka') IS NULL
	EXECUTE('create schema MelaveMalka');

CREATE TABLE MelaveMalka.MelaveMalkaInfo (
	RowId			UNIQUEIDENTIFIER	NOT NULL	ROWGUIDCOL	PRIMARY KEY DEFAULT(newid()),
	[RowVersion]	RowVersion,
	
	[Year]			INTEGER				NOT NULL	UNIQUE,
	AdDeadline		DATETIME			NOT NULL,
	MelaveMalkaDate	DATETIME			NOT NULL,

	Honoree			UNIQUEIDENTIFIER	NOT NULL	REFERENCES Data.MasterDirectory(Id),
	Speaker			NVARCHAR(128)		NOT NULL
);

--This table has the same format as a ListMaker list.
CREATE TABLE MelaveMalka.Invitees (
	RowId			UNIQUEIDENTIFIER	NOT NULL	ROWGUIDCOL	PRIMARY KEY DEFAULT(newid()),
	PersonId		UNIQUEIDENTIFIER	NOT NULL	REFERENCES Data.MasterDirectory(Id),
	[RowVersion]	RowVersion,
	DateAdded		DATETIME			NOT NULL	DEFAULT getdate(),
	--Custom fields:
	[Year]			INTEGER				NOT NULL	DEFAULT year(getdate()),
	[Source]		NVARCHAR(64)		NOT NULL,

	--For call list:
	ShouldCall		BIT					NOT NULL	DEFAULT(0),
	[Caller]		UNIQUEIDENTIFIER	NULL		DEFAULT(NULL)	REFERENCES Data.MasterDirectory(Id),
	CallerNote		NVARCHAR(512)		NULL,

	--For reminder emails
	ShouldEmail		BIT					NOT NULL	DEFAULT(0),
	EmailSubject	NVARCHAR(256)		NULL		DEFAULT(NULL),
	EmailSource		NTEXT				NULL		DEFAULT(NULL)
);
--For call list
ALTER TABLE MelaveMalka.Invitees ADD ShouldCall	BIT					NOT NULL	DEFAULT(0);
ALTER TABLE MelaveMalka.Invitees ADD [Caller]	UNIQUEIDENTIFIER	NULL		DEFAULT(NULL)	REFERENCES MelaveMalka.Callers(RowId);
ALTER TABLE MelaveMalka.Invitees ADD CallerNote	NVARCHAR(512)		NULL;

--For reminder emails
ALTER TABLE MelaveMalka.Invitees ADD ShouldEmail	BIT				NOT NULL	DEFAULT(0);
ALTER TABLE MelaveMalka.Invitees ADD EmailSubject	NVARCHAR(256)	NULL		DEFAULT(NULL);
ALTER TABLE MelaveMalka.Invitees ADD EmailSource	NTEXT			NULL		DEFAULT(NULL);

CREATE TABLE MelaveMalka.ReminderEmailLog (
	RowId			UNIQUEIDENTIFIER	NOT NULL	ROWGUIDCOL	PRIMARY KEY DEFAULT(newid()),
	[RowVersion]	RowVersion,
	InviteId		UNIQUEIDENTIFIER	NOT NULL	REFERENCES MelaveMalka.Invitees(RowId),

	[Date]			DATETIME			NOT NULL,
	EmailSubject	NVARCHAR(256)		NOT NULL,
	EmailSource		NTEXT				NOT NULL
);

--This table also has the same format as a ListMaker list.
CREATE TABLE MelaveMalka.Callers (
	RowId			UNIQUEIDENTIFIER	NOT NULL	ROWGUIDCOL	PRIMARY KEY DEFAULT(newid()),
	PersonId		UNIQUEIDENTIFIER	NOT NULL	REFERENCES Data.MasterDirectory(Id),
	[RowVersion]	RowVersion,
	DateAdded		DATETIME			NOT NULL	DEFAULT getdate(),
	--Custom fields:
	[Year]			INTEGER				NOT NULL	DEFAULT year(getdate()),
);

CREATE TABLE MelaveMalka.Ads (
	AdId			UNIQUEIDENTIFIER	NOT NULL	ROWGUIDCOL	PRIMARY KEY DEFAULT(newid()),
	[Year]			INTEGER				NOT NULL	DEFAULT year(getdate()),
	DateAdded		DATETIME			NOT NULL	DEFAULT getdate(),
	AdType			NVARCHAR(64)		NOT NULL,
	ExternalId		INTEGER				NOT NULL,
	Comments		NVARCHAR(512)		NULL,

	[RowVersion]	RowVersion
);
--This table also has the same format as a ListMaker list.
CREATE TABLE MelaveMalka.SeatReservations (
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



CREATE TABLE MelaveMalka.RaffleTickets (
	RowId			UNIQUEIDENTIFIER	NOT NULL	ROWGUIDCOL	PRIMARY KEY DEFAULT(newid()),
	PersonId		UNIQUEIDENTIFIER	NOT NULL	REFERENCES Data.MasterDirectory(Id),
	[RowVersion]	RowVersion,
	DateAdded		DATETIME			NOT NULL	DEFAULT getdate(),
	--Custom fields:
	[Year]			INTEGER				NOT NULL	DEFAULT year(getdate()),

	TicketId		INTEGER				NOT NULL,
	Paid			BIT					NOT NULL	DEFAULT(0),
	Comments		NVARCHAR(512)		NULL
);
