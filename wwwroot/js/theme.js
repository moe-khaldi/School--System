(() => {
    const key = "schoolsystem-theme";
    const root = document.documentElement;
    function apply(theme) {
        const dark = theme === "dark";
        root.dataset.theme = dark ? "dark" : "light";
        document.querySelectorAll("[data-theme-toggle]").forEach(button => {
            button.hidden = false;
            button.textContent = "Dark theme: " + (dark ? "on" : "off");
            button.setAttribute("aria-pressed", String(dark));
        });
    }
    let saved = "light";
    try { saved = localStorage.getItem(key) || "light"; } catch { /* Use light if storage is unavailable. */ }
    apply(saved);
    document.addEventListener("DOMContentLoaded", () => {
        apply(root.dataset.theme);
        document.querySelectorAll("[data-theme-toggle]").forEach(button => {
            button.addEventListener("click", () => {
                const theme = root.dataset.theme === "dark" ? "light" : "dark";
                apply(theme);
                try { localStorage.setItem(key, theme); } catch { /* Switching still works without persistence. */ }
            });
        });
    });
    window.addEventListener("storage", event => {
        if (event.key === key || event.key === null) apply(event.newValue);
    });
})();
