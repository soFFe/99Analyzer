// override jquery validate plugin defaults
$.validator.setDefaults({
    highlight: function (element) {
        $(element).addClass('is-invalid');
    },
    unhighlight: function (element) {
        $(element).removeClass('is-invalid');
    },
    errorElement: 'div',
    errorClass: 'invalid-feedback',
    errorPlacement: function (error, element) {
        if (element.parent('.input-group').length) {
            error.insertAfter(element.parent());
        } else if (element.prop('type') === 'checkbox') {
            error.appendTo(element.parent().parent().parent());
        } else if (element.prop('type') === 'radio') {
            error.appendTo(element.parent().parent().parent());
        } else {
            error.insertAfter(element);
        }
    },
});

/**
 * Add an error to the error container and let it fade in
 * @param {string} str
 */
function showError(str) {
    let alertHtml =
        '<div class="alert alert-danger alert-dismissible fade" role="alert">'
        + '<h4 class="alert-heading">Oops! Something went wrong.</h4>'
        + '<p class="error-text">' + str + '</p>'
        + '<hr>'
        + '<p>If you keep encountering this issue, please <a target="_blank" class="alert-link" href="https://github.com/soFFe/99Analyzer/issues">create an Issue on GitHub</a> or '
        + 'contact me <a class="alert-link" href="mailto:soffe@tunichtgut.org">via E-Mail.</p>'
        + '<button type="button" class="close" data-dismiss="alert" style="line-height: 1.2rem;"><span aria-hidden="true">&times;</span></button>'
        + '</div>';

    let $html = $($.parseHTML(alertHtml)[0]);
    $html.appendTo($('#errors-container'));
    setTimeout(function () {
        $html.addClass("show");
    }, 100);
}

$(function () {
    feather.replace();
    $('.alert').alert();
});