
// ACCESIBILIDAD 
document.addEventListener("DOMContentLoaded", () => {

    const btn = document.getElementById("btn-accesibilidad");
    const panel = document.getElementById("panel-accesibilidad");
    const chkContraste = document.getElementById("chk-contraste");
    const selFont = document.getElementById("sel-font");
    const btnReset = document.getElementById("btn-reset-a11y");

    if (!btn || !panel) return;

    // Helpers
    const setExpanded = (value) => {
        btn.setAttribute("aria-expanded", value ? "true" : "false");
    };

    const openPanel = () => {
        panel.style.display = "block";
        setExpanded(true);
    };

    const closePanel = () => {
        panel.style.display = "none";
        setExpanded(false);
    };

    const togglePanel = () => {
        const isOpen = panel.style.display === "block";
        if (isOpen) closePanel();
        else openPanel();
    };

    // LocalStorage
    const LS_CONTRASTE = "lp_a11y_contraste";
    const LS_FONT = "lp_a11y_font";

    const applyContrast = (enabled) => {
        document.body.classList.toggle("modo-alto-contraste", enabled);
    };

    const applyFont = (fontClass) => {
        document.documentElement.classList.remove(
            "font-sm", "font-md", "font-lg", "font-xl"
        );
        document.documentElement.classList.add(fontClass || "font-md");
    };

    // Cargar preferencias guardadas
    const savedContrast = localStorage.getItem(LS_CONTRASTE);
    const savedFont = localStorage.getItem(LS_FONT);

    const contrastEnabled = savedContrast === "true";
    applyContrast(contrastEnabled);
    if (chkContraste) chkContraste.checked = contrastEnabled;

    applyFont(savedFont || "font-md");
    if (selFont) selFont.value = savedFont || "font-md";

    // Eventos UI
    btn.addEventListener("click", togglePanel);

    // Cerrar si clic afuera
    document.addEventListener("click", (e) => {
        const clickedInside = panel.contains(e.target) || btn.contains(e.target);
        if (!clickedInside) closePanel();
    });

    // Tecla ESC para cerrar
    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape") closePanel();
    });

    // Alto contraste
    if (chkContraste) {
        chkContraste.addEventListener("change", (e) => {
            const enabled = e.target.checked;
            applyContrast(enabled);
            localStorage.setItem(LS_CONTRASTE, String(enabled));
        });
    }

    // Tamaño de texto
    if (selFont) {
        selFont.addEventListener("change", (e) => {
            const fontClass = e.target.value;
            applyFont(fontClass);
            localStorage.setItem(LS_FONT, fontClass);
        });
    }

    // Reset
    if (btnReset) {
        btnReset.addEventListener("click", () => {
            localStorage.removeItem(LS_CONTRASTE);
            localStorage.removeItem(LS_FONT);

            applyContrast(false);
            applyFont("font-md");

            if (chkContraste) chkContraste.checked = false;
            if (selFont) selFont.value = "font-md";
        });
    }
});