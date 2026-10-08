/* ============================================================
   بطاقات الدخول بـ QR الموقّع — منتزه المرح العائلي
   SQL Server Express / LocalDB.
   تم الربط مع جداول Beneficiaries و Organizations في ParkGate.
   ============================================================ */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* المفاتيح العامة لأجهزة الإصدار. المفتاح الخاص لا يُخزَّن هنا أبداً. */
IF OBJECT_ID(N'dbo.QrSigningKeys', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.QrSigningKeys
    (
        KeyId          tinyint         NOT NULL CONSTRAINT PK_QrSigningKeys PRIMARY KEY,
        PublicKeySpki  varbinary(200)  NOT NULL CONSTRAINT UQ_QrSigningKeys_PublicKey UNIQUE,
        StationName    nvarchar(100)   NOT NULL,
        CreatedAt      datetime2(0)    NOT NULL CONSTRAINT DF_QrSigningKeys_CreatedAt DEFAULT SYSUTCDATETIME(),
        RevokedAt      datetime2(0)    NULL      -- عند اشتباه اختراق جهاز الإصدار: كل بطاقاته تُرفض فوراً
    );
END
GO

/* بطاقات الدخول لكل مستفيد */
IF OBJECT_ID(N'dbo.EntryCards', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EntryCards
    (
        Id             int IDENTITY(1,1) NOT NULL CONSTRAINT PK_EntryCards PRIMARY KEY,
        CardId         uniqueidentifier  NOT NULL CONSTRAINT UQ_EntryCards_CardId UNIQUE,
        BeneficiaryId  uniqueidentifier  NOT NULL CONSTRAINT FK_EntryCards_Beneficiaries REFERENCES dbo.Beneficiaries(Id),
        KeyId          tinyint           NOT NULL CONSTRAINT FK_EntryCards_Keys REFERENCES dbo.QrSigningKeys(KeyId),
        IssuedAt       datetime2(0)      NOT NULL CONSTRAINT DF_EntryCards_IssuedAt DEFAULT SYSUTCDATETIME(),
        ExpiresOn      date              NOT NULL,
        RevokedAt      datetime2(0)      NULL,
        RevokeReason   nvarchar(200)     NULL
    );
END
GO

/* بطاقة فعّالة واحدة فقط لكل مستفيد */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_EntryCards_OneActivePerBeneficiary' AND object_id = OBJECT_ID(N'dbo.EntryCards'))
BEGIN
    CREATE UNIQUE INDEX UX_EntryCards_OneActivePerBeneficiary
        ON dbo.EntryCards(BeneficiaryId) WHERE RevokedAt IS NULL;
END
GO

/* سجل كل محاولات المسح (المقبولة والمرفوضة) — للتدقيق وكشف محاولات التزوير */
IF OBJECT_ID(N'dbo.CardScanLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CardScanLog
    (
        Id           bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_CardScanLog PRIMARY KEY,
        ScannedAt    datetime2(0)     NOT NULL CONSTRAINT DF_CardScanLog_ScannedAt DEFAULT SYSUTCDATETIME(),
        StationName  nvarchar(100)    NOT NULL,
        Status       tinyint          NOT NULL,  -- = CardScanStatus: 0 مقبول، 1 غير مقروء، 2 مفتاح غير معتمد، 3 توقيع مزور، 4 منتهية، 5 غير مسجلة، 6 ملغاة، 7 تصريح موقوف
        CardId       uniqueidentifier NULL,
        RawPrefix    nvarchar(32)     NULL       -- أول 32 حرفاً من الرمز المرفوض
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CardScanLog_ScannedAt' AND object_id = OBJECT_ID(N'dbo.CardScanLog'))
BEGIN
    CREATE INDEX IX_CardScanLog_ScannedAt ON dbo.CardScanLog(ScannedAt) INCLUDE (Status, StationName);
END
GO

/* ما تقرأه البوابة عند المسح — IEntryCardRepository.FindAsync */
CREATE OR ALTER VIEW dbo.vw_EntryCardLookup
AS
SELECT  c.CardId,
        c.BeneficiaryId,
        CAST(IIF(c.RevokedAt IS NULL, 0, 1) AS bit) AS IsRevoked,
        CAST(IIF(o.IsActive = 1, 1, 0) AS bit) AS PermitActive
FROM    dbo.EntryCards c
INNER JOIN dbo.Beneficiaries b ON b.Id = c.BeneficiaryId
INNER JOIN dbo.Organizations o ON o.Id = b.OrganizationId;
GO

/* إصدار بطاقة: يلغي الفعالة السابقة ويضيف الجديدة في معاملة واحدة */
CREATE OR ALTER PROCEDURE dbo.usp_IssueEntryCard
    @BeneficiaryId uniqueidentifier,
    @CardId        uniqueidentifier,
    @KeyId         tinyint,
    @ExpiresOn     date
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRAN;
        UPDATE dbo.EntryCards
           SET RevokedAt = SYSUTCDATETIME(),
               RevokeReason = N'استُبدلت ببطاقة جديدة'
         WHERE BeneficiaryId = @BeneficiaryId
           AND RevokedAt IS NULL;

        INSERT dbo.EntryCards (CardId, BeneficiaryId, KeyId, ExpiresOn)
        VALUES (@CardId, @BeneficiaryId, @KeyId, @ExpiresOn);
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RevokeEntryCard
    @CardId uniqueidentifier,
    @Reason nvarchar(200)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.EntryCards
       SET RevokedAt = SYSUTCDATETIME(), RevokeReason = @Reason
     WHERE CardId = @CardId AND RevokedAt IS NULL;
END
GO

/* تسجيل المفتاح العام لجهاز إصدار — يعيد KeyId الموجود إن سبق تسجيله */
CREATE OR ALTER PROCEDURE dbo.usp_RegisterQrSigningKey
    @PublicKeySpki varbinary(200),
    @StationName   nvarchar(100),
    @KeyId         tinyint OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRAN;
        DECLARE @Revoked datetime2(0);

        SELECT @KeyId = KeyId, @Revoked = RevokedAt
          FROM dbo.QrSigningKeys WITH (UPDLOCK, HOLDLOCK)
         WHERE PublicKeySpki = @PublicKeySpki;

        IF @KeyId IS NULL
        BEGIN
            SELECT @KeyId = ISNULL(MAX(KeyId), 0) + 1
              FROM dbo.QrSigningKeys WITH (UPDLOCK, HOLDLOCK);

            INSERT dbo.QrSigningKeys (KeyId, PublicKeySpki, StationName)
            VALUES (@KeyId, @PublicKeySpki, @StationName);
        END
        ELSE IF @Revoked IS NOT NULL
            THROW 50001, N'مفتاح التوقيع لهذا الجهاز ملغى. احذف المفتاح المحلي (CngSigningKeyStore.Delete) وأعد التشغيل.', 1;
    COMMIT;
END
GO
