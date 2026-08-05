# Gün 29 — Oracle Cloud Free Tier'a Deploy

Bu adımların çoğu Oracle Cloud Console'da (web arayüzü) hesap/kimlik doğrulama
gerektirdiği için elle yapılmalı. VPS ayağa kalkıp SSH erişimi sağlandıktan sonraki
adımlar (`docker compose up -d --build` vb.) buradan da yürütülebilir.

## 1. Instance oluşturma (Oracle Cloud Console)

1. https://cloud.oracle.com üzerinden Always Free hesabı aç (kredi kartı istenir,
   Always Free kapsamındaki kaynaklar ücretlendirilmez).
2. **Compute → Instances → Create Instance**
   - Name: `sakaryaerp-vps`
   - Image: **Ubuntu 24.04** (Always Free uyumlu)
   - Shape: **VM.Standard.A1.Flex** (ARM, Always Free — 4 OCPU/24GB'a kadar) veya
     **VM.Standard.E2.1.Micro** (AMD, Always Free — 1 OCPU/1GB, 2 adet ücretsiz)
   - Networking: varsayılan VCN/subnet, **"Assign a public IPv4 address"** işaretli olsun
   - SSH keys: **"Generate a key pair for me"** seç ve private key'i (.key/.pem) indir
     (bu dosya tek seferlik gösterilir, kaybetme)
   - Create.
3. Instance "Running" durumuna geçince **Public IP** adresini not al.

## 2. Güvenlik kuralları (Security List)

**Networking → Virtual Cloud Networks → (VCN'in) → Security Lists → Default Security List**
→ Ingress Rules'a ekle:
- Source: `0.0.0.0/0`, IP Protocol: TCP, Destination Port: **80**
- Source: `0.0.0.0/0`, IP Protocol: TCP, Destination Port: **443** (SSL için, ileride)

(22/tcp SSH için zaten varsayılan olarak açıktır.)

## 3. SSH bağlantısı ve sunucu hazırlığı

```bash
ssh -i indirilen-key.pem ubuntu@<PUBLIC_IP>

sudo apt update && sudo apt install -y docker.io docker-compose-plugin git
sudo usermod -aG docker $USER
# grup üyeliğinin geçmesi için tekrar SSH ile bağlan (exit && ssh ... tekrar)
```

**Önemli:** Oracle'ın Ubuntu image'ı Security List'e ek olarak kendi `iptables`
kurallarıyla da paketleri düşürür — sadece Security List'i açmak yetmez:

```bash
sudo iptables -I INPUT -p tcp --dport 80 -j ACCEPT
sudo iptables -I INPUT -p tcp --dport 443 -j ACCEPT
sudo netfilter-persistent save
```

## 4. Projeyi sunucuya taşıma

```bash
git clone <repo-url> sakaryaerp
cd sakaryaerp
cp .env.example .env
nano .env   # POSTGRES_PASSWORD'ü güçlü bir şifreyle değiştir; SMTP bilgilerin varsa doldur;
            # APP_BASE_URL=http://<PUBLIC_IP> satırını ekle (aşağıdaki not) — eklenmezse
            # docker compose up net bir hatayla durur.
            # SEED_ADMIN_EMAIL / SEED_ADMIN_PASSWORD ekle (aşağıdaki not) — bunlar
            # olmadan compose çalışır ama web container'ı açık, anlaşılır bir hatayla
            # sürekli yeniden başlar (crash-loop) — `docker compose logs web` ile görülür.
```

**Önemli — `APP_BASE_URL`:** Şifre sıfırlama e-postasındaki link, güvenlik nedeniyle
gelen isteğin Host header'ından değil bu değerden kurulur (bkz. `AccountController`
— `AllowedHosts: "*"` olduğu için Host header'a güvenmek bir saldırganın sıfırlama
linkini kendi sunucusuna yönlendirip hesap ele geçirmesine izin verirdi). `.env`'e
`APP_BASE_URL=http://<PUBLIC_IP>` (domain alındığında `https://domain.com` olarak
güncellenmeli) eklenmeden `docker compose up` başlamaz.

**Önemli — `SEED_ADMIN_EMAIL` / `SEED_ADMIN_PASSWORD`:** İlk kurulumda (veritabanı
boşken) tek bir Admin hesabı bu bilgilerle oluşturulur (bkz. `DbSeeder.
SeedFoundationAsync`). Development ortamındaki sabit demo şifreler (`Admin123!` vb.)
kaynak kodda herkese açık olduğu için Production'da KULLANILMAZ — kontrol `docker
compose`'ta değil uygulama içinde: bu değerler boşken `web` container'ı açık bir
hatayla başlamayı reddedip yeniden başlamayı dener (`docker compose logs web` ile
görülür). Güçlü, benzersiz bir şifre seçin; diğer kullanıcıları
(Muhasebe/Satış) ilk girişten sonra Kullanıcı Yönetimi ekranından ekleyin.

## 5. Ayağa kaldırma

```bash
docker compose up -d --build
```

İlk build .NET SDK/runtime image'larını indireceği için birkaç dakika sürebilir.
`docker compose ps` ile üç servisin de (db, web, nginx) `running/healthy` olduğunu doğrula.

## 6. Demo verisini yükleme (tek seferlik)

```bash
docker compose run --rm web dotnet SakaryaERP.dll --seed-demo
```

Bu komut sadece veriyi yükler ve çıkar, web sunucusunu başlatmaz (zaten `docker compose up`
ile ayrı çalışıyor). Cari tablosu doluysa (ikinci çalıştırmada) hiçbir şey yapmadan çıkar.

## 7. İlk giriş ve ÖNEMLİ güvenlik adımı

`http://<PUBLIC_IP>` adresine git. Varsayılan demo hesapları:

| Rol | E-posta | Şifre |
|---|---|---|
| Admin | admin@sakaryaerp.com | Admin123! |
| Muhasebe | muhasebe@sakaryaerp.com | Muhasebe123! |
| Satış | satis@sakaryaerp.com | Satis123! |

**Bu şifreler kod içinde sabit (DbSeeder.SeedFoundationAsync) ve herkese açık bir IP'de
çalışıyor olacak.** Public sunucuya ilk girişten hemen sonra en azından Admin şifresini
değiştirmen önerilir (uygulama içinde henüz bir "şifre değiştir" ekranı yok — şifremi
unuttum akışıyla e-posta üzerinden sıfırlanabilir, ya da `dotnet ef`/psql ile elle).

## 8. Domain ve Let's Encrypt SSL (domain temin edilince)

Bu oturumda domain olmadığı için atlandı. Domain alındığında:

1. Domain'in A kaydını sunucunun Public IP'sine yönlendir.
2. Sunucuda `certbot` kur, `certbot certonly --standalone -d <domain>` ile sertifika al
   (nginx'i geçici durdurup almak gerekebilir) veya nginx için certbot eklentisini kullan.
3. `nginx/nginx.conf`'a 443 sunucu bloğu ekle (ssl_certificate/ssl_certificate_key ile),
   80'i 443'e yönlendirecek şekilde güncelle.
4. `docker-compose.yml`'de nginx'e `443:443` port'u ve sertifika volume'ünü ekle.
5. `.env`'e veya `docker-compose.yml`'deki `web` servisine `EnableHttpsRedirection=true`
   ortam değişkenini ekle (appsettings.json'daki varsayılan `false`'u ezer).
6. `docker compose up -d --build` ile yeniden ayağa kaldır.

## Yerel doğrulama (bu oturumda yapıldı)

VPS henüz yokken şu ikisi tamamen izole test edilerek doğrulandı:
- `nginx/nginx.conf` gerçek bir nginx container'ı üzerinden reverse-proxy olarak
  çalıştırılıp (host'taki uygulamaya proxy edilerek) Host header forwarding, statik
  dosya servisi ve login akışının proxy üzerinden sorunsuz çalıştığı doğrulandı.
- `DemoSeeder` geçici/izole bir PostgreSQL container'ında migration'lar uygulanıp
  `--seed-demo` çalıştırılarak test edildi: 8 cari, 25 malzeme, uçtan uca 3 satış +
  3 alış zinciri (hepsi onaylı), 6 dengeli muhasebe fişi (Borç=Alacak), negatif stok
  hatası yok. Test sonunda tüm geçici container/volume silindi.

`docker compose up -d --build` komutunun tam çalıştığı (SDK image indirmesi dahil)
bu sandbox ortamının bant genişliği kısıtı yüzünden doğrulanamadı; VPS'te ilk
`docker compose up -d --build` birkaç dakika sürebilir, bu normaldir.
