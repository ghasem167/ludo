import resolve from "@rollup/plugin-node-resolve";
import commonjs from "@rollup/plugin-commonjs";
import typescript from "@rollup/plugin-typescript";

export default {
    input: "src/main.ts",

    output: {
        file: "build/index.js",
        format: "cjs",
        name: "NakamaModule"
    },

    treeshake: false,

    plugins: [
        resolve(),
        commonjs(),
        typescript({
            tsconfig: "./tsconfig.json"
        })
    ]
};