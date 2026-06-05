**File giải thích**

- Ở trang chủ ấn sau khi ấn thêm sách là thẻ a có controller là Book, Action sẽ là Create. Gọi đến Controller BookController với Action là Create

- Ở Action Create gọi đến view cùng tên Create.cshtml và chạy giao diện đó

- Ở trong View Create đó, có 1 thẻ form có asp-controller là Book với Action là Create và hàm method là POST với tham số truyền vào sẽ là model Book.cs có 2 validation Required và Range ở Name và Price. Ở label chứa thẻ Name sẽ asp-for cho Name và Price cũng vậy. Thêm 2 thẻ span vào dưới mỗi label nhập liệu đó Validation tương ứng.

- Sau khi ấn Thêm sách ở Create.cshtml có thẻ button với type là submit sẽ gửi dữ liệu của form nhập liệu cho Action Create với tham số truyền vào sẽ là Book

- Ở Action Create đó nếu validation lỗi sẽ trả lại dữ liệu View(book) cho người dùng nhập lại. Nếu thành công lưu TempData cho việc thêm sách đó thành công để lưu vào View Create.cshtml đó

- Sau khi thành công quay lại trang tạo sách đó bằng RedirectToAction("Create")
