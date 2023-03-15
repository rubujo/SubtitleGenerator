namespace SubtitleGenerator.Commons.Sets;

/// <summary>
/// 列舉組
/// </summary>
internal class EnumSet
{
    /// <summary>
    /// 抽樣策略類型
    /// </summary>
    public enum SamplingStrategyType
    {
        /// <summary>
        /// 預設
        /// </summary>
        Default,
        /// <summary>
        /// 貪婪
        /// </summary>
        Greedy,
        /// <summary>
        /// 集束搜尋
        /// </summary>
        BeamSearch
    }

    /// <summary>
    /// OpenCC 類型
    /// </summary>
    public enum OpenCCMode
    {
        None,
        S2TWP,
        TW2SP
    }
}