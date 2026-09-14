(function () {
    function addAuthInput() {
        try {
            var topbar = document.querySelector('.swagger-ui .topbar');
            var container = document.createElement('div');
            container.style.padding = '8px';
            container.style.display = 'flex';
            container.style.alignItems = 'center';

            var input = document.createElement('input');
            input.id = 'bearer-token';
            input.placeholder = 'Paste Bearer token here';
            input.style.width = '420px';
            input.style.marginRight = '8px';

            var btn = document.createElement('button');
            btn.id = 'set-bearer';
            btn.textContent = 'Set Token';

            btn.addEventListener('click', function () {
                var token = document.getElementById('bearer-token').value;
                if (token) {
                    localStorage.setItem('swagger_bearer_token', token);
                    alert('Bearer token saved for this session');
                } else {
                    localStorage.removeItem('swagger_bearer_token');
                    alert('Bearer token removed');
                }
            });

            container.appendChild(input);
            container.appendChild(btn);

            if (topbar && topbar.parentNode) {
                topbar.parentNode.insertBefore(container, topbar.nextSibling);
            }
        } catch (e) {
            console.error(e);
        }
    }

    function addRequestInterceptor() {
        try {
            if (window.ui && window.ui.getConfigs) {
                var configs = window.ui.getConfigs();
                var original = configs.requestInterceptor;
                configs.requestInterceptor = function (req) {
                    var token = localStorage.getItem('swagger_bearer_token');
                    if (token) {
                        req.headers['Authorization'] = 'Bearer ' + token;
                    }
                    if (original) return original(req);
                    return req;
                };
            } else {
                setTimeout(addRequestInterceptor, 500);
            }
        } catch (e) {
            console.error(e);
        }
    }

    window.addEventListener('load', function () {
        setTimeout(function () {
            addAuthInput();
            addRequestInterceptor();
        }, 500);
    });
})();
