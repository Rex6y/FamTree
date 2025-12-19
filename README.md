# Hệ Thống Quản Lý Cây Gia Phả

## Tính năng chính

- Tạo và quản lý thông tin thành viên (tên, giới tính, ngày sinh, ảnh đại diện)
- Thiết lập quan hệ gia đình (cha, mẹ, vợ/chồng, con cái)
- Tìm kiếm thành viên theo tên
- Hiển thị cây gia phả trực quan với khả năng zoom
- Tính toán khoảng cách huyết thống giữa hai người
- Lọc và tìm kiếm trong cây gia phả
- Quản lý ảnh đại diện cho từng thành viên

## Yêu cầu hệ thống

- Hệ điều hành: Windows 10 trở lên
- .NET Framework 4.7.2 hoặc cao hơn
- Dung lượng: Tối thiểu 100MB trống
## Hướng dẫn sử dụng

**Các bước chạy:**

1. **Mở project:**
   - Mở file `FarmTree.sln` bằng Visual Studio
   
2. **Chạy ở chế độ Debug:**
   - Click vào nút `▶︎ WpfApp1` trên toolbar
   - Hoặc nhấn `F5`

3. **Build ứng dụng:**
   
   **Cách 1: Dùng Visual Studio**
   - Right-click vào project `WpfApp1` trong Solution Explorer
   - Chọn `Publish...`
   - Chọn `Folder` → `Next` → `Finish`
   - Click `Publish`
   - File .exe sẽ nằm trong thư mục publish được chỉ định
   
   **Cách 2: Dùng Terminal/Command Line**
   - Mở Terminal trong Visual Studio: `View → Terminal` hoặc nhấn `Ctrl + ~` (dấu backtick)
   - Nhập lệnh sau:
     ```bash
     dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true
     ```
   - File .exe sẽ được tạo tại:  
     `\FarmTree\WpfApp1\bin\Release\net8.0-windows\win-x64\publish\WpfApp1.exe`
   
4. **Chạy ứng dụng đã build:**
   - Vào thư mục publish
   - Double-click vào file `WpfApp1.exe`

### Cài đặt .NET Framework (nếu chưa có)

**Kiểm tra .NET đã cài chưa:**
1. Mở Command Prompt (CMD) với quyền Administrator
2. Gõ lệnh: `reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release`
3. Nếu có kết quả hiển thị → Đã cài .NET
4. Nếu báo lỗi "không tìm thấy" → Chưa cài .NET

**Cách 1: Cài bằng winget (Windows 10 1709 trở lên)**
1. Mở Command Prompt (CMD) với quyền Administrator
2. Gõ lệnh: `winget install Microsoft.DotNet.DesktopRuntime.8` & 'winget install Microsoft.DotNet.SDK.8'
3. Chờ quá trình cài đặt hoàn tất
4. Khởi động lại máy tính

**Cách 2: Tải trực tiếp từ Microsoft**
1. Truy cập: https://dotnet.microsoft.com/download/dotnet-framework
2. Chọn phiên bản .NET Framework 4.8
3. Tải file cài đặt và chạy
4. Làm theo hướng dẫn trên màn hình
5. Khởi động lại máy tính
