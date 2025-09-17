import { createEnv } from "@t3-oss/env-core";
import { z } from "zod";
import { loadEnv } from "vite";

const viteEnv = loadEnv("", process.cwd(), "VITE_");

export const env = createEnv({
  server: {},

  clientPrefix: "VITE_",
  client: {
    VITE_API_BASE_URL: z.string().min(1),
  },

  runtimeEnv: viteEnv,
});
