# Muses Player Console

**Muses Player Console** là ứng dụng nghe nhạc chạy trên terminal, được xây dựng bằng **C#** và **.NET 10**.  
Dự án mô phỏng một hệ thống quản lý và phát nhạc với các vai trò người dùng khác nhau như **Guest**, **User** và **Artist**.

Ứng dụng sử dụng **SQL Server** để lưu trữ dữ liệu và tích hợp **LibVLCSharp** để hỗ trợ phát nhạc từ đường dẫn âm thanh.

---

## Giới thiệu

Dự án được phát triển nhằm mục đích học tập và thực hành các nội dung:

- Làm việc với SQL Server thông qua `Microsoft.Data.SqlClient`
- Tổ chức chương trình theo các lớp model và service
- Xử lý đăng nhập, phân quyền theo vai trò
- Quản lý bài hát, playlist, nghệ sĩ và danh mục
- Tích hợp thư viện phát nhạc bên ngoài
- Xây dựng giao diện TUI cho người dùng

---

## Công nghệ sử dụng

- **.NET 10**
- **C# 14**
- **SQL Server**
- **Microsoft.Data.SqlClient** (v7.1.0)
- **Terminal.Gui** (v2.4.4)
- **LibVLCSharp** (v3.9.7.1)
- **VLC / LibVLC Runtime** (Cần cài đặt VLC trước)

---

## Tính năng chính

### Guest

Người dùng chưa đăng nhập có thể:

- Xem danh sách bài hát

### User

Người dùng sau khi đăng nhập có thể:

- Xem danh sách bài hát
- Tìm kiếm bài hát
- Xem danh sách nghệ sĩ
- Xem playlist cá nhân
- Tạo playlist mới
- Thêm bài hát vào playlist
- Xóa bài hát khỏi playlist
- Xóa playlist
- Phát nhạc từ playlist hoặc bài hát được chọn
- Thay đổi thứ tự bài hát trong playlist (chưa làm)
- Tạo tài khoản Artist

### Artist

Người dùng có quyền nghệ sĩ có thể:

- Chuyển sang chế độ Artist (nếu user có tài khoản Artist)
- Xem danh sách bài hát của mình
- Tạo bài hát mới
- Xóa bài hát
- Cập nhật thông tin bài hát (chưa làm)

### Phát nhạc

Ứng dụng hỗ trợ:

- Phát nhạc từ `AudioURL` (đường dẫn âm thanh online)
- Tạm dừng / tiếp tục phát
- Chuyển bài
- Thoát trình phát
- Ghi nhận lượt nghe bài hát (khi được 30 giây)
- Hiện tại chưa hỗ trợ phát nhạc xoay vòng, phát ngẫu nhiên, phát chỉ 1 bài,...

---

## Yêu cầu hệ thống

Trước khi chạy dự án, cần chuẩn bị:

### Phần mềm

- .NET SDK tương thích với `.NET 10`
- SQL Server
- VLC Media Player hoặc LibVLC runtime
- IDE khuyến nghị:
  - JetBrains Rider
  - Visual Studio
  - Visual Studio Code

### Database

Ứng dụng yêu cầu database SQL Server đã được tạo sẵn.

Thông tin database mặc định:

- Server: `localhost,1433`
- Database: `Muses_DB`

Cần đảm bảo database có đầy đủ:

- Bảng dữ liệu
- Stored procedure
- Function
- Tài khoản đăng nhập SQL Server
- Quyền truy cập phù hợp cho từng vai trò

---

## Cài đặt và chạy ứng dụng

1. Clone repository về máy:
   ```bash
   git clone https://github.com/hakuimaku/Muses_Player_Console.git
   ````
2. Mở dự án bằng IDE yêu thích.
3. Khôi phục các package NuGet cần thiết.
  ``` bash
  dotnet restore
  ```
4. Build dự án để kiểm tra lỗi.
  ``` bash
  dotnet build
  ```

5. Chạy ứng dụng:
  ``` bash
  dotnet run --project Muses_Player_Console
  ```

## Mô tả cấu trúc dự án
| File | Mô tả |
|---|---|
| `Program.cs` | Điểm khởi chạy ứng dụng |
| `MusesService.cs` | Xử lý nghiệp vụ chính, kết nối database và thao tác dữ liệu |
| `TuiDisplay.cs` | Xử lý giao diện terminal |
| `Song.cs` | Model đại diện cho bài hát |
| `Playlist.cs` | Model đại diện cho playlist |
| `Artist.cs` | Model đại diện cho nghệ sĩ |
| `User.cs` | Model đại diện cho người dùng |
| `Category.cs` | Model đại diện cho danh mục bài hát |

Ứng dụng phụ thuộc vào SQL Server và database `Muses_DB`.
Một số nhóm dữ liệu chính trong hệ thống gồm:

- Người dùng
- Nghệ sĩ
- Bài hát
- Playlist
- Danh mục
- Quan hệ giữa bài hát, nghệ sĩ, playlist và danh mục

Ngoài ra, project sử dụng các stored procedure/function để xử lý:

- Đăng nhập
- Kiểm tra vai trò artist
- Lấy danh sách bài hát
- Lấy bài hát theo playlist
- Tạo playlist
- Thêm/xóa bài hát khỏi playlist
- Đổi thứ tự bài hát trong playlist
- Tạo/xóa/cập nhật bài hát
- Tăng lượt nghe
- Sao lưu databaseMSSQL



---

## Lưu ý

- SQL Server cần chạy trước khi khởi động ứng dụng.
- Database `Muses_DB` phải tồn tại.
- Các stored procedure và function phải đúng tên và đúng tham số.
- VLC hoặc LibVLC runtime cần được cài đặt để chức năng phát nhạc hoạt động.
- Connection string cần phù hợp với môi trường máy đang chạy.
- Đây là dự án học tập, chưa tối ưu hoàn toàn cho môi trường production.

---

## Hướng phát triển

Một số hướng có thể mở rộng trong tương lai:

- Tách tầng dữ liệu theo Repository Pattern
- Sử dụng `appsettings.json` để quản lý cấu hình
- Mã hóa mật khẩu người dùng
- Thêm chức năng yêu thích bài hát
- Thêm lịch sử nghe nhạc
- Thêm tìm kiếm nâng cao theo nghệ sĩ, danh mục, ngày phát hành
- Thêm unit test và integration test
- Chuẩn hóa logging
- Cải thiện xử lý lỗi
- Phát triển thêm giao diện web hoặc desktop

---

## Trạng thái dự án

Dự án hiện phục vụ mục đích học tập, demo chức năng quản lý và phát nhạc bằng console.

---

## Tác giả

Dự án được phát triển trong khuôn khổ bài tập nhóm môn học.

