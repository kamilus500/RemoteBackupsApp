$(document).ready(function () {
    $("#create-backup-button").click(function () {
        var url = '/Backup/Create';

        $.get(url).done(function (data) {
            $('.modal-body').html(data);
        });
    })
})