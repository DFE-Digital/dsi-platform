function initSelectionListElement(element) {
  const selectAllButtonElements = element.querySelectorAll(
    ".app-selection-list__action--select-all",
  );
  const selectNoneButtonElements = element.querySelectorAll(
    ".app-selection-list__action--select-none",
  );
  const checkboxElements = element.querySelectorAll(
    ".app-selection-list__select",
  );

  selectAllButtonElements.forEach((buttonElement) => {
    buttonElement.addEventListener("click", () =>
      checkboxElements.forEach(
        (checkboxElement) => (checkboxElement.checked = true),
      ),
    );
  });

  selectNoneButtonElements.forEach((buttonElement) => {
    buttonElement.addEventListener("click", () =>
      checkboxElements.forEach(
        (checkboxElement) => (checkboxElement.checked = false),
      ),
    );
  });
}

document.addEventListener("DOMContentLoaded", function () {
  document
    .querySelectorAll(".app-selection-list")
    .forEach(initSelectionListElement);
});
