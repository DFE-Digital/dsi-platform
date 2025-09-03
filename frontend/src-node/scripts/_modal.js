var NSA = NSA || {};

NSA.modal = {
  elems: {
    links: $(".modal-link"),
    modals: $(".modal"),
  },
  init: function () {
    this.modalBg();
    this.setUpEvents();
  },
  modalBg: function () {
    var that = this;
    var bg = $("<div>")
      .addClass("modal-bg")
      .hide()
      .on("click", function () {
        that.hideModals();
      });
    $("body").append(bg);
  },
  setUpEvents: function () {
    var that = this;
    this.elems.links.on("click", function (e) {
      var target = $(this).attr("href");
      that.showModal($(target));
      e.preventDefault();
    });

    that.elems.modals.find(".close").on("click", function (e) {
      that.hideModals();
      e.preventDefault();
    });

    $("body").on("keydown", function (e) {
      if ((e.keyCode || e.which) === 27) that.hideModals();
    });
  },
  showModal: function (modal) {
    $("html").addClass("modal-open");
    $(".modal-bg").show();
    modal.show();
  },
  hideModals: function () {
    $("html").removeClass("modal-open");
    $(".modal-bg").hide();
    this.elems.modals.hide();
  },
};

NSA.modal.init();
