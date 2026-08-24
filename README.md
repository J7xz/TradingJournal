 J7 Journal

### Track. Analyze. Improve.

A professional trading journal web application for traders of Gold, NASDAQ, and S&P 500. Track trades, analyze performance, and identify winning patterns with real-time market data.

![ASP.NET](https://img.shields.io/badge/ASP.NET_MVC-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-0078D7?style=for-the-badge&logo=signalr&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white)
![Chart.js](https://img.shields.io/badge/Chart.js-FF6384?style=for-the-badge&logo=chart.js&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](https://opensource.org/licenses/MIT)

---

## Features

### 📝 Trade Journaling
- Card-based dashboard with screenshot previews
- Full CRUD operations (Create, Read, Update, Delete)
- Screenshot storage as byte[] in database
- Tag system for trade categorization
- 5-star rating system
- Modal-based interactions (no page reloads)

### 📈 Analytics Dashboard
- Performance overview cards (Win Rate, Profit Factor, Total P&L)
- Cumulative P&L line chart
- P&L by symbol bar chart
- Win rate by setup type chart
- Daily bias accuracy table
- Monthly performance table
- Timeframe filters (7D, 30D, All Time, Custom Range)
- Auto-extracted key insights

### 📡 Live Price Feed
- Real-time price updates via SignalR
- Finnhub API integration
- Price change indicators (green/red)
- Day high/low, open, and previous close data

### 🎨 User Experience
- Dark/Light theme toggle with persistent preferences
- Fully responsive (Desktop, Tablet, Mobile)
- Smooth animations and transitions

---

## Tech Stack

| Category | Technology |
|----------|------------|
| Backend | ASP.NET MVC (.NET Framework 4.7.2) |
| ORM | Entity Framework 6 |
| Database | SQL Server |
| Real-Time | SignalR |
| Frontend | Bootstrap 5, Font Awesome 6 |
| Charts | Chart.js |
| API | Finnhub |

---

## Getting Started

### Prerequisites
- Visual Studio 2019 or 2022
- SQL Server (LocalDB / Express / Full)
- .NET Framework 4.7.2+
- Finnhub API key (free)

### Installation

**1. Clone the repository**
```bash
git clone https://github.com/J7xz/TradingJournal.git
2. Update the database

powershell
Update-Database
3. Add your API key

xml
<add key="FinnhubApiKey" value="YOUR_API_KEY_HERE" />
Get a free key at Finnhub.io/register

4. Run

text
Press F5 in Visual Studio
Configuration
Database Connection (Web.config)
xml
<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=J7JournalDb;Integrated Security=SSPI;" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
Finnhub API (Web.config)
xml
<appSettings>
    <add key="FinnhubApiKey" value="YOUR_API_KEY_HERE" />
</appSettings>
License
This project is licensed under the MIT License.

Acknowledgments
Finnhub - Market data API

Chart.js - Charts

SignalR - Real-time communication

Bootstrap - Responsive design

Track. Analyze. Improve. 📈

text

---

## Add to Your Repo:

1. **Create README.md** in your project folder
2. **Copy and paste** the code above
3. **Save**
4. **Push**:

```powershell
git add README.md
git commit -m "Add README"
git push
