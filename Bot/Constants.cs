namespace Bot;

public static class Constants {
    public static string Directory_Base => AppDomain.CurrentDomain.BaseDirectory;
    
    // Logs
    public static string Directory_Data => Path.Combine(Directory_Base, "Data");
    public static string Directory_Downloaded => Path.Combine(Directory_Data, "Downloaded");
    public static string Directory_Extracted => Path.Combine(Directory_Data, "Extracted");
    
    // Other
    public static string Directory_Session => Path.Combine(Directory_Base, "Session");
}