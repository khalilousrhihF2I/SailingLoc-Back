CREATE TYPE [dbo].[AddressType] AS TABLE (
    [Street]     NVARCHAR (256) NOT NULL,
    [City]       NVARCHAR (128) NOT NULL,
    [State]      NVARCHAR (128) NOT NULL,
    [PostalCode] NVARCHAR (32)  NOT NULL,
    [Country]    NVARCHAR (128) NOT NULL);

