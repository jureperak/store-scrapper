import "@/lib/config";
import { drizzle } from "drizzle-orm/vercel-postgres";
import { sql } from "@vercel/postgres";
import { pages } from "./schema";
import * as schema from "./schema";

export const db = drizzle(sql, { schema });

export const getPages = async () => {
  const selectResult = await db.select().from(pages);
  console.log("Results", selectResult);
  return selectResult;
};

export type SelectPage = typeof pages.$inferSelect;
export type InsertPage = typeof pages.$inferInsert;

export const insertPage = async (page: InsertPage) => {
  return db.insert(pages).values(page).returning();
};

export const getPages2 = async (): Promise<SelectPage[]> => {
  const result: SelectPage[] = await db.query.pages.findMany();
  return result;
};
