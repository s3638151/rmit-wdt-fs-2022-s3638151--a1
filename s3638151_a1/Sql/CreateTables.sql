
if not exists (select * from Information_Schema.Tables where Table_Name = 'Customer')
begin
create table Customer
(
    CustomerID int not null,
    Name nvarchar(50) not null,
    Address nvarchar(50) null,
    City nvarchar(40) null,
    PostCode nvarchar(4) null,
    constraint PK_Customer primary key (CustomerID)
);
end

if not exists (select * from Information_Schema.Tables where Table_Name = 'Login')
begin
create table Login
(
    LoginID char(8) not null,
    CustomerID int not null,
    PasswordHash char(64) not null,
    constraint PK_Login primary key (LoginID),
    constraint FK_Login_Customer foreign key (CustomerID) references Customer (CustomerID),
    constraint CH_Login_LoginID check (len(LoginID) = 8),
    constraint CH_Login_PasswordHash check (len(PasswordHash) = 64)
);
end

if not exists (select * from Information_Schema.Tables where Table_Name = 'Account')
begin
create table Account
(
    AccountNumber int not null,
    AccountType char not null,
    CustomerID int not null,
    Balance money not null,
    constraint PK_Account primary key (AccountNumber),
    constraint FK_Account_Customer foreign key (CustomerID) references Customer (CustomerID),
    constraint CH_Account_AccountType check (AccountType in ('C', 'S')),
    constraint CH_Account_Balance check (Balance >= 0)
);
end

if not exists (select * from Information_Schema.Tables where Table_Name = 'Transaction')
begin
create table [Transaction]
(
    TransactionID int identity not null,
    TransactionType char not null,
    AccountNumber int not null,
    DestinationAccountNumber int null,
    Amount money not null,
    Comment nvarchar(30) null,
    TransactionTimeUtc datetime2 not null,
    constraint PK_Transaction primary key (TransactionID),
    constraint FK_Transaction_Account_AccountNumber
    foreign key (AccountNumber) references Account (AccountNumber),
    constraint FK_Transaction_Account_DestinationAccountNumber
    foreign key (DestinationAccountNumber) references Account (AccountNumber),
    constraint CH_Transaction_TransactionType check (TransactionType in ('D', 'W', 'T', 'S')),
    constraint CH_Transaction_Amount check (Amount > 0)
);
end
