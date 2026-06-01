# Muses Player Console

`Muses Player Console` là một ứng dụng console được xây dựng bằng C#/.NET, phục vụ mục đích học tập trong khuôn khổ bài tập nhóm.  
Dự án mô phỏng một hệ thống nghe nhạc và quản lý dữ liệu âm nhạc với các vai trò khác nhau như Guest, User và Artist.
Ứng dụng kết nối với SQL Server để thao tác dữ liệu về:
- người dùng
- nghệ sĩ
- bài hát
- playlist
- danh mục

Ngoài ra, ứng dụng hỗ trợ phát nhạc bằng `LibVLCSharp`.

---

## 1. Mục tiêu dự án

Dự án được phát triển nhằm:
- Thực hành lập trình C# console
- Làm quen với mô hình làm việc với SQL Server
- Sử dụng stored procedure và function trong database
- Tổ chức dữ liệu theo nhiều vai trò người dùng
- Tích hợp phát nhạc bằng thư viện ngoài

---

## 2. Công nghệ sử dụng

- `.NET 10`
- `C#`
- `SQL Server`
- `Microsoft.Data.SqlClient`
- `LibVLCSharp`
- `VLC / LibVLC` runtime

---

## 3. Yêu cầu trước khi chạy

Để chạy được dự án, cần chuẩn bị các thành phần sau:

### 3.1. Môi trường phát triển
- .NET SDK tương thích với project
- IDE như JetBrains Rider hoặc Visual Studio
- SQL Server đang hoạt động trên máy cục bộ

### 3.2. Cơ sở dữ liệu
Ứng dụng được thiết kế để kết nối tới database đặt tại `localhost`:

- Server: `localhost,1433`
- Database: `Muses_DB`

Cần đảm bảo:
- SQL Server đã được cài đặt và chạy
- Database `Muses_DB` đã được tạo
- Các bảng, stored procedure và function đã có sẵn
- Các tài khoản đăng nhập trong connection string tồn tại và có quyền phù hợp

### 3.3. Phụ thuộc phát nhạc
Dự án dùng `LibVLCSharp`, vì vậy cần:
- cài `VLC`
- có thư viện `libvlc`
- đảm bảo ứng dụng có thể tìm thấy thư viện VLC khi chạy

Nếu máy chưa có VLC hoặc thư viện tương ứng, chức năng phát nhạc có thể không hoạt động.

---

## 4. Cấu trúc project

### Solution
- `Muses_Player_Console.sln`

### Các file chính
- `Program.cs`  
  Điểm khởi chạy của ứng dụng, điều hướng luồng Guest / User / Artist.

- `MusesService.cs`  
  Lớp nghiệp vụ chính, chịu trách nhiệm:
    - kết nối database
    - gọi stored procedure / function
    - tải dữ liệu
    - xử lý phát nhạc
    - xử lý tương tác console

- `Song.cs`  
  Mô hình bài hát.

- `Playlist.cs`  
  Mô hình playlist.

- `Artist.cs`  
  Mô hình nghệ sĩ.

- `User.cs`  
  Mô hình người dùng.

- `Category.cs`  
  Mô hình danh mục.

- `ConsoleTableFormatter.cs`  
  Hỗ trợ căn chỉnh dữ liệu khi in ra console.

---

## 5. Chức năng của ứng dụng

### 5.1. Guest Mode
Người dùng chưa đăng nhập có thể:
- xem danh sách bài hát
- xem danh sách nghệ sĩ

### 5.2. User Mode
Người dùng đăng nhập có thể:
- xem toàn bộ bài hát
- xem playlist của mình
- xem nghệ sĩ
- chọn playlist để phát nhạc
- tạo playlist mới
- thêm bài hát vào playlist
- xoá bài hát khỏi playlist
- đổi thứ tự bài hát trong playlist
- xoá playlist
- chuyển sang Artist Mode nếu tài khoản có vai trò artist

### 5.3. Artist Mode
Người dùng có quyền artist có thể:
- xem danh sách bài hát của chính mình
- tạo bài hát mới
- xoá bài hát
- cập nhật URL phát nhạc của bài hát

### 5.4. Phát nhạc
Ứng dụng hỗ trợ:
- phát nhạc từ `AudioURL`
- pause / resume
- next
- quit player
- tăng `PlayCount` sau một khoảng thời gian phát

---

## 6. Các hàm tương tác với database

Tất cả logic truy vấn database chủ yếu nằm trong `MusesService.cs`.

### 6.1. Xác thực và phân quyền
- `Login(string username, string password)`  
  Gọi stored procedure: `dbo.sp_Auth_GetUserByLogin`

- `IsArtist()`  
  Gọi function: `dbo.fn_IsUserAnArtist`

### 6.2. Đọc dữ liệu
- `GetPlaylists()`  
  Lấy danh sách playlist theo `UserID`

- `GetAllSongs()`  
  Gọi stored procedure: `dbo.sp_GetAllSongs`

- `GetAllArtists()`  
  Lấy toàn bộ nghệ sĩ

- `GetArtistSongs(string artistId)`  
  Gọi function: `dbo.fn_GetSongsByArtistID`

- `GetSongsInPlaylist(string playlistId)`  
  Gọi stored procedure: `dbo.sp_GetSongsByPlaylistID`

- `GetAllCategories()`  
  Lấy toàn bộ danh mục

- `FindSong(string title)`  
  Gọi function: `dbo.fn_SearchSongsByTitle`

### 6.3. Ghi dữ liệu
- `IncrementPlayCount(string songId)`  
  Gọi stored procedure: `dbo.sp_IncrementSongPlayCount`

- `CreateNewPlaylist(string playlistName, string userId, bool isFavorite = false)`  
  Gọi stored procedure: `dbo.sp_AddNewPlaylist`

- `AddSongToPlaylist(string playlistId, string songId)`  
  Gọi stored procedure: `dbo.sp_AddSongToPlaylist`

- `RemoveSongFromPlaylist(string playlistId, string songId, string userId)`  
  Gọi stored procedure: `dbo.sp_RemoveSongFromPlaylist`

- `SwapSongsInPlaylist(string playlistId, string songId1, string songId2, string userId)`  
  Gọi stored procedure: `dbo.sp_SwapPlaylistSongOrder`

- `DeletePlaylist(string playlistId, string userId)`  
  Gọi stored procedure: `dbo.sp_DeletePlaylistByID`

- `CreateNewSong(...)`  
  Gọi stored procedure: `dbo.sp_AddNewSong`

- `DeleteSong(string songId, string artistId)`  
  Gọi stored procedure: `dbo.sp_DeleteSongByID`

- `UpdateSongAudioUrl(string songId, string newAudioUrl, string artistId)`  
  Gọi stored procedure: `dbo.sp_UpdateSongAudioURL`

---

## 7. Hướng dẫn build

### 7.1. Clone / mở project
Mở solution:
- `Muses_Player_Console.sln`

### 7.2. Khôi phục dependencies
Sau khi mở project trong IDE, thực hiện restore NuGet packages nếu cần.

Nếu dùng terminal, bạn có thể chạy:

```bash
dotnet restore
```

### 7.3. Build project
```bash
dotnet build
```



## 8. Hướng dẫn chạy
Sau khi build thành công, chạy ứng dụng bằng:
```bash
dotnet run
```

## 9. Lưu ý quan trọng

### 9.1. Database phải chạy ở máy local

Đây là bài tập nhóm học tập, nên project đang cấu hình để kết nối tới SQL Server cục bộ trên localhost.

Nếu database không chạy:

- không đăng nhập được
- không tải được dữ liệu
- các chức năng playlist / bài hát / artist sẽ không hoạt động

### 9.2. Stored procedure và function phải tồn tại

Project phụ thuộc vào các object trong database.

Nếu tên proc/function hoặc tên cột thay đổi, code có thể lỗi khi runtime.

### 9.3. VLC là bắt buộc cho chức năng phát nhạc

Muốn nghe nhạc trong ứng dụng, máy cần có:

- VLC
- libvlc
- thư viện tương thích với LibVLCSharp

### 9.4. Dữ liệu đăng nhập

Project đang sử dụng các connection string riêng cho:

- guest
- user
- artist

Cần đảm bảo các tài khoản này có sẵn trong môi trường database local.

### 9.5. Đây là project học tập

Mã nguồn được tối ưu cho mục đích:

- demo chức năng
- thực hành làm việc với DB
- minh hoạ phân quyền và phát nhạc

Vì vậy, một số phần có thể cần cải tiến thêm để phù hợp với chuẩn production.

## 10. Khả năng mở rộng
- Dự án có thể phát triển thêm theo các hướng sau:
- Tách lớp dữ liệu theo Repository Pattern
- Dùng appsettings.json thay vì hard-code connection string
- Mã hoá mật khẩu thay vì lưu plain text
- Bổ sung tìm kiếm nâng cao
- Thêm like/favorite cho bài hát
- Thêm lịch sử nghe nhạc
- Phát triển giao diện GUI hoặc Web API
- Thêm unit test và integration test
- Chuẩn hoá logging và validation đầu vào