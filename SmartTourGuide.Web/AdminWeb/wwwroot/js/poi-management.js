/* File: wwwroot/js/poi-management.js */

function confirmDelete(poiId, poiName) {
    Swal.fire({
        title: 'Xác nhận xóa?',
        text: `Bạn có chắc muốn xóa "${poiName}" không? Dữ liệu sẽ mất vĩnh viễn!`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#e74a3b', // Màu đỏ Danger đồng bộ với Dashboard
        cancelButtonColor: '#858796',  // Màu xám trung tính
        confirmButtonText: 'Đúng, xóa nó!',
        cancelButtonText: 'Hủy bỏ',
        reverseButtons: true, // Đưa nút Hủy sang trái nhìn tự nhiên hơn
        background: '#ffffff',
        borderRadius: '16px'
    }).then((result) => {
        if (result.isConfirmed) {
            // Chuyển hướng đến Action Delete trong Controller
           
            window.location.href = '/POI/Delete/' + poiId;
        }
    });
}