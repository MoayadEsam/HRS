# HotelReservation - macOS Çalıştırma Talimatları (Kısa)

Aşağıdaki adımları macOS (Terminal) üzerinde kopyala-yapıştır yaparak uygulayı başlatabilir ve veritabanı (SQLite) ile ilgili sorunları giderebilirsiniz.

Önemli: proje SQLite kullanıyor ve `hotelreservation.db` dosyası uygulamanın çalıştığı dizine oluşturulur. VS/IDE ile çalıştırırken hangi dizinden çalıştırdıklarına dikkat edin.

1) .NET SDK kontrol / yükleme

```bash
dotnet --version
```

Eğer .NET 9 SDK yüklü değilse Homebrew kullanıyorsanız:

```bash
brew install --cask dotnet-sdk
```

Veya Microsoft sayfasından `.pkg` yükleyiciyi indirip kurun: https://dotnet.microsoft.com/download/dotnet/9.0

2) Proje dizinine gidip bağımlılıkları yükleyin

```bash
# ZIP'i açtıktan sonra proje köküne gidin
cd /Path/To/HRS

# Paketleri indir
dotnet restore

# Derle (hata var mı kontrol)
dotnet build
```

3) Web uygulamasını terminalden çalıştırma (en güvenli yöntem)

```bash
cd /Path/To/HRS/HotelReservation.Web
dotnet run
```

Tarayıcıda `http://localhost:5029` adresini açın. Konsolda seed (başlangıç verisi) ve DB oluşturma ile ilgili loglar görünecektir. Eğer seed sırasında hata olursa konsol çıktısını paylaşın.

4) Veritabanı dosyasını bulma / kontrol etme

```bash
# Proje genelinde arama
find . -name "hotelreservation.db"

# Sadece web proje dizininin içinde kontrol
find HotelReservation.Web -name "hotelreservation.db"
```

5) Dosya kilidi/bozulma kontrolü

```bash
# DB başka bir process tarafından tutuluyor mu kontrol etmek için
lsof | grep hotelreservation.db

# Eğer DB bozuk veya kilitliyse (ve veri kaybı problemi yoksa) silebilir ve yeniden başlatabilirsiniz
rm /full/path/to/hotelreservation.db
cd HotelReservation.Web
dotnet run
```

6) İzin sorunları

```bash
# Eğer "permission denied" hatası alıyorsanız dizinin yazılabilir olduğundan emin olun
chmod u+w /Path/To/HRS/HotelReservation.Web
```

7) Opsiyonel: EF Core migrations kullanmak isterseniz

```bash
# dotnet-ef aracını yükleyin (eğer yoksa)
dotnet tool install --global dotnet-ef

# Migration oluşturma (HRS kökünden çalıştırın)
cd HotelReservation.Data
dotnet ef migrations add InitialCreate --project . --startup-project ../HotelReservation.Web

# Veritabanına uygula
dotnet ef database update --project . --startup-project ../HotelReservation.Web
```

Kısa tek-paragraf mesaj (arkadaşlarınıza gönderilecek):

"ZIP'i aç, Terminal açıp proje köküne git (HRS). `dotnet restore` ve `dotnet build` çalıştır. Sonra `cd HotelReservation.Web` ve `dotnet run` ile başlat. Tarayıcıda `http://localhost:5029` açılır. Eğer DB görünmüyorsa proje dizininde `find . -name 'hotelreservation.db'` çalıştırıp bana çıktısını gönder."

Gerekirse Windows/VS Code/VS for Mac için ayrı kısa talimat da ekleyebilirim.
