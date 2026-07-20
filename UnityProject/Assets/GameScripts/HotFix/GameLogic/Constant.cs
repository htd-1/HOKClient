namespace GameLogic
{
    public static class Constant
    {
        // 仅保留编译期豁免常量。可配项已迁 Luban：
        //   - BGM/UI 音效（原 MainCityBGMusic/BattleBGMusic/LoginBtnClick/NormalBtnClick/MatchBtnClick/MatchSureBtnClick）
        //     → client_audio 表，经 ConfigService.GetAudio(AudioKey)
        public const bool DebugMuteAudio = true;
    }

    /// <summary>
    /// HOK 客户端常量（编译期豁免）。从旧 HOKClient ClientConfig 迁移。
    /// 可配项（手感数值 ScreenOPDis/SkillOPDis/SkillCancelDis）已迁 Luban client_setting 表，
    /// 经 ConfigService.ClientSetting 查询。
    /// </summary>
    public static class ClientConfig
    {
        public const int ScreenStandardWidth = 1920;
        public const int ScreenStandardHeight = 1080;
        // public const int ScreenOPDis = 135;
        /// <summary>
        /// 通用移动攻击 buff ID（全局语义标记，非可配数据）
        /// </summary>
        public const int CommonMoveAttackBuffID = 90000;
    }
}
