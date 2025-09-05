((global) => {
  const globalObject = global;
  const $ = global.jQuery;
  const GOVUK = global.GOVUK || {};

  function ShowHideContent() {
    const self = this;

    // Radio and Checkbox selectors
    const selectors = {
      namespace: "ShowHideContent",
      radio: '[data-target] > input[type="radio"]',
      checkbox: '[data-target] > input[type="checkbox"]',
      link: "a.js-toggle-content",
      select: "[data-target] > select",
    };

    // Escape name attribute for use in DOM selector
    function escapeElementName(str) {
      return str.replace("[", "\\[").replace("]", "\\]");
    }

    // Return toggled content for control
    function getToggledContent($control) {
      let id = $control.attr("aria-controls");

      // ARIA attributes aren't set before init
      if (!id) {
        id = $control.closest("[data-target]").data("target");
      }

      // Find show/hide content by id
      return $(`#${id}`);
    }

    // Adds ARIA attributes to control + associated content
    function initToggledContent() {
      const $control = $(this);
      const $content = getToggledContent($control);

      // Set aria-controls and defaults
      if ($content.length) {
        $control.attr("aria-controls", $content.attr("id"));
        $control.attr("aria-expanded", "false");
      }
    }

    // Show toggled content for control
    function showToggledContent($control, $content) {
      // Show content
      if ($content.hasClass("js-hidden")) {
        $content.removeClass("js-hidden");

        // If the controlling input, update aria-expanded
        if ($control.attr("aria-controls")) {
          $control.attr("aria-expanded", "true");
        }
      }
    }

    // Hide toggled content for control
    function hideToggledContent($control, $content) {
      const content = $content || getToggledContent($control);

      // Hide content
      if (!content.hasClass("js-hidden")) {
        content.addClass("js-hidden");

        // If the controlling input, update aria-expanded
        if ($control.attr("aria-controls")) {
          $control.attr("aria-expanded", "false");
        }
      }
    }

    // Handle radio show/hide
    function handleRadioContent($control, $content) {
      // All radios in this group which control content
      const selector = `${selectors.radio}[name=${escapeElementName($control.attr("name"))}][aria-controls]`;
      const $form = $control.closest("form");
      const $radios = $form.length ? $form.find(selector) : $(selector);

      // Hide content for radios in group
      $radios.each(function () {
        hideToggledContent($(this));
      });

      // Select content for this control
      if ($control.is("[aria-controls]")) {
        showToggledContent($control, $content);
      }
    }

    // Handle checkbox show/hide
    function handleCheckboxContent($control, $content) {
      // Show checkbox content
      if ($control.is(":checked")) {
        showToggledContent($control, $content);
      } else {
        // Hide checkbox content
        hideToggledContent($control, $content);
      }
    }

    // Handle checkbox show/hide
    function handleLinkContent($control, $content, $event) {
      if ($content.hasClass("js-hidden")) {
        showToggledContent($control, $content);
      } else {
        hideToggledContent($control, $content);
      }
      $event.preventDefault();
    }

    function handleSelectContent($control, $content) {
      if ($control.val() === $control.data("option")) {
        showToggledContent($control, $content);
        return;
      }

      hideToggledContent($content, $content);
      if ($control.data("clear")) {
        $content.find(":input").each((_, element) => {
          const input = element;
          input.value = "";
        });
      }
    }

    // Set up event handlers etc
    function init($container, elementSelector, eventSelectors, handler) {
      const container = $container || $(document.body);

      // Handle control clicks
      function deferred(event) {
        const $control = $(this);
        handler($control, getToggledContent($control), event);
      }

      // Prepare ARIA attributes
      const $controls = $(elementSelector);
      $controls.each(initToggledContent);

      // Handle events
      $.each(eventSelectors, (_, eventSelector) => {
        if (handler === handleSelectContent) {
          container.on(
            `change.${selectors.namespace}`,
            eventSelector,
            deferred,
          );
        } else {
          container.on(`click.${selectors.namespace}`, eventSelector, deferred);
        }
      });

      // Any already :checked on init?
      if (handler === handleCheckboxContent && $controls.is(":checked")) {
        $controls.filter(":checked").each(deferred);
      }

      if (handler === handleSelectContent) {
        $controls.each(deferred);
      }
    }

    // Get event selectors for all radio groups
    function getEventSelectorsForRadioGroups() {
      const radioGroups = [];

      // Build an array of radio group selectors
      return $(selectors.radio).map(function () {
        const groupName = $(this).attr("name");

        if ($.inArray(groupName, radioGroups) === -1) {
          radioGroups.push(groupName);
          return `input[type="radio"][name="${$(this).attr("name")}"]`;
        }
        return null;
      });
    }

    // Set up radio show/hide content for container
    self.showHideRadioToggledContent = function ($container) {
      init(
        $container,
        selectors.radio,
        getEventSelectorsForRadioGroups(),
        handleRadioContent,
      );
    };

    // Set up checkbox show/hide content for container
    self.showHideCheckboxToggledContent = function ($container) {
      init(
        $container,
        selectors.checkbox,
        [selectors.checkbox],
        handleCheckboxContent,
      );
    };

    // Set up links show/hide content for container
    self.showHideLinkToggledContent = function ($container) {
      init($container, selectors.link, [selectors.link], handleLinkContent);
    };

    self.showHideSelectToggledContent = function ($container) {
      init(
        $container,
        selectors.select,
        [selectors.select],
        handleSelectContent,
      );
    };

    // Remove event handlers
    self.destroy = function ($container) {
      const container = $container || $(document.body);
      container.off(`.${selectors.namespace}`);
    };
  }

  ShowHideContent.prototype.init = function ($container) {
    this.showHideRadioToggledContent($container);
    this.showHideCheckboxToggledContent($container);
    this.showHideLinkToggledContent($container);
    this.showHideSelectToggledContent($container);
  };

  GOVUK.ShowHideContent = ShowHideContent;
  globalObject.GOVUK = GOVUK;
})(window);
