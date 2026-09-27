// This function is serialized into every standalone game HTML.
window.MobileForgeRuntime = function mobileForgeRuntime(source, spriteData) {
  const canvas = document.querySelector('canvas');
  const ctx = canvas.getContext('2d');
  const errorBox = document.querySelector('#game-error');
  const keys = new Set();
  const pointer = { x: 0, y: 0, down: false };
  const cache = new Map();
  const report = (error) => {
    const message = error?.stack || error?.message || String(error);
    errorBox.hidden = false;
    errorBox.textContent = message;
    if (window.parent !== window) window.parent.postMessage({ mobileForge: true, type: 'error', message }, '*');
  };
  window.addEventListener('error', (event) => report(event.error || event.message));
  window.addEventListener('unhandledrejection', (event) => report(event.reason));
  function position(event) {
    const bounds = canvas.getBoundingClientRect();
    pointer.x = Math.max(0, Math.min(360, (event.clientX - bounds.left) * 360 / bounds.width));
    pointer.y = Math.max(0, Math.min(640, (event.clientY - bounds.top) * 640 / bounds.height));
  }
  canvas.addEventListener('pointerdown', (event) => { position(event); pointer.down = true; canvas.setPointerCapture(event.pointerId); });
  canvas.addEventListener('pointermove', position);
  canvas.addEventListener('pointerup', () => { pointer.down = false; });
  canvas.addEventListener('pointercancel', () => { pointer.down = false; });
  window.addEventListener('keydown', (event) => { keys.add(event.key); if (event.key.startsWith('Arrow') || event.key === ' ') event.preventDefault(); });
  window.addEventListener('keyup', (event) => keys.delete(event.key));
  window.addEventListener('blur', () => { keys.clear(); pointer.down = false; });
  function getSprite(name) {
    if (cache.has(name)) return cache.get(name);
    const sprite = spriteData.find((item) => item.name === name);
    if (!sprite) return null;
    const image = document.createElement('canvas');
    image.width = sprite.w; image.height = sprite.h;
    const context = image.getContext('2d');
    const data = context.createImageData(sprite.w, sprite.h);
    for (let i = 0; i < sprite.pixels.length; i++) {
      const hex = sprite.pixels[i];
      if (!hex) continue;
      const value = hex.replace('#', '');
      data.data[i * 4] = parseInt(value.slice(0, 2), 16);
      data.data[i * 4 + 1] = parseInt(value.slice(2, 4), 16);
      data.data[i * 4 + 2] = parseInt(value.slice(4, 6), 16);
      data.data[i * 4 + 3] = value.length === 8 ? parseInt(value.slice(6, 8), 16) : 255;
    }
    context.putImageData(data, 0, 0);
    cache.set(name, image);
    return image;
  }
  const game = {
    width: 360, height: 640, pointer,
    key: (name) => keys.has(name),
    clear(color = '#101827') { ctx.fillStyle = color; ctx.fillRect(0, 0, 360, 640); },
    rect(x, y, w, h, color = '#fff') { ctx.fillStyle = color; ctx.fillRect(x, y, w, h); },
    circle(x, y, radius, color = '#fff') { ctx.fillStyle = color; ctx.beginPath(); ctx.arc(x, y, radius, 0, Math.PI * 2); ctx.fill(); },
    text(value, x, y, size = 20, color = '#fff') { ctx.fillStyle = color; ctx.font = `${size}px system-ui,sans-serif`; ctx.fillText(String(value), x, y); },
    sprite(name, x, y, scale = 1) {
      const image = getSprite(name);
      if (!image) return false;
      ctx.imageSmoothingEnabled = false;
      ctx.drawImage(image, x, y, image.width * scale, image.height * scale);
      return true;
    }
  };
  let hooks;
  try {
    hooks = new Function('game', `${source}\nreturn { setup: typeof setup === 'function' ? setup : null, update: typeof update === 'function' ? update : null, draw: typeof draw === 'function' ? draw : null };`)(game);
    if (!hooks.draw) throw new Error('Crie uma função draw(game) em main.js.');
    hooks.setup?.(game);
    if (window.parent !== window) window.parent.postMessage({ mobileForge: true, type: 'ready' }, '*');
  } catch (error) { report(error); return; }
  let last = performance.now();
  let running = true;
  function frame(now) {
    if (!running) return;
    const dt = Math.min(Math.max((now - last) / 1000, 0), 0.05);
    last = now;
    try { hooks.update?.(game, dt); hooks.draw(game); }
    catch (error) { running = false; report(error); return; }
    requestAnimationFrame(frame);
  }
  requestAnimationFrame(frame);
};
