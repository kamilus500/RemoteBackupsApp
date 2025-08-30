//$(document).ready(function () {
//    $('#fileInput').change(function () {
//        var formData = new FormData();
//        var file = $(this)[0].files[0];
//        if (!file) return;
//        debugger;
//        formData.append('file', file);

//        $.ajax({
//            url: '/File/Upload',
//            type: 'POST',
//            data: formData,
//            processData: false,
//            contentType: false,
//            success: function (data) {
                
//            },
//            error: function () {
                
//            }
//        });
//    });

//});