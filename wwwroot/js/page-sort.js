// Reorder only rows already loaded; pagination and database ordering stay unchanged.
(() => {
    const collator = new Intl.Collator(undefined, { numeric: true, sensitivity: "base" });
    document.querySelectorAll("table[data-page-sort]").forEach(table => {
        const body = table.tBodies[0];
        if (!body || body.rows.length < 2) return;
        const originalRows = Array.from(body.rows);
        const fields = Array.from(table.tHead.rows[0].cells)
            .map((cell, index) => ({ cell, index }))
            .filter(({ cell }) => cell.dataset.sortType);
        const wrapper = document.createElement("div");
        wrapper.className = "mb-3";
        const label = document.createElement("label");
        label.className = "form-label";
        label.htmlFor = `${table.id}-sort`;
        label.textContent = "Sort this page";
        const select = document.createElement("select");
        select.id = label.htmlFor;
        select.className = "form-select w-auto";
        select.add(new Option("Default order", "default"));
        fields.forEach(({ cell, index }) => {
            const name = cell.dataset.sortLabel || cell.textContent.trim();
            const directions = cell.dataset.sortType === "date"
                ? ["Oldest first", "Newest first"] : ["A–Z", "Z–A"];
            directions.forEach((direction, i) => select.add(new Option(`${name}: ${direction}`, `${index}:${i}`)));
        });
        const status = document.createElement("p");
        status.className = "form-text";
        status.id = `${table.id}-sort-status`;
        status.setAttribute("role", "status");
        status.textContent = "Only records on this page are sorted.";
        select.setAttribute("aria-describedby", status.id);
        wrapper.append(label, select, status);
        (table.closest(".table-responsive") || table).before(wrapper);
        const storageKey = `page-sort:${location.pathname}:${table.id}`;
        function apply() {
            let rows = originalRows.slice();
            if (select.value !== "default") {
                const [index, descending] = select.value.split(":").map(Number);
                const field = fields.find(field => field.index === index);
                if (!field) return;
                const value = row => row.cells[index].dataset.sortValue ?? row.cells[index].textContent.trim();
                rows.sort((a, b) => {
                    const left = value(a), right = value(b);
                    const comparison = field.cell.dataset.sortType === "date"
                        ? (left < right ? -1 : left > right ? 1 : 0)
                        : collator.compare(left, right);
                    return descending ? -comparison : comparison;
                });
            }
            body.append(...rows);
            status.textContent = `Only records on this page are sorted. ${select.selectedOptions[0].text}.`;
        }
        try {
            const saved = sessionStorage.getItem(storageKey);
            if (Array.from(select.options).some(option => option.value === saved)) select.value = saved;
        } catch { /* Sorting still works when browser storage is unavailable. */ }
        select.addEventListener("change", () => {
            apply();
            try { sessionStorage.setItem(storageKey, select.value); } catch { }
        });
        apply();
    });
})();
