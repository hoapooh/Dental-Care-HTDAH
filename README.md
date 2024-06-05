 Hướng dẫn sử dụng trước khi dùng
===================================================================================
### 1. Clone file về máy / down file zip cũng được
- Mở SQL lên, chạy lệnh tạo DB trước : `CREATE DATABASE DentalClinicDb`
### 2. Tải các framework cần thiết về (Nếu có sẵn trên file tải về rồi thì không cần)
- Bên phải IDE chọn Dependencies -> Packages
- Chuột phải chọn Manage NuGet Package
- Chuyển qua mục Browse ở cái vừa hiện ra, tìm 4 thứ sau và nhấn Install lần lượt từng cái:
 > - Microsoft.EntityFrameworkCore<br>
  > - Microsoft.EntityFrameworkCore.Design<br>
  > - Microsoft.EntityFrameworkCore.SqlServer<br>
  > - Microsoft.EntityFrameworkCore.Tools
### 3. Kết nối DB
- Vào Solution chọn mục appsettings.json -> ConnectionStrings -> Sửa chuỗi đằng sau DBConnection
- Chuỗi lấy bằng cách nhấn View -> Server Explorer -> Chọn biểu tượng Connects to database trên Data Connections
- Chọn Microsoft SQL Server, ở dưới chọn Data Framework...SQL Server -> Nhập tên Server, chọn SQL Server Authentication
- Nhập account SQL và chọn ***TRUST SERVER CERTIFICATE***
- Nhấn Advanced để lấy chuỗi dán vào mục bên trên + OK để tạo kết nối

### 4. Xóa project Migration
- Project có tên Migration nằm phía bên phải trong IDE và xóa hết

### 5. Thêm Migration và cập nhật DB
***LƯU Ý CẤP ĐỘ ĐỊA NGỤC: KHÔNG ĐƯỢC MỞ DATABASE (SSMS) ĐỂ ĐĂNG NHẬP TRƯỚC KHI UPDATE-DATABASE***
- Phía trên nhấn Tools -> NuGet Package Manager -> Package Manager Console
 > - add-migration InitMigration<br>
  > - update-database
- Mỗi lần có thay đổi liên quan đến File DentalClinicDbContext thì Add Migration, không có lệnh Update
- Tạm thời chỉ cần 2 lệnh này để tạo Migration và tạo bẳng, attribute trên DB bằng Code First, những câu lệnh liên quan khác liên quan đến chỉnh sửa Migration và update lại DB vui lòng hỏi trực tiếp, xin cảm ơn!
