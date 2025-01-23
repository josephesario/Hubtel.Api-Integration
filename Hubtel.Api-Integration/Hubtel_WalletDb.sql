--Database Creation--
--Hubtel Wallet By Jose Rodolfo Esapa Riochi
--------------------------------------------

Create Database HubtelWalletDb

go

use HubtelWalletDb

go

--1.User Type
CREATE TABLE t_UserType(

   ID uniqueidentifier primary key default newId(),
   [Name] nvarchar(4) check ([Name] IN ('Momo','Card')) unique,
   CreatedAt DateTime  null default GETDATE(),

);

go

CREATE TABLE t_UserAccess
(

   ID uniqueidentifier primary key default newId(),
   UserTypeID  uniqueidentifier not null,
   Email_PhoneNumber varchar(120) unique not null,
   UserSecret varchar(500),
   CreatedAt DateTime  null default GETDATE(),

   Constraint[fk_t_UserAccess_And_t_UserType] foreign key (UserTypeID) references t_UserType(ID)

);

go

CREATE TABLE t_UserProfile(

   ID uniqueidentifier primary key default newId(),
   UserAccessID  uniqueidentifier not null,
   LegalName VARCHAR(120)  unique not null,
   IdentityCardNumber varchar(20) unique not null,
   CreatedAt DateTime  null default GETDATE(),
   PhoneNumber VARCHAR(15)  unique not null,
   
   Constraint[fk_t_UserAccess_And_t_UserProfile] foreign key (UserAccessID) references t_UserAccess(ID)

);

go

CREATE TABLE t_CardType(

   ID uniqueidentifier primary key default newId(),
   [Name] Char(4) check ([Name] IN ('VISA','MASTER CARD')) unique,
   CreatedAt DateTime  null default GETDATE(),
);

CREATE TABLE t_SimcardType(

   ID uniqueidentifier primary key default newId(),
   [Name] Char(9) check ([Name] IN ('vodafone','mtn','airteltigo')) unique,
   CreatedAt DateTime  null default GETDATE(),
);



go

CREATE TABLE t_Card_AccountDetails(

   ID uniqueidentifier primary key default newId(),
   UserAccessID  uniqueidentifier not null,
   UserProfileID uniqueidentifier not null,
   CardTypeID    uniqueidentifier not null,
   CardNumber VARCHAR(16)  unique not null,
   CreatedAt DateTime  null default GETDATE(),

   Constraint[fk_t_Card_AccountDetails_And_t_UserAccess] foreign key (UserAccessID) references t_UserAccess(ID),
   Constraint[fk_t_Card_AccountDetails_And_t_UserProfile] foreign key (UserProfileID) references t_UserProfile(ID),
   Constraint[fk_t_Card_AccountDetails_And_t_CardType] foreign key (CardTypeID) references t_CardType(ID),

);

go

CREATE TABLE t_Phone_AccountDetails(

   ID uniqueidentifier primary key default newId(),
   UserAccessID     uniqueidentifier not null,
   UserProfileID    uniqueidentifier not null,
   SimCardTypeID    uniqueidentifier not null,
   PhoneNumber varchar(15) unique not null,
   CreatedAt DateTime  null default GETDATE(),

   Constraint[fk_t_Phone_AccountDetails_And_t_UserAccess] foreign key (UserAccessID) references t_UserAccess(ID),
   Constraint[fk_t_Phone_AccountDetails_And_t_UserProfile] foreign key (UserProfileID) references t_UserProfile(ID),
   Constraint[fk_t_Phone_AccountDetails_And_t_SimcardType] foreign key (SimCardTypeID) references t_SimcardType(ID),

);