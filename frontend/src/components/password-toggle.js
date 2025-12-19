const BUTTON_LABEL_SHOW = "Show";
const BUTTON_LABE_HIDE = "Hide";

function initPasswordToggleElement(element) {
  const toggleButton = document.createElement("button");
  toggleButton.type = "button";
  toggleButton.className = "app-password-toggle__show govuk-button govuk-button--secondary";
  toggleButton.textContent = BUTTON_LABEL_SHOW;
  toggleButton.addEventListener("click", togglePasswordVisibilityOnClick);
  element.appendChild(toggleButton);
}

function togglePasswordVisibilityOnClick(event) {
  const isPasswordVisible = event.target.textContent === BUTTON_LABE_HIDE;
  const isNowVisible = !isPasswordVisible;

  const passwordToggleComponent = event.target.closest(".app-password-toggle");
  const inputComponent = passwordToggleComponent.querySelector(".govuk-input");
  inputComponent.type = isNowVisible ? "text" : "password";

  event.target.textContent = isNowVisible ? BUTTON_LABE_HIDE : BUTTON_LABEL_SHOW;
}

document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll(".app-password-toggle").forEach(initPasswordToggleElement);
});
