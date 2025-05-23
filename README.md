
# File Server Log Analiz Sistemi

Bu proje, Windows Event Log dosyalarını (.evtx) analiz eden ve web arayüzü üzerinden görüntülemenizi sağlayan bir uygulamadır.

## Özellikler

- Windows Event Log dosyalarını (.evtx) otomatik olarak analiz eder
- Dosya erişim, silme ve açma işlemlerini takip eder
- Modern ve kullanıcı dostu web arayüzü
- Gerçek zamanlı log güncellemesi
- Filtreleme ve arama özellikleri

## Kurulum

1. Python 3.7 veya üstü sürümünün yüklü olduğundan emin olun
2. Gerekli paketleri yükleyin:  pip install -r requirements.txt
   

## Kullanım

1. Windows Event Log dosyalarınızı (.evtx) `logs` klasörüne kopyalayın
2. Uygulamayı başlatın:
   ```
   python app.py
   ```
3. Web tarayıcınızda `http://localhost:5000` adresine gidin

## IIS Üzerinde Çalıştırma

1. IIS'de yeni bir site oluşturun
2. Python FastCGI modülünü yükleyin
3. Uygulama klasörünü IIS'de yayınlayın
4. Web.config dosyasını oluşturun ve gerekli yapılandırmaları yapın

## Notlar

- Log dosyaları otomatik olarak 30 saniyede bir yenilenir
- Farklı işlem türleri farklı renklerle gösterilir:
  - Erişim: Yeşil
  - Silme: Kırmızı
  - Açma: Mavi
  - Diğer: Gri 
  
  
  💡 Katkı Sağla
Katkılara her zaman açığız!
PR gönderin, hata bildirin veya yeni özellik önerin.