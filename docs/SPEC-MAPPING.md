# Đối chiếu đặc tả Excel -> source

## Sheet thong_tin_chung
14 trường được map trong `Models/Claim.cs`: mã/tên đơn vị, mã/tên khu vực, biển số, ngày nhập, ngày nhập Call, số hợp đồng, tình trạng, ngày quyết định, ngày xảy ra, ngày thông báo, số hồ sơ, mã GĐV. UI: `Views/Claim/Edit.cshtml`.

Rule triển khai: số hồ sơ duy nhất; ngày thông báo >= ngày xảy ra; bắt buộc các trường theo Excel; trạng thái dạng danh mục; gán GĐV; repository cho phép tra/lọc theo số hồ sơ, biển số, hợp đồng. Việc kiểm tra thời hạn bảo hiểm cần nguồn hợp đồng thực tế nên để ở extension/repository khi tích hợp hệ thống hợp đồng.

## Sheet ton_that-chi_trả
Map vào `LossPaymentViewModel`: loại hình, ngoại tệ, tiền bảo hiểm, tiền tổn thất, khấu trừ, tiền bồi thường, thuế; mã nguyên nhân/hành vi/khu vực/sự kiện; diễn biến/nguyên nhân/hậu quả; gara; thanh toán qua gara; đối tượng hưởng khác; bên thứ ba. UI: `Views/LossPayment/Edit.cshtml`.

## Sheet bao-gia
Map vào `QuoteViewModel`: kiểu duyệt, giá trị thực tế, ngày trình, lý do giảm trừ; danh sách phụ tùng, số lượng, phương án, loại PT, giá PT/sơn/công/tổng; tổng thay thế/sửa chữa/sơn/công/cẩu kéo; giảm giá liên kết; khấu hao, tỷ lệ tham gia, mức khấu trừ, giảm trừ bồi thường, chia sẻ rủi ro; tổng KH thanh toán, tổng duyệt giá, checker. UI: `Views/Quote/Edit.cshtml`.

## Giới hạn cố ý
Workbook mô tả nghiệp vụ nhưng không cung cấp schema hợp đồng, danh mục nguyên nhân/hành vi/khu vực/sự kiện, API gara, công thức chi tiết cho mọi chế tài. Source không bịa các tích hợp đó: Demo Mode dùng dữ liệu mẫu; SDK cung cấp điểm mở rộng để nối rule/API thật sau này.
