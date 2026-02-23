const RESPONSE_TYPE_CODE = "code";
const RESPONSE_TYPE_ID_TOKEN = "id_token";

const FLOW_TYPE_IMPLICIT = "implicit";
const FLOW_TYPE_AUTHORIZATION = "authorization";
const FLOW_TYPE_HYBRID = "hybrid";

function updateSections() {
  const selectedTypes = [];

  if ($("#response_types-code").is(":checked")) {
    selectedTypes.push(RESPONSE_TYPE_CODE);
  }
  if ($("#response_types-id_token").is(":checked")) {
    selectedTypes.push(RESPONSE_TYPE_ID_TOKEN);
  }

  // 1. DETERMINE FLOW TYPE
  let flowType = "";
  let warningMessage = "";

  if (selectedTypes.length === 0) {
    flowType = "";
  }
  else if (selectedTypes.length === 1 && selectedTypes.includes(RESPONSE_TYPE_CODE)) {
    flowType = FLOW_TYPE_AUTHORIZATION;
    warningMessage = `
      <div class="govuk-warning-text govuk-!-margin-top-5 govuk-!-margin-bottom-0" id="warning-response-types">
          <span class="govuk-warning-text__icon" aria-hidden="true">!</span>
          <strong class="govuk-warning-text__text">
              <span class="govuk-warning-text__assistive">Warning</span>
              You have selected code as your response type. This means you are using Authorization Code Flow.
          </strong>
      </div>
    `;
  }
  else if (selectedTypes.length === 2) {
    // Hybrid
    flowType = FLOW_TYPE_HYBRID;
    warningMessage = `
      <div class="govuk-warning-text govuk-!-margin-top-5 govuk-!-margin-bottom-0" id="warning-response-types">
          <span class="govuk-warning-text__icon" aria-hidden="true">!</span>
          <strong class="govuk-warning-text__text">
              <span class="govuk-warning-text__assistive">Warning</span>
              You have selected code and id_token. This means Hybrid Flow is used.
          </strong>
      </div>
    `;
  }
  else {
    // id_token only (implicit)
    flowType = FLOW_TYPE_IMPLICIT;
  }

  // 2. UPDATE WARNING DISPLAY
  $("#warning-response-types").remove();

  if (warningMessage) {
    $("#response_types-fieldset").append(warningMessage);
  }

  // 3. ENABLE/DISABLE FIELDS BASED ON FLOW TYPE
  const shouldEnable = (
    flowType === FLOW_TYPE_AUTHORIZATION ||
    flowType === FLOW_TYPE_HYBRID
  );

  if (shouldEnable) {
    $("#refresh_token-wrapper :input, #clientSecret-wrapper :input, #tokenEndpointAuthMethod-wrapper select")
      .prop("disabled", false);

    $("#refresh_token-wrapper, #clientSecret-wrapper, #tokenEndpointAuthMethod-wrapper")
      .slideDown(500);
  } else {
    $("#refresh_token-wrapper, #clientSecret-wrapper, #tokenEndpointAuthMethod-wrapper")
      .slideUp(500, () => {

        $("#refresh_token-wrapper :input, #clientSecret-wrapper :input, #tokenEndpointAuthMethod-wrapper select")
          .prop("disabled", true);

      });
  }
}

$(() => {
  const createServiceConfigUrlSections = (
    sectionId,
    formGroupSelector,
    labelText,
  ) => {
    const addButton = $(`#${sectionId}-add`);
    const formGroup = $(`${formGroupSelector}`);

    addButton.on("click", function addInputFIeld() {
      let counter = parseInt(formGroup.data(`${sectionId}-counter`), 10);
      const newInputId = `${sectionId}-${counter}`;
      const newElement = `
      <div class="govuk-body dfe-flex-container" id="${sectionId}-input-group-${counter}">
        <label for="${newInputId}" class="govuk-label govuk-label--s govuk-visually-hidden">
          ${labelText}
        </label>
        <input
          class="form-control dfe-flex-input-grow govuk-input"
          id="${newInputId}"
          name="${sectionId}"
        />
        <a href="#" class="govuk-link govuk-link--no-visited-state remove-redirect" id="${sectionId}-remove-${counter}" data-group-id="${counter}">Remove</a>
      </div>`;

      $(newElement).appendTo(formGroup);
      counter += 1;
      formGroup.data(`${sectionId}-counter`, counter);
      $(this).trigger("blur");
      return false;
    });

    formGroup.on("click", ".remove-redirect", function removeInput(e) {
      e.preventDefault();
      const groupId = $(this).data("group-id");
      $(`#${sectionId}-input-group-${groupId}`).remove();
      $(this).trigger("blur");

      const newCounter = formGroup.find(".dfe-flex-container").length;
      formGroup.data(`${sectionId}-counter`, newCounter);
    });
  };

  createServiceConfigUrlSections(
    "redirect_uris",
    "#redirect_uris-form-group",
    "Redirect URL",
  );
  createServiceConfigUrlSections(
    "post_logout_redirect_uris",
    "#post_logout_redirect_uris-form-group",
    "Logout redirect URL",
  );

  function handleSecretGeneration(eventId, inputId, confirmMessage) {
    $(eventId).on("click", function generateSecret() {
      const secretArray = window.niceware.generatePassphrase(8);
      const secret = secretArray.join("-");

      const isConfirm = window.confirm(confirmMessage);
      if (isConfirm) {
        $(`input#${inputId}`).attr("value", secret);
      }
      $(this).trigger("blur");
      return false;
    });
  }

  handleSecretGeneration(
    "#generate-clientSecret",
    "clientSecret",
    "Are you sure you want to regenerate the client secret?",
  );
  handleSecretGeneration(
    "#generate-apiSecret",
    "apiSecret",
    "Are you sure you want to regenerate the API secret?",
  );

  // Only auto-run in a real browser (not under Jest/Node)
  if (typeof document !== 'undefined' && typeof window !== 'undefined') {
    updateSections();
  }

  // Event listener for the checkboxes
  $("#response_types-id_token, #response_types-code").on("change", () => {
    updateSections();
  });

});

if (typeof module !== 'undefined' && module.exports) {
  module.exports = { updateSections };
}
