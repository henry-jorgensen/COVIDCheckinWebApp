function disableManual(value) {
    document.getElementById("firstname").disabled = value != "";
    document.getElementById("lastname").disabled = value != "";
    document.getElementById("contact").disabled = value != "";
}

function disableUnique(value) {
    document.getElementById("unique").disabled = value != "";
}

$("#checkinForm").submit(function (e) {

    // Prevents normal submit of form
    e.preventDefault();

    // Grabs form data and URL for AJAX Request
    var form = $(this);

    // Determine URL for POST based on Checkin method.
    if ($('#unique').val()) {
        var url = "api/Checkin/Unique"
    }
    else {
        var url = "api/Checkin/Manual"
    }

    html5QrCode.stop().then((ignore) => {
        // QR Code scanning is stopped.
    }).catch((err) => {
        // Stop failed, handle it.
    });

    // AJAX Request
    $.ajax({
        type: "POST",
        url: url,
        data: form.serialize(),

        // To be done if AJAX Request is successful
        success: function (resp) {
            // Displaying name in modal.
            var modalName = document.getElementById("modalName");
            modalName.innerText = resp.toUpperCase();

            // Shows "manualSuccessful" modal
            $('#manualSuccessful').modal('show');

            // Clears form contents
            $("#checkinForm").trigger('reset');
            document.getElementById("firstname").disabled = false;
            document.getElementById("lastname").disabled = false;
            document.getElementById("contact").disabled = false;
            document.getElementById("unique").disabled = false;

            // Hides "manualSuccessful" modal after 5 seconds

            setTimeout(function () {
                $('#manualSuccessful').modal('hide')
                html5QrCode.start({ facingMode: "user" }, config, qrCodeSuccessCallback);
            }, 5000);
        },
        error: function (resp) {
            $("#checkinForm").trigger('reset');
            $('#uniqueFailure').modal('show');
            document.getElementById("firstname").disabled = false;
            document.getElementById("lastname").disabled = false;
            document.getElementById("contact").disabled = false;
            document.getElementById("unique").disabled = false;
            setTimeout(function () {
                $('#uniqueFailure').modal('hide')
                html5QrCode.start({ facingMode: "user" }, config, qrCodeSuccessCallback);
            }, 5000);
        }

    });
});

const html5QrCode = new Html5Qrcode("reader");
const qrCodeSuccessCallback = (decodedText) => {
    document.getElementById("firstname").disabled = true;
    document.getElementById("lastname").disabled = true;
    document.getElementById("contact").disabled = true;
    document.getElementById("unique").value = decodedText;
    document.getElementById("checkinSubmit").click();

};

const config = { fps: 10, qrbox: 250 };

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

html5QrCode.start({ facingMode: "user" }, config, qrCodeSuccessCallback);