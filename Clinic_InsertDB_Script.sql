USE [DentalClinicDb]
GO
	 
INSERT INTO Province (Name) VALUES
(N'An Giang'),(N'Bà Rịa - Vũng Tàu'),(N'Bắc Giang'),(N'Bắc Kạn'),
(N'Bạc Liêu'),(N'Bắc Ninh'),(N'Bến Tre'),(N'Bình Định'),(N'Bình Dương'),(N'Bình Phước'),(N'Bình Thuận'),
(N'Cà Mau'),(N'Cần Thơ'),(N'Cao Bằng'),(N'Đà Nẵng'),(N'Đắk Lắk'),(N'Đắk Nông'),(N'Điện Biên'),
(N'Đồng Nai'),(N'Đồng Tháp'),(N'Gia Lai'),(N'Hà Giang'),(N'Hà Nam'),(N'Hà Nội'),(N'Hà Tĩnh'),
(N'Hải Dương'),(N'Hải Phòng'),(N'Hậu Giang'),(N'Hòa Bình'),(N'Hưng Yên'),(N'Khánh Hòa'),(N'Kiên Giang'),
(N'Kon Tum'),(N'Lai Châu'),(N'Lâm Đồng'),(N'Lạng Sơn'),(N'Lào Cai'),(N'Long An'),(N'Nam Định'),
(N'Nghệ An'),(N'Ninh Bình'),(N'Ninh Thuận'),(N'Phú Thọ'),(N'Phú Yên'),(N'Quảng Bình'),(N'Quảng Nam'),
(N'Quảng Ngãi'),(N'Quảng Ninh'),(N'Quảng Trị'),(N'Sóc Trăng'),(N'Sơn La'),(N'Tây Ninh'),(N'Thái Bình'),
(N'Thái Nguyên'),(N'Thanh Hóa'),(N'Thừa Thiên - Huế'),(N'Tiền Giang'),(N'TP. Hồ Chí Minh'),(N'Trà Vinh'),
(N'Tuyên Quang'),(N'Vĩnh Long'),(N'Vĩnh Phúc'),(N'Yên Bái');

INSERT INTO Account (ProvinceID, Username, Password, Role, Gender, FirstName, LastName, Email, Phone, Image, Account_Status) VALUES 
(3, 'manager1', 'managerpass', 'Manager', 'Male', N'An', N'Nguyễn', 'annguyen@gmail.com', '0989962815', NULL, 'Active'),
(4, 'manager2', 'passmanager@', 'Manager', 'Female', N'Bình', N'Trần', 'binhtran@gmail.com', '0969654821', NULL, 'Active'),
(5, 'manager3', 'pass123', 'Manager', 'Male', N'Ngôn', N'Đặng', 'ngondangvt@gmail.com', '0868726812', NULL, 'Active'),
(2, 'admin', 'Adminpass', 'Admin', 'Female', N'Thy Thy', N'Đỗ', 'dothyadmin@gmail.com', '0987654321', NULL, 'Active'),
(1, 'nguyenvan', 'password123', 'Patient', 'Male', N'Nguyễn', N'Văn', 'nguyenvan@gmail.com', '0982647213', NULL, 'Active'),
(10, 'banhkhot', 'le101112', 'Patient', 'Male', N'Anh', N'Mai', 'maianhkt@gmail.com', '0962564821', NULL, 'Active'),
(15, 'member', '123123', 'Patient', 'Female', N'Thi Hoàng', N'Nguyễn', 'bnguyen@gmail.com', '0982768909', NULL, 'Active'),
(23, 'bobapop', 'trasua30k', 'Patient', 'Male', N'Kim', N'Ngô', 'kimngo@gmail.com', '0969922555', NULL, 'Active'),
(31, 'lichsu', '123', 'Patient', 'Female', N'Bố', N'Lữ', 'lubo@gmail.com', '0952415621', NULL, 'Active'),
(44, 'anhthu', '54321', 'Patient', 'Female', N'Anh Thư', N'Phạm', 'anhthupham@gmail.com', '0952361621', NULL, 'Active'),
(41, 'hoahong', 'bonghongmini@', 'Patient', 'Female', N'Huyền', N'Nguyễn', 'huyennguyen@gmail.com', '0956782312', NULL, 'Active'),
(22, 'minhky', 'minhminh2k2', 'Patient', 'Male', N'Minh Kỵ', N'Trương', 'truongminhky@gmail.com', '0984678312', NULL, 'Active'),
(41, 'lanhuong', 'abcabc', 'Patient', 'Female', N'Ngọc Lan Hương', N'Hoàng', 'hoangngoclanhuong@gmail.com', '0868256812', NULL, 'Active'),
(6, 'dentist', 'dentistpass', 'Dentist', 'Male', N'Thanh Bình', N'Trần', 'tranthanhbinh@gmail.com', '0987654321', 'TranThanhBinh_Dentist.png', 'Active'),
(51, 'huyhoang', 'thisZ', 'Dentist', 'Male', N'Huy Hoàng', N'Nguyễn', 'nguyenhuyhoanng@gmail.com', '0985234174', 'NguyenHuyHoang_Dentist.png', 'Active'),
(11, 'ngocquynh', 'pass', 'Dentist', 'Female', N'Ngọc Quỳnh', N'Nguyễn', 'nguyenngocquynh@gmail.com', '0987625221', 'NguyenNgocQuynh_Dentist.png', 'Active'),
(62, 'tuananh', 'qwerty', 'Dentist', 'Male', N'Tuấn Anh', N'Trần', 'trantuananh@gmail.com', '0984234831', 'TranTuanAnh_Dentist.png', 'Active'),
(36, 'phuhoa', '1', 'Dentist', 'Male', N'Phú Hòa', N'Nguyễn', 'nguyenphuhoa@gmail.com', '0984341112', 'NguyenPhuHoa_Dentist.png', 'Active'),
(55, 'thutrang', 'nhasi', 'Dentist', 'Female', N'Thu Trang', N'Dương', 'duongthutrang@gmail.com', '0987641321', 'DuongThuTrang_Dentist.png', 'Active'),
(50, 'tranhung', 'banh@123', 'Dentist', 'Male', N'Hưng', N'Trần', 'tranhung@gmail.com', '0989923241', 'TranHung_Dentist.png', 'Active'),
(57, 'tranthinhung', 'nhungvt321', 'Dentist', 'Female', N'Thị Nhung', N'Trần', 'tranthinhung@gmail.com', '0983612413', 'TranThiNhung_Dentist.png', 'Active');

--INSERT INTO TimeSlot...

INSERT INTO Degree (Name) VALUES 
	(N'Bác sĩ Răng Hàm Mặt'),
	(N'Cử nhân Răng Hàm Mặt'),
    (N'Chuyên khoa cấp I Răng Hàm Mặt'),
    (N'Chuyên khoa cấp II Răng Hàm Mặt'),
    (N'Thạc sĩ Răng Hàm Mặt'),
    (N'Tiến sĩ Răng Hàm Mặt'),
	--(N'Cử nhân Điều dưỡng Nha khoa'),
	--(N'Chứng nhận đào tạo liên tục'),
	(N'Cử nhân Kỹ thuật Phục hình Răng');

INSERT INTO Patient (Account_ID, MemberCard) VALUES
(5, null), (6, null), (7, null), (8, null), (9, null), (10, null), 
(11, null), (12, null), (13, null)


INSERT INTO Specialty (Name, Description, Image) VALUES
(N'Nha Khoa Tổng Quát', NULL, 'images\specialty\NKTQ.png'),
(N'Niềng Răng', NULL, 'images\specialty\NiengRang.png'),
(N'Bọc Răng Sứ', NULL, 'images\specialty\BocRangSu.png'),
(N'Trồng Răng Implant', NULL, 'images\specialty\TrongRang.png'),
(N'Nhổ Răng Khôn', NULL, 'images\specialty\NhoRangKhon.png'),
(N'Nha Khoa Trẻ Em', NULL, 'images\specialty\NKTE.png');


INSERT INTO Clinic (Province_ID, Manager_ID, Name, Address, Description, Image) VALUES
((SELECT ID FROM Province WHERE Name = N'Hà Nội'), 1, N'Phòng khám Nha khoa Đại Dương', N'200 Hoàng Quốc Việt, Cầu Giấy', N'Description', 'hanoi_clinic.jpg'),
((SELECT ID FROM Province WHERE Name = N'Hải Phòng'), 2, N'Nha khoa Lạc Việt', N'Số 107 Tô Hiệu, Lê Chân', N'Description', 'haiphong_clinic.jpg'),
((SELECT ID FROM Province WHERE Name = N'TP. Hồ Chí Minh'), 3, N'Hệ thống Nha khoa Hoa Hồng Phương Đông', N'Lầu 2, Trung tâm Thời trang - Vàng Bạc - Đá quý Bến Thành, số 30-36 Phan Bội Châu, Phường Bến Thành, Quận 1', N'Description', 'binhduong_clinic.jpg');
		--((SELECT ID FROM Province WHERE Name = N'TP. Hồ Chí Minh'), 2, N'Nha khoa Dr. Care', N'P3.SH08, Tòa nhà Park 3, Khu đô thị Vinhomes Central Park 208 Nguyễn Hữu Cảnh, Phường 22, Q. Bình Thạnh', N'Description', 'saigon_clinic.jpg'),
		--((SELECT ID FROM Province WHERE Name = N'TP. Hồ Chí Minh'), 3, N'Elite Dental - Trung tâm nha khoa chuyên sâu', N'75 Huỳnh Tịnh Của, Phường Võ Thị Sáu, Quận 3', N'Description', 'hcm_clinic.jpg'),
		--?? chx có mô tả, img lưu trên firebase hay here (Chx làm img), PHÒNG KHÁM NHIỀU CƠ SỞ THÌ LƯU SAO???

INSERT INTO Dentist (Account_ID, Clinic_ID, Degree_ID, Description) VALUES 
    (14, 1, 1, N'Bác sĩ nha khoa với kinh nghiệm 5 năm làm việc tại phòng khám Đại Dương.'),
    (16, 2, 2, N'Cử nhân răng hàm mặt với kỹ năng chuyên môn cao và tận tâm trong công việc.'),
    (15, 2, 3, N'Bác sĩ chuyên khoa RHM, đã từng tham gia nhiều khóa học nâng cao chuyên môn.'),
    (17, 1, 4, N'Chuyên gia RHM với nhiều năm kinh nghiệm trong việc điều trị các vấn đề về răng miệng.'),
    (18, 3, 5, N'Bác sĩ RHM đã có bằng thạc sĩ và nghiên cứu sâu về các phương pháp mới trong nha khoa.'),
    (19, 3, 6, N'Tiến sĩ nha khoa với nhiều năm nghiên cứu và công bố các bài báo chuyên ngành.'),
    (20, 1, 7, N'Bác sĩ nha khoa cử nhân kỹ thuật phục hình răng, có khả năng thực hiện các ca phức tạp.'),
    (21, 1, 1, N'Bác sĩ có hơn 25 năm kinh nghiệm về lĩnh vực Nha khoa, nội nha, trồng implant, răng sứ.');

INSERT INTO Dentist_Specialty (Specialty_ID, Dentist_ID) VALUES
(1, 1), (3, 1), (4, 1), (1, 2), (3, 2), (1, 3), (2, 3), (4, 3), (6, 3), (1, 4), 
(1, 5), (4, 5), (1, 6), (3, 6), (4, 6), (1, 7), (3, 7), (4, 7), (6, 7), (1, 8)

--INSERT INTO Service...

INSERT INTO Review (Dentist_ID, Patient_ID, Comment, Date)
VALUES
(1, 1, N'Tôi rất hài lòng với dịch vụ của bác sĩ. Rất chuyên nghiệp!', '2023-01-15'),
(2, 2, N'Tôi cảm thấy thoải mái khi đến đây. Nhân viên rất thân thiện.', '2023-02-20'),
(3, 3, N'Tôi đã từng đến nhiều nơi, nhưng đây là nơi tốt nhất.', '2023-03-18'),
(4, 4, N'Tôi sẽ giới thiệu bạn bè đến đây. Dịch vụ rất tốt.', '2023-05-10'),
(5, 5, N'Tôi đã có trải nghiệm tốt. Sẽ quay lại trong tương lai.', '2023-06-08')
	
--INSERT INTO Schedule...

--INSERT INTO Appointment...

--INSERT INTO Transactions...

--===========================================================================================
select * from Province
select * from Account
--select * from TimeSlot
select * from Degree
select * from Patient
select * from Specialty
select * from Clinic
select * from Dentist
select * from Dentist_Specialty
--select * from Service
select * from Review
--select * from Schedule
--select * from Appointment
--select * from Transactions
