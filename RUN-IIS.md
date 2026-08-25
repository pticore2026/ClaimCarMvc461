# Chạy demo trên IIS Express / IIS

## Yêu cầu
- Windows 10/11 hoặc Windows Server.
- Visual Studio 2019/2022 có workload **ASP.NET and web development**.
- Cài **.NET Framework 4.6.1 Developer Pack / Targeting Pack** nếu VS báo thiếu reference assemblies.
- IIS thật: bật IIS > World Wide Web Services > Application Development Features > ASP.NET 4.x, .NET Extensibility 4.x, ISAPI Extensions, ISAPI Filters.

## IIS Express nhanh nhất
1. Giải nén source, mở `ClaimCarMvc461.sln`.
2. Chuột phải Solution > Restore NuGet Packages.
3. Set `ClaimCar.Web` là Startup Project.
4. Chạy IIS Express bằng Ctrl+F5.
5. Login: `admin / 123456`.
6. `Data.Mode=Demo` nên không cần Oracle.

## IIS thật
1. Build Release hoặc Publish bằng profile `FolderProfile`.
2. Tạo Application Pool: .NET CLR v4.0, Integrated, Enable 32-Bit Applications=False (khuyến nghị).
3. Tạo site/application trỏ vào thư mục publish.
4. Cấp Read/Execute cho identity của Application Pool.
5. Nếu dùng Oracle, đổi `Data.Mode` thành `Oracle` và điền `OracleClaimDb`.

## Font/tiếng Việt
Toàn bộ Razor/CSS/config được lưu UTF-8. `Web.config` đặt `requestEncoding`, `responseEncoding`, `fileEncoding` = `utf-8`; layout có `<meta charset="utf-8">`. Không lưu source bằng ANSI/Windows-1252 vì đó là nguồn gốc của chuỗi kiểu `HÃ» sÆ¡`.
