# Hệ Thống Quản Lý Cây Gia Phả

## Giới thiệu

Ứng dụng quản lý cây gia phả là một phần mềm desktop được xây dựng bằng WPF giúp bạn dễ dàng tạo, quản lý và xem trực quan cây gia đình của mình. Ứng dụng cho phép lưu trữ thông tin chi tiết về từng thành viên, thiết lập các mối quan hệ gia đình, và hiển thị cây gia phả một cách trực quan.

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

### 1. Màn hình trang chủ

Khi khởi động ứng dụng, bạn sẽ thấy màn hình trang chủ với:
- Ô tìm kiếm lớn ở giữa màn hình
- Nút "Nhập thông tin người mới" màu xanh lá

**Chức năng:**
- **Tìm kiếm**: Nhập tên người cần tìm vào ô tìm kiếm, sau đó nhấn Enter hoặc click icon 🔍
- **Tạo người mới**: Click nút "Nhập thông tin người mới" để thêm thành viên mới vào hệ thống

### 2. Tạo thành viên mới

Sau khi click nút tạo người mới, bạn sẽ thấy form nhập liệu:

**Các bước thực hiện:**

1. **Chọn ảnh đại diện** (không bắt buộc):
   - Click vào vòng tròn có icon 📷
   - Chọn file ảnh từ máy tính (hỗ trợ .png, .jpg, .jpeg)
   - Nếu không chọn, hệ thống sẽ hiển thị chữ cái đầu tên trên nền màu

2. **Nhập thông tin bắt buộc:**
   - **Họ và tên**: Nhập đầy đủ họ tên (bắt buộc)
   - **Giới tính**: Chọn Nam hoặc Nữ (bắt buộc)
   - **Ngày sinh**: Chọn ngày tháng năm sinh (bắt buộc)

3. **Hoàn tất**: Click nút "Tạo thành viên" màu xanh lá

**Lưu ý:**
- Tất cả thông tin đánh dấu * là bắt buộc
- Bên phải màn hình hiển thị danh sách tất cả thành viên hiện có để tham khảo
- Click "Back" để quay về trang chủ

### 3. Tìm kiếm thành viên

**Cách 1: Từ trang chủ**
- Nhập tên vào ô tìm kiếm
- Nhấn Enter hoặc click icon 🔍

**Cách 2: Trong trang tìm kiếm**
- Nhập tên mới vào ô tìm kiếm
- Nhấn Enter hoặc click icon 🔍 để tìm lại

**Kết quả tìm kiếm:**
- Hiển thị danh sách các thành viên có tên khớp
- Mỗi thành viên hiển thị: avatar, tên, ngày sinh
- Click nút "Xem" để xem cây gia phả của người đó

### 4. Xem và chỉnh sửa thông tin cá nhân

Sau khi click "Xem" trên một thành viên, bạn sẽ vào trang Profile với đầy đủ thông tin.

#### 4.1 Quản lý ảnh đại diện

- **Thay ảnh**: Click nút "Thay ảnh" màu xanh lá → Chọn file ảnh mới
- **Xóa ảnh**: Click nút "Xóa ảnh" màu đỏ → Ảnh sẽ bị xóa, hiển thị chữ cái đầu tên

#### 4.2 Thêm bố

1. Click nút "Thêm bố" màu xanh dương (chỉ hiện khi chưa có bố)
2. Cửa sổ chọn người sẽ hiện ra, chỉ hiển thị:
   - Nam giới
   - Sinh trước người hiện tại (đảm bảo bố lớn tuổi hơn con)
   - Chưa có quan hệ với người này
3. Có thể tìm kiếm theo tên trong cửa sổ chọn
4. Chọn một người và click "Chọn" màu xanh lá
5. Click "Hủy" nếu không muốn thêm

**Xóa bố:**
- Click nút "Xóa" màu đỏ bên cạnh tên bố
- Xác nhận trong hộp thoại
- Chỉ xóa quan hệ, không xóa người khỏi hệ thống

#### 4.3 Thêm mẹ

1. Click nút "Thêm mẹ" màu xanh dương (chỉ hiện khi chưa có mẹ)
2. Cửa sổ chọn người sẽ hiện ra, chỉ hiển thị:
   - Nữ giới
   - Sinh trước người hiện tại
   - Chưa có quan hệ với người này
3. Tìm kiếm và chọn người phù hợp
4. Click "Chọn" để xác nhận

**Xóa mẹ:**
- Click nút "Xóa" màu đỏ bên cạnh tên mẹ
- Xác nhận trong hộp thoại

#### 4.4 Thêm vợ/chồng

1. Click nút "Thêm vợ/chồng" màu xanh dương
2. Cửa sổ chọn người sẽ hiện ra, chỉ hiển thị:
   - Giới tính ngược lại (nam chọn nữ, nữ chọn nam)
   - Chưa có vợ/chồng
   - Chưa có quan hệ với người này
3. Chọn người phù hợp và click "Chọn"

**Lưu ý:**
- Mỗi người chỉ có thể có một vợ/chồng duy nhất
- Nếu đã có vợ/chồng, nút "Thêm vợ/chồng" sẽ không hoạt động

#### 4.5 Thêm con

1. Click nút "+ Thêm con" màu xanh lá
2. Cửa sổ chọn người sẽ hiện ra, chỉ hiển thị:
   - Người sinh sau người hiện tại (hoặc sau vợ/chồng nếu có)
   - Chưa có bố (nếu người hiện tại là nam) hoặc chưa có mẹ (nếu là nữ)
   - Chưa có quan hệ với người này
3. Chọn người và click "Chọn"

**Quản lý danh sách con:**
- Danh sách con hiển thị dạng cuộn nếu có nhiều con
- Mỗi con hiển thị: avatar, tên, nút xóa "✕" màu đỏ
- **Double-click** vào tên con để xem chi tiết của con đó
- Click nút "✕" để xóa con khỏi danh sách (chỉ xóa quan hệ, không xóa người)

### 5. Xem cây gia phả

#### 5.1 Truy cập cây gia phả

**Cách 1:** Từ trang tìm kiếm, click "Xem" trên một thành viên

**Cách 2:** Từ trang Profile, click nút "Xem cây gia phả" màu tím

#### 5.2 Các tính năng trên cây gia phả

**Thanh công cụ phía trên:**

1. **Nút Back**: Quay về trang trước
2. **Tên người đang xem**: "Đang xem: [Tên người]"
3. **Nút Zoom:**
   - **+**: Phóng to cây
   - **−**: Thu nhỏ cây
   - **⟲**: Reset về kích thước ban đầu
4. **Nút "Khoảng cách huyết thống"**: 
   - Click để bật chế độ tính khoảng cách
   - Click lại để tắt chế độ
5. **Nút 🔍**: Mở panel lọc và tìm kiếm

**Hiển thị cây:**
- Thành viên nam có viền xanh dương
- Thành viên nữ có viền hồng
- Người đang xem có viền vàng
- Các đường nối thể hiện quan hệ gia đình:
  - Đường ngang nối vợ chồng
  - Đường dọc nối cha mẹ với con

#### 5.3 Tính khoảng cách huyết thống

**Cách sử dụng:**

1. Click nút "Khoảng cách huyết thống" màu cam (nút sẽ chuyển sang màu đỏ)
2. Click chọn **người thứ nhất** → viền chuyển sang màu xanh lá
3. Click chọn **người thứ hai** → viền cũng chuyển sang màu xanh lá
4. Kết quả hiển thị: "Khoảng cách: [số]" hoặc "Khác dòng máu"
5. Click lại nút để thoát chế độ tính khoảng cách

**Ý nghĩa khoảng cách:**
- **0**: Cùng một người
- **1**: Cha/mẹ - con, anh/chị/em ruột
- **2**: Ông/bà - cháu, chú/bác/cô/dì - cháu
- **3**: Cụ - chắt, ...
- **Khác dòng máu**: Hai người không có quan hệ huyết thống

#### 5.4 Lọc và tìm kiếm trong cây

1. Click nút 🔍 ở góc phải → Panel tìm kiếm hiện ra
2. **Tìm theo tên**: Nhập tên vào ô "Họ và tên"
3. **Lọc theo giới tính**: Chọn Nam, Nữ, hoặc không chọn (cả hai)
4. **Lọc theo thế hệ**: Chọn thế hệ từ dropdown
5. Nhấn **Enter** để tìm kiếm
6. Kết quả hiển thị trong panel bên phải
7. Click "Xem" trên kết quả để chuyển sang xem cây của người đó

**Lưu ý:**
- Có thể bỏ chọn giới tính bằng cách click lại vào radio button đã chọn
- Click lại nút 🔍 để đóng panel tìm kiếm

#### 5.5 Thao tác trên cây

**Click vào một thành viên:**
- Nếu **KHÔNG** trong chế độ tính khoảng cách → Chuyển sang xem cây của người đó
- Nếu **ĐANG** trong chế độ tính khoảng cách → Chọn người đó để tính khoảng cách

**Double-click vào thành viên:**
- Mở trang Profile chi tiết của người đó

**Cuộn chuột:**
- Cuộn lên/xuống để xem các thế hệ khác
- Cuộn ngang để xem các nhánh gia đình rộng

### 6. Xóa thành viên

**Cảnh báo:** Thao tác này **KHÔNG THỂ HOÀN LẠI**!

1. Vào trang Profile của người cần xóa
2. Kéo xuống phần "Thao tác khác"
3. Click nút "Xóa thành viên" màu đỏ
4. Đọc kỹ cảnh báo trong hộp thoại
5. Click "Yes" để xác nhận xóa hoặc "No" để hủy

**Hệ quả khi xóa:**
- Người đó sẽ bị xóa hoàn toàn khỏi hệ thống
- Tất cả quan hệ gia đình liên quan đều bị xóa
- Không thể khôi phục được

## Một số lưu ý quan trọng

### Về quan hệ gia đình

1. **Bố/Mẹ:**
   - Một người chỉ có tối đa một bố và một mẹ
   - Bố/mẹ phải sinh trước con
   - Có thể xóa bố/mẹ mà không ảnh hưởng đến người khác

2. **Vợ/Chồng:**
   - Một người chỉ có tối đa một vợ/chồng
   - Quan hệ vợ chồng là hai chiều (A là vợ/chồng của B thì B cũng là vợ/chồng của A)
   - Phải khác giới tính

3. **Con:**
   - Một người có thể có nhiều con
   - Con phải sinh sau bố/mẹ (hoặc sau người sinh sau trong cặp vợ chồng)
   - Khi xóa con, chỉ xóa quan hệ, không xóa người đó khỏi hệ thống

### Về ảnh đại diện

- Hỗ trợ các định dạng: .jpg, .jpeg, .png, .bmp, .gif
- Ảnh sẽ được hiển thị dạng hình tròn
- Nếu không có ảnh, hiển thị chữ cái đầu tên trên nền màu:
  - Nam: nền xanh dương (#2196F3)
  - Nữ: nền hồng (#E91E63)

### Về thế hệ

- Thế hệ được tính tự động dựa trên vị trí trong cây gia phả
- Người ở cùng hàng ngang là cùng thế hệ
- Thế hệ được đánh số từ 0, 1, 2, ...

## Xử lý sự cố thường gặp

### Không tìm thấy người cần tìm

**Nguyên nhân:**
- Tên nhập sai chính tả
- Người chưa được thêm vào hệ thống

**Giải pháp:**
- Kiểm tra lại chính tả
- Tìm kiếm bằng một phần tên
- Tạo người mới nếu chưa có

### Không thể thêm bố/mẹ/vợ/chồng

**Nguyên nhân:**
- Người đó đã có bố/mẹ/vợ/chồng rồi
- Không có người phù hợp trong hệ thống (sai giới tính, sai độ tuổi)

**Giải pháp:**
- Xóa quan hệ cũ trước (nếu cần)
- Tạo người mới phù hợp vào hệ thống
- Kiểm tra lại giới tính và ngày sinh

### Cây gia phả hiển thị lỗi hoặc thiếu người

**Nguyên nhân:**
- Dữ liệu quan hệ chưa đầy đủ
- Người chưa được liên kết vào cây

**Giải pháp:**
- Kiểm tra và bổ sung quan hệ gia đình cho từng người
- Đảm bảo mỗi người đều có ít nhất một quan hệ với người khác

### Ảnh không hiển thị

**Nguyên nhân:**
- File ảnh bị hỏng
- Định dạng không được hỗ trợ

**Giải pháp:**
- Xóa ảnh và thêm lại
- Chuyển ảnh sang định dạng .png hoặc .jpg
- Sử dụng ảnh có dung lượng nhỏ hơn

## Mẹo sử dụng hiệu quả

1. **Tạo từ trên xuống:**
   - Bắt đầu từ người cao tuổi nhất (ông bà, cụ)
   - Sau đó thêm con cháu theo từng thế hệ
   - Cách này giúp dễ quản lý quan hệ hơn

2. **Sử dụng ảnh đại diện:**
   - Thêm ảnh giúp dễ nhận diện
   - Sử dụng ảnh rõ mặt, kích thước vừa phải

3. **Kiểm tra thông tin:**
   - Thường xuyên xem lại cây gia phả
   - Kiểm tra quan hệ đã đúng chưa
   - Sửa ngay nếu phát hiện sai sót

4. **Backup dữ liệu:**
   - Thường xuyên sao lưu file database
   - Lưu nhiều bản backup ở các nơi khác nhau

5. **Double-click để xem nhanh:**
   - Trong danh sách con, double-click để xem chi tiết nhanh
   - Trong cây gia phả, double-click vào thành viên để xem Profile

## Câu hỏi thường gặp (FAQ)

**Q: Có thể thêm nhiều vợ/chồng không?**

A: Không. Hệ thống chỉ hỗ trợ mỗi người có một vợ/chồng duy nhất.

**Q: Xóa người có xóa hết thông tin liên quan không?**

A: Có. Khi xóa một người, tất cả quan hệ gia đình của họ đều bị xóa. Nhưng người thân của họ vẫn còn trong hệ thống.

**Q: Có thể thay đổi ngày sinh sau khi tạo không?**

A: Hiện tại chưa hỗ trợ. Bạn cần xóa người đó và tạo lại với thông tin đúng.

**Q: Dữ liệu được lưu ở đâu?**

A: Dữ liệu được lưu trong database của ứng dụng, thường ở thư mục cài đặt hoặc thư mục AppData.

**Q: Có giới hạn số lượng thành viên không?**

A: Không có giới hạn cứng, nhưng nên giữ dưới 1000 người để đảm bảo hiệu năng tốt.

**Q: Làm sao để in cây gia phả?**

A: Hiện tại chưa hỗ trợ tính năng in trực tiếp. Bạn có thể chụp màn hình (Print Screen) để lưu ảnh.

---

**Phiên bản:** 1.0.0  
**Cập nhật:** Tháng 12/2024  
**Bản quyền:** © 2024 Family Tree Management Application
