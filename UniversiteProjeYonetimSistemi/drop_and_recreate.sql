-- Önce mevcut tabloyu düşürüyoruz
IF OBJECT_ID('DanismanlikGorusmeleri', 'U') IS NOT NULL
    DROP TABLE DanismanlikGorusmeleri;

-- Şimdi tabloyu yeniden oluşturuyoruz (NO ACTION ile)
CREATE TABLE DanismanlikGorusmeleri (
    Id int NOT NULL IDENTITY(1,1),
    ProjeId int NOT NULL,
    AkademisyenId int NOT NULL,
    OgrenciId int NOT NULL,
    Baslik nvarchar(100) NOT NULL,
    GorusmeTarihi datetime2 NOT NULL,
    GorusmeTipi nvarchar(max) NOT NULL,
    Notlar nvarchar(max) NULL,
    Durum nvarchar(max) NOT NULL,
    CreatedAt datetime2 NOT NULL,
    UpdatedAt datetime2 NOT NULL,
    CONSTRAINT PK_DanismanlikGorusmeleri PRIMARY KEY (Id),
    CONSTRAINT FK_DanismanlikGorusmeleri_Akademisyenler_AkademisyenId FOREIGN KEY (AkademisyenId) REFERENCES Akademisyenler (Id) ON DELETE NO ACTION,
    CONSTRAINT FK_DanismanlikGorusmeleri_Ogrenciler_OgrenciId FOREIGN KEY (OgrenciId) REFERENCES Ogrenciler (Id) ON DELETE NO ACTION,
    CONSTRAINT FK_DanismanlikGorusmeleri_Projeler_ProjeId FOREIGN KEY (ProjeId) REFERENCES Projeler (Id) ON DELETE NO ACTION
);

-- İndeksleri oluşturuyoruz
CREATE INDEX IX_DanismanlikGorusmeleri_AkademisyenId ON DanismanlikGorusmeleri (AkademisyenId);
CREATE INDEX IX_DanismanlikGorusmeleri_OgrenciId ON DanismanlikGorusmeleri (OgrenciId);
CREATE INDEX IX_DanismanlikGorusmeleri_ProjeId ON DanismanlikGorusmeleri (ProjeId); 