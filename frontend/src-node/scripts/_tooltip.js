var toggleTips = $(".toggle-help");

toggleTips.each(function () {
  var $that, liveRegion, container, toggleTip;

  $that = $(this);

  container = $("<span />").attr("class", "toggletip");

  toggleTip = $("<button />")
    .attr("type", "button")
    .attr("aria-label", "Help")
    .attr("data-toggletip-content", $that.html())
    .text("?")
    .on("click", function () {
      var message = $(this).data("toggletip-content");
      var liveRegion = $(this).next();
      window.setTimeout(function () {
        liveRegion.html(function () {
          return $("<span />")
            .attr("class", function () {
              return $that.html().length > 50 ? "bubble long" : "bubble";
            })
            .text(message);
        });
      }, 100);
    })
    .on("keydown", function (e) {
      if ((e.keyCode || e.which) === 27) liveRegion.html("");
    });

  liveRegion = $("<span />").attr("role", "status");

  container.append(toggleTip, liveRegion);

  $that.before(container).remove();

  $(document).on("click", function (e) {
    if ($that[0] !== $(e.target)) {
      liveRegion.html("");
    }
  });
});
