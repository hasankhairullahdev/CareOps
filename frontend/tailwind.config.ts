import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./app/**/*.{js,ts,jsx,tsx,mdx}",
    "./components/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: { DEFAULT: "#2E37A4", light: "#EEF0FF", hover: "#252d8a" },
        teal:    { DEFAULT: "#00D3C7", light: "#E6FAF9" },
        danger:  { DEFAULT: "#FF3667", light: "#FFE9EF" },
        warning: { DEFAULT: "#FFBC00", light: "#FFF8E0" },
        success: { DEFAULT: "#5CB85C", light: "#EFF9EF" },
        sidebar: "#ffffff",
      },
      boxShadow: {
        card:  "0 1px 6px rgba(0,0,0,0.06)",
        card2: "0 4px 24px rgba(0,0,0,0.08)",
      },
      borderRadius: { "2xl": "16px" },
    },
  },
  plugins: [],
};
export default config;
