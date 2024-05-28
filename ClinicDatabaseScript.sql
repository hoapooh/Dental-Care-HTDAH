GO
create database DentalClinicDb
GO
use DentalClinicDb
GO

create table Province ( 
	ID int identity(1,1) primary key,
	Name nvarchar(20) NOT NULL
)

create table Account (
	ID int identity(1,1) primary key,
	ProvinceID int REFERENCES Province(ID),
	Username varchar(30) NOT NULL,
	Password varchar(30) NOT NULL,
	Role varchar(7), constraint Role check(Role='Admin' OR Role='Manager' OR Role='Patient' OR Role='Dentist'),
	Gender varchar(6), constraint Gender check(Gender='Male' OR Gender='Female'),
	FirstName nvarchar(50) NOT NULL,
	LastName nvarchar(50) NOT NULL,
	Email varchar(50) NOT NULL,
	Phone char(11) NOT NULL,
	Image varchar(256),
	Account_Status varchar(7), constraint Account_Status check(Account_Status='Active' OR Account_Status='Banned')
)

create table TimeSlot (
	ID int identity(1,1) primary key,
	StartTime Time NOT NULL,
	EndTime Time NOT NULL,
)

create table Degree (
	ID int identity(1,1) primary key,
	Name nvarchar(100),
)

create table Patient (
	ID int identity(1,1) primary key,
	Account_ID int REFERENCES Account(ID),
	MemberCard char(15)
)

create table Specialty(
	ID int identity(1,1) primary key,
	Name nvarchar(50) NOT NULL,
	Description ntext,
	Image varchar(256) NOT NULL
)
create table Clinic(
	ID int identity(1,1) primary key,
	Province_ID int REFERENCES Province(ID),
	Manager_ID int REFERENCES Account(ID),
	Name nvarchar(100) NOT NULL,
	Address nvarchar(150) NOT NULL,
	Description ntext,
	Image varchar(256)  NOT NULL,
	CONSTRAINT FK_Clinic_Manager FOREIGN KEY (Manager_ID) REFERENCES Account(ID)
)

create table Dentist(
	ID int identity(1,1) primary key,
	Account_ID int REFERENCES Account(ID),
	Clinic_ID int REFERENCES Clinic(ID),
	Degree_ID int REFERENCES Degree(ID),
	Description ntext
)

create table Dentist_Specialty(
	ID int identity(1,1) primary key,
	Specialty_ID int REFERENCES Specialty(ID),
	Dentist_ID int REFERENCES Dentist(ID)
)

create table Service(
	ID int identity(1,1) primary key,
	Clinic_ID int REFERENCES Clinic(ID),
	Specialty_ID int REFERENCES Specialty(ID),
	Name nvarchar(100) NOT NULL,
	Description ntext,
	Price money
)

create table Review(
	ID int identity(1,1) primary key,
	Dentist_ID int REFERENCES Dentist(ID),
	Patient_ID int REFERENCES Patient(ID),
	Comment nvarchar(2000) NOT NULL,
	Date date NOT NULL
)

create table Schedule(
	ID int identity(1,1) primary key,
	Dentist_ID int REFERENCES Dentist(ID),
	TimeSlot_ID int REFERENCES TimeSlot(ID),
	Date date NOT NULL
)

create table Appointment (
	ID int identity(1,1) primary key,
	Schedule_ID int REFERENCES Schedule(ID),
	PatientID int REFERENCES Patient(ID),
	Specialty_ID int REFERENCES Specialty(ID),
	Appointment_Status varchar, constraint Appointment_Status check(Appointment_Status='Pending' OR Appointment_Status='Canceled' OR Appointment_Status='Approved' OR Appointment_Status='Paid'),
	TotalPrice money NOT NULL
)

create table Transactions(
	ID int identity(1,1) primary key,
	Appointment_ID int REFERENCES Appointment(ID),
	Date datetime NOT NULL,
	BankAccountNumber char(20) NOT NULL,
	Bank nvarchar(100) NOT NULL
)

GO
CREATE TRIGGER trg_Clinic_Manager_Insert
ON Clinic
INSTEAD OF INSERT
AS
BEGIN
    DECLARE @ManagerID int;
    SELECT @ManagerID = INSERTED.Manager_ID FROM INSERTED;

    IF EXISTS (SELECT 1 FROM Account WHERE ID = @ManagerID AND Role = 'Manager')
    BEGIN
        INSERT INTO Clinic (Province_ID, Manager_ID, Name, Address, Description, Image)
        SELECT Province_ID, Manager_ID, Name, Address, Description, Image
        FROM INSERTED;
    END
    ELSE
    BEGIN
        RAISERROR ('The Manager_ID does not refer to an Account with Role = ''Manager''.', 16, 1);
    END
END;
