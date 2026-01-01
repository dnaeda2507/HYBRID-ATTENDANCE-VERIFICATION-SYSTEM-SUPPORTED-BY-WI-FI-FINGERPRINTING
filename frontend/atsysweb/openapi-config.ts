import type { ConfigFile } from "@rtk-query/codegen-openapi";

const config: ConfigFile = {
  schemaFile: "http://localhost:5000/swagger/v1/swagger.json",
  apiFile: "./Redux/emptyApi.ts",
  apiImport: "emptySplitApi",
  outputFile: "./Redux/generatedTypes.ts",
  exportName: "atsysApi",
  hooks: true,
};

export default config;
