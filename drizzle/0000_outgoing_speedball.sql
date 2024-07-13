CREATE TABLE IF NOT EXISTS "pages" (
	"id" serial PRIMARY KEY NOT NULL,
	"article" text NOT NULL,
	"availabilityUrl" text NOT NULL,
	"articleUrl" text NOT NULL,
	"neededSku" text,
	"notNeededSkus" text,
	"status" text NOT NULL,
	"createdTime" timestamp DEFAULT now() NOT NULL,
	"archivedTime" timestamp
);
--> statement-breakpoint
CREATE UNIQUE INDEX IF NOT EXISTS "uniqueAvailabilityUrlIdx" ON "pages" USING btree ("availabilityUrl");--> statement-breakpoint
CREATE UNIQUE INDEX IF NOT EXISTS "uniqueNeededSkuIdx" ON "pages" USING btree ("neededSku");