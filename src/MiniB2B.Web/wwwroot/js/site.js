(function () {
  const modalEl = document.getElementById("productDetailModal");
  const bodyEl = document.getElementById("productDetailBody");
  const titleEl = document.getElementById("productDetailTitle");
  if (!modalEl || !bodyEl || !titleEl || !window.bootstrap) return;

  const modal = bootstrap.Modal.getOrCreateInstance(modalEl);

  document.addEventListener("click", function (e) {
    const link = e.target.closest("a.js-product-detail");
    if (!link) return;
    if (e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;

    const url = link.getAttribute("data-detail-url");
    if (!url) return;

    e.preventDefault();
    titleEl.textContent = link.getAttribute("data-product-name") || "Ürün detayı";
    bodyEl.innerHTML = '<p class="text-muted mb-0">Yükleniyor…</p>';
    modal.show();

    fetch(url, {
      headers: {
        "X-Requested-With": "XMLHttpRequest",
        "Accept": "text/html"
      }
    }).then(function (res) {
      if (!res.ok) throw new Error();
      if (res.url && res.url.indexOf("/Account/Login") !== -1) {
        window.location.href = link.href;
        return "";
      }
      return res.text();
    }).then(function (html) {
      if (!html) return;
      bodyEl.innerHTML = html;
    }).catch(function () {
      window.location.href = link.href;
    });
  });
})();

(function () {
  const viewport = document.querySelector(".brand-marquee-viewport");
  const track = document.querySelector(".brand-marquee-track");
  if (!viewport || !track) return;

  let paused = false;
  let x = 0;
  const speed = 1.25;

  viewport.addEventListener("mouseenter", function () { paused = true; });
  viewport.addEventListener("mouseleave", function () { paused = false; });

  function groupWidth() {
    const group = track.querySelector(".brand-marquee-group");
    return group ? group.offsetWidth : 0;
  }

  function apply() {
    track.style.transform = "translateX(" + x + "px)";
  }

  function tick() {
    if (!paused) {
      const w = groupWidth();
      if (w > 0) {
        x -= speed;
        if (x <= -w) x += w;
        apply();
      }
    }
    requestAnimationFrame(tick);
  }

  x = 0;
  apply();
  requestAnimationFrame(tick);
})();
