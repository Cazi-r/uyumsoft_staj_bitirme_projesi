
USE uyumsoft_staj_database;
GO

-- 1. Kullanicilar (Auth) Tablosu
CREATE TABLE kullanicilar (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ad NVARCHAR(50) NOT NULL,
    soyad NVARCHAR(50) NOT NULL,
    email NVARCHAR(100) NOT NULL UNIQUE,
    sifre NVARCHAR(255) NOT NULL,
    rol NVARCHAR(20) NOT NULL CHECK (rol IN ('Admin', 'Akademisyen', 'Ogrenci')),
    aktif BIT DEFAULT 1,
    son_giris DATETIME2,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- 2. Proje Kategorileri Tablosu
CREATE TABLE proje_kategorileri (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ad NVARCHAR(100) NOT NULL,
    aciklama NVARCHAR(MAX),
    renk NVARCHAR(7) DEFAULT '#3B82F6',
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- 3. Öğrenciler Tablosu
CREATE TABLE ogrenciler (
    id INT IDENTITY(1,1) PRIMARY KEY,
    kullanici_id INT FOREIGN KEY REFERENCES kullanicilar(id),
    ad NVARCHAR(50) NOT NULL,
    soyad NVARCHAR(50) NOT NULL,
    email NVARCHAR(100) NOT NULL UNIQUE,
    ogrenci_no NVARCHAR(10) NOT NULL UNIQUE,
    telefon NVARCHAR(15),
    adres NVARCHAR(MAX),
    kayit_tarihi DATETIME2 DEFAULT GETDATE(),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- 4. Akademisyenler Tablosu
CREATE TABLE akademisyenler (
    id INT IDENTITY(1,1) PRIMARY KEY,
    kullanici_id INT FOREIGN KEY REFERENCES kullanicilar(id),
    ad NVARCHAR(50) NOT NULL,
    soyad NVARCHAR(50) NOT NULL,
    email NVARCHAR(100) NOT NULL UNIQUE,
    unvan NVARCHAR(50) NOT NULL,
    telefon NVARCHAR(15),
    ofis NVARCHAR(50),
    uzmanlik_alani NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- 5. Projeler Tablosu
CREATE TABLE projeler (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ad NVARCHAR(100) NOT NULL,
    aciklama NVARCHAR(MAX) NOT NULL,
    olusturma_tarihi DATETIME2 DEFAULT GETDATE(),
    teslim_tarihi DATETIME2,
    status NVARCHAR(20) DEFAULT 'Beklemede' CHECK (status IN ('Beklemede', 'Atanmis', 'Devam', 'Tamamlandi', 'Iptal')),
    ogrenci_id INT FOREIGN KEY REFERENCES ogrenciler(id),
    mentor_id INT FOREIGN KEY REFERENCES akademisyenler(id),
    kategori_id INT FOREIGN KEY REFERENCES proje_kategorileri(id),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- 6. Proje Dosyaları Tablosu
CREATE TABLE proje_dosyalari (
    id INT IDENTITY(1,1) PRIMARY KEY,
    dosya_adi NVARCHAR(255) NOT NULL,
    dosya_yolu NVARCHAR(MAX) NOT NULL,
    dosya_tipi NVARCHAR(50),
    dosya_boyutu BIGINT DEFAULT 0,
    yukleme_tarihi DATETIME2 DEFAULT GETDATE(),
    proje_id INT FOREIGN KEY REFERENCES projeler(id) ON DELETE CASCADE,
    yukleyen_id INT,
    yukleyen_tipi NVARCHAR(20) DEFAULT 'Ogrenci' CHECK (yukleyen_tipi IN ('Ogrenci', 'Akademisyen')),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- 7. Proje Yorumları Tablosu
CREATE TABLE proje_yorumlari (
    id INT IDENTITY(1,1) PRIMARY KEY,
    icerik NVARCHAR(MAX) NOT NULL,
    olusturma_tarihi DATETIME2 DEFAULT GETDATE(),
    yorum_tipi NVARCHAR(20) DEFAULT 'Genel' CHECK (yorum_tipi IN ('Genel', 'Geri Bildirim', 'Soru', 'Onay')),
    proje_id INT FOREIGN KEY REFERENCES projeler(id) ON DELETE CASCADE,
    ogrenci_id INT FOREIGN KEY REFERENCES ogrenciler(id),
    akademisyen_id INT FOREIGN KEY REFERENCES akademisyenler(id),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    
    -- Sadece bir tane yorum yapan olabilir (ogrenci VEYA akademisyen)
    CONSTRAINT CHK_YorumYapan CHECK (
        (ogrenci_id IS NOT NULL AND akademisyen_id IS NULL) OR
        (ogrenci_id IS NULL AND akademisyen_id IS NOT NULL)
    )
);

-- 8. Değerlendirmeler Tablosu
CREATE TABLE degerlendirmeler (
    id INT IDENTITY(1,1) PRIMARY KEY,
    puan INT CHECK (puan >= 0 AND puan <= 100),
    aciklama NVARCHAR(MAX),
    degerlendirme_tarihi DATETIME2 DEFAULT GETDATE(),
    degerlendirme_tipi NVARCHAR(20) DEFAULT 'Genel' CHECK (degerlendirme_tipi IN ('Ara', 'Final', 'Genel')),
    proje_id INT FOREIGN KEY REFERENCES projeler(id) ON DELETE CASCADE,
    akademisyen_id INT FOREIGN KEY REFERENCES akademisyenler(id) ON DELETE CASCADE,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- 9. Bildirimler Tablosu
CREATE TABLE bildirimler (
    id INT IDENTITY(1,1) PRIMARY KEY,
    baslik NVARCHAR(200) NOT NULL,
    icerik NVARCHAR(MAX) NOT NULL,
    olusturma_tarihi DATETIME2 DEFAULT GETDATE(),
    okundu BIT DEFAULT 0,
    bildirim_tipi NVARCHAR(20) DEFAULT 'Bilgi' CHECK (bildirim_tipi IN ('Bilgi', 'Uyari', 'Hata', 'Basari')),
    ogrenci_id INT FOREIGN KEY REFERENCES ogrenciler(id) ON DELETE CASCADE,
    akademisyen_id INT FOREIGN KEY REFERENCES akademisyenler(id) ON DELETE CASCADE,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    
    -- Sadece bir tane al�c� olabilir (ogrenci VEYA akademisyen)
    CONSTRAINT CHK_BildirimAlici CHECK (
        (ogrenci_id IS NOT NULL AND akademisyen_id IS NULL) OR
        (ogrenci_id IS NULL AND akademisyen_id IS NOT NULL)
    )
);

-- 10. Proje Aşamaları Tablosu proje görevleri denebilir
CREATE TABLE proje_asamalari (
    id INT IDENTITY(1,1) PRIMARY KEY,
    proje_id INT FOREIGN KEY REFERENCES projeler(id) ON DELETE CASCADE,
    asama_adi NVARCHAR(100) NOT NULL,
    aciklama NVARCHAR(MAX),
    baslangic_tarihi DATETIME2,
    bitis_tarihi DATETIME2,
    tamamlandi BIT DEFAULT 0,
    tamamlanma_tarihi DATETIME2,
    sira_no INT NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- 11. Proje Kaynakları Tablosu
CREATE TABLE proje_kaynaklari (
    id INT IDENTITY(1,1) PRIMARY KEY,
    proje_id INT FOREIGN KEY REFERENCES projeler(id) ON DELETE CASCADE,
    kaynak_adi NVARCHAR(200) NOT NULL,
    kaynak_tipi NVARCHAR(50) CHECK (kaynak_tipi IN ('Kitap', 'Makale', 'Website', 'API', 'Dokuman', 'Video', 'Di�er')),
    url NVARCHAR(MAX),
    aciklama NVARCHAR(MAX),
    yazar NVARCHAR(100),
    yayin_tarihi DATE,
    ekleme_tarihi DATETIME2 DEFAULT GETDATE(),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- 12. Danışmanlık Görüşmeleri Tablosu
CREATE TABLE danismanlik_gorusmeleri (
    id INT IDENTITY(1,1) PRIMARY KEY,
    proje_id INT FOREIGN KEY REFERENCES projeler(id) ON DELETE CASCADE,
    akademisyen_id INT FOREIGN KEY REFERENCES akademisyenler(id),
    ogrenci_id INT FOREIGN KEY REFERENCES ogrenciler(id),
    baslik NVARCHAR(100),
    gorusme_tarihi DATETIME2 NOT NULL,
    gorusme_tipi NVARCHAR(20) CHECK (gorusme_tipi IN ('Online', 'Yüz Yüze')),
    notlar NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);


-- Indexler (Performans i�in)
CREATE INDEX idx_projeler_ogrenci ON projeler(ogrenci_id);
CREATE INDEX idx_projeler_mentor ON projeler(mentor_id);
CREATE INDEX idx_projeler_kategori ON projeler(kategori_id);
CREATE INDEX idx_projeler_status ON projeler(status);
CREATE INDEX idx_proje_dosyalari_proje ON proje_dosyalari(proje_id);
CREATE INDEX idx_proje_yorumlari_proje ON proje_yorumlari(proje_id);
CREATE INDEX idx_degerlendirmeler_proje ON degerlendirmeler(proje_id);
CREATE INDEX idx_bildirimler_ogrenci ON bildirimler(ogrenci_id);
CREATE INDEX idx_bildirimler_akademisyen ON bildirimler(akademisyen_id);
CREATE INDEX idx_bildirimler_okundu ON bildirimler(okundu);
CREATE INDEX idx_proje_asamalari_proje ON proje_asamalari(proje_id);
CREATE INDEX idx_proje_asamalari_sira ON proje_asamalari(sira_no);
CREATE INDEX idx_proje_kaynaklari_proje ON proje_kaynaklari(proje_id);
CREATE INDEX idx_proje_kaynaklari_tip ON proje_kaynaklari(kaynak_tipi);

-- Auth tablosu için indexler
CREATE INDEX idx_kullanicilar_email ON kullanicilar(email);
CREATE INDEX idx_kullanicilar_rol ON kullanicilar(rol);
CREATE INDEX idx_ogrenciler_kullanici ON ogrenciler(kullanici_id);
CREATE INDEX idx_akademisyenler_kullanici ON akademisyenler(kullanici_id);

-- Danışmanlık Görüşmeleri tablosu için indexler
CREATE INDEX idx_danismanlik_gorusmeleri_proje ON danismanlik_gorusmeleri(proje_id);
CREATE INDEX idx_danismanlik_gorusmeleri_akademisyen ON danismanlik_gorusmeleri(akademisyen_id);
CREATE INDEX idx_danismanlik_gorusmeleri_ogrenci ON danismanlik_gorusmeleri(ogrenci_id);
CREATE INDEX idx_danismanlik_gorusmeleri_tarih ON danismanlik_gorusmeleri(gorusme_tarihi);