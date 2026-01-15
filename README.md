# WeDeLi - Logistics Management System

Backend API for WeDeLi delivery management platform.

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- MySQL 8.0+ (or Aiven MySQL)
- Redis (optional)

### Local Development

1. **Clone repository**
```bash
git clone https://github.com/meanwuan/WeDeLi-V2.git
cd WeDeLi-V2/wedeli
```

2. **Set environment variables**

Create `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "PlatformConnection": "Server=YOUR_HOST;Port=PORT;Database=wedeli_platform;User=USER;Password=PASSWORD;SslMode=Required;",
    "CompanyConnection": "Server=YOUR_HOST;Port=PORT;Database=wedeli_company;User=USER;Password=PASSWORD;SslMode=Required;"
  }
}
```

3. **Run migrations**
```bash
dotnet ef database update --context PlatformDbContext
dotnet ef database update --context AppDbContext
```

4. **Run application**
```bash
dotnet run
```

API will be available at `http://localhost:5000`

## 📦 Deployment

### Deploy to Render

1. Push code to GitHub
2. Create new Web Service on Render
3. Select Docker runtime
4. Set environment variables (see Render guide)
5. Deploy!

See [Render Deployment Guide](../docs/render_deployment.md) for details.

## 🔒 Security

- Never commit `appsettings.Development.json`
- Use environment variables for production
- Rotate secrets if accidentally exposed

## 📝 License

MIT
