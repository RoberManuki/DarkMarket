window.cryptoMarketModal = (() => {
    let activeModal = null;
    let previousFocusedElement = null;
    let previousBodyOverflow = null;

    function getFocusableElements(modal) {
        if (!modal) {
            return [];
        }

        const selector = [
            "a[href]",
            "button:not([disabled])",
            "input:not([disabled]):not([type='hidden'])",
            "select:not([disabled])",
            "textarea:not([disabled])",
            "[tabindex]:not([tabindex='-1'])"
        ].join(",");

        return Array.from(modal.querySelectorAll(selector)).filter((element) => {
            if (!(element instanceof HTMLElement)) {
                return false;
            }

            return element.offsetParent !== null || element === document.activeElement;
        });
    }

    function trapTabKey(event) {
        if (event.key !== "Tab" || !activeModal) {
            return;
        }

        const focusable = getFocusableElements(activeModal);
        if (focusable.length === 0) {
            event.preventDefault();
            return;
        }

        const first = focusable[0];
        const last = focusable[focusable.length - 1];
        const current = document.activeElement;

        if (event.shiftKey && current === first) {
            event.preventDefault();
            last.focus();
            return;
        }

        if (!event.shiftKey && current === last) {
            event.preventDefault();
            first.focus();
        }
    }

    function focusFirstElement(modal) {
        const focusable = getFocusableElements(modal);
        const first = focusable[0] || modal;

        if (first instanceof HTMLElement) {
            first.focus();
        }
    }

    function close() {
        if (!activeModal) {
            return;
        }

        activeModal.removeEventListener("keydown", trapTabKey);
        activeModal = null;

        if (previousBodyOverflow !== null) {
            document.body.style.overflow = previousBodyOverflow;
        }

        if (previousFocusedElement instanceof HTMLElement) {
            previousFocusedElement.focus();
        }

        previousFocusedElement = null;
        previousBodyOverflow = null;
    }

    function open(modalSelector) {
        const modal = document.querySelector(modalSelector);
        if (!(modal instanceof HTMLElement)) {
            return;
        }

        if (activeModal === modal) {
            return;
        }

        close();

        activeModal = modal;
        previousFocusedElement = document.activeElement instanceof HTMLElement ? document.activeElement : null;
        previousBodyOverflow = document.body.style.overflow;
        document.body.style.overflow = "hidden";

        activeModal.addEventListener("keydown", trapTabKey);

        requestAnimationFrame(() => {
            focusFirstElement(activeModal);
        });
    }

    return {
        open,
        close
    };
})();

