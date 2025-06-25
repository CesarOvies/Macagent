namespace MacAgent.Services;

public static class DirectoryReferences
{
    // Operation System directorys.
    public static string SystemRoot => Environment.GetFolderPath(Environment.SpecialFolder.System);
    public static string ProgramFiles => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
    public static string ProgramFilesX86 => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
    public static string? SystemDrive => Path.GetPathRoot(Environment.SystemDirectory);

    // User directorys.
    public static string UserProfile => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    public static string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    public static string Documents => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public static string Downloads => Path.Combine(UserProfile, "Downloads");
    public static string Pictures => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    public static string Music => Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
    public static string Videos => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

    // Application directorys.
    public static string AppDirectory => AppContext.BaseDirectory;
    public static string AppData => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public static string CommonAppData => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    public static string Temp => Path.GetTempPath();

    // Especial directorys.
    public static string MacApplications => "/Applications";
    public static string MacUserApplications => Path.Combine(UserProfile, "Applications");
}