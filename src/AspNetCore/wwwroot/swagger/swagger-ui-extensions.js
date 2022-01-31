const callback = function () {
    const config = window.ui.getConfigs();

    // api code
    const apiCodeWrapper = document.createElement("div");
    apiCodeWrapper.className = "api-code";
    apiCodeWrapper.innerText = config.apiCode;

    const topbarWrapper = document.getElementsByClassName("topbar-wrapper")[0];
    topbarWrapper.insertBefore(apiCodeWrapper, topbarWrapper.childNodes[1]);

    // env
    const envWrapper = document.createElement("div");
    envWrapper.className = "env-tag";
    envWrapper.innerHTML = `<span>${config.apiEnv}</span>`;

    const topbar = document.getElementsByClassName("topbar")[0];
    topbar.className += " " + config.apiEnv;
    topbar.appendChild(envWrapper);

    // bg
    if (config.topbarLight) {
        topbar.className += " light";
    }
};

_callbackRunnerCount = 0;
var callbackRunner = setInterval(function () {
    _callbackRunnerCount++;
    if (_callbackRunnerCount > 20) {
        clearInterval(callbackRunner);
    }

    if (document.getElementById("swagger-ui") != null
        && document.getElementsByClassName("topbar-wrapper").length) {
        clearInterval(callbackRunner);
        callback();
    }
}, 100);

document.addEventListener("DOMContentLoaded", callbackRunner);

