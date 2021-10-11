const callback = function () {
    const configObject = JSON.parse('%(ConfigObject)');

    const apiCodeWrapper = document.createElement("div");
    apiCodeWrapper.className = "api-code";
    apiCodeWrapper.innerText = configObject.apiCode;

    const topbarWrapper = document.getElementsByClassName("topbar-wrapper")[0];
    topbarWrapper.insertBefore(apiCodeWrapper, topbarWrapper.childNodes[1]);
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

