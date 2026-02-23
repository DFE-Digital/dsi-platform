/**
 * @jest-environment jsdom
 */

const $ = require("jquery");
global.$ = $;

$.fn.slideDown = function () { return this; };
$.fn.slideUp = function (duration, cb) { if (typeof cb === "function") cb(); return this; };

function buildDOM() {
  document.body.innerHTML = `
    <fieldset id="response_types-fieldset"></fieldset>

    <input type="checkbox" id="response_types-code">
    <input type="checkbox" id="response_types-id_token">

    <div id="refresh_token-wrapper"><input id="rt" /></div>
    <div id="clientSecret-wrapper"><input id="cs" /></div>
    <div id="tokenEndpointAuthMethod-wrapper"><select id="tea"><option value="basic"></option></select></div>
  `;
}

describe("updateSections", () => {
  let updateSections;

  beforeEach(() => {
    buildDOM();
    jest.resetModules();
    ({ updateSections } = require("../_serviceConfigForm"));
  });

  function fieldsEnabled() {
    return (
      $("#refresh_token-wrapper :input").prop("disabled") === false &&
      $("#clientSecret-wrapper :input").prop("disabled") === false &&
      $("#tokenEndpointAuthMethod-wrapper select").prop("disabled") === false
    );
  }

  function fieldsDisabled() {
    return (
      $("#refresh_token-wrapper :input").prop("disabled") === true &&
      $("#clientSecret-wrapper :input").prop("disabled") === true &&
      $("#tokenEndpointAuthMethod-wrapper select").prop("disabled") === true
    );
  }

  test("code → shows authorisation warning & enables fields", () => {
    $("#response_types-code").prop("checked", true);

    updateSections();

    expect($("#warning-response-types").length).toBe(1);
    expect(fieldsEnabled()).toBe(true);
  });

  test("none → hides warning & disables fields", () => {
    updateSections();

    expect($("#warning-response-types").length).toBe(0);
    expect(fieldsDisabled()).toBe(true);
  });

  test("hybrid → shows hybrid warning & enables fields", () => {
    $("#response_types-code").prop("checked", true);
    $("#response_types-id_token").prop("checked", true);

    updateSections();

    expect($("#warning-response-types").length).toBe(1);
    expect(fieldsEnabled()).toBe(true);
  });

  test("id_token only → no warning & fields disabled", () => {
    $("#response_types-id_token").prop("checked", true);

    updateSections();

    expect($("#warning-response-types").length).toBe(0);
    expect(fieldsDisabled()).toBe(true);
  });

  test("calling updateSections twice removes old warning", () => {
    $("#response_types-code").prop("checked", true);

    updateSections();
    expect($("#warning-response-types").length).toBe(1);

    $("#response_types-code").prop("checked", false);
    $("#response_types-id_token").prop("checked", false);

    updateSections();
    expect($("#warning-response-types").length).toBe(0);
  });
});