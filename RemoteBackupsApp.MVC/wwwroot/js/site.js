$(document).ready(function () {
    $("#create-backup-button").click(function () {
        var url = '/Backup/Create';

        $.get(url).done(function (data) {
            $('.modal-body').html(data);
        });
    })

    $("#register-button").click(function () {
        var url = '/Auth/Register';

        $.get(url).done(function (data) {
            $('.modal-body').html(data);
        });
    })

    $("#login-button").click(function () {
        var url = '/Auth/Login';

        $.get(url).done(function (data) {
            $('.modal-body').html(data);
        });
    })
})