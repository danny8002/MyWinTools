# Azure Portal Automation with Playwright

Automatically remove users from Azure App Registrations.

## Setup

1. Configure the following in [`Program.cs`](Program.cs):

   ```csharp
   public static string WindowsUser = "johndoe"; // Your Windows username
   
   public static string[] UsersToDelete = new[]
   {
       "user1@microsoft.com",
       "user2@microsoft.com"
   };
   
   public static string AppFilterContainString = "entitymatching";
   ```

2. Run the application. It will open a browser where you need to:
   - Sign in to Azure Portal
   - Navigate to **Microsoft Entra ID** > **App Registrations**
   - URL: https://ms.portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps

3. Type `yes` when prompted to start the automation.

## Requirements

- .NET 8.0
- Microsoft Playwright
- Google Chrome
