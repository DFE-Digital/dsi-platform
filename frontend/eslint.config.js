const globals = require("globals");
const pluginJest = require("eslint-plugin-jest");
const pluginJs = require("@eslint/js");

module.exports = [
  {
    plugins: {
      jest: pluginJest,
    },
  },

  pluginJs.configs.recommended,

  {
    ignores: ["coverage/**", "build/**", "dist/**"],
  },

  {
    files: ["**/*.js"],
    languageOptions: {
      globals: {
        ...globals.node,
        $: true,
        window: true,
        GOVUK: true,
        document: true,
        location: true,
      },
      ecmaVersion: "latest",
      sourceType: "commonjs",
      parserOptions: {
        ecmaFeatures: {
          impliedStrict: true,
        },
      },
    },
    rules: {
      strict: ["error", "never"],
    },
  },
];
