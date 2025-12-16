const tabId = Number(window.localStorage.getItem('session-timeout:next-id'));
window.localStorage.setItem('session-timeout:next-id', tabId + 1);

function initSessionTimeoutElement(element) {
  const sessionDurationInMinutes = parseInt(element.dataset.sessionDurationInMinutes);
  const notifyRemainingMinutes = parseInt(element.dataset.notifyRemainingMinutes);
  const endSessionUrl = element.dataset.endSessionUrl;
  const timeoutUrl = element.dataset.timeoutUrl;

  const modalElement = element.querySelector('.app-session-timeout__modal');
  const timerElement = element.querySelector('.app-session-timeout__timer');
  const minutesElement = element.querySelector('.app-session-timeout__minutes');
  const secondsElement = element.querySelector('.app-session-timeout__seconds');
  const timeoutNowElement = element.querySelector('.app-session-timeout__now');
  const extendButtonElement = element.querySelector('.app-session-timeout__extend');

  extendButtonElement.addEventListener("click", () => location.reload());

  window.localStorage.setItem('session-timeout:end', Date.now() + sessionDurationInMinutes * 60 * 1000);

  const intervalId = setInterval(() => {
    const timeout = Number(window.localStorage.getItem('session-timeout:end'));
    if (timeout === 1) {
      setTimeout(() => {
        location.href = timeoutUrl;
        clearInterval(intervalId);
      }, 2000);
      return;
    }

    const popupTime = timeout - notifyRemainingMinutes * 60 * 1000;
    if (Date.now() >= popupTime) {
      if (!element.classList.contains('app-session-timeout--show')) {
        element.classList.add('app-session-timeout--show');
        if (modalElement.showModal) {
          modalElement.showModal();
        }
        else {
          modalElement.focus();
        }
      }

      // End several seconds earlier so that session timeout can use cookies.
      const allowanceInSeconds = 10;
      const totalSecondsRemaining = Math.round((timeout - Date.now()) / 1000 - allowanceInSeconds);

      if (totalSecondsRemaining <= 0) {
        clearInterval(intervalId);
        timerElement.style.display = "none";
        timeoutNowElement.style.display = "";

        window.localStorage.setItem('session-timeout:closer', tabId);
        setTimeout(() => {
          // Allow one tab to lead; and then all of the others can follow shortly after.
          // Avoid scenario where all tabs initiate the sign out.
          if (tabId === Number(window.localStorage.getItem('session-timeout:closer'))) {
            location.href = endSessionUrl;
          }
          else {
            // Close the other non-leader tabs a short while later.
            setTimeout(() => location.href = timeoutUrl, 2000);
          }
        }, 1000);
      }
      else {
        let minutesRemaining = Math.floor(totalSecondsRemaining / 60);
        let secondsRemaining = totalSecondsRemaining % 60;

        timerElement.style.display = "";
        timeoutNowElement.style.display = "none";
        minutesElement.textContent = minutesRemaining > 0 ? minutesRemaining + ' minutes' : '';
        secondsElement.textContent = secondsRemaining + ' seconds';
      }
    }
    else {
      element.classList.remove('app-session-timeout--show');
      if (modalElement.showModal) {
        modalElement.close();
      }
    }
  }, 500);
}

document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll('.app-session-timeout').forEach(initSessionTimeoutElement);
});
