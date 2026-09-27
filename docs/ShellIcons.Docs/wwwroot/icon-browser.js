/* Icon browser behavior. Listeners sit on `document` and look elements up per event, so they keep
   working on static hosting, after Blazor enhanced navigation, and when an interactive render
   replaces the prerendered DOM. */
(function () {
    function filter(input) {
        const cells = document.querySelectorAll('#icon-grid .icon-cell');
        const q = input.value.trim().toLowerCase();
        let visible = 0;
        for (const cell of cells) {
            const match = !q || cell.dataset.name.includes(q);
            cell.hidden = !match;
            if (match) visible++;
        }

        const count = document.getElementById('icon-count');
        if (count) count.textContent = q ? visible + ' / ' + cells.length : cells.length;

        const empty = document.getElementById('empty-state');
        if (empty) {
            empty.hidden = visible !== 0;
            const query = document.getElementById('empty-query');
            if (query) query.textContent = q;
        }
    }

    function pascal(name) {
        return name.split('-').map(w => w[0].toUpperCase() + w.slice(1)).join('');
    }

    function showToast(text) {
        const toast = document.getElementById('copy-toast');
        if (!toast) return;
        toast.textContent = text;
        toast.hidden = false;
        requestAnimationFrame(() => toast.classList.add('show'));
        clearTimeout(showToast.timer);
        showToast.timer = setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => (toast.hidden = true), 200);
        }, 1500);
    }

    // Debounced with a timer rather than requestAnimationFrame, which stalls when the page isn't painting.
    let timer = 0;
    document.addEventListener('input', e => {
        if (e.target.id !== 'icon-search') return;
        clearTimeout(timer);
        timer = setTimeout(() => filter(e.target), 50);
    });

    document.addEventListener('click', e => {
        const cell = e.target.closest && e.target.closest('#icon-grid .icon-cell');
        if (!cell) return;
        const name = cell.dataset.name;
        // `shell` has no suffixed form (ShellIcon is the dispatcher).
        const snippet = name === 'shell' ? '@Icon.Shell()' : '<' + pascal(name) + 'Icon />';
        navigator.clipboard?.writeText(snippet).then(() => showToast('Copied: ' + snippet));
    });

    document.addEventListener('keydown', e => {
        const input = document.getElementById('icon-search');
        if (e.key === '/' && input && document.activeElement !== input) {
            e.preventDefault();
            input.focus();
        }
    });
})();
