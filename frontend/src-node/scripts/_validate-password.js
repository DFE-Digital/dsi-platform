function validatePassword() {
  var id = $(this).attr("id");
  var password = $(this).val();
  $(`#length-${id}`).removeClass(
    password.length >= 14 ? "icon-remove" : "icon-ok",
  );
  $(`#length-${id}`).addClass(
    password.length >= 14 ? "icon-ok" : "icon-remove",
  );
}

$(".password-input-check").each(function () {
  var $passwordInput = $(this);
  var id = $passwordInput.attr("id");
  var validatePasswordHtml = $(`<div id='validation-feedback-${id}'>`).append(
    `<div id='length-${id}' class='icon-remove'>be at least 14 characters</div>`,
  );

  $passwordInput.before(validatePasswordHtml);
});

$(".password-input-check").on("keyup", validatePassword);
