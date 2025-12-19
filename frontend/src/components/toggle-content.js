function initToggleContentElement(element) {
  const targetElement = document.getElementById(element.dataset.toggleTarget);
  let testShouldShowTarget;

  if (element instanceof HTMLSelectElement) {
    testShouldShowTarget = () => element.value === element.dataset.toggleOption;
  }
  else if (element instanceof HTMLInputElement) {
    if (element.type == "checkbox") {
      testShouldShowTarget = () => element.checked;
    }
    else {
      testShouldShowTarget = () => element.value != "";
    }
  }

  targetElement.hidden = !testShouldShowTarget();

  element.addEventListener("change", () => {
    targetElement.hidden = !testShouldShowTarget();
    if (!targetElement.hidden && element.dataset.toggleClearInputs === "true") {
      targetElement.querySelectorAll("input, select, textarea").forEach(inputElement => {
        inputElement.value = "";

        // Simulate key event so that associated components (eg. govuk-character-count)
        // have an opportunity to update as needed.
        inputElement.dispatchEvent(new Event('keyup'));
      });
    }
  });
}

document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll(".app-toggle-content").forEach(initToggleContentElement);
});
