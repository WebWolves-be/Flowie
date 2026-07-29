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
        // A dialog header needs its own padding *plus* the inset. `pt-safe-t`
        // alone overwrites the `py-4` that precedes it, leaving zero padding
        // above the title on any device without a notch.
        "4-safe-t": "calc(1rem + env(safe-area-inset-top))"
      },
      minHeight: {
        dvh: "100dvh"
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
