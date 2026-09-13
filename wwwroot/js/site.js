// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Show placeholders during server navigation, without delaying loaded content.
(() => {
    const main = document.querySelector("main");
    if (!main) return;
    const status = document.createElement("div");
    status.className = "visually-hidden";
    status.setAttribute("role", "status");
    document.body.append(status);
    let showTimer, resetTimer;
    function reset() {
        clearTimeout(showTimer);
        clearTimeout(resetTimer);
        main.classList.remove("page-loading");
        main.removeAttribute("aria-busy");
        main.inert = false;
        status.textContent = "";
    }
    document.addEventListener("click", event => {
        if (event.defaultPrevented || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
        const link = event.target.closest("a[href]");
        if (!link || link.hasAttribute("download") || (link.target && link.target !== "_self") || link.hasAttribute("data-bs-toggle")) return;
        const url = new URL(link.href, location.href);
        if (url.origin !== location.origin || !/^https?:$/.test(url.protocol)) return;
        if (url.pathname === location.pathname && url.search === location.search) return;
        reset();
        showTimer = setTimeout(() => {
            if (event.defaultPrevented) return;
            main.classList.add("page-loading");
            main.setAttribute("aria-busy", "true");
            main.inert = true;
            status.textContent = "Loading page…";
            resetTimer = setTimeout(reset, 10000);
        }, 150);
    });
    window.addEventListener("pageshow", reset);
    window.addEventListener("pagehide", reset);
    window.addEventListener("keydown", event => {
        if (event.key === "Escape") reset();
    });
})();
