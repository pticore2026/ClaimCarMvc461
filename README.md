# ClaimCar MVC 5 - .NET Framework 4.6.1

Bản dựng lại dựa trên toàn bộ workbook `ĐẶC TẢ DỮ LIỆU VÀ NGHIỆP VỤ BỒI THƯỜNG XE CƠ GIỚI.xlsx`, gồm 3 sheet và 3 ảnh giao diện nhúng:

- `thong_tin_chung`: 14 trường dữ liệu, tiếp nhận hồ sơ, tra cứu hợp đồng, ngày xảy ra/ngày thông báo, số hồ sơ duy nhất, gán GĐV.
- `ton_that-chi_trả`: phạm vi bảo hiểm, tiền tổn thất/khấu trừ/bồi thường, mã nguyên nhân/hành vi, diễn biến-nguyên nhân-hậu quả, gara, đối tượng hưởng, truy đòi bên thứ ba.
- `bao-gia`: kiểu duyệt, giá trị thực tế, chi tiết phụ tùng, chi phí phụ tùng/sơn/công, chiết khấu, khấu hao/giảm trừ, tổng khách hàng chịu và tổng duyệt giá, checker.

## Công nghệ
- ASP.NET MVC 5, .NET Framework 4.6.1.
- SQLite, Oracle.ManagedDataAccess hoặc MySql.Data (NuGet).
- Forms Authentication, tài khoản trong `Web.config`.
- Demo mode in-memory để chạy ngay trên IIS Express/IIS mà chưa cần DB.
- SDK plugin riêng `ClaimCar.Sdk` cho validation và hook Create/Update/Delete.

## Chạy ngay
Mở `ClaimCarMvc461.sln`, Restore NuGet, Ctrl+F5. Login `admin / 123456`.

## Kết nối MySQL

1. Chạy `Database/03_mysql_schema.sql` bằng tài khoản quản trị MySQL để tạo database, user và schema.
2. Thay `YOUR_PASSWORD` trong connection string `MySqlClaimDb` tại `ClaimCar.Web/Web.config` bằng mật khẩu đã chọn.
3. Đổi `Data.Mode` từ `Demo` thành `MySql`, restore NuGet rồi khởi động ứng dụng.

Connection string mặc định kết nối tới MySQL tại `127.0.0.1:3306`, database `claim_car`, user `claim_car_app`. Không commit mật khẩu thật vào Git; nên dùng Web.config transform khi triển khai.

## Chạy trực tiếp với SQLite

Đổi `Data.Mode` thành `SQLite` trong `ClaimCar.Web/Web.config` rồi chạy ứng dụng. Repository tự tạo file `ClaimCar.Web/App_Data/claimcar.db` và bảng cần thiết ở lần kết nối đầu tiên; không cần cài database server hay chạy script thủ công. Tài khoản chạy IIS cần quyền ghi thư mục `App_Data`.

Xem `RUN-IIS.md` để chạy IIS thật và `docs/SDK-EXTENSIONS.md` để viết plugin.
