MERGE INTO "Templates" AS target
    USING (
        VALUES
            (1, 'Our Story', 'our-story'),
            (2, 'Memories', 'memories'),
            (3, 'Forever', 'forever')
    ) AS source ("Id", "Name", "IconId")
    ON target."Id" = source."Id"

    WHEN MATCHED THEN
        UPDATE SET
            "Name" = source."Name",
            "IconId" = source."IconId"

    WHEN NOT MATCHED THEN
        INSERT ("Id", "Name", "IconId")
            VALUES (source."Id", source."Name", source."IconId");