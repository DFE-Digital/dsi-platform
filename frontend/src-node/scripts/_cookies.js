var COOKIE_NAMES = {
  USER_BANNER_LAST_SEEN: "user_banner_last_seen",
};

var GOVUK_COOKIE_OPTIONS = {
  expires: 365, // days
  domain: ".education.gov.uk",
};

function getCookie(name) {
  const cookies = document.cookie.split(";");
  for (let cookie of cookies) {
    const [key, value] = cookie.trim().split("=");
    if (key === name) {
      return JSON.parse(decodeURIComponent(value));
    }
  }
  return null;
}

function setCookie(name, value) {
  name = encodeURIComponent(name);
  value = encodeURIComponent(value);

  const expires = new Date(
    Date.now() + GOVUK_COOKIE_OPTIONS.expires * 864e5,
  ).toUTCString();

  document.cookie = `${name}=${value};expires=${expires};path=/;domain=${GOVUK_COOKIE_OPTIONS.domain};secure;SameSite=Strict`;
}

/**
 * Section to handle review users banner
 * It uses a dedicated cookie 'user_banner_last_seen'
 */

function checkConditionForUsersBanner() {
  var lastSeen = getCookie(COOKIE_NAMES.USER_BANNER_LAST_SEEN);
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
  setCookie(COOKIE_NAMES.USER_BANNER_LAST_SEEN, new Date().getTime());
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
window.setReviewUsersBannerLastSeen = setReviewUsersBannerLastSeen;
