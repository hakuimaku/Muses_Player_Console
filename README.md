# Muses Player Console

**Muses Player Console** là ứng dụng nghe nhạc chạy trên console, được xây dựng bằng **C#** và **.NET 10**.  
Dự án mô phỏng một hệ thống quản lý và phát nhạc với các vai trò người dùng khác nhau như **Guest**, **User** và **Artist**.

Ứng dụng sử dụng **SQL Server** để lưu trữ dữ liệu và tích hợp **LibVLCSharp** để hỗ trợ phát nhạc từ đường dẫn âm thanh.

---

## Mục lục

- [Giới thiệu](#giới-thiệu)
- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Tính năng chính](#tính-năng-chính)
- [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
- [Cài đặt và chạy dự án](#cài-đặt-và-chạy-dự-án)
- [Cấu trúc dự án](#cấu-trúc-dự-án)
- [Cấu hình cơ sở dữ liệu](#cấu-hình-cơ-sở-dữ-liệu)
- [Lưu ý](#lưu-ý)
- [Hướng phát triển](#hướng-phát-triển)

---

## Giới thiệu

Dự án được phát triển nhằm mục đích học tập và thực hành các nội dung:

- Làm việc với SQL Server thông qua `Microsoft.Data.SqlClient`
- Tổ chức chương trình theo các lớp model và service
- Xử lý đăng nhập, phân quyền theo vai trò
- Quản lý bài hát, playlist, nghệ sĩ và danh mục
- Tích hợp thư viện phát nhạc bên ngoài
- Xây dựng giao diện console/TUI cho người dùng

---

## Công nghệ sử dụng

- **.NET 10**
- **C# 14**
- **SQL Server**
- **Microsoft.Data.SqlClient**
- **Terminal.Gui**
- **LibVLCSharp**
- **VLC / LibVLC Runtime**

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
- Tạo playlist mới (Chưa làm)
- Thêm bài hát vào playlist
- Xóa bài hát khỏi playlist
- Thay đổi thứ tự bài hát trong playlist (chưa làm)
- Xóa playlist
- Phát nhạc từ playlist hoặc bài hát được chọn
- Tạo tài khoản Artist (chưa làm)

### Artist

Người dùng có quyền nghệ sĩ có thể:

- Chuyển sang chế độ Artist (nếu user có tài khoản Artist)
- Xem danh sách bài hát của mình
- Tạo bài hát mới (chưa làm)
- Xóa bài hát (chưa làm)
- Cập nhật đường dẫn âm thanh của bài hát (chưa làm)

### Phát nhạc

Ứng dụng hỗ trợ:

- Phát nhạc từ `AudioURL`
- Tạm dừng / tiếp tục phát
- Chuyển bài
- Thoát trình phát
- Ghi nhận lượt nghe bài hát

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
