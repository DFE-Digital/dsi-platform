function initAuthActionElement(element) {
  const signOutLinkElement = element.querySelector('.app-auth-action__link');
  signOutLinkElement.addEventListener("click", () => {
    window.localStorage.setItem('session-timeout:end', 1);
  });
}

document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll('.app-auth-action').forEach(initAuthActionElement);
});
