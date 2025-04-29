$(async () => {

  //---------------------------------------------------------------------------
  // Helpers
  //---------------------------------------------------------------------------
  const currentAbsolutePath = window.location.origin + window.location.pathname;

  function resolveAbsolutePath(href) {
    return (href !== null && href !== undefined)
      ? new URL(href, currentAbsolutePath).toString()
      : null;
  }

  const relUri = resolveAbsolutePath($("meta[property='docfx\\:rel']").attr("content"));

  function resolveAbsoluteUri(href, baseUri) {
    return (href !== null && href !== undefined)
      ? new URL(href, baseUri).toString()
      : null;
  }

  //---------------------------------------------------------------------------
  // Highlight code syntax
  //---------------------------------------------------------------------------
  hljs.highlightAll();

  //---------------------------------------------------------------------------
  // Setup search feature
  //---------------------------------------------------------------------------
  let lastTimeoutId = null;
  let highlightIndex = null;

  const $search = $("#app-search");
  const $searchInput = $("#app-search .app-site-search__input");
  const $searchWrapper = $("#app-search .app-site-search__results-wrapper");
  const $searchResults = $("#app-search .app-site-search__results");

  // Focus search input when the '/' key is pressed.
  $(window).keypress(e => {
    if (e.key === "/" && !$searchInput.is(':focus')) {
      $searchInput.focus();
      e.preventDefault();
    }
  });

  $searchInput
    .attr("placeholder", $search.data("hint"))
    .focus(e => {
      $searchInput.attr("placeholder", $search.data("focusedHint"));
      autoShowSearchResults();
    })
    .blur(e => {
      $searchInput.attr("placeholder", $search.data("hint"));
    })
    .keydown(e => {
      if (e.key === "Enter" && highlightIndex !== null) {
        window.ee = $searchResults.find(".app-site-search__item--highlight");
        $searchResults.find(".app-site-search__item--highlight")[0].click();
        e.preventDefault();
      }

      if (e.key === "ArrowUp" || e.key === "ArrowDown") {
        if (highlightIndex === null) {
          highlightResultByIndex(0, true);
        }
        else {
          if (e.key === "ArrowUp") {
            highlightResultByIndex(highlightIndex - 1, true);
          }
          else if (e.key === "ArrowDown") {
            highlightResultByIndex(highlightIndex + 1, true);
          }
        }
        e.preventDefault();
      }
    })
    .on("input", (e => {
      let delay = 0;
      if (lastTimeoutId !== null) {
        clearTimeout(lastTimeoutId);
        delay = 80;
      }
      lastTimeoutId = setTimeout(handleSearch, delay);
      highlightResultByIndex(null);
    }));

  $search
    .keydown(e => {
      if (e.key === "Escape") {
        clearTimeout(lastTimeoutId);
        $searchInput.blur();
        hideSearchResults();
      }
    })
    .focusout(e => {
      if (!$search[0].contains(e.relatedTarget)) {
        hideSearchResults();
      }
    });

  hideSearchResults();

  function highlightResultByIndex(index, autoScroll = false) {
    $searchResults.find(".app-site-search__item--highlight")
      .removeClass("app-site-search__item--highlight");

    let resultsCount = $searchResults.children().length;
    if (index === null || resultsCount === 0 || (resultsCount === 1 && $searchResults.find(".app-site-search__item--no-results").length === 1)) {
      highlightIndex = null;
      return;
    }
    highlightIndex = Math.min(Math.max(0, index), resultsCount - 1);

    const $activeElement = $searchResults.find(".app-site-search__item:nth-child(" + (1 + highlightIndex) + ")")
      .addClass("app-site-search__item--highlight");

    if (autoScroll === true) {
      $activeElement[0].scrollIntoView({ block: "center" });
    }
  }

  function autoShowSearchResults() {
    $searchWrapper.toggle(
      $searchResults.find(".app-site-search__item").length !== 0
    );
  }

  function hideSearchResults() {
    $searchWrapper.hide();
    highlightResultByIndex(null);
  }

  let searchIndexData = null;
  let searchIndex = null;

  async function getSearchIndex() {
    if (searchIndex === null) {
      await initSearchIndex();
    }
    return searchIndex;
  }

  async function initSearchIndex() {
    const uri = resolveAbsoluteUri("search.json", relUri);
    searchIndexData = await loadSearchIndexData(uri);
    searchIndex = initLunr(searchIndexData);
  }

  async function loadSearchIndexData(uri) {
    var response = await fetch(uri);
    var data = await response.json();
    return data.map(entry => {
      entry.href = resolveAbsoluteUri(entry.href, relUri);
      return entry;
    });
  }

  function initLunr(data) {
    return lunr(function () {
      this.field("title", { boost: 20 });
      this.field("summary");
      for (let doc of data) {
        // Boost presence of guide pages slightly over API reference pages.
        let boost = doc.href.startsWith("reference/") ? 0 : 20;
        if (/^interface/.test(doc.section)) {
          boost += 20;
        }
        this.add(doc, { boost });
      }
    });
  }

  async function handleSearch() {
    const $input = $searchInput;
    let query = $input.val();

    if (query.length === 0) {
      hideSearchResults();
      return;
    }

    if (/^[^ :*^]+$/.test(query)) {
      // Add wildcards when just one search term is provided.
      // To avoid issues with "stemming" include non-wildcard term with a significant
      // term boost so that boosting rules behave more appropriately.
      query = query + "^100 *" + query + "*";
    }

    const results = (await getSearchIndex()).search(query)
      .slice(0, 50)
      .map(result => searchIndexData[result.ref]);

    const $results = $searchResults;
    $results.empty();
    $results[0].scrollTo(0, 0);

    if (results.length) {
      $results.append(results.map(result => {
        const $element = $("<a>", {
          class: "app-site-search__item",
          href: result.href,
          text: result.title,
          "data-id": result.id,
        });

        $element
          .mouseenter(function () {
            highlightResultByIndex($(this).index());
          })
          .append(
            $("<span>", {
              class: "app-site-search__summary",
              href: result.href,
              text: result.section,
            })
          );

        return $element;
      }));
    }
    else {
      $results.append(
        $("<li/>", {
          class: "app-site-search__item app-site-search__item--no-results",
          text: $search.data("no-results"),
        })
      );
    }

    autoShowSearchResults();
  }

});
