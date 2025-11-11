using Microsoft.Playwright;

class Program
{
    public static string WindowsUser = ""; // eg. "johndoe"

    // Define the users to delete
    public static string[] UsersToDelete = new[]
          {
                "xxx@microsoft.com",
            };

    public static string AppFilterContainString = "entitymatching";


    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Azure Portal Automation...");

        using var playwright = await Playwright.CreateAsync();

        string userDataDir = $@"C:\Users\{WindowsUser}\AppData\Local\Google\Chrome\User Data\Default";

        var browserContext = await playwright.Chromium.LaunchPersistentContextAsync(
            userDataDir,
            new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = false,
                ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                Args = new[] { "--start-maximized" }
            });

        var page = await browserContext.NewPageAsync();
        // Step 1: Open Azure Portal
        Console.WriteLine("\nStep 1: Opening Azure Portal...");
        await page.GotoAsync("https://portal.azure.com");
        Console.WriteLine("Azure Portal opened. Please complete any login if required.");

        // Step 2: Wait for user input
        Console.WriteLine("\nStep 2: Waiting for user confirmation...");
        var msg = "Please type 'yes' when ready to continue, you should be in app registration page https://ms.portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps ";
        Console.Write(msg);
        string? userInput = null;
        while (userInput?.ToLower() != "yes")
        {
            userInput = Console.ReadLine();
            if (userInput?.ToLower() != "yes")
            {
                Console.Write("Please type 'yes' when ready to continue");
            }
        }

        // Step 3: Perform operations
        while (await SelectApp(page))
        {
            // Step 4: Delete user operations
            Console.WriteLine("\nStep 4: Starting delete user operations...");
            await DeleteUserOperation(page);

            await GoBackToAppPage(page);
        }



        Console.WriteLine("\nAutomation completed successfully!");
        Console.WriteLine("Press any key to close the browser...");
        Console.ReadKey();
    }

    static async Task GoBackToAppPage(IPage page)
    {
        Console.WriteLine("Navigating back to App registrations page...");

        // Execute JavaScript to find and click the breadcrumb link
        var clicked = await page.EvaluateAsync<bool>(@"
            (function() {
                var link = document.querySelector('a[href=""#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/Overview""]');
                if (link) {
                    link.click();
                    console.log('Clicked breadcrumb link to App registrations');
                    return true;
                } else {
                    console.error('Breadcrumb link not found');
                    return false;
                }
            })()
        ");

        if (clicked)
        {
            Console.WriteLine("Successfully navigated back to App registrations page");
            await Task.Delay(3000); // Wait 3 seconds for page to load

            // Execute JavaScript to find and click the refresh button
            Console.WriteLine("Clicking refresh button...");
            var refreshClicked = await page.EvaluateAsync<bool>(@"
                (function() {
                    var refreshButton = document.querySelector('div[data-telemetryname=""Command-Refresh""]');
                    if (refreshButton) {
                        refreshButton.click();
                        console.log('Clicked refresh button');
                        return true;
                    } else {
                        console.error('Refresh button not found');
                        return false;
                    }
                })()
            ");

            if (refreshClicked)
            {
                Console.WriteLine("Successfully clicked refresh button");
                await Task.Delay(3000); // Wait for refresh to complete
            }
            else
            {
                Console.WriteLine("Warning: Could not find refresh button");
            }
        }
        else
        {
            Console.WriteLine("Warning: Could not find breadcrumb link");
        }
    }

    static async Task<bool> SelectApp(IPage page)
    {
        try
        {
            Console.WriteLine($"Executing JavaScript to find and click {AppFilterContainString} app...");

            // Execute JavaScript to find and click the app
            var result = await page.EvaluateAsync<bool>(@"
                (function selectEntityMatchingApp() {
                    var items = document.querySelectorAll('a[class~=""ext-ad-appslist-appname""]');
                    
                    for (var i = 0; i < items.length; i++) {
                        var item = items[i];
                        if (item.textContent.includes('XXXX')) {
                            console.log('Found XXXX app: ' + item.textContent);
                            item.click();
                            return true;
                        }
                    }
                    
                    return false;
                })()
            ".Replace("XXXX", AppFilterContainString));

            if (result)
            {
                Console.WriteLine($"Successfully clicked {AppFilterContainString} app");
                await Task.Delay(2000); // Wait for navigation
            }
            else
            {
                Console.WriteLine($"Warning: {AppFilterContainString} app not found");
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SelectApp: {ex}");
            return false;
        }
    }

    static async Task DeleteUserOperation(IPage page)
    {
        try
        {
            // Click Owners menu item using JavaScript
            Console.WriteLine("Clicking 'Owners' menu item...");
            var ownersClicked = await page.EvaluateAsync<bool>(@"
                (function clickOwners() {
                    var ownersButton = document.querySelector('div.fxc-menu-listView-item[data-telemetryname=""Menu-Owners""]');
                    if (ownersButton) {
                        ownersButton.click();
                        console.log('Clicked Owners menu item');
                        return true;
                    } else {
                        console.error('Owners menu item not found');
                        return false;
                    }
                })()
            ");

            if (!ownersClicked)
            {
                Console.WriteLine("Warning: Could not find Owners menu item");
                return;
            }

            Console.WriteLine("Clicked 'Owners'");
            await Task.Delay(3000); // Wait for page to load

            Console.WriteLine("Executing JavaScript to select and delete users...");


            // Select users one by one
            foreach (var user in UsersToDelete)
            {
                Console.WriteLine($"Selecting user: {user}");
                await page.EvaluateAsync($@"
                    (function() {{
                        var items = document.querySelectorAll('.fxc-gc-text');
                        
                        for (var i = 0; i < items.length; i++) {{
                            var item = items[i];
                            if (item.innerText.includes('{user}')) {{
                                var target = item.closest('.fxs-portal-hover');
                                if (target) {{
                                    var checkbox = target.querySelector('.fxc-gc-selectioncheckbox');
                                    if (checkbox) {{
                                        checkbox.click();
                                        console.log('Selected user: {user}');
                                    }}
                                }}
                            }}
                        }}
                    }})()
                ");
                await Task.Delay(300); // Small delay between selections
            }

            // Wait a bit to see the selections
            await Task.Delay(1000);

            // Click the remove button
            Console.WriteLine("Clicking remove button...");
            await page.EvaluateAsync(@"
                var removeButton = document.querySelector('div[data-telemetryname=""Command-removeButton""]');
                if (removeButton) {
                    removeButton.click();
                    console.log('Clicked remove button');
                } else {
                    console.error('Remove button not found');
                }
            ");

            // Wait for confirmation dialog
            await Task.Delay(1500);

            // Click the Delete confirmation button
            Console.WriteLine("Clicking Delete confirmation button...");
            await page.EvaluateAsync(@"
                var deleteButton = document.querySelector('div[class~=""fxs-messagebox-buttons""] div[title=""Delete""]');
                if (deleteButton) {
                    deleteButton.click();
                    console.log('Clicked Delete confirmation button');
                } else {
                    console.error('Delete button not found');
                }
            ");

            // Wait to see the result
            await Task.Delay(2000);

            Console.WriteLine("\nDelete user operations completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError during delete user operations: {ex.Message}");
            throw;
        }
    }
}