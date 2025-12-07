CREATE PROCEDURE sp_SeedTestData
AS
BEGIN
    SET NOCOUNT ON;

    ------------------------------------------------------------------
    -- PASSWORD HASH MOCK (utilise un vrai hash Identity en prod)
    ------------------------------------------------------------------
    DECLARE @PasswordHash NVARCHAR(MAX) =
        'AQAAAAIAAYagAAAAEGIempla0OHVlebMR6MImMIYeycuMDU4n/CfaVu+v6IOWyG63nskYQoBc4dkLcHPRg==';


    ------------------------------------------------------------------
    -- USERS : Admin + Owners + Renters (UNIQUEMENT SI MANQUANTS)
    ------------------------------------------------------------------

    -- Admin principal SailingLoc
    IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'admin@sailingloc.com')
    BEGIN
        INSERT INTO AspNetUsers
        (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
         PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber,
         FirstName, LastName, BirthDate,
         Address_Street, Address_City, Address_State, Address_PostalCode, Address_Country,
         Status, UserType, Verified, MemberSince, AvatarUrl, CreatedAt, UpdatedAt)
        VALUES
        (NEWID(), 'admin@sailingloc.com', 'ADMIN@SAILINGLOC.COM',
         'admin@sailingloc.com', 'ADMIN@SAILINGLOC.COM', 1,
         @PasswordHash, NEWID(), NEWID(), '+33123456789',
         'Administrateur', 'SailingLoc', '1985-01-01',
         '', '', '', '', 'France',
         1, 'admin', 1, GETUTCDATE(), NULL, GETUTCDATE(), GETUTCDATE());
    END

    -- Owner 1
    IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'jean.dupont@example.com')
    BEGIN
        INSERT INTO AspNetUsers
        (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
         PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber,
         FirstName, LastName, BirthDate,
         Address_Street, Address_City, Address_State, Address_PostalCode, Address_Country,
         Status, UserType, Verified, MemberSince, AvatarUrl, CreatedAt, UpdatedAt)
        VALUES
        (NEWID(), 'jean.dupont@example.com', 'JEAN.DUPONT@EXAMPLE.COM',
         'jean.dupont@example.com', 'JEAN.DUPONT@EXAMPLE.COM', 1,
         @PasswordHash, NEWID(), NEWID(), '+33612345678',
         'Jean', 'Dupont', '1988-01-01',
         '', '', '', '', 'France',
         1, 'owner', 1, DATEADD(MONTH, -24, GETUTCDATE()), NULL, GETUTCDATE(), GETUTCDATE());
    END

    -- Owner 2
    IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'marie.martin@example.com')
    BEGIN
        INSERT INTO AspNetUsers
        (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
         PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber,
         FirstName, LastName, BirthDate,
         Address_Street, Address_City, Address_State, Address_PostalCode, Address_Country,
         Status, UserType, Verified, MemberSince, AvatarUrl, CreatedAt, UpdatedAt)
        VALUES
        (NEWID(), 'marie.martin@example.com', 'MARIE.MARTIN@EXAMPLE.COM',
         'marie.martin@example.com', 'MARIE.MARTIN@EXAMPLE.COM', 1,
         @PasswordHash, NEWID(), NEWID(), '+33698765432',
         'Marie', 'Martin', '1990-01-01',
         '', '', '', '', 'France',
         1, 'owner', 1, DATEADD(MONTH, -18, GETUTCDATE()), NULL, GETUTCDATE(), GETUTCDATE());
    END

    -- Renter 1
    IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'thomas.petit@example.com')
    BEGIN
        INSERT INTO AspNetUsers
        (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
         PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber,
         FirstName, LastName, BirthDate,
         Address_Street, Address_City, Address_State, Address_PostalCode, Address_Country,
         Status, UserType, Verified, MemberSince, AvatarUrl, CreatedAt, UpdatedAt)
        VALUES
        (NEWID(), 'thomas.petit@example.com', 'THOMAS.PETIT@EXAMPLE.COM',
         'thomas.petit@example.com', 'THOMAS.PETIT@EXAMPLE.COM', 1,
         @PasswordHash, NEWID(), NEWID(), '+33687654321',
         'Thomas', 'Petit', '1995-01-01',
         '', '', '', '', 'France',
         1, 'renter', 1, DATEADD(MONTH, -12, GETUTCDATE()), NULL, GETUTCDATE(), GETUTCDATE());
    END

    -- Renter 2
    IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'sophie.bernard@example.com')
    BEGIN
        INSERT INTO AspNetUsers
        (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
         PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber,
         FirstName, LastName, BirthDate,
         Address_Street, Address_City, Address_State, Address_PostalCode, Address_Country,
         Status, UserType, Verified, MemberSince, AvatarUrl, CreatedAt, UpdatedAt)
        VALUES
        (NEWID(), 'sophie.bernard@example.com', 'SOPHIE.BERNARD@EXAMPLE.COM',
         'sophie.bernard@example.com', 'SOPHIE.BERNARD@EXAMPLE.COM', 1,
         @PasswordHash, NEWID(), NEWID(), '+33676543210',
         'Sophie', 'Bernard', '1993-01-01',
         '', '', '', '', 'France',
         1, 'renter', 1, DATEADD(MONTH, -6, GETUTCDATE()), NULL, GETUTCDATE(), GETUTCDATE());
    END

    ------------------------------------------------------------------
    -- VARS réutilisables (owners, renters, admin pour documents)
    ------------------------------------------------------------------
    DECLARE @OwnerA UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserType='owner' ORDER BY FirstName);
    DECLARE @OwnerB UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserType='owner' ORDER BY FirstName DESC);

    DECLARE @RenterA UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM AspNetUsers WHERE Email='thomas.petit@example.com');
    DECLARE @RenterB UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM AspNetUsers WHERE Email='sophie.bernard@example.com');

    DECLARE @AdminVerifier UNIQUEIDENTIFIER =
    (
        SELECT TOP 1 u.Id
        FROM AspNetUsers u
        JOIN AspNetUserRoles ur ON ur.UserId = u.Id
        JOIN AspNetRoles r ON r.Id = ur.RoleId
        WHERE r.Name = 'Admin'
        ORDER BY u.Email
    );

    IF @AdminVerifier IS NULL
    BEGIN
        SELECT TOP 1 @AdminVerifier = Id FROM AspNetUsers WHERE UserType = 'admin';
    END


------------------------------------------------------------------
-- DESTINATIONS (7) - FIXED COLUMN ALIAS
------------------------------------------------------------------
SET IDENTITY_INSERT Destinations ON;

INSERT INTO Destinations
(Id, Name, Region, Country, Description, AveragePrice, PopularMonths, Highlights)
SELECT *
FROM (VALUES
    (1, 'Côte d''Azur', 'Provence-Alpes-Côte d''Azur', 'France',
     'La Côte d''Azur offre des eaux cristallines et des paysages exceptionnels.',
     450, '["Mai","Juin","Juillet","Août","Septembre"]',
     '["Calanques de Cassis","Îles de Lérins","Saint-Tropez","Monaco","Antibes"]'),

    (2, 'Grèce', 'Méditerranée', 'Grèce',
     'Explorez les îles grecques et leur beauté intemporelle.',
     380, '["Juin","Juillet","Août","Septembre"]',
     '["Santorin","Mykonos","Cyclades","Îles Ioniennes","Crète"]'),

    (3, 'Corse', 'Corse', 'France',
     'L''île de beauté avec ses criques sauvages et ses montagnes.',
     420, '["Juin","Juillet","Août","Septembre"]',
     '["Bonifacio","Calvi","Porto-Vecchio","Réserve de Scandola","Calanques de Piana"]'),

    (4, 'Croatie', 'Adriatique', 'Croatie',
     'Des milliers d''îles à découvrir le long de la côte dalmate.',
     350, '["Mai","Juin","Juillet","Août","Septembre"]',
     '["Dubrovnik","Split","Îles Kornati","Hvar","Zadar"]'),

    (5, 'Baléares', 'Îles Baléares', 'Espagne',
     'Majorque, Minorque, Ibiza et Formentera vous attendent.',
     320, '["Mai","Juin","Juillet","Août","Septembre","Octobre"]',
     '["Majorque","Minorque","Ibiza","Formentera","Cabrera"]'),

    (6, 'Bretagne', 'Bretagne', 'France',
     'Découvrez les côtes bretonnes et leurs traditions maritimes.',
     280, '["Juin","Juillet","Août"]',
     '["Golfe du Morbihan","Belle-Île","Archipel des Glénan","Roscoff","Cancale"]'),

    (7, 'Sardaigne', 'Sardaigne', 'Italie',
     'Eaux turquoise et plages paradisiaques de la Méditerranée.',
     390, '["Juin","Juillet","Août","Septembre"]',
     '["Costa Smeralda","Archipel de La Maddalena","Alghero","Cagliari","Golfe d''Orosei"]')
) AS D(Id, Name, Region, Country, Description, AveragePrice, PopularMonths, Highlights)
WHERE NOT EXISTS (SELECT 1 FROM Destinations WHERE Id = D.Id);

SET IDENTITY_INSERT Destinations OFF;



------------------------------------------------------------------
-- BOATS (14) - FIXED COLUMN ALIAS
------------------------------------------------------------------
SET IDENTITY_INSERT Boats ON;

INSERT INTO Boats
(Id, Name, Type, Location, City, DestinationId, Country,
 Price, Capacity, Cabins, Length, Year, Rating, ReviewCount,
 Equipment, Description, OwnerId, IsActive, IsVerified)
SELECT *
FROM (VALUES
    (1,'Bénéteau Oceanis 45','sailboat','Nice','Nice',1,'France',350,8,4,13.5,2018,4.8,12,'["GPS","Pilote automatique","Guindeau électrique","Annexe avec moteur"]','Magnifique voilier pour découvrir la Côte d''Azur',@OwnerA,1,1),
    (2,'Lagoon 42 Premium','catamaran','Athènes','Athènes',2,'Grèce',580,10,4,12.8,2020,4.9,18,'["GPS","Pilote automatique","Climatisation","Dessalinisateur","Annexe avec moteur"]','Catamaran de luxe pour explorer les Cyclades',@OwnerB,1,1),
    (3,'Jeanneau Sun Odyssey 419','sailboat','Ajaccio','Ajaccio',3,'France',320,8,3,12.5,2019,4.7,9,'["GPS","Guindeau électrique","Annexe"]','Idéal pour naviguer autour de la Corse',@OwnerA,1,1),
    (4,'Bavaria Cruiser 46','sailboat','Split','Split',4,'Croatie',380,10,4,14.3,2017,4.6,15,'["GPS","Pilote automatique","Guindeau électrique"]','Parfait pour explorer la côte croate',@OwnerB,1,1),
    (5,'Fountaine Pajot Astrea 42','catamaran','Palma','Palma de Majorque',5,'Espagne',520,10,4,12.6,2021,4.9,14,'["GPS","Pilote automatique","Climatisation","Dessalinisateur"]','Catamaran moderne pour les Baléares',@OwnerA,1,1),
    (6,'Dufour 460 Grand Large','sailboat','La Rochelle','La Rochelle',6,'France',420,10,5,14.15,2019,4.8,11,'["GPS","Pilote automatique","Guindeau électrique","Annexe avec moteur"]','Grand voilier confortable pour la Bretagne',@OwnerB,1,1),
    (7,'Bali 4.3 Catamaran','catamaran','Cagliari','Cagliari',7,'Italie',550,12,4,13.1,2020,5,8,'["GPS","Pilote automatique","Climatisation","Dessalinisateur","Annexe avec moteur"]','Catamaran spacieux pour la Sardaigne',@OwnerA,1,1),
    (8,'Zodiac Medline 850','semirigid','Nice','Nice',1,'France',180,12,0,8.5,2021,4.5,6,'["GPS","Sondeur","Bimini","Échelle de bain"]','Semi-rigide rapide pour balades côtières',@OwnerB,1,1),
    (9,'Bavaria 50 Cruiser','sailboat','Mykonos','Mykonos',2,'Grèce',450,12,5,15.4,2018,4.7,13,'["GPS","Pilote automatique","Climatisation","Guindeau électrique"]','Grand voilier luxueux pour les Cyclades',@OwnerA,1,1),
    (10,'Prestige 520 Fly','motor','Cannes','Cannes',1,'France',890,8,3,15.9,2019,4.9,7,'["GPS","Pilote automatique","Climatisation","Bow thruster","Annexe avec moteur"]','Yacht à moteur de prestige',@OwnerB,1,1),
    (11,'Jeanneau Leader 30','motor','Saint-Tropez','Saint-Tropez',1,'France',420,6,1,9.14,2020,4.6,10,'["GPS","Sondeur","Bimini","Plateforme de bain"]','Bateau à moteur idéal pour la journée',@OwnerA,1,1),
    (12,'Hanse 458','sailboat','Porto-Vecchio','Porto-Vecchio',3,'France',410,10,4,14.0,2018,4.8,12,'["GPS","Pilote automatique","Guindeau électrique"]','Voilier performant et confortable',@OwnerB,1,1),
    (13,'Lagoon 450 F','catamaran','Rhodes','Rhodes',2,'Grèce',620,12,4,13.96,2017,4.9,16,'["GPS","Pilote automatique","Climatisation","Dessalinisateur","Annexe avec moteur"]','Catamaran spacieux et rapide',@OwnerA,1,1),
    (14,'Bénéteau First 40.7','sailboat','Barcelone','Barcelone',5,'Espagne',290,8,3,12.37,2016,4.5,14,'["GPS","Pilote automatique","Guindeau électrique"]','Voilier sportif et maniable',@OwnerB,1,1)
) AS B(Id, Name, Type, Location, City, DestinationId, Country,
       Price, Capacity, Cabins, Length, Year, Rating, ReviewCount,
       Equipment, Description, OwnerId, IsActive, IsVerified)
WHERE NOT EXISTS (SELECT 1 FROM Boats WHERE Id = B.Id);

SET IDENTITY_INSERT Boats OFF;



    ------------------------------------------------------------------
    -- BOAT IMAGES (1..3 images pour certains bateaux)
    ------------------------------------------------------------------
    INSERT INTO BoatImages (BoatId, ImageUrl, Caption, DisplayOrder, CreatedAt)
    SELECT *
    FROM (VALUES
        (1, '/images/boats/1-main.jpg', 'Vue générale du voilier', 1, GETUTCDATE()),
        (1, '/images/boats/1-cockpit.jpg', 'Cockpit spacieux', 2, GETUTCDATE()),
        (1, '/images/boats/1-cabins.jpg', 'Cabines confortables', 3, GETUTCDATE()),

        (2, '/images/boats/2-main.jpg', 'Catamaran Lagoon 42', 1, GETUTCDATE()),
        (2, '/images/boats/2-salon.jpg', 'Carré lumineux', 2, GETUTCDATE()),

        (5, '/images/boats/5-main.jpg', 'Astrea 42 au mouillage', 1, GETUTCDATE()),

        (10, '/images/boats/10-main.jpg', 'Yacht Prestige 520 Fly', 1, GETUTCDATE())
    ) AS BI(BoatId, ImageUrl, Caption, DisplayOrder, CreatedAt)
    WHERE NOT EXISTS (
        SELECT 1 FROM BoatImages 
        WHERE BoatId = BI.BoatId AND DisplayOrder = BI.DisplayOrder
    );


    ------------------------------------------------------------------
    -- BOAT AVAILABILITY (quelques plages d'indispo / dispo)
    ------------------------------------------------------------------
    INSERT INTO BoatAvailability (BoatId, StartDate, EndDate, IsAvailable, Reason, CreatedAt)
    SELECT *
    FROM (VALUES
        (1, '2025-06-01', '2025-06-30', 1, NULL, GETUTCDATE()),
        (1, '2025-07-01', '2025-07-07', 0, 'Révision moteur', GETUTCDATE()),

        (2, '2025-07-01', '2025-07-31', 1, NULL, GETUTCDATE()),
        (2, '2025-08-01', '2025-08-15', 0, 'Bloqué par le propriétaire', GETUTCDATE()),

        (5, '2025-08-01', '2025-08-31', 1, NULL, GETUTCDATE())
    ) AS BA(BoatId, StartDate, EndDate, IsAvailable, Reason, CreatedAt)
    WHERE NOT EXISTS (
        SELECT 1 
        FROM BoatAvailability 
        WHERE BoatId = BA.BoatId 
          AND StartDate = BA.StartDate 
          AND EndDate = BA.EndDate
    );


    ------------------------------------------------------------------
    -- REVIEWS (10, comme ton script)
    ------------------------------------------------------------------
    INSERT INTO Reviews (BoatId, UserId, UserName, Rating, Comment, CreatedAt)
    SELECT *
    FROM (VALUES
        (1, @RenterA, 'Thomas Petit', 5, 'Excellente semaine sur ce voilier ! Très bien équipé et confortable.', GETUTCDATE()),
        (1, @RenterB, 'Sophie Bernard', 5, 'Bateau impeccable, propriétaire très accueillant.', GETUTCDATE()),
        (2, @RenterA, 'Thomas Petit', 5, 'Catamaran de rêve ! Navigation facile et très spacieux.', GETUTCDATE()),
        (2, @RenterB, 'Sophie Bernard', 5, 'Parfait pour les Cyclades, nous avons adoré !', GETUTCDATE()),
        (3, @RenterA, 'Thomas Petit', 5, 'Belle découverte de la Corse, bateau en excellent état.', GETUTCDATE()),
        (4, @RenterB, 'Sophie Bernard', 4, 'Très bon voilier, quelques petits détails à améliorer.', GETUTCDATE()),
        (5, @RenterA, 'Thomas Petit', 5, 'Catamaran moderne et très confortable, hautement recommandé !', GETUTCDATE()),
        (7, @RenterB, 'Sophie Bernard', 5, 'Le meilleur catamaran que nous ayons loué !', GETUTCDATE()),
        (10, @RenterA, 'Thomas Petit', 5, 'Yacht magnifique, service 5 étoiles.', GETUTCDATE()),
        (13, @RenterB, 'Sophie Bernard', 5, 'Parfait pour notre croisière en Grèce.', GETUTCDATE())
    ) AS R(BoatId, UserId, UserName, Rating, Comment, CreatedAt)
    WHERE NOT EXISTS (
        SELECT 1 FROM Reviews 
        WHERE BoatId = R.BoatId 
          AND UserId = R.UserId 
          AND Rating = R.Rating
          AND Comment = R.Comment
    );


    ------------------------------------------------------------------
    -- BOOKINGS (3 réservations de test)
    ------------------------------------------------------------------
    INSERT INTO Bookings
    (Id, BoatId, RenterId, StartDate, EndDate, DailyPrice, Subtotal, ServiceFee, TotalPrice,
     Status, RenterName, RenterEmail, PaymentStatus)
    SELECT *
    FROM (VALUES
        ('BK202501', 1, @RenterA, '2025-06-15', '2025-06-22', 350, 2450, 245, 2695, 'confirmed', 'Thomas Petit', 'thomas.petit@example.com', 'succeeded'),
        ('BK202502', 2, @RenterB, '2025-07-01', '2025-07-08', 580, 4060, 406, 4466, 'confirmed', 'Sophie Bernard', 'sophie.bernard@example.com', 'succeeded'),
        ('BK202503', 5, @RenterA, '2025-08-10', '2025-08-17', 520, 3640, 364, 4004, 'pending', 'Thomas Petit', 'thomas.petit@example.com', 'pending')
    ) AS BK(Id, BoatId, RenterId, StartDate, EndDate, DailyPrice, Subtotal, ServiceFee, TotalPrice, Status, RenterName, RenterEmail, PaymentStatus)
    WHERE NOT EXISTS (SELECT 1 FROM Bookings WHERE Id = BK.Id);


    ------------------------------------------------------------------
    -- MESSAGES (échanges entre locataires et propriétaires)
    ------------------------------------------------------------------
    INSERT INTO Messages
    (SenderId, ReceiverId, BookingId, BoatId, Subject, Content, IsRead, ReadAt, CreatedAt)
    SELECT *
    FROM (VALUES
        (@RenterA, @OwnerA, 'BK202501', 1,
         'Question sur l''embarquement',
         'Bonjour, à quelle heure pouvons-nous embarquer le premier jour ?',
         0, NULL, GETUTCDATE()),

        (@OwnerA, @RenterA, 'BK202501', 1,
         'Re: Question sur l''embarquement',
         'Bonjour Thomas, vous pouvez embarquer à partir de 15h au port de Nice.',
         0, NULL, DATEADD(MINUTE, 5, GETUTCDATE())),

        (@RenterB, @OwnerB, 'BK202502', 2,
         'Itinéraire conseillé dans les Cyclades',
         'Bonjour, avez-vous des suggestions d''itinéraire pour une semaine ?',
         0, NULL, GETUTCDATE())
    ) AS M(SenderId, ReceiverId, BookingId, BoatId, Subject, Content, IsRead, ReadAt, CreatedAt)
    WHERE NOT EXISTS (
        SELECT 1 FROM Messages
        WHERE BookingId = M.BookingId
          AND SenderId = M.SenderId
          AND ReceiverId = M.ReceiverId
          AND Subject = M.Subject
    );


    ------------------------------------------------------------------
    -- USER DOCUMENTS (KYC : CNI, permis bateau, etc.)
    ------------------------------------------------------------------
    INSERT INTO UserDocuments
    (UserId, DocumentType, DocumentUrl, FileName, FileSize,
     IsVerified, VerifiedAt, VerifiedBy, UploadedAt)
    SELECT *
    FROM (VALUES
        (@OwnerA, 'ID Card', '/docs/users/owner1-id.pdf', 'owner1-id.pdf', 150000, 1, DATEADD(DAY, -20, GETUTCDATE()), @AdminVerifier, DATEADD(DAY, -22, GETUTCDATE())),
        (@OwnerA, 'Boat License', '/docs/users/owner1-license.pdf', 'owner1-license.pdf', 120000, 1, DATEADD(DAY, -18, GETUTCDATE()), @AdminVerifier, DATEADD(DAY, -19, GETUTCDATE())),

        (@OwnerB, 'ID Card', '/docs/users/owner2-id.pdf', 'owner2-id.pdf', 145000, 1, DATEADD(DAY, -15, GETUTCDATE()), @AdminVerifier, DATEADD(DAY, -16, GETUTCDATE())),

        (@RenterA, 'ID Card', '/docs/users/renter1-id.pdf', 'renter1-id.pdf', 130000, 1, DATEADD(DAY, -10, GETUTCDATE()), @AdminVerifier, DATEADD(DAY, -11, GETUTCDATE())),
        (@RenterB, 'ID Card', '/docs/users/renter2-id.pdf', 'renter2-id.pdf', 125000, 0, NULL, @AdminVerifier, DATEADD(DAY, -5, GETUTCDATE()))
    ) AS UD(UserId, DocumentType, DocumentUrl, FileName, FileSize, IsVerified, VerifiedAt, VerifiedBy, UploadedAt)
    WHERE NOT EXISTS (
        SELECT 1 
        FROM UserDocuments 
        WHERE UserId = UD.UserId 
          AND DocumentType = UD.DocumentType
    );


    ------------------------------------------------------------------
    -- UPDATE BOAT RATING (comme ton script)
    ------------------------------------------------------------------
    EXEC sp_UpdateBoatRating 1;
    EXEC sp_UpdateBoatRating 2;
    EXEC sp_UpdateBoatRating 3;
    EXEC sp_UpdateBoatRating 4;
    EXEC sp_UpdateBoatRating 5;
    EXEC sp_UpdateBoatRating 7;
    EXEC sp_UpdateBoatRating 10;
    EXEC sp_UpdateBoatRating 13;


    ------------------------------------------------------------------
    PRINT 'SAILINGLOC SEED COMPLETE — toutes les données de test ont été insérées / mises à jour.';
END
GO
