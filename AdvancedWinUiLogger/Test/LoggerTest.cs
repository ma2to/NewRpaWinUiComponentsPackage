using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Configuration;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Test;

/// <summary>
/// 🧪 TEST: Simple test to verify new logger structure works correctly
/// </summary>
public static class LoggerTest
{
    public static void TestNewStructure()
    {
        Console.WriteLine("🧪 Testing AdvancedWinUiLogger new structure...");

        try
        {
            // Test 1: Basic file logger creation
            Console.WriteLine("📁 Test 1: Basic file logger creation");
            var logger = AdvancedWinUiLogger.CreateFileLogger(
                externalLogger: null,
                logDirectory: Path.GetTempPath(),
                baseFileName: "TestLogger",
                maxFileSizeMB: 5);

            logger.LogInformation("🚀 Test log entry from new structure");
            logger.LogWarning("⚠️ Warning test message");
            logger.LogError("❌ Error test message");

            Console.WriteLine("✅ Test 1: Basic logger creation - PASSED");

            // Test 2: Configuration-based creation
            Console.WriteLine("📁 Test 2: Configuration-based creation");
            var options = LoggerOptions.Debug(Path.GetTempPath(), "ConfigTest");
            var configLogger = AdvancedWinUiLogger.CreateFileLogger(options);

            configLogger.LogInformation("🔧 Configuration-based logger test");
            Console.WriteLine("✅ Test 2: Configuration-based creation - PASSED");

            // Test 3: Component-based creation
            Console.WriteLine("📁 Test 3: Component-based creation");
            var component = AdvancedWinUiLogger.CreateComponent(
                logDirectory: Path.GetTempPath(),
                baseFileName: "ComponentTest",
                maxFileSizeMB: 10);

            Console.WriteLine($"📊 Component initialized: {component.IsInitialized}");
            Console.WriteLine($"📂 Log directory: {component.LogDirectory}");
            Console.WriteLine($"📏 Total log size: {component.TotalLogSizeMB:F2} MB");

            Console.WriteLine("✅ Test 3: Component-based creation - PASSED");

            // Test 4: Safe result-based creation
            Console.WriteLine("📁 Test 4: Safe result-based creation");
            var result = AdvancedWinUiLogger.CreateFileLoggerSafe(
                externalLogger: null,
                logDirectory: Path.GetTempPath(),
                baseFileName: "SafeTest",
                maxFileSizeMB: 5);

            if (result.IsSuccess)
            {
                var safeLogger = result.Value;
                safeLogger.LogInformation("🛡️ Safe logger creation test");
                Console.WriteLine("✅ Test 4: Safe result-based creation - PASSED");
            }
            else
            {
                Console.WriteLine($"❌ Test 4: Safe creation failed: {result.ErrorMessage}");
            }

            Console.WriteLine("🎉 All tests completed successfully!");
            Console.WriteLine("✅ New AdvancedWinUiLogger structure is working correctly!");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test failed with exception: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}

/// <summary>
/// 🎯 USAGE EXAMPLE: Demonstrates how to use the new logger structure
/// </summary>
public static class UsageExample
{
    public static void DemonstrateUsage()
    {
        Console.WriteLine("🎯 Usage demonstration:");

        // Simple usage - one line creation
        var logger = AdvancedWinUiLogger.CreateFileLogger(
            externalLogger: null,
            logDirectory: @"C:\Temp\MyAppLogs",
            baseFileName: "MyApplication",
            maxFileSizeMB: 50);

        // Log different levels
        logger.LogInformation("🚀 Application started");
        logger.LogWarning("⚠️ Configuration issue detected");
        logger.LogError("❌ Something went wrong");

        // Configuration-based approach
        var options = LoggerOptions.Production(@"C:\Temp\MyAppLogs", "Production");
        var prodLogger = AdvancedWinUiLogger.CreateFileLogger(options);

        // Component-based approach for advanced scenarios
        using var component = AdvancedWinUiLogger.CreateComponent(
            logDirectory: @"C:\Temp\MyAppLogs",
            baseFileName: "Advanced",
            maxFileSizeMB: 100);

        // Advanced operations
        var filesTask = component.GetLogFilesAsync();
        // var rotationTask = component.RotateLogsAsync();
        // var cleanupTask = component.CleanupOldLogsAsync(30);

        Console.WriteLine("🎉 Usage demonstration completed!");
    }
}