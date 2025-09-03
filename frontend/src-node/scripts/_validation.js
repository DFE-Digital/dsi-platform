var forms = $("#content form");
//var inputs = forms.find('.form-group input');

forms.attr("novalidate", "novalidate");

/*
forms.on('submit', function () {
  inputs.each(function () {
    var result = this.checkValidity();
    if (!result) {
      showValidationMessage(this);
    } else {
      hideValidationMessage(this);
    }
  });
  return this.checkValidity();
});

inputs.on('blur', function () {
  var result = this.checkValidity();
  if (!result) {
    showValidationMessage(this);
  } else {
    hideValidationMessage(this);
  }
});

var showValidationMessage = function (field) {

  var field = field,
    $field = $(field),
    $parent = $field.parent();

  if ($parent.hasClass('show-password')) {
    $parent = $parent.parent();
  }

  if (!$parent.hasClass('govuk-form-group--error')) {

    var labelText = $field.data('label') || $("label[for='" + $field.attr('id') + "']").text(),
      errorMessageText = 'Enter a valid ' + labelText.toLowerCase(),
      errorMessage = $('<p class="govuk-body">')
        .html(errorMessageText)
        .prop('class', 'error-message')
        .prop('id', 'validation-' + slugify(labelText));

    $field.before(errorMessage);
    $parent.addClass('govuk-form-group--error');
    $field.attr({
      'aria-describedby' : 'validation-' + slugify(labelText),
      'aria-invalid': 'true'
    });
  }
}

var hideValidationMessage = function (field) {
  var field = field,
    $field = $(field),
    $parent = $field.parent();

  if ($parent.hasClass('show-password')) {
    $parent = $parent.parent();
  }

  $parent.removeClass('govuk-form-group--error');
  $parent.find('p.error-message').remove();
  $field.removeAttr('aria-describedby');
  $field.removeAttr('aria-invalid');
}

var slugify = function (text) {
  return text.toString().toLowerCase()
    .replace(/\s+/g, '-')
    .replace(/[^\w\-]+/g, '')
    .replace(/\-\-+/g, '-')
    .replace(/^-+/, '')
    .replace(/-+$/, '');
}

*/
