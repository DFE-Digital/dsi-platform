function toKebabCase(str) {
  return str.toLowerCase().replace(/\s+/g, "-");
}

function hideElement() {
  $(this).hide();
}

function uncheckCheckbox() {
  $(this).prop("checked", false);
}

function extractStartingNumberFromId(elementId) {
  const element = $(`#${elementId}`);
  const startingNumber = parseInt(element.prop("id").split("-").pop(), 10);
  return startingNumber;
}

function extractFirstPartFromId(id) {
  const parts = id.split("-");
  if (parts.length >= 2) {
    parts.pop();
    return `${parts.join("-")}-`;
  }
  return id;
}

function updateCount(categoryName) {
  const safeCategoryName = toKebabCase(categoryName);
  const checkedCount = $(
    `.${safeCategoryName} .govuk-checkboxes__input:checked`,
  ).length;

  $(`#${safeCategoryName}-count`).text(checkedCount);
}

function updateCategoryHeader(categoryName) {
  const safeCategoryName = toKebabCase(categoryName);
  const categoryHeader = $(`#${safeCategoryName}-text`);
  const isAnyCheckedInCategory =
    $(`input[id^='show-hide-${safeCategoryName}']:checked`).length > 0;
  categoryHeader.css("display", isAnyCheckedInCategory ? "block" : "none");
}

function updateNoFilterMessage() {
  const isAnyCheckedGlobal =
    $(".govuk-checkboxes__input.dfe-filter-input:checked").length > 0;
  $("#show-hide-selected-filters").css(
    "display",
    isAnyCheckedGlobal ? "none" : "block",
  );
}

function showMe(boxId, categoryName, checkedElementId) {
  const isChecked = $(`#${checkedElementId}`).prop("checked");
  $(`#${boxId}`).css("display", isChecked ? "block" : "none");

  updateCategoryHeader(categoryName);
  updateNoFilterMessage();
}

function filterAndPerformAction(
  firstPartOfListId,
  listIdStartingNumber,
  performAction,
) {
  const elementsToProcess = $(`[id^="${firstPartOfListId}"]`).filter(
    function () {
      const id = parseInt(this.id.split("-").pop(), 10);
      return !Number.isNaN(id) && id > listIdStartingNumber;
    },
  );

  elementsToProcess.each(performAction);
}

/* eslint-disable-next-line no-unused-vars */
function hideMe(listId, categoryName, checkboxId) {
  if (categoryName === "Last login") {
    const listIdStartingNumber = extractStartingNumberFromId(listId);

    const firstPartOfListId = extractFirstPartFromId(listId);
    const firstPartOfCheckboxId = extractFirstPartFromId(checkboxId);

    filterAndPerformAction(
      firstPartOfListId,
      listIdStartingNumber,
      hideElement,
    );
    filterAndPerformAction(
      firstPartOfCheckboxId,
      listIdStartingNumber,
      uncheckCheckbox,
    );

    const lastLoginCheckboxes = $(".last-login-checkboxes-input");
    lastLoginCheckboxes.prop("disabled", false);
  }
  $(`#${listId}`).hide();
  $(`#${checkboxId}`).prop("checked", false);

  updateCount(categoryName);
  updateCategoryHeader(categoryName);
  updateNoFilterMessage();
}

function initializeFilters() {
  $(".govuk-checkboxes__input.dfe-filter-input").each(function () {
    const isChecked = $(this).prop("checked");

    if (isChecked) {
      const checkboxId = $(this).attr("id");
      const parts = checkboxId.match(/show-hide-(.+)-(\d+)/);
      if (parts && parts.length === 3) {
        const categoryName = parts[1];
        const index = parts[2];
        const boxId = `${categoryName}-option-${index}`;

        if (categoryName === "last-login" && index === "6") {
          const lastLoginCheckboxes = $(".last-login-checkboxes-input");
          lastLoginCheckboxes.slice(0, index - 1).prop("disabled", true);
        }
        showMe(boxId, categoryName, checkboxId);
        updateCount(categoryName);
        updateCategoryHeader(categoryName);
        updateNoFilterMessage();
      }
    }
  });
}

function handleLastLoginCheckboxChange(checkboxes, lastLoginListTags) {
  checkboxes.on("change", function () {
    const index = checkboxes.index(this);
    let firstPartOfCheckboxId;
    if (this.checked) {
      switch (this.value) {
        case "never":
          lastLoginListTags.css("display", "none");
          checkboxes.slice(0, index).prop("checked", false);
          checkboxes.slice(0, index).prop("disabled", true);
          initializeFilters();
          break;

        default:
          checkboxes.slice(0, index).prop("checked", true);
          initializeFilters();
      }
    } else {
      firstPartOfCheckboxId = extractFirstPartFromId(this.id);
      filterAndPerformAction(firstPartOfCheckboxId, index + 1, uncheckCheckbox);
      checkboxes.slice(0, index).prop("disabled", false);
      filterAndPerformAction("last-login-option-", index + 1, hideElement);
      updateCount("last-login");
      updateCategoryHeader("last-login");
    }
  });
}

$(() => {
  initializeFilters();

  const lastLoginCheckboxes = $(".last-login-checkboxes-input");
  const lastLoginListTags = $(".last-login-option");

  lastLoginCheckboxes.on("click", () => {
    handleLastLoginCheckboxChange(lastLoginCheckboxes, lastLoginListTags);
  });
});
