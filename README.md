# **PetSoLive: Hayvan Refahı İçin Dijital Platform**

## **Giriş**

PetSoLive, evcil hayvan sahipleri, veterinerler, barınaklar ve hayvan refahıyla ilgilenen topluluk üyeleri arasında etkili bir iletişim ağı oluşturmayı amaçlayan yenilikçi bir sosyal medya platformudur. Platform, geleneksel iletişim yöntemlerinin eksikliklerini giderirken, özelleştirilmiş çözümler sunarak evlat edinme süreçlerini iyileştirir, acil durum yardımları için destek sağlar ve veterinerlik hizmetlerini kolaylaştırır.

Proje, modern yazılım teknolojileri ve çoklu kullanıcı destekli mimarisiyle, hem bireylerin hem de kurumların hayvan refahı için daha fazla çaba göstermesine olanak tanır.

---

## **Projenin Amacı**
PetSoLive'ın genel hedefleri:
- Hayvan sahiplerinin ve veterinerlerin iletişim engellerini ortadan kaldırmak.
- Hayvan refahı için topluluk odaklı bir yaklaşım oluşturmak.
- Acil durumlarda etkili destek sağlayarak hayvan hayatlarını kurtarmak.
- Daha fazla hayvanın yuva bulmasını sağlamak ve evlat edinme süreçlerini optimize etmek.

PetSoLive, PetFinder ve AdoptAPet gibi mevcut platformlardan esinlenmiş ancak bu platformların eksik bıraktığı alanları kapsayacak şekilde tasarlanmıştır. Bu şekilde, kayıp hayvan duyuruları, acil veterinerlik yardımları ve topluluk özelinde dinamik bir etkileşim ağı oluşturulması hedeflenmiştir.

---

## **Platform Özellikleri**

### **1. Temel Fonksiyonlar**

#### **1.1 Dinamik Zaman Tüneli**
PetSoLive, kullanıcılarına gerçek zamanlı bir zaman tüneli sunar. Bu tünel, kullanıcıların görebileceği çeşitli içerikleri şu şekilde organize eder:
- **Evlat Edinme Hikayeleri:** Yeni yuva bulan hayvanların hikayeleri ve fotoğrafları.
- **Veterinerlik Tavsiyeleri:** Veterinerler tarafından sunulan ipuçları ve rehberlik.
- **Kayıp Hayvan Duyuruları:** Topluluğun kayıp hayvan bulması için organize olmasına yardımcı olan ilanlar.

#### **1.2 Kayıp Hayvan Duyuruları**
Kullanıcıların kayıp hayvanlarını bulmaları için şu özellikler sağlanır:
- **Lokasyon Etiketi:** Son görülen konum bilgisinin eklenmesi.
- **Bildirim Sistemi:** Yerel topluluğa otomatik olarak bildirim gönderme.
- **Geri Bildirim:** Kullanıcıların kayıp hayvanı gördüklerinde bilgi paylaşması.

#### **1.3 Acil Yardım Duyuruları**
Bu fonksiyon, evcil hayvanın acil tıbbi yardıma ihtiyacı olduğunda kullanıcıları hızlı ve etkin bir şekilde destekleyebilir:
- **Durum Etiketleri:** Duyurunun aciliyeti (“Düşük,” “Orta,” “Yüksek”) ile tanımlanabilir.
- **Veteriner Bildirimi:** Veterinerlere otomatik bildirim gönderimi.
- **Destek Mesajlaşma:** Veterinerler ve hayvan sahipleri arasında iletişim kurma aracı.

#### **1.4 Evlat Edinme Süreçleri**
Barınaklar ve bireyler, evlat edinme ilanları oluşturabilir. Bu ilanların detayları şunları kapsar:
- **Hayvan Profilleri:** Yaş, tür, sağlık durumu gibi bilgiler.
- **Takip Sistemi:** Evlat edinme talipleriyle takip süreçlerini yönetme.

### **2. Teknik Fonksiyonlar**

#### **2.1 Bildirim Sistemi**
- Káyıp hayvan ilanları veya acil durum yardımlarında topluluk üyelerine ve veterinerlere hızlı bildirimler sağlar.

#### **2.2 Gerçek Zamanlı Mesajlaşma**
Veterinerler ile hayvan sahipleri arasındaki iletişim bu özellikle kolaylaştırılır.

#### **2.3 Aşı Takvimi**
Veterinerler, hayvan sahiplerine yaklaşan aşı tarihleriyle ilgili otomatik hatırlatıcılar gönderebilir.

---

## **Teknik Mimari**

PetSoLive, **3 Katmanlı Mimari** kullanarak geliştirilmiştir:

### **1. Sunum Katmanı**
Kullanıcı deneyimini yükseltmek için tasarlanır:
- **ASP.NET MVC:** Kullanıcı arayüzü görüsü için.
- **HTML5/CSS3 ve JavaScript:** Mobil uyumlu, etkileşimli bir arayüz oluşturmak için kullanılmıştır.

### **2. Uygulama Katmanı**
Platformun tüm iş mantığı bu katmanda gerçekleştirilir:
- **ASP.NET Core:** Uygulama mantığı ve API yönetimi.
- **RESTful API:** Frontend ile backend arasında veri alışverişi.

### **3. Veri Katmanı**
Tüm kullanıcı bilgileri ve medya dosyaları burada saklanır:
- **PostgreSQL:** Yapısal veri saklama.
- **Windows IIS:** Medya dosyası yönetimi ve veri şifreleme.

---

## **Gelecekteki Geliştirmeler**

### **1. Mobil Uygulama**
Kullanıcıların platforma hızlı erişim sağlaması için bir mobil uygulama planlanmaktadır.

### **2. Yapay Zeka Destekli Tavsiyeler**
Kullanıcı alışkanlıklarına göre özel veteriner tavsiyeleri ve uygun evlat edinme ilanları sunmak.

### **3. Video Danışmanlık Hizmeti**
Acil durumlarda veterinerlerle gerçek zamanlı video görüşmesi yapma olanağı.

### **4. Topluluk Forumları**
Hayvan sahiplerinin deneyimlerini paylaştığı ve tavsiyeler aldığı forumlar oluşturma.

### **5. IoT Entegrasyonu**
GPS takip cihazları veya sağlık monitörleri gibi akıllı cihazlarla entegrasyon.

---

## **Sonuç**

PetSoLive, hayvan refahını iyileştirme konusunda yenilikçi bir yaklaşımla geliştirilmiştir. Teknik altyapısı, çoklu kullanıcı desteği ve özelleştirilmiş çözümleriyle bu platform, hayvan sahipleri, veterinerler ve barınaklar arasındaki iletişimi daha etkili hale getirerek toplumsal farkındalığı artırmayı hedefler. Daha fazla geliştirme ve yenilik çalışmalarıyla, PetSoLive'in gelecekte de hayvan refahı için önemli bir rol oynaması beklenmektedir.

