# CityPress 0.2.0-beta.14

修复生成成功却因覆盖 Word 文件失败而不显示报纸的问题。

- 日志确认 IOException 位于替换旧 Word 文件时。现在先保存本地 JSON 归档，再导出 Word；原文件无法覆盖时自动另存带时间与标识的新副本，保留旧文件。
- 自定义目录不可用时自动保存到本地归档目录。所有保存位置都失败时，成稿仍在游戏内显示，可重新点击导出；界面明确提示保存状态。缓存写入失败也不再丢弃成稿。
- 修复统计采集使用 Unity 不支持的通用枚举接口而产生的 NotSupportedException，改用原生枚举方式。

离线测试已覆盖文件占用、目录不可用、全部保存失败及旧文件完整性。完整打包另验证游戏程序集和回调兼容性。未调用玩家付费API。

安装后重启游戏加载；新生成/重新导出使用新逻辑。

## English

Completed papers are archived before Word export and remain available in-game even if persistence fails. Locked Word targets receive a uniquely named new copy; unavailable export locations fall back to the local issue archive. Localized notices report recovery or deferred saves. Cache write failures are nonfatal. World.Systems scanning uses Unity's concrete enumerator rather than unsupported generic LINQ enumeration. Offline tests cover locks, blocked folders, total save failure and preservation of the existing document. Restart to load the update; no paid player API was used.
