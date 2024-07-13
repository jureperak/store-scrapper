import {
  pgTable,
  serial,
  text,
  timestamp,
  uniqueIndex,
} from "drizzle-orm/pg-core";

export const pages = pgTable(
  "pages",
  {
    id: serial("id").primaryKey(),
    article: text("article").notNull(),
    availabilityUrl: text("availabilityUrl").notNull(),
    articleUrl: text("articleUrl").notNull(),
    neededSku: text("neededSku"),
    notNeededSkus: text("notNeededSkus"),
    status: text("status").notNull(),
    createdTime: timestamp("createdTime").defaultNow().notNull(),
    archivedTime: timestamp("archivedTime"),
  },
  (pages) => {
    return {
      uniqueAvailabilityUrlIdx: uniqueIndex("uniqueAvailabilityUrlIdx").on(
        pages.availabilityUrl
      ),
      uniqueNeededSkuIdx: uniqueIndex("uniqueNeededSkuIdx").on(pages.neededSku),
    };
  }
);
