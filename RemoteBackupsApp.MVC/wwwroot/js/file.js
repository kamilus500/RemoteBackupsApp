$(document).ready(function () {
    $(".loading-file-btn").on("click", function () {
        $("#fileInput").trigger("click");
    });

    $("#fileInput").on("change", function () {
        if (this.files.length > 0) {
            var formData = new FormData($("#uploadForm")[0]);

            var token = $('input[name="__RequestVerificationToken"]').val();

            $("#loadingSpinner").removeClass("d-none");

            var baseUrl = window.location.origin;

            $.ajax({
                url: baseUrl + '/File/Upload',
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                headers: {
                    'RequestVerificationToken': token
                },
                success: function () {

                },
                error: function () {
                    alert("Wystąpił błąd przy przesyłaniu pliku.");
                },
                complete: function () {
                    $("#loadingSpinner").addClass("d-none");
                    window.location.reload();
                }
            });
        }
    });
});