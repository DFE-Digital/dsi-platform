const Cookies = require("js-cookie");

var COOKIE_NAMES = {
  PREFERENCES_SET: "cookies_preferences_set",
  POLICY: "cookies_policy",
  GA: "_ga",
  GA_GID: "_gid",
  GA_GAT: "_gat",
  USER_BANNER_LAST_SEEN: "user_banner_last_seen",
};

var GOVUK_COOKIE_OPTIONS = {
  expires: 365, // days
  secure: true,
  domain: ".education.gov.uk",
};

var GovUKCookie = {
  getRaw: function (name) {
    return Cookies.get(name);
  },
  get: function (name) {
    var value = Cookies.get(name);
    if (value) {
      return JSON.parse(value);
    }
    return value;
  },
  set: function (name, value) {
    return Cookies.set(name, value, GOVUK_COOKIE_OPTIONS);
  },
  remove: function (name) {
    return Cookies.remove(name, GOVUK_COOKIE_OPTIONS);
  },
};

// function that will remove any existing tracking cookies on page load
(function () {
  GovUKCookie.remove(COOKIE_NAMES.POLICY);
  GovUKCookie.remove(COOKIE_NAMES.PREFERENCES_SET);
  GovUKCookie.remove(COOKIE_NAMES.GA);
  GovUKCookie.remove(COOKIE_NAMES.GA_GAT);
  GovUKCookie.remove(COOKIE_NAMES.GA_GID);
})();

/**
 * Section to handle review users banner
 * It uses a dedicated cookie 'user_banner_last_seen'
 */

function checkConditionForUsersBanner() {
  var lastSeen = GovUKCookie.getRaw(COOKIE_NAMES.USER_BANNER_LAST_SEEN);
  if (lastSeen) {
    var numberOfDays = (new Date().getTime() - lastSeen) / (1000 * 3600 * 24);
    if (numberOfDays > 90) {
      // if the period is longer than 90 days show/update the banner
      return true;
    }
  } else {
    // if there is no value then show/update the banner
    return true;
  }
  return false;
}

function showReviewUsersBanner() {
  $("#review-users-banner").show();
}

function setReviewUsersBannerLastSeen() {
  GovUKCookie.set(COOKIE_NAMES.USER_BANNER_LAST_SEEN, new Date().getTime());
}

function loadReviewUsersBanner() {
  if (checkConditionForUsersBanner()) {
    showReviewUsersBanner();
  }
}

function updateCookieReviewUsersBanner() {
  if (checkConditionForUsersBanner()) {
    setReviewUsersBannerLastSeen();
  }
}

// Exports

window.loadReviewUsersBanner = loadReviewUsersBanner;
window.updateCookieReviewUsersBanner = updateCookieReviewUsersBanner;
