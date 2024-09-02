mergeInto(LibraryManager.library, {
    DispatchReactUnityEvent: function () {
        try {
            window.dispatchReactUnityEvent("DispatchReactUnityEvent");
        } catch (e) {
            console.warn("Failed to dispatch event");
        }
    },
});
