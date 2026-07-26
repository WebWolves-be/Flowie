/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}"
  ],
  theme: {
    extend: {
      spacing: {
        "safe-t": "env(safe-area-inset-top)",
        "safe-b": "env(safe-area-inset-bottom)",
        "safe-l": "env(safe-area-inset-left)",
        "safe-r": "env(safe-area-inset-right)",
        // App chrome heights combined with their safe-area inset, so a single
        // utility keeps content clear of the notch and the home indicator.
        "header-safe": "calc(3.5rem + env(safe-area-inset-top))",
        "nav-safe": "calc(4rem + env(safe-area-inset-bottom))",
        // Apple's minimum comfortable touch target.
        touch: "2.75rem"
      },
      minHeight: {
        touch: "2.75rem",
        dvh: "100dvh"
      },
      minWidth: {
        touch: "2.75rem"
      },
      height: {
        // Dynamic viewport height — shrinks when the iOS keyboard or the
        // Safari toolbars are on screen, unlike 100vh.
        dvh: "100dvh",
        "header-safe": "calc(3.5rem + env(safe-area-inset-top))",
        "nav-safe": "calc(4rem + env(safe-area-inset-bottom))"
      },
      maxHeight: {
        dvh: "100dvh"
      }
    },
  },
  plugins: [],
};
