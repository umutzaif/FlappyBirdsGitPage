window.flappyKeyboard = {
    registerSpace: function (dotNetHelper) {
        document.addEventListener('keydown', function (event) {
            if (event.code === 'Space') {
                event.preventDefault();
                dotNetHelper.invokeMethodAsync('OnSpacePressed');
            }
        });
    }
};
