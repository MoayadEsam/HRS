# Deploy to Railway.app 🚀

## Prerequisites
- GitHub account
- Railway.app account (sign up at https://railway.app)

## Deployment Steps

### 1. Push to GitHub
```powershell
cd C:\Users\Administrator\HRS
git init
git add .
git commit -m "Initial commit - Hotel Reservation System"
```

Create a new repository on GitHub, then:
```powershell
git remote add origin https://github.com/YOUR_USERNAME/hotel-reservation-system.git
git branch -M main
git push -u origin main
```

### 2. Deploy on Railway

1. Go to https://railway.app
2. Click **"Start a New Project"**
3. Select **"Deploy from GitHub repo"**
4. Connect your GitHub account and select the `hotel-reservation-system` repository
5. Railway will automatically detect the ASP.NET Core project

### 3. Configure Environment

Railway will automatically:
- Build using `dotnet publish`
- Run the application
- Provide a public URL

### 4. Access Your App

Once deployed, Railway will give you a URL like:
`https://your-app-name.up.railway.app`

Share this URL with your teammates!

## Default Credentials

**Admin Account:**
- Email: admin@hotel.com
- Password: Admin123!

**Staff Account:**
- Email: staff@hotel.com
- Password: Staff123!

**User Account:**
- Email: user@hotel.com
- Password: User123!

## Features
✅ Premium Navy & Gold Design
✅ Role-Based Access (Admin/Staff/User)
✅ Hotel Management
✅ Room Management with Amenities
✅ Booking System
✅ Custom Hotel Images
✅ PDF Export for Reservations

## Notes
- SQLite database will persist on Railway's volume
- First deployment takes 2-3 minutes
- Free tier available with 500 hours/month
