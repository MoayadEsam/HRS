# HotelReservation - Windows Çalıştırma Talimatları (Kısa)

Aşağıdaki adımları Windows (PowerShell veya Command Prompt) üzerinde kopyala-yapıştır yaparak uygulamayı başlatabilir ve veritabanı (SQLite) ile ilgili sorunları giderebilirsiniz.

**Önemli:** Proje SQLite kullanıyor ve `hotelreservation.db` dosyası uygulamanın çalıştığı dizine oluşturulur. Visual Studio veya VS Code ile çalıştırırken hangi dizinden çalıştırdığınıza dikkat edin.

## 1) .NET SDK kontrol / yükleme

```powershell
dotnet --version
```

Eğer .NET 9 SDK yüklü değilse Microsoft sayfasından indirip kurun: https://dotnet.microsoft.com/download/dotnet/9.0

## 2) Proje dizinine gidip bağımlılıkları yükleyin

```powershell
# ZIP'i açtıktan sonra proje köküne gidin
cd C:\Path\To\HRS

# Paketleri indir
dotnet restore

# Derle (hata var mı kontrol)
dotnet build
```

## 3) Web uygulamasını terminalden çalıştırma (en güvenli yöntem)

```powershell
cd C:\Path\To\HRS\HotelReservation.Web
dotnet run
```

Tarayıcıda `http://localhost:5029` adresini açın. Konsolda seed (başlangıç verisi) ve DB oluşturma ile ilgili loglar görünecektir. Eğer seed sırasında hata olursa konsol çıktısını paylaşın.

## 4) Veritabanı dosyasını bulma / kontrol etme

```powershell
# Proje genelinde arama
Get-ChildItem -Path . -Filter hotelreservation.db -Recurse -Force

# Sadece web proje dizininin içinde kontrol
Get-ChildItem -Path .\HotelReservation.Web -Filter hotelreservation.db -Recurse -Force
```

Visual Studio ile çalıştırdıysanız DB genelde `bin\Debug\net9.0` içinde olabilir.

## 5) Dosya kilidi/bozulma kontrolü

```powershell
# DB dosyasını hangi processlerin kullandığını görmek için (SysInternals Handle tool gerekir)
# Ya da basitçe tüm VS/DB Browser uygulamalarını kapatın

# Eğer DB bozuk veya kilitliyse (ve veri kaybı problemi yoksa) silebilir ve yeniden başlatabilirsiniz
Remove-Item .\HotelReservation.Web\hotelreservation.db -Force -ErrorAction SilentlyContinue

cd .\HotelReservation.Web
dotnet run
```

## 6) İzin sorunları

Eğer "access denied" hatası alıyorsanız:
- PowerShell'i **Administrator olarak** çalıştırın
- Veya proje klasörünün yazılabilir olduğundan emin olun (klasöre sağ tık > Properties > Security)

## 7) Visual Studio ile çalıştırma

1. `HotelReservation.sln` dosyasını Visual Studio ile açın
2. Solution Explorer'da `HotelReservation.Web` projesine sağ tıklayıp **Set as Startup Project** seçin
3. `F5` veya **Debug > Start Debugging** ile çalıştırın
4. Eğer IIS Express ile sorun yaşıyorsanız, terminalden `dotnet run` deneyin

## 8) VS Code ile çalıştırma

1. VS Code ile HRS klasörünü açın (File > Open Folder)
2. Terminal açın (`Ctrl + ` `)
3. Yukarıdaki PowerShell komutlarını çalıştırın

## 9) Opsiyonel: EF Core migrations kullanmak isterseniz

```powershell
# dotnet-ef aracını yükleyin (eğer yoksa)
dotnet tool install --global dotnet-ef

# Migration oluşturma (HRS kökünden çalıştırın)
cd HotelReservation.Data
dotnet ef migrations add InitialCreate --project . --startup-project ..\HotelReservation.Web

# Veritabanına uygula
dotnet ef database update --project . --startup-project ..\HotelReservation.Web
```

---

## Kısa tek-paragraf mesaj (arkadaşlarınıza gönderilecek):

"ZIP'i aç, PowerShell veya CMD açıp proje köküne git (HRS). `dotnet restore` ve `dotnet build` çalıştır. Sonra `cd HotelReservation.Web` ve `dotnet run` ile başlat. Tarayıcıda `http://localhost:5029` açılır. Eğer DB görünmüyorsa `Get-ChildItem -Path . -Filter hotelreservation.db -Recurse -Force` çalıştırıp bana çıktısını gönder."

---

**Test Kullanıcıları:**
- Admin: `admin@hotel.com` / `Admin123!`
- Kullanıcı: `user@hotel.com` / `User123!`

**Dil Değiştirme:**
Sağ üst köşedeki bayrak ikonundan Türkçe/İngilizce arası geçiş yapabilirsiniz.
