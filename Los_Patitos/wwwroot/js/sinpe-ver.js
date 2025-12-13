(() => {
    const root = document.getElementById('sinpe-root');
    if (!root) return;

    // Data-* desde la vista
    const apiBaseUrl = (root.dataset.api || '').trim();
    const idCaja = parseInt(root.dataset.idcaja || '0', 10);
    const idComercio = parseInt(root.dataset.idcomercio || '0', 10);
    const syncLocalUrl = (root.dataset.syncLocalUrl || '/Caja/SincronizarAjax').trim();
    const authUrl = (root.dataset.authUrl || '/api/autenticacion/token').trim();
    const syncApiUrl = (root.dataset.syncApiUrl || '/api/sinpe/sincronizar').trim();

    const csrf = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') || '';

    // Une base + path sin duplicar barras
    const joinUrl = (base, path) => {
        if (!base) return path;
        const b = base.endsWith('/') ? base.slice(0, -1) : base;
        const p = path.startsWith('/') ? path : `/${path}`;
        return `${b}${p}`;
    };

    // Fallback local al MVC
    const postLocalFallback = async (idSinpe) => {
        const resp = await fetch(syncLocalUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': csrf,
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({ idSinpe: Number(idSinpe), idCaja: Number(idCaja) })
        });
        const data = await resp.json().catch(() => ({ esValido: false, mensaje: 'Respuesta inválida.' }));
        const result = {
            ok: !!(resp.ok && data.esValido),
            mensaje: data.mensaje || 'Operación realizada.',
            via: data.via || 'LOCAL',
            fallback: !!data.fallback
        };
        console.info('[SINPE] fallback LOCAL', { idSinpe, resultado: result });
        return result;
    };

    // Autenticar y sincronizar contra el API externo
    const authAndSyncExternal = async (idSinpe) => {
        if (!apiBaseUrl || !idComercio) {
            const r = { ok: false, mensaje: 'API externo no configurado.', via: 'API', fallback: false };
            console.warn('[SINPE] intento API - configuración incompleta', r);
            return r;
        }

        // 1) token
        const authResp = await fetch(joinUrl(apiBaseUrl, authUrl), {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ idComercio })
        });

        if (!authResp.ok) {
            const r = { ok: false, mensaje: `No se pudo autenticar en el API externo (${authResp.status}).`, via: 'API', fallback: false };
            console.warn('[SINPE] intento API - auth falló', r);
            return r;
        }

        const auth = await authResp.json().catch(() => null);
        if (!auth || !auth.esValido || !auth.token) {
            const r = { ok: false, mensaje: (auth && auth.mensaje) || 'Token inválido del API externo.', via: 'API', fallback: false };
            console.warn('[SINPE] intento API - token inválido', r);
            return r;
        }

        // 2) sincronizar
        const syncResp = await fetch(joinUrl(apiBaseUrl, syncApiUrl), {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${auth.token}`
            },
            body: JSON.stringify({ idSinpe: Number(idSinpe) })
        });

        const syncData = await syncResp.json().catch(() => null);

        if (!syncResp.ok) {
            const r = { ok: false, mensaje: `API externo respondió ${syncResp.status}. ${syncData?.mensaje || ''}`, via: 'API', fallback: false };
            console.warn('[SINPE] intento API - HTTP error', r);
            return r;
        }
        if (!syncData || !syncData.esValido) {
            const r = { ok: false, mensaje: syncData?.mensaje || 'El API externo no pudo sincronizar.', via: 'API', fallback: false };
            console.warn('[SINPE] intento API - negocio falló', r);
            return r;
        }

        const r = { ok: true, mensaje: syncData.mensaje || 'Sincronizado por API.', via: (syncData.via || 'API'), fallback: !!syncData.fallback };
        console.info('[SINPE] intento API - ok', { idSinpe, resultado: r });
        return r;
    };

    // Helpers de UI
    const setBusy = (btn, busyText = 'Sincronizando...') => {
        btn.dataset.prev = btn.innerHTML;
        btn.disabled = true;
        btn.innerHTML = busyText;
    };
    const setIdle = (btn) => {
        btn.disabled = false;
        if (btn.dataset.prev) btn.innerHTML = btn.dataset.prev;
    };

    // Pinta "Sincronizado" + chip de vía
    const markSynced = (btn, res) => {
        const card = btn.closest('.border.border-gray-200');
        if (card) {
            const badge = card.querySelector('span.bg-yellow-100');
            if (badge) {
                badge.classList.remove('bg-yellow-100', 'text-yellow-800');
                badge.classList.add('bg-green-100', 'text-green-700');
                badge.textContent = 'Sincronizado';
            }
            const headerRow = card.querySelector('.flex.items-center.gap-2');
            if (headerRow) {
                let chip = headerRow.querySelector('.js-chip-via');
                const texto = res?.via === 'API'
                    ? 'API'
                    : res?.fallback ? 'LOCAL (fallback)' : 'LOCAL';
                if (!chip) {
                    chip = document.createElement('span');
                    chip.className = 'js-chip-via ml-2 px-2 py-1 rounded-md text-xs font-bold bg-blue-100 text-blue-700';
                    headerRow.appendChild(chip);
                }
                chip.textContent = texto;
            }
        }
        btn.classList.remove('bg-yellow-500', 'hover:bg-yellow-600');
        btn.classList.add('bg-green-600');
        btn.innerHTML = 'Sincronizado';
    };

    // Wire buttons
    const btns = document.querySelectorAll('.js-sincronizar-sinpe');
    btns.forEach(btn => {
        btn.addEventListener('click', async function () {
            const idSinpe = this.dataset.idSinpe;
            if (!idSinpe || !idCaja) return;

            setBusy(this);

            try {
                // 1) intenta API externo
                let res = await authAndSyncExternal(idSinpe);

                // 2) si falla, fallback local
                if (!res.ok) {
                    res = await postLocalFallback(idSinpe);
                }

                if (res.ok) {
                    markSynced(this, res);
                } else {
                    this.title = res.mensaje || 'No fue posible sincronizar.';
                    setIdle(this);
                }
            } catch (e) {
                console.error('[SINPE] error de conexión', e);
                this.title = 'Error de conexión.';
                setIdle(this);
            }
        });
    });
})();
