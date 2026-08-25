# ClaimCar MVC 5 - .NET Framework 4.6.1

Bản dựng lại dựa trên toàn bộ workbook `ĐẶC TẢ DỮ LIỆU VÀ NGHIỆP VỤ BỒI THƯỜNG XE CƠ GIỚI.xlsx`, gồm 3 sheet và 3 ảnh giao diện nhúng:

- `thong_tin_chung`: 14 trường dữ liệu, tiếp nhận hồ sơ, tra cứu hợp đồng, ngày xảy ra/ngày thông báo, số hồ sơ duy nhất, gán GĐV.
- `ton_that-chi_trả`: phạm vi bảo hiểm, tiền tổn thất/khấu trừ/bồi thường, mã nguyên nhân/hành vi, diễn biến-nguyên nhân-hậu quả, gara, đối tượng hưởng, truy đòi bên thứ ba.
- `bao-gia`: kiểu duyệt, giá trị thực tế, chi tiết phụ tùng, chi phí phụ tùng/sơn/công, chiết khấu, khấu hao/giảm trừ, tổng khách hàng chịu và tổng duyệt giá, checker.

## Công nghệ
- ASP.NET MVC 5, .NET Framework 4.6.1.
- Oracle.ManagedDataAccess (NuGet).
- Forms Authentication, tài khoản trong `Web.config`.
- Demo mode in-memory để chạy ngay trên IIS Express/IIS mà chưa cần DB.
- SDK plugin riêng `ClaimCar.Sdk` cho validation và hook Create/Update/Delete.

## Chạy ngay
Mở `ClaimCarMvc461.sln`, Restore NuGet, Ctrl+F5. Login `admin / 123456`.

Hoặc khởi động ứng dụng bằng Apache:

```bash
sudo apache2ctl configtest
sudo apache2ctl start
```

Kiểm tra ứng dụng đã phản hồi và Apache đang lắng nghe trên cổng `8080`:

```bash
curl -I http://127.0.0.1:8080
sudo ss -ltnp | grep 8080
```

Xem `RUN-IIS.md` để chạy IIS thật và `docs/SDK-EXTENSIONS.md` để viết plugin.



------------------------