namespace SubtitleGenerator.Commons.Sets;

/// <summary>
/// 資料夾組
/// </summary>
public class FolderSet
{
    /// <summary>
    /// Bins 資料夾的路徑
    /// </summary>
    public static readonly string BinsFolderPath = Path.Combine(AppContext.BaseDirectory, "Bins");

    /// <summary>
    /// Models 資料夾的路徑
    /// </summary>
    public static readonly string ModelsFolderPath = Path.Combine(AppContext.BaseDirectory, "Models");

    /// <summary>
    /// Temp 資料夾的路徑
    /// </summary>
    public static readonly string TempFolderPath = Path.Combine(AppContext.BaseDirectory, "Temp");
}