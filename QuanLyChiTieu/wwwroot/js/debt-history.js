var netAmountChart;
function createChart(data) {
    // Tạo dữ liệu cho biểu đồ
    var labels = data.map(x => new Date(x.transactionDate).toLocaleDateString("vi-VN"));
    var netAmounts = data.map(x => x.netAmountAtThatTime);

    if (netAmountChart) {
        netAmountChart.destroy();
    }

    var ctx = document.getElementById('netAmountChart').getContext('2d');
    netAmountChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels.reverse(), // đảo ngược để từ cũ -> mới
            datasets: [{
                label: 'Số dư công nợ',
                data: netAmounts.reverse(),
                fill: false,
                borderColor: 'blue',
                tension: 0.2,
                pointBackgroundColor: function (ctx) {
                    return ctx.raw >= 0 ? "green" : "red";
                }
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: false }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return value.toLocaleString("vi-VN") + " đ";
                        }
                    }
                }
            }
        }
    });
}
$(document).ready(function () {
    var table = $('#transactionsTable').DataTable({
        "processing": true,
        "searching": true,
        "lengthMenu": [5, 10, 25, 50, 100],
        "pageLength": 25,
        "order": [],
        "ajax": {
            "url": "/Debt/GetDebtHistory",
            "type": "GET",
            "data": {
                partnerId: $('#PartnerId').val()
            },
            "dataSrc": function (json) {
                // Cập nhật số dư hiện tại
                var currentNetAmount = json.currentNetAmount || 0;
                var partmerName = $("#PartnerName").val(); 
                if (currentNetAmount == 0) {
                    $('#currentNetAmount').text('');
                    $('.alert').removeClass("alert-success alert-danger").addClass("alert-info");
                    $('.alert strong').text('Hiện tại không có công nợ với đối tác này.');
                }
                else if (currentNetAmount > 0) {
                    $('.alert strong').text(partmerName +' đang thiếu bạn: ');
                    $('#currentNetAmount').text(parseFloat(Math.abs(currentNetAmount)).toLocaleString('vi-VN') + ' đ');
                    $('.alert').addClass("alert-success");
                }
                else {
                    $('.alert strong').text('Bạn đang nợ: ');
                    $('#currentNetAmount').text(parseFloat(Math.abs(currentNetAmount)).toLocaleString('vi-VN') + ' đ');
                    $('.alert').addClass("alert-danger");
                }

                createChart(json.data);

                return json.data;
            }
        },
        "columns": [
            { "data": "type" },
            {
                "data": "transactionDate",
                "render": function (data) {
                    var date = new Date(data);
                    return date.toLocaleDateString("vi-VN");
                }
            },
            {
                "data": "amount",
                "render": function (data, type, row) {
                    var isNegative = row.inDebt && row.state === "Debt" || !row.inDebt && row.state === "PayDebt";
                    if (isNegative) {
                        data = -data;
                    }
                    var colorClass = isNegative ? "text-danger" : "text-success";
                    return `<span class="fw-bold ${colorClass}">${parseFloat(data).toLocaleString('vi-VN')} VNĐ</span>`;
                },
                "className": "text-end"
            },
            {
                "data": "netAmountAtThatTime",
                "render": function (data) {
                    var colorClass = data >= 0 ? "text-success" : "text-danger";
                    return `<span class="fw-bold ${colorClass}">${parseFloat(data).toLocaleString('vi-VN')} VNĐ</span>`;
                },
                "className": "text-end"
            },
            { "data": "description" },
            {
                "data": "id",
                "render": function (data, type, row) {
                    var editurl = row.state === "Debt" ? `/debt/editdebt/${data}` : `/debt/editpaydebt/${data}`
                    var deleteurl = row.state === "Debt" ? `/debt/deletedebt/${data}` : `/debt/deletepaydebt/${data}`;
                    return `<div class="text-end">
                                        <a href="${editurl}" class="btn btn-sm btn-warning me-1" title="Chỉnh sửa">
                                            <i class="bi bi-pencil-square"></i>
                                        </a>
                                        <a href="#" data-url="${deleteurl}" class="btn-delete btn btn-sm btn-danger" title="Xóa">
                                            <i class="bi bi-trash"></i>
                                        </a>
                                    </div>`;
                },
                "orderable": false,
                "className": "text-center"
            }
        ],
        "language": {
            "url": "//cdn.datatables.net/plug-ins/1.13.4/i18n/vi.json"
        }
    });


    // Xử lý xóa
    $(document).on('click', '.btn-delete', function () {
        var url = $(this).data('url');
        Swal.fire({
            title: 'Bạn có chắc chắn muốn xóa giao dịch này?',
            text: "Hành động này không thể hoàn tác!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy',
            customClass: {
                confirmButton: 'btn btn-danger me-2',
                cancelButton: 'btn btn-secondary'
            },
            buttonsStyling: false
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: url,
                    type: 'POST',
                    success: function (res) {
                        if (res.status) {
                            showToast('success', 'Xóa thành công!');
                            table.ajax.reload();
                        }
                        else {
                            Swal.fire('Lỗi!', 'Không thể xóa ' + res.message, 'error');
                        }
                    },
                    error: function () {
                        Swal.fire('Lỗi!', 'Không thể xóa.', 'error');
                    }
                });
            }
        });
    });
});
