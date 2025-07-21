-- Kullanicilar için sahte veriler
INSERT INTO Kullanicilar (Ad, Soyad, Email, Sifre, Rol, Aktif, CreatedAt, UpdatedAt)
VALUES 
    ('Ahmet', 'Yılmaz', 'admin@universite.edu.tr', 'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae', 'Admin', 1, GETDATE(), GETDATE()),
    ('Mehmet', 'Kaya', 'mehmet.kaya@universite.edu.tr', 'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae', 'Akademisyen', 1, GETDATE(), GETDATE()),
    ('Ayşe', 'Demir', 'ayse.demir@universite.edu.tr', 'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae', 'Akademisyen', 1, GETDATE(), GETDATE()),
    ('Fatma', 'Çelik', 'fatma.celik@universite.edu.tr', 'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae', 'Akademisyen', 1, GETDATE(), GETDATE()),
    ('Mustafa', 'Şahin', 'mustafa.sahin@universite.edu.tr', 'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae', 'Akademisyen', 1, GETDATE(), GETDATE()),
    ('Ali', 'Öztürk', 'ali.ozturk@universite.edu.tr', 'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae', 'Akademisyen', 1, GETDATE(), GETDATE()),
    ('Zeynep', 'Yıldız', 'ogrenci1@universite.edu.tr', 'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae', 'Ogrenci', 1, GETDATE(), GETDATE()),
    ('Elif', 'Arslan', 'ogrenci2@universite.edu.tr', 'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae', 'Ogrenci', 1, GETDATE(), GETDATE()),
    ('Mert', 'Aydın', 'ogrenci3@universite.edu.tr', 'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae', 'Ogrenci', 1, GETDATE(), GETDATE()),
    ('Deniz', 'Kara', 'ogrenci4@universite.edu.tr', 'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae', 'Ogrenci', 1, GETDATE(), GETDATE()),
    ('Burak', 'Koç', 'ogrenci5@universite.edu.tr', 'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae', 'Ogrenci', 1, GETDATE(), GETDATE());

-- Akademisyenler için sahte veriler
INSERT INTO Akademisyenler (KullaniciId, Ad, Soyad, Email, Unvan, Telefon, Ofis, UzmanlikAlani, CreatedAt, UpdatedAt)
VALUES 
    (18, 'Mehmet', 'Kaya', 'mehmet.kaya@universite.edu.tr', 'Prof. Dr.', '5551234567', 'A-101', 'Yapay Zeka', GETDATE(), GETDATE()),
    (19, 'Ayşe', 'Demir', 'ayse.demir@universite.edu.tr', 'Doç. Dr.', '5551234568', 'A-102', 'Web Teknolojileri', GETDATE(), GETDATE()),
    (20, 'Fatma', 'Çelik', 'fatma.celik@universite.edu.tr', 'Dr. Öğr. Üyesi', '5551234569', 'A-103', 'Mobil Uygulamalar', GETDATE(), GETDATE()),
    (21, 'Mustafa', 'Şahin', 'mustafa.sahin@universite.edu.tr', 'Prof. Dr.', '5551234570', 'A-104', 'Veri Analitiği', GETDATE(), GETDATE()),
    (22, 'Ali', 'Öztürk', 'ali.ozturk@universite.edu.tr', 'Doç. Dr.', '5551234571', 'A-105', 'Siber Güvenlik', GETDATE(), GETDATE());

-- Öğrenciler için sahte veriler
INSERT INTO Ogrenciler (KullaniciId,Ad, Soyad, Email, OgrenciNo, Telefon, Adres, KayitTarihi, CreatedAt, UpdatedAt)
VALUES 
    (23, 'Zeynep', 'Yıldız', 'ogrenci1@universite.edu.tr', '20210001', '5559876543', 'Merkez Mah. Üniversite Cad. No:1 Öğrenci Yurdu', GETDATE(), GETDATE(), GETDATE()),
    (24, 'Elif', 'Arslan', 'ogrenci2@universite.edu.tr', '20210002', '5559876544', 'Atatürk Mah. Cumhuriyet Sok. No:5', GETDATE(), GETDATE(), GETDATE()),
    (25, 'Mert', 'Aydın', 'ogrenci3@universite.edu.tr', '20210003', '5559876545', 'Çamlık Mah. Gül Sok. No:12', GETDATE(), GETDATE(), GETDATE()),
    (26, 'Deniz', 'Kara', 'ogrenci4@universite.edu.tr', '20210004', '5559876546', 'Yenişehir Mah. Lale Cad. No:8', GETDATE(), GETDATE(), GETDATE()),
    (27, 'Burak', 'Koç', 'ogrenci5@universite.edu.tr', '20210005', '5559876547', 'Bahçelievler Mah. Menekşe Sok. No:3', GETDATE(), GETDATE(), GETDATE());

-- Proje Kategorileri için sahte veriler
INSERT INTO ProjeKategorileri (Ad, Aciklama, Renk, CreatedAt, UpdatedAt)
VALUES 
    ('Yapay Zeka', 'Yapay zeka ve makine öğrenmesi ile ilgili projeler', '#FF5733', GETDATE(), GETDATE()),
    ('Web Geliştirme', 'Web uygulamaları ve site geliştirme projeleri', '#33FF57', GETDATE(), GETDATE()),
    ('Mobil Uygulama', 'Android ve iOS uygulamaları geliştirme projeleri', '#3357FF', GETDATE(), GETDATE()),
    ('Veri Analizi', 'Büyük veri ve veri analizi projeleri', '#F3FF33', GETDATE(), GETDATE()),
    ('Siber Güvenlik', 'Güvenlik ve penetrasyon testleri projeleri', '#FF33E9', GETDATE(), GETDATE()),
    ('Gömülü Sistemler', 'Donanım ve gömülü yazılım projeleri', '#33FFF6', GETDATE(), GETDATE()),
    ('Otomasyon', 'Endüstriyel ve ev otomasyonu projeleri', '#9B33FF', GETDATE(), GETDATE());

-- Projeler için sahte veriler
    INSERT INTO Projeler (Ad, Aciklama, OlusturmaTarihi, TeslimTarihi, Status, OgrenciId, MentorId, KategoriId, CreatedAt, UpdatedAt)
VALUES 
    ('Yapay Zeka Tabanlı Öğrenci Takip Sistemi', 'Yapay zeka kullanarak öğrenci performansını analiz eden ve tahmin eden bir sistem geliştirilmesi', '2023-10-01', '2024-05-30', 'Devam', 8, 11, 5, GETDATE(), GETDATE()),
    ('Akıllı Ev Otomasyonu', 'IoT cihazları kullanarak ev otomasyonu projesi', '2023-09-15', '2024-04-20', 'Devam', 9, 12, 6, GETDATE(), GETDATE()),
    ('Mobil Sağlık Asistanı', 'Hastaların ilaç takibini yapabileceği mobil uygulama', '2023-11-05', '2024-06-15', 'Beklemede', 10, 13, 7, GETDATE(), GETDATE()),
    ('Online Eğitim Platformu', 'Kişiselleştirilmiş eğitim içeriği sunan web uygulaması', '2023-10-20', '2024-05-15', 'Atanmis', 11, 14, 8, GETDATE(), GETDATE()),
    ('Veri Madenciliği ile Müşteri Analizi', 'E-ticaret sitelerinde müşteri davranışlarını analiz eden sistem', '2023-09-01', '2024-03-30', 'Devam', 12, 15, 9, GETDATE(), GETDATE()),
    ('Akıllı Trafik Sistemi', 'Yapay zeka kullanarak trafik akışını optimize eden sistem', '2023-10-10', '2024-05-01', 'Atanmis', 12, 15, 10, GETDATE(), GETDATE()),
    ('Sanal Gerçeklik Eğitim Platformu', 'VR teknolojisi kullanarak mesleki eğitim simülasyonu', '2023-09-20', '2024-04-15', 'Devam', 11, 12, 12, GETDATE(), GETDATE()),
    ('Biyometrik Kimlik Doğrulama Sistemi', 'Yüz tanıma ve parmak izi kullanarak kimlik doğrulama sistemi', '2023-11-20', '2024-07-01', 'Beklemede', 11, 11, 15, GETDATE(), GETDATE()),
    ('Drone ile Tarım Takip Sistemi', 'Drone ve görüntü işleme teknolojileri ile tarım arazilerinin analizi', '2023-10-05', '2024-05-10', 'Tamamlandi', 10, 14, 16, GETDATE(), GETDATE());

-- Proje Aşamaları için sahte veriler
INSERT INTO ProjeAsamalari (ProjeId, AsamaAdi, Aciklama, BaslangicTarihi, BitisTarihi, Tamamlandi, TamamlanmaTarihi, SiraNo, CreatedAt, UpdatedAt)
VALUES 
    (1, 'Gereksinim Analizi', 'Projenin gereksinim ve kapsamının belirlenmesi', '2023-10-01', '2023-10-15', 1, '2023-10-14', 1, GETDATE(), GETDATE()),
    (1, 'Tasarım', 'Sistem mimarisi ve arayüz tasarımlarının oluşturulması', '2023-10-16', '2023-11-15', 1, '2023-11-13', 2, GETDATE(), GETDATE()),
    (1, 'Geliştirme', 'Kodlama ve yazılım geliştirme süreci', '2023-11-16', '2024-03-15', 0, NULL, 3, GETDATE(), GETDATE()),
    (1, 'Test', 'Sistemin test edilmesi ve hataların giderilmesi', '2024-03-16', '2024-04-30', 0, NULL, 4, GETDATE(), GETDATE()),
    (1, 'Dokümantasyon ve Sunum', 'Proje dökümantasyonunun hazırlanması ve sunumu', '2024-05-01', '2024-05-30', 0, NULL, 5, GETDATE(), GETDATE());

-- Proje Yorumları için sahte veriler
INSERT INTO ProjeYorumlari (Icerik, OlusturmaTarihi, YorumTipi, ProjeId, OgrenciId, AkademisyenId, CreatedAt, UpdatedAt)
VALUES 
    ('Projenin ilk aşamasında gereksinimleri netleştirmemiz gerekiyor.', '2023-10-02', 'Genel', 29, NULL, 11, GETDATE(), GETDATE()),
    ('Veri seti hazırlandı, analiz aşamasına geçebiliriz.', '2023-10-10', 'Genel', 29, 11, NULL, GETDATE(), GETDATE()),
    ('Algoritma seçimi için daha fazla literatür taraması yapmalısınız.', '2023-10-25', 'Geri Bildirim', 29, NULL, 11, GETDATE(), GETDATE());

-- Değerlendirmeler için sahte veriler
INSERT INTO Degerlendirmeler (Puan, Aciklama, DegerlendirmeTarihi, DegerlendirmeTipi, ProjeId, AkademisyenId, CreatedAt, UpdatedAt)
VALUES 
    (85, 'Gereksinim analizi aşaması başarılı bir şekilde tamamlanmış, ancak bazı teknik detaylar eksik.', '2023-10-20', 'Ara', 29, 11, GETDATE(), GETDATE()),
    (78, 'Donanım seçimleri uygun, ancak maliyet optimizasyonu gerekiyor.', '2023-11-20', 'Ara', 29, 11, GETDATE(), GETDATE()),
    (92, 'Kullanıcı analizi kapsamlı ve detaylı hazırlanmış.', '2023-12-05', 'Ara', 29, 11, GETDATE(), GETDATE());

-- Proje Dosyaları için sahte veriler
INSERT INTO ProjeDosyalari (DosyaAdi, DosyaYolu, DosyaTipi, DosyaBoyutu, YuklemeTarihi, ProjeId, YukleyenId, YukleyenTipi, CreatedAt, UpdatedAt)
VALUES 
    ('Gereksinim Analizi.pdf', '/uploads/projeler/1/gereksinim_analizi.pdf', 'application/pdf', 2048000, '2023-10-15', 1, 1, 'Ogrenci', GETDATE(), GETDATE()),
    ('Sistem Tasarımı.docx', '/uploads/projeler/1/sistem_tasarimi.docx', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', 1536000, '2023-11-13', 1, 1, 'Ogrenci', GETDATE(), GETDATE());

-- Bildirimler için sahte veriler
INSERT INTO Bildirimler (Baslik, Icerik, OlusturmaTarihi, Okundu, BildirimTipi, OgrenciId, AkademisyenId, CreatedAt, UpdatedAt)
VALUES 
    ('Proje Değerlendirmesi Yapıldı', 'Yapay Zeka Tabanlı Öğrenci Takip Sistemi projeniz değerlendirildi. Sonuçları görüntüleyin.', '2023-10-20', 0, 'Bilgi', 1, NULL, GETDATE(), GETDATE()),
    ('Yeni Yorum', 'Prof. Dr. Mehmet Kaya projenize yeni bir yorum ekledi.', '2023-10-25', 1, 'Bilgi', 1, NULL, GETDATE(), GETDATE()),
    ('Proje Aşaması Tamamlandı', 'Tasarım aşaması başarıyla tamamlandı. Bir sonraki aşamaya geçebilirsiniz.', '2023-11-13', 0, 'Basari', 1, NULL, GETDATE(), GETDATE());

-- Proje Kaynakları için sahte veriler
INSERT INTO ProjeKaynaklari (ProjeId, KaynakAdi, KaynakTipi, Url, Aciklama, Yazar, YayinTarihi, EklemeTarihi, CreatedAt, UpdatedAt)
VALUES 
    (1, 'Deep Learning', 'Kitap', NULL, 'Yapay zeka ve derin öğrenme temelleri', 'Ian Goodfellow, Yoshua Bengio, Aaron Courville', '2016-11-01', GETDATE(), GETDATE(), GETDATE()),
    (1, 'Machine Learning for Education', 'Makale', 'https://example.com/ml-education', 'Eğitimde makine öğrenmesi uygulamaları', 'John Smith, Maria Garcia', '2022-03-15', GETDATE(), GETDATE(), GETDATE()),
    (1, 'TensorFlow Documentation', 'Website', 'https://www.tensorflow.org/docs', 'TensorFlow kütüphanesi resmi dökümantasyonu', NULL, NULL, GETDATE(), GETDATE(), GETDATE());