-- Migration: Add Slug column to Boats table
-- Date: 2026-03-28
-- Description: Adds nullable Slug column for SEO-friendly boat URLs

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Boats' AND COLUMN_NAME = 'Slug'
)
BEGIN
    ALTER TABLE [dbo].[Boats] ADD [Slug] NVARCHAR(300) NULL;
    
    -- Create unique index on Slug (filtered to non-null)
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Boats_Slug] 
        ON [dbo].[Boats] ([Slug]) 
        WHERE [Slug] IS NOT NULL;
    
    PRINT 'Added Slug column and index to Boats table.';
END
ELSE
BEGIN
    PRINT 'Slug column already exists on Boats table. Skipping.';
END
GO

-- Backfill existing boats with slugs (lowercase name + id)
UPDATE [dbo].[Boats]
SET [Slug] = LOWER(
    REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
            LTRIM(RTRIM([Name])),
            ' ', '-'),
            '''', ''),
            '.', ''),
            ',', ''),
            '/', '-'),
            '(', ''),
            ')', ''),
            '&', 'et'),
            'é', 'e'),
            'è', 'e')
    ) + '-' + CAST([Id] AS NVARCHAR(20))
WHERE [Slug] IS NULL;
GO
