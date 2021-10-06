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
        var url = "api/QrCodeGeneration/Unique"
    }
    else {
        var url = "api/QrCodeGeneration/Manual"
    }

    // AJAX Request
    $.ajax({
        type: "POST",
        url: url,
        data: form.serialize(),

        // To be done if AJAX Request is successful
        success: function (resp) {

            let html = '<h5 class="card-title">unique id:<span style="color:Orange">' + resp + '</span> </h5>'
            html += '<!--startprint-->'
            html += '<img class="img-thumbnail qrimg" src="/api/QrCodeGeneration/QrCode?text=' + resp + '" />';
            html += '<!--endprint-->'
            html += '<div class="text-center"><a class="btn btn-primary" href="javascript:doPrint(\'' + resp +  '\')">print</a>  <a class="btn btn-primary" href="/api/QrCodeGeneration/SaveQrCode?text=' + resp + '">save</a></div>'

            $(".QrCodeImg").html(html)

        },
        error: function (resp) {
            $("#checkinForm").trigger('reset');
            $('#qrFailure').modal('show');
            document.getElementById("firstname").disabled = false;
            document.getElementById("lastname").disabled = false;
            document.getElementById("contact").disabled = false;
            document.getElementById("unique").disabled = false;

        }

    });
});

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

function doPrint(resp) {

    bdhtml = window.document.body.innerHTML;
    ////sprnstr = "<!--startprint-->";
    ////eprnstr = "<!--endprint-->";
    ////prnhtml = bdhtml.substr(bdhtml.indexOf(sprnstr) + 17);
    ////prnhtml = prnhtml.substring(0, prnhtml.indexOf(eprnstr));
    window.document.body.innerHTML = '<img width="100%" src="/api/QrCodeGeneration/QrCode?text=' + resp + '" />';
    window.print();

    setTimeout(function () {
        window.document.body.innerHTML = bdhtml;
    }, 1 * 1000)
    
}
