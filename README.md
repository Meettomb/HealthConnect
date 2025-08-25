# Setup ASP.NET Core in Visual Studio & SSMS (SQL Server Management Studio)

## **1. Download & Install Prerequisites**

### **1.1 Install Visual Studio**
1. Go to [Visual Studio Download](https://visualstudio.microsoft.com/downloads/)
2. Download **Visual Studio Community/Professional/Enterprise** (Latest version)
3. Run the installer and select the following workloads:
   - **ASP.NET and web development**
   - **.NET Core cross-platform development**
4. Click **Install** and wait for the installation to complete.
5. Restart your system if prompted.

### **1.2 Install .NET SDK**
1. Visit [Download .NET](https://dotnet.microsoft.com/en-us/download)
2. Download and install the latest **.NET SDK (LTS version recommended).**
3. Verify installation:
   - Open **Command Prompt** or **PowerShell**
   - Run: `dotnet --version`
   - If installed correctly, it will display the version.

### **1.3 Install SQL Server & SSMS**
1. Download SQL Server:
   - Go to [SQL Server Download](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
   - Select **Developer Edition (Free for development and testing)**
   - Run the installer and choose **Basic Installation**
   - Complete the setup and note down the **server name**
2. Download SSMS:
   - Go to [SSMS Download](https://aka.ms/ssmsfullsetup)
   - Install and open SSMS
   - Connect to **SQL Server** using **Windows Authentication**

## **2. Setting Up an ASP.NET Core Project**

### **2.1 Create a New ASP.NET Core Project**
1. Open **Visual Studio**
2. Click **Create a new project**
3. Select **ASP.NET Core Web App** and click **Next**
4. Enter a **Project Name**, select a **Location**, and click **Create**
5. Choose **.NET version**, select **Razor Pages / MVC / API**
6. Click **Create**

### **2.2 Run the Project**
1. Open **Program.cs** and **Startup.cs** (if available)
2. Click the **Run Button (IIS Express / Kestrel)**
3. The project will open in your default browser
4. Verify that the ASP.NET web page is running

## **3. Connect ASP.NET Core to SQL Server**

### **3.1 Configure Connection String**
1. Open `appsettings.json`
2. Add the SQL Server connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=YOUR_DB_NAME;Trusted_Connection=True;"
   }
   ```
3. Replace `YOUR_SERVER_NAME` with your SQL Server instance name.
4. Replace `YOUR_DB_NAME` with your database name.

### **3.2 Install Entity Framework Core**
1. Open **Visual Studio Package Manager Console**
2. Run the following commands:
   ```powershell
   Install-Package Microsoft.EntityFrameworkCore.SqlServer
   Install-Package Microsoft.EntityFrameworkCore.Tools
   ```

### **3.3 Create and Apply Migrations**
1. Open **Package Manager Console**
2. Run:
   ```powershell
   Add-Migration InitialCreate
   Update-Database
   ```
3. Verify that the tables are created in SSMS.

## **4. Run & Test the Application**
1. Click **Run** in Visual Studio
2. Open **SSMS**, select your database, and check if data is being stored
3. Test endpoints (if using API) with Postman or Swagger

## **5. Deploy the ASP.NET Core Project**
### **5.1 Publish Locally**
1. Right-click on the **Project** in Solution Explorer
2. Select **Publish** > **Folder**
3. Choose a location and click **Publish**

### **5.2 Deploy to IIS**
1. Install IIS via Windows Features
2. Add **ASP.NET Core Hosting Bundle** ([Download](https://dotnet.microsoft.com/en-us/download/dotnet))
3. Deploy your project to IIS using **Web Deploy**

---
### ✅ **You have successfully set up ASP.NET Core with Visual Studio and SSMS!** 🎉

