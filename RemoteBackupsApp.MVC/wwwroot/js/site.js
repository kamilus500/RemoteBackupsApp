
$(document).ready(function () {
    if (window.innerWidth < 768) {
        document.querySelector(".table-file-import")?.remove();
    } else {
        document.querySelector(".d-md-none")?.remove();
    }

    let connection = new signalR.HubConnectionBuilder()
        .withUrl("/uploadHub")
        .build();

    $(".progress-bar ").each(function () {
        const pct = $(this).data("pct");
        if (pct !== undefined) {
            $(this).css("width", pct + "%");
        }
    });

    connection.on("UploadSuccess", function (data) {
        if (data.isCompleted === true) {
            showPopup(`<i class="bi bi-info-circle-fill text-primary"></i> Plik ${data.fileName} załadowany pomyślnie`);
        } else {
            showPopup(`<i class="bi bi-info-circle-fill text-primary"></i> Plik ${data.fileName} nie został załadowany pomyślnie`);
        }
    })

    connection.on("ProgressUpdated", function (data) {
        let $progressBar = $("#process-" + data.processId);
        if ($progressBar.length === 0) return;

        $progressBar.attr("data-pct", data.percent);
        $progressBar[0].style.width = data.percent + "%";

        $progressBar.attr("aria-valuenow", data.percent);
        $progressBar.text(data.percent + "%");

        $progressBar.removeClass("bg-success bg-info bg-danger");
        if (data.status === "Completed") {
            $progressBar.addClass("bg-success");

            let $progressStatus = $("#progress-status-" + data.processId);
            $progressStatus.removeClass("bg-secondary bg-primary").addClass("bg-success");
            $progressStatus.text("Completed");

            let $progressCompleteDate = $("#progress-completed-" + data.processId);
            let completedAt = new Date(data.date);
            $progressCompleteDate.text(completedAt.toISOString().slice(0, 16).replace("T", " "));
        }
        else if (data.status === "Failed") {
            $progressBar.addClass("bg-danger");
            $progressBar.text("Failed");        }
        else {
            $progressBar.addClass("bg-info");
        }
    });


    connection.start()
        .then(() => console.log("SignalR connected"))
        .catch(err => console.error(err));
})

function showPopup(message, duration = 4000) {
    let popup = $("#popup-message");
    popup.html(message);
    popup.stop(true, true).fadeIn(400).delay(duration).fadeOut(600);
}