﻿USE DentalClinicDb
GO

--DELETE DATA IN DATABASE
--delete from [dbo].[Specialty]; 
--delete from [dbo].[TimeSlot]; 
--delete from [dbo].[Dentist]; 
--delete from [dbo].[Degree]; 
--delete from [dbo].[Clinic];
--delete from [dbo].[Account]; 
--delete from [dbo].[Service]; 
--delete from [dbo].[Dentist_Specialty]; 
--delete from [dbo].[Schedule]; 
--delete from [dbo].[PatientRecord]; 
--delete from [dbo].[Appointment]; 
--delete from [dbo].[Transaction];
--delete from [dbo].[Review]; 

--DEGREE DATA
insert into Degree (Name) values(N'Bác sĩ Răng Hàm Mặt')
insert into Degree (Name) values(N'Cử nhân Răng Hàm Mặt')
insert into Degree (Name) values(N'Chuyên khoa cấp I Răng Hàm Mặt')
insert into Degree (Name) values(N'Chuyên khoa cấp II Răng Hàm Mặt')
insert into Degree (Name) values(N'Thạc sĩ Răng Hàm Mặt')
insert into Degree (Name) values(N'Tiến sĩ Răng Hàm Mặt')
insert into Degree (Name) values(N'Cử nhân Kỹ thuật Phục hình Răng')

--SPECIALTY DATA
insert into Specialty (Name, Description, Image) values (N'Nha Khoa Tổng Quát',N'Nha Khoa Tổng Quát là phân ngành trong lĩnh vực nha khoa tập trung vào chăm sóc và điều trị các vấn đề răng miệng phổ biến như kiểm tra định kỳ, làm sạch răng, điều trị các vấn đề về sâu răng và viêm nướu. Các dịch vụ của nha khoa tổng quát giúp duy trì sức khỏe toàn diện cho răng miệng và hỗ trợ trong việc phòng ngừa các vấn đề nha khoa phức tạp hơn.', 'https://firebasestorage.googleapis.com/v0/b/auth-demo-123e3.appspot.com/o/Specialties%2Fspecial1.png?alt=media&token=713269b6-bfdd-414f-acc1-6f405c84c783')
insert into Specialty (Name, Description, Image) values (N'Niềng Răng', N'Niềng Răng còn được gọi là chỉnh nha, là phương pháp sử dụng các khí cụ như mắc cài, dây cung, chun buộc, minivis, khay niềng để tạo lực siết và kéo răng về đúng vị trí, đảm bảo khớp cắn chuẩn.', 'https://firebasestorage.googleapis.com/v0/b/auth-demo-123e3.appspot.com/o/Specialties%2Fspecial2.png?alt=media&token=bca34301-89b3-4dbe-acfd-9917e25d6534')
insert into Specialty (Name, Description, Image) values (N'Bọc Răng Sứ', N'Bọc Răng Sứ là phương pháp nha khoa thẩm mỹ và phục hình răng, trong đó một lớp vỏ sứ mỏng được gắn lên răng thật để cải thiện hình dáng, màu sắc và chức năng của răng. Quy trình này giúp khắc phục các vấn đề như răng sứt mẻ, răng nhiễm màu hoặc răng không đều, mang lại nụ cười đẹp tự nhiên và bền vững. Bọc răng sứ là giải pháp lý tưởng cho những ai mong muốn sở hữu hàm răng trắng sáng và hoàn hảo.', 'https://firebasestorage.googleapis.com/v0/b/auth-demo-123e3.appspot.com/o/Specialties%2Fspecial3.png?alt=media&token=e0b2f028-4a55-496c-bd4f-888d9aae3892')
insert into Specialty (Name, Description, Image) values (N'Trồng Răng Implant', N'Trồng Răng Implant là một phương pháp tiên tiến trong nha khoa, sử dụng trụ titanium cấy vào xương hàm để thay thế cho chân răng đã mất. Sau khi trụ implant tích hợp chặt chẽ với xương hàm, một mão răng sứ sẽ được gắn lên trên, tái tạo lại răng bị mất với hình dáng và chức năng như răng tự nhiên. Phương pháp này không chỉ cải thiện thẩm mỹ mà còn giúp khôi phục khả năng ăn nhai hiệu quả.', 'https://firebasestorage.googleapis.com/v0/b/auth-demo-123e3.appspot.com/o/Specialties%2Fspecial4.png?alt=media&token=b08b2712-1f0e-4797-b95f-81231dfbd3dd')
insert into Specialty (Name, Description, Image) values (N'Nhổ Răng Khôn', N'Nhổ Răng Khôn là quá trình loại bỏ các răng khôn (hay còn gọi là răng số 8) từ hàm dưới và trên của một người. Đây thường là quá trình phục vụ cho việc điều trị khi răng khôn gây đau, viêm nhiễm hoặc gây ra các vấn đề khác cho sức khỏe răng miệng. Quá trình này thường được thực hiện dưới sự giám sát của bác sĩ nha khoa hoặc bác sĩ phẫu thuật nha khoa.', 'https://firebasestorage.googleapis.com/v0/b/auth-demo-123e3.appspot.com/o/Specialties%2Fspecial5.png?alt=media&token=058218ff-79df-437b-adda-db3078d460be')
insert into Specialty (Name, Description, Image) values (N'Nha Khoa Trẻ Em', N'Nha Khoa Trẻ Em là chuyên ngành y học chuyên về chăm sóc và điều trị về răng miệng cho trẻ em và thanh thiếu niên. Các dịch vụ trong lĩnh vực này không chỉ tập trung vào việc điều trị các vấn đề răng miệng mà còn làm việc để tạo ra môi trường thân thiện, thoải mái và độc đáo để trẻ em có thể cảm thấy thoải mái và tự tin khi đến bác sĩ nha khoa. Đội ngũ chuyên gia Nha Khoa Trẻ Em thường được đào tạo đặc biệt để làm việc với trẻ em, với mục tiêu mang lại cho các bé một nụ cười khỏe mạnh từ khi còn nhỏ.', 'https://firebasestorage.googleapis.com/v0/b/auth-demo-123e3.appspot.com/o/Specialties%2Fspecial6.png?alt=media&token=077814a8-69b2-4c22-836a-aa92ea68f26c')


-- ACCOUNT DATA (ENCRYPTED)
insert into Account (Username, Password, Role, Gender, FirstName, LastName, Email, Image, PhoneNumber, AccountStatus, Address, DateOfBirth, Province, District, Ward) values 
('igrigaut0', 'wE2}NKWa|KW8,', N'Nha Sĩ', N'Nam', N'Hưng', N'Hoàng Thanh', 'kbradie0@amazon.de', null, '8132727246', N'Hoạt Động', N'45 Bà Triệu', '2000-04-20', N'Quảng Ngãi', N'Phú Hiệp',N'Phường 1'),
('mhallede1', 'qH8)=Slz''vA<A', N'Nha Sĩ', N'Nữ', N'Hương', N'Đinh Thị Cẩm', 'kbolingbroke1@last.fm', null, '6792750064', N'Hoạt Động', N'89 Hùng Vương', '1992-05-14', N'TP.Hồ Chí Minh', N'TP.Thủ Đức', N'Phường Long Trường'),
('abenediktovich2', 'wO1`$ugSol<|HY', N'Nha Sĩ', N'Nam', N'Sơn', N'Lại Thế', 'ehaughan2@dedecms.com', null, '2671796935', N'Hoạt Động', N'89 Hùng Vương', '1993-04-29', N'TP.Hồ Chí Minh', N'TP.Thủ Đức', N'Phường Long Trường'),
('gspaxman3', 'jT3+>u6h}', N'Nha Sĩ', N'Nữ', N'Hương', N'Hà Minh', 'reckersall3@flavors.me', null, '2276273708', N'Hoạt Động', N'45 Nguyễn Huệ', '1989-05-03', N'Bà Rịa - Vũng Tàu', N'Vũng Tàu', N'Phường Nguyễn An Ninh'),
('bcrush4', 'dZ7/CNQAZ>%"*Y', N'Quản Lý', N'Nam', N'Nhật', N'Tống Mạnh', 'dgrant4@intel.com', null, '2764281343', N'Hoạt Động', N'45 Lý Thường Kiệt', '1991-05-07', N'Bà Rịa - Vũng Tàu', N'Vũng Tàu', N'Phường 1'),
('sdominiak5', 'pK8>{4a?', N'Quản Lý', N'Nam', N'Tùng', N'Nguyễn Văn', 'krobion5@i2i.jp', null, '9182392509', N'Hoạt Động', N'78 Trần Hưng Đạo', '1992-05-14', N'TP.Hồ Chí Minh', N'TP.Thủ Đức', N'Phường Long Trường'),
('admin', '1', N'Admin', N'Nam', N'Minh', N'Ngô Nhật', 'cmees6@google.es', null, '2416708233', N'Hoạt Động', N'23 Lê Lai', '1982-05-10', N'Bà Rịa - Vũng Tàu', N'Vũng Tàu', N'Phường 2'),
('jnewlyn7', 'hT3=So_2?ek', N'Bệnh Nhân', N'Nam', N'Hạnh', N'Hồng Thị Mỹ', 'hhurlestone7@un.org', null, '8968929919', N'Hoạt Động', N'78 Trần Hưng Đạo', '1992-06-15', N'TP.Hồ Chí Minh', N'TP.Thủ Đức', N'Phường Long Trường'),
('kmozzetti8', 'pX1!bX7a', N'Nha Sĩ', N'Nữ', N'Nhung', N'Phạm Thị Cẩm', 'mmabbett8@themeforest.net', null, '5161817210', N'Hoạt Động', N'45 Nguyễn Huệ', '1980-12-12', N'Bà Rịa - Vũng Tàu', N'Vũng Tàu', N'Phường Nguyễn An Ninh'),
('cwellard9', 'yK9`Xbgyy(cMn', N'Nha Sĩ', N'Nữ', N'Thảo', N'Huỳnh Ngọc', 'llardeur9@geocities.com', null, '9789372570', N'Hoạt Động', N'34 Trần Phú', '1985-01-18', N'TP.Hồ Chí Minh', N'TP.Thủ Đức', N'Phường Phước Long A'),
('sdbhfyu', 'hfdkug8fdgbn', N'Quản Lý', N'Nữ', N'Hằng', N'Hồng Phương Minh', 'hhurfghfghstone7@un.org', null, '89634559919', N'Hoạt Động', N'78 Trần Hưng Đạo', '1992-06-15', N'TP.Hồ Chí Minh', N'TP.Thủ Đức', N'Phường Long Trường'),
('jkdfg98fbd', 'jhdfg97t*Y', N'Quản Lý', N'Nữ', N'Tú', N'Phạm Nguyễn Cẩm', 'mmgffgghfett8@themeforest.net', null, '516134210', N'Hoạt Động', N'45 Nguyễn Huệ', '1980-12-12', N'Bà Rịa - Vũng Tàu', N'Vũng Tàu', N'Phường Nguyễn An Ninh'),
('98dyfgbfd', 'huidfgh()jhb', N'Quản Lý', N'Nữ', N'Tâm', N'Huỳnh Thị', 'llardeur9@geofgggies.com', null, '97345672570', N'Hoạt Động', N'34 Trần Phú', '1985-01-18', N'TP.Hồ Chí Minh', N'TP.Thủ Đức', N'Phường Phước Long A');

-- CLINIC DATA
insert into Clinic (Province, District, Ward, ManagerID, Name, Address, Basis, Description, Image) values (N'Hà Nội', N'Cầu Giấy', N'', 5, N'Phòng khám Nha khoa Đại Dương', N'200 Hoàng Quốc Việt, Cầu Giấy', N'Cơ sở 1', N'Description', 'https://firebasestorage.googleapis.com/v0/b/auth-demo-123e3.appspot.com/o/Clinic%2FDai-Duong-Dental.jpg?alt=media&token=afbd10b2-67a5-426d-a64a-226890a49f35')
insert into Clinic (Province, District, Ward, ManagerID, Name, Address, Basis, Description, Image) values (N'Hà Nội', N'Lê Chân', N'Phường 5', 6, N'Nha khoa Lạc Việt', N'Số 107 Tô Hiệu, Lê Chân', N'Cơ sở 1', N'Description', 'https://firebasestorage.googleapis.com/v0/b/auth-demo-123e3.appspot.com/o/Clinic%2FLac-Viet-Dental.jpg?alt=media&token=74c88119-a4f1-451f-9528-d04bb48f310e')
insert into Clinic (Province, District, Ward, ManagerID, Name, Address, Basis, Description, Image) values (N'TP. Hồ Chí Minh', N'Quận 1', N'Bến Thành', 11, N'Nha khoa Hoa Hồng Phương Đông', N'Lầu 2, Trung tâm Thời trang - Vàng Bạc - Đá quý Bến Thành, số 30-36 Phan Bội Châu, Phường Bến Thành, Quận 1', N'Cơ sở 1', N'Description', 'https://firebasestorage.googleapis.com/v0/b/auth-demo-123e3.appspot.com/o/Clinic%2FHoa-Hong-Phuong-Dong-Dental.jpg?alt=media&token=e15e9780-5c60-47b4-909f-e62d691de6fc')
insert into Clinic (Province, District, Ward, ManagerID, Name, Address, Basis, Description, Image) values (N'Bà Rịa - Vũng Tàu', N'Vũng Tàu', N'Phường 4', 12, N'Nha khoa Hoa Sứ', N'89B Nguyễn Văn Trỗi, phường 4, Vũng Tàu', N'Cơ sở 1', N'Description', 'https://firebasestorage.googleapis.com/v0/b/auth-demo-123e3.appspot.com/o/Clinic%2FHoa-Su-Dental.jpg?alt=media&token=11f94870-6f79-484b-b998-d8d1403e5569')
insert into Clinic (Province, District, Ward, ManagerID, Name, Address, Basis, Description, Image) values (N'Nam Định', N'Bà Triệu', N'', 13, N'Nha khoa Thăng Long', N'Số 264 Trần Hưng Đạo, Bà Triệu, TP.Nam Định', N'Cơ sở 1', N'Description', 'https://firebasestorage.googleapis.com/v0/b/auth-demo-123e3.appspot.com/o/Clinic%2FThang-Long-Dental.jpg?alt=media&token=1535a65d-d2b8-4165-af1f-7b3d3e805617')

--DENTIST DATA
insert into Dentist(AccountID, ClinicID, DegreeID, Description) values (1, 1, 4, N'BS. Hoàng thanh Hưng hiện đang công tác tại Phòng Khám đa khoa Đại Dương. Bác sĩ Hưng đã tham gia thường xuyên các lớp đào tạo chuyên sâu, và cũng là tác giả trong nhiều bài viết về khoa học được đăng trên các tạp chí y học trong nước.')
insert into Dentist(AccountID, ClinicID, DegreeID, Description) values (9, 1, 5, N'TS. BS. Phạm Thị Cẩm Nhung có hơn 17 năm kinh nghiệm trong lĩnh vực Phục Hình Răng và có chuyên môn sâu về điều trị bệnh lý Nội tổng hợp.')
insert into Dentist(AccountID, ClinicID, DegreeID, Description) values (10, 2, 5, N'ThS. BS. CKI. Huỳnh Ngọc Thảo có hơn 12 năm kinh nghiệm trong lĩnh vực Chỉnh Hình, có chuyên môn sâu về chẩn đoán và điều trị các bệnh liên quan về Răng, tư vấn các phương pháp điều trị về cấu trúc Răng , trồng Răng.')
insert into Dentist(AccountID, ClinicID, DegreeID, Description) values (2, 2, 2, N'BS. Đinh Thị Cẩm Hương hiện đang công tác tại Phòng Khám đa khoa Lạc Việt. Bác sĩ Hương từng được đi đào tạo chuyên môn về Nha khoa ở Mỹ và trong quá trình công tác bác sĩ Hương còn tích cực tham gia các lớp đào tạo chuyên môn được tổ chức trong nước và nước ngoài. Ngoài ra, cô còn là tác giả và đồng tác giả của nhiều bài viết về khoa học được đăng trên các tạp chí Y học ở trong và ngoài nước.')
insert into Dentist(AccountID, ClinicID, DegreeID, Description) values (3, 3, 1, N'BS. Lại Thế Sơn hiện là Phó khoa Răng-Hàm-Mặt của bệnh viện Nhi Đồng Thành phố Hồ Chí Minh và đồng thời cũng đang là bác sĩ điều trị tại phòng khám Hoa Hồng Phương Đông, bác sĩ cũng từng có thời gian làm việc tại Bệnh viện Nhi Đồng 2. Bác sĩ Khải đã tham gia các lớp đào tạo chuyên môn được tổ chức trong nước.')
insert into Dentist(AccountID, ClinicID, DegreeID, Description) values (4, 4, 3, N'BS. Hà Minh Hương hiện đang công tác tại Phòng Khám Nha khoa Hoa Sứ. Bác sĩ Hương đã tham gia thường xuyên các lớp đào tạo chuyên sâu, và cũng là tác giả trong nhiều bài viết về khoa học được đăng trên các tạp chí y học trong nước.')
															
-- DENTIST - SPECIALTY DATA
insert into Dentist_Specialty(SpecialtyID, DentistID) values (1,1)
insert into Dentist_Specialty(SpecialtyID, DentistID) values (2,1)
insert into Dentist_Specialty(SpecialtyID, DentistID) values (2,2)
insert into Dentist_Specialty(SpecialtyID, DentistID) values (3,2)
insert into Dentist_Specialty(SpecialtyID, DentistID) values (1,3)
insert into Dentist_Specialty(SpecialtyID, DentistID) values (4,4)
insert into Dentist_Specialty(SpecialtyID, DentistID) values (5,4)
insert into Dentist_Specialty(SpecialtyID, DentistID) values (2,5)
insert into Dentist_Specialty(SpecialtyID, DentistID) values (6,5)
insert into Dentist_Specialty(SpecialtyID, DentistID) values (1,6)
insert into Dentist_Specialty(SpecialtyID, DentistID) values (2,6)
insert into Dentist_Specialty(SpecialtyID, DentistID) values (3,6)
insert into Dentist_Specialty(SpecialtyID, DentistID) values (4,6)

--TIMESLOT DATA
insert into TimeSlot(StartTime, EndTime) values ('07:00','07:30')
insert into TimeSlot(StartTime, EndTime) values ('07:30','08:00')
insert into TimeSlot(StartTime, EndTime) values ('08:00','08:30')
insert into TimeSlot(StartTime, EndTime) values ('08:30','09:00')
insert into TimeSlot(StartTime, EndTime) values ('09:00','09:30')
insert into TimeSlot(StartTime, EndTime) values ('09:30','10:00')
insert into TimeSlot(StartTime, EndTime) values ('10:00','10:30')
insert into TimeSlot(StartTime, EndTime) values ('10:30','11:00')
insert into TimeSlot(StartTime, EndTime) values ('11:00','11:30')
insert into TimeSlot(StartTime, EndTime) values ('11:30','12:00')
insert into TimeSlot(StartTime, EndTime) values ('12:00','12:30')
insert into TimeSlot(StartTime, EndTime) values ('12:30','13:00')
insert into TimeSlot(StartTime, EndTime) values ('13:00','13:30')
insert into TimeSlot(StartTime, EndTime) values ('13:30','14:00')
insert into TimeSlot(StartTime, EndTime) values ('14:00','14:30')
insert into TimeSlot(StartTime, EndTime) values ('14:30','15:00')
insert into TimeSlot(StartTime, EndTime) values ('15:00','15:30')
insert into TimeSlot(StartTime, EndTime) values ('15:30','16:00')
insert into TimeSlot(StartTime, EndTime) values ('16:00','16:30')
insert into TimeSlot(StartTime, EndTime) values ('16:30','17:00')
insert into TimeSlot(StartTime, EndTime) values ('17:00','17:30')
insert into TimeSlot(StartTime, EndTime) values ('17:30','18:00')

--PATIENT RECORD DATA
insert into PatientRecord(AccountID, MemberCard, FullName, DateOfBirth, PhoneNumber, Gender, Job, IdentityNumber, EmailReceiver, Province, District, Ward, Address) values (8, '2345567542222', N'Hồng Thị Mỹ Hạnh', '1990-07-10', '01536728634', N'Nữ', N'Nội Trợ', '0762543678', 'hanh@123@gt.com', N'Hồ Chí Minh', N'Gò Vấp', N'Bình Tân', N'200, Hai Bà Trưng')
insert into PatientRecord(AccountID, MemberCard, FullName, DateOfBirth, PhoneNumber, Gender, Job, IdentityNumber, EmailReceiver, Province, District, Ward, Address) values (8, '2345567542222', N'Hồng Thị Mỹ Hạnh', '1990-07-10', '01536728634', N'Nữ', N'Nội Trợ', '0762543678', 'hanh@123@gt.com', N'Hồ Chí Minh', N'Gò Vấp', N'Bình Tân', N'200, Hai Bà Trưng')
insert into PatientRecord(AccountID, MemberCard, FullName, DateOfBirth, PhoneNumber, Gender, Job, IdentityNumber, EmailReceiver, Province, District, Ward, Address) values (8, '2345567542222', N'Hồng Thị Mỹ Hạnh', '1990-07-10', '01536728634', N'Nữ', N'Nội Trợ', '0762543678', 'hanh@123@gt.com', N'Hồ Chí Minh', N'Gò Vấp', N'Bình Tân', N'200, Hai Bà Trưng')
insert into PatientRecord(AccountID, MemberCard, FullName, DateOfBirth, PhoneNumber, Gender, Job, IdentityNumber, EmailReceiver, Province, District, Ward, Address) values (8, '2345567542222', N'Hồng Thị Mỹ Hạnh', '1990-07-10', '01536728634', N'Nữ', N'Nội Trợ', '0762543678', 'hanh@123@gt.com', N'Hồ Chí Minh', N'Gò Vấp', N'Bình Tân', N'200, Hai Bà Trưng')
insert into PatientRecord(AccountID, MemberCard, FullName, DateOfBirth, PhoneNumber, Gender, Job, IdentityNumber, EmailReceiver, Province, District, Ward, Address) values (8, '2345567542222', N'Hồng Thị Mỹ Hạnh', '1990-07-10', '01536728634', N'Nữ', N'Nội Trợ', '0762543678', 'hanh@123@gt.com', N'Hồ Chí Minh', N'Gò Vấp', N'Bình Tân', N'200, Hai Bà Trưng')
insert into PatientRecord(AccountID, MemberCard, FullName, DateOfBirth, PhoneNumber, Gender, Job, IdentityNumber, EmailReceiver, Province, District, Ward, Address) values (8, '2345567542222', N'Hồng Thị Mỹ Hạnh', '1990-07-10', '01536728634', N'Nữ', N'Nội Trợ', '0762543678', 'hanh@123@gt.com', N'Hồ Chí Minh', N'Gò Vấp', N'Bình Tân', N'200, Hai Bà Trưng')
insert into PatientRecord(AccountID, MemberCard, FullName, DateOfBirth, PhoneNumber, Gender, Job, IdentityNumber, EmailReceiver, Province, District, Ward, Address) values (8, '2345567542222', N'Hồng Thị Mỹ Hạnh', '1990-07-10', '01536728634', N'Nữ', N'Nội Trợ', '0762543678', 'hanh@123@gt.com', N'Hồ Chí Minh', N'Gò Vấp', N'Bình Tân', N'200, Hai Bà Trưng')
insert into PatientRecord(AccountID, MemberCard, FullName, DateOfBirth, PhoneNumber, Gender, Job, IdentityNumber, EmailReceiver, Province, District, Ward, Address) values (8, '2345567542222', N'Hồng Thị Mỹ Hạnh', '1990-07-10', '01536728634', N'Nữ', N'Nội Trợ', '0762543678', 'hanh@123@gt.com', N'Hồ Chí Minh', N'Gò Vấp', N'Bình Tân', N'200, Hai Bà Trưng')
insert into PatientRecord(AccountID, MemberCard, FullName, DateOfBirth, PhoneNumber, Gender, Job, IdentityNumber, EmailReceiver, Province, District, Ward, Address) values (8, '2345567542222', N'Hồng Thị Mỹ Hạnh', '1990-07-10', '01536728634', N'Nữ', N'Nội Trợ', '0762543678', 'hanh@123@gt.com', N'Hồ Chí Minh', N'Gò Vấp', N'Bình Tân', N'200, Hai Bà Trưng')
insert into PatientRecord(AccountID, MemberCard, FullName, DateOfBirth, PhoneNumber, Gender, Job, IdentityNumber, EmailReceiver, Province, District, Ward, Address) values (8, '2345567542222', N'Hồng Thị Mỹ Hạnh', '1990-07-10', '01536728634', N'Nữ', N'Nội Trợ', '0762543678', 'hanh@123@gt.com', N'Hồ Chí Minh', N'Gò Vấp', N'Bình Tân', N'200, Hai Bà Trưng')


--SCHEDULE DATA
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(1, 2, '2024-06-07', N'Đã Đặt')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(1, 3, '2024-06-07', N'Còn Trống')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(1, 4, '2024-06-07', N'Còn Trống')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(1, 5, '2024-06-07', N'Còn Trống')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(1, 2, '2024-06-08', N'Đã Đặt')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(1, 3, '2024-06-09', N'Đã Đặt')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(1, 4, '2024-06-09', N'Đã Đặt')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(1, 5, '2024-06-09', N'Đã Đặt')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(2, 3, '2024-06-04', N'Còn Trống')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(2, 4, '2024-06-05', N'Còn Trống')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(2, 5, '2024-06-06', N'Còn Trống')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(2, 3, '2024-06-04', N'Đã Đặt')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(2, 4, '2024-06-05', N'Đã Đặt')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(2, 5, '2024-06-06', N'Đã Đặt')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(3, 1, '2024-06-07', N'Còn Trống')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(3, 2, '2024-06-07', N'Đã Đặt')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(3, 3, '2024-06-08', N'Còn Trống')
insert into Schedule(DentistID, TimeSlotID, Date, ScheduleStatus) values(3, 4, '2024-06-09', N'Còn Trống')
		
--APPOINTMENT DATA		
insert into Appointment(ScheduleID, PatientRecordID, SpecialtyID, AppointmentStatus, TotalPrice) values (1, 1, 1, N'Đã Chấp Nhận', 120000)
insert into Appointment(ScheduleID, PatientRecordID, SpecialtyID, AppointmentStatus, TotalPrice) values (5, 2, 2, N'Chờ Xác Nhận', 550000)
insert into Appointment(ScheduleID, PatientRecordID, SpecialtyID, AppointmentStatus, TotalPrice) values (6, 4, 2, N'Đã Khám', 220000)
insert into Appointment(ScheduleID, PatientRecordID, SpecialtyID, AppointmentStatus, TotalPrice) values (7, 5, 3, N'Đã Hủy', 100000)
insert into Appointment(ScheduleID, PatientRecordID, SpecialtyID, AppointmentStatus, TotalPrice) values (8, 8, 3, N'Đã Chấp Nhận', 50000)
insert into Appointment(ScheduleID, PatientRecordID, SpecialtyID, AppointmentStatus, TotalPrice) values (12, 6, 4, N'Đã Khám', 120000)
insert into Appointment(ScheduleID, PatientRecordID, SpecialtyID, AppointmentStatus, TotalPrice) values (13, 7, 4, N'Đã Khám', 120000)
insert into Appointment(ScheduleID, PatientRecordID, SpecialtyID, AppointmentStatus, TotalPrice) values (14, 3, 4, N'Đã Khám', 120000)
insert into Appointment(ScheduleID, PatientRecordID, SpecialtyID, AppointmentStatus, TotalPrice) values (16, 9, 4, N'Đã Khám', 120000)

--SERVICE DATA
insert into Service (ClinicID, SpecialtyID, Name, Description, Price) values(1, 1, N'Chụp Phim Toàn Cảnh Panorex - Sọ Nghiêng Cephalo (Niềng răng chỉnh nha)', NULL , '150.000 - 200.000')
insert into Service (ClinicID, SpecialtyID, Name, Description, Price) values(1, 1, N'Chụp phim Cone Beam CT)', NULL , '300.000')
insert into Service (ClinicID, SpecialtyID, Name, Description, Price) values(2, 1, N'Chụp Phim Toàn Cảnh Panorex - Sọ Nghiêng Cephalo (Niềng răng chỉnh nha)', NULL , '190.000')
insert into Service (ClinicID, SpecialtyID, Name, Description, Price) values(2, 1, N'Chụp phim Cone Beam CT)', NULL , '320.000')
insert into Service (ClinicID, SpecialtyID, Name, Description, Price) values(3, 1, N'Chụp Phim Toàn Cảnh Panorex - Sọ Nghiêng Cephalo (Niềng răng chỉnh nha)', NULL , '180.000')
insert into Service (ClinicID, SpecialtyID, Name, Description, Price) values(3, 1, N'Chụp phim Cone Beam CT)', NULL , '290.000')
insert into Service (ClinicID, SpecialtyID, Name, Description, Price) values(2, 2, N'Mắc cài Inox thường', NULL , '35.000.000')
insert into Service (ClinicID, SpecialtyID, Name, Description, Price) values(2, 2, N'Mắc cài Inox tự đóng', NULL , '44.000.000')
insert into Service (ClinicID, SpecialtyID, Name, Description, Price) values(2, 2, N'Mắc cài sứ thường', NULL , '47.000.000')
insert into Service (ClinicID, SpecialtyID, Name, Description, Price) values(2, 2, N'Mắc sài sứ tự đóng', NULL , '57.000.000')
insert into Service (ClinicID, SpecialtyID, Name, Description, Price) values(1, 2, N'Mắc sài pha lê', NULL , '49.000.000')
insert into Service (ClinicID, SpecialtyID, Name, Description, Price) values(1, 2, N'Mắc cài pha lê tự đóng', NULL , '61.000.000')
insert into Service (ClinicID, SpecialtyID, Name, Description, Price) values(1, 2, N'Mắc cài cánh cam cải tiến', NULL , '50.000.000')

--TRANSACTION DATA
insert into [dbo].[Transaction](AppointmentID, Date, BankAccountNumber, BankName) values (3, '2024-06-07', '074653864731', N'Ngân Hàng VietcomBank') 
insert into [dbo].[Transaction](AppointmentID, Date, BankAccountNumber, BankName) values (6, '2024-06-04', '1564788253672', N'Ngân Hàng Shinhan') 
insert into [dbo].[Transaction](AppointmentID, Date, BankAccountNumber, BankName) values (7, '2024-06-05', '681236362533', N'Ngân Hàng TechcomBank') 
insert into [dbo].[Transaction](AppointmentID, Date, BankAccountNumber, BankName) values (8, '2024-06-06', '79123512367521', N'Ngân Hàng Quân Đội') 
insert into [dbo].[Transaction](AppointmentID, Date, BankAccountNumber, BankName) values (9, '2024-06-07', '8716263713', N'Ngân Hàng AgriBank') 

--REVIEW DATA
--insert into Review (DentistID, PatientID, Comment, Date) values()					NOT NECESSARY NOW
